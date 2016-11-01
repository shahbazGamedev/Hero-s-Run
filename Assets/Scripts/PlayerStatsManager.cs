using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum PlayerInventoryEvent {
	Key_Changed = 0,
	Life_Changed = 1,
	Star_Changed = 2,
	Star_Doubler_Changed = 3,
	Score_Changed = 4,
	Key_Found_In_Episode_Changed = 5

}

public class PlayerStatsManager {
	
	private static PlayerStatsManager playerStatsManager = null;
	
	//Coin management
	int currentCoins = 0;			//The amount of coins (stars) the player currently has. This number always goes up, unless the player purchases something.
	int coinAccumulator = 0;
	Transform coinParent = null;
	int lifetimeCoins = 0;			//The amount of coins (stars) the player has earned over time. This is a statistic. It is used by the achievement system.
	//Assume there are 9 episodes for now. For each episode, the player has received between 0 and 3 stars. The number of stars is displayed on the world map.
	//The index is the episode number. The value is the number of stars between 0 and 3. The initial values are 0.
	int[] displayStarsArray = new int[LevelData.NUMBER_OF_EPISODES];

	//In each episode, a limited number of treasure chest keys have been hidden.
	//The index is the episode number. The value is the number of keys found by the player. The initial values are 0.
	int[] keysFoundInEpisodeArray = new int[LevelData.NUMBER_OF_EPISODES];
	//Number of treasure keys owned by the player
	int treasureKeysOwned = 0;

	//Delegate used to communicate to other classes when the number of keys, lives, stars or star doubler changes
	public delegate void PlayerInventoryChanged( PlayerInventoryEvent eventType, int newValue );
	public static event PlayerInventoryChanged playerInventoryChanged;

	//Delegate used to communicate to other classes when the power-up quantity changes
	public delegate void PowerUpInventoryChanged();
	public static event PowerUpInventoryChanged powerUpInventoryChanged;

	//High-score per episode.
	//The index is the episode number. The value is the score for that episode (sum of stars collected and distance run). The initial values are 0.
	int[] highScoreArray = new int[LevelData.NUMBER_OF_EPISODES];
	float distanceTravelled = 0;
	bool firstTimePlaying;
	bool sharedOnFacebook = false;	//Has the player shared an image on Facebook?

	bool showDebugInfoOnHUD = false; //Should we show the FPS, player speed, etc. on the HUD or not.
	int lives = 0;
	const int INITIAL_NUMBER_LIVES = 6;

	//If the user has logged in with Facebook this value is true, if he is logged in as guest, this value is false.
	bool usesFacebook = false;

	//DateTime when player last started WorldMapHandler
	DateTime dateLastPlayed;

	//For stats and achievements
	static EventCounter novice_runner = new EventCounter( GameCenterManager.NoviceRunner, 1, CounterType.Total_any_level, 1000 );
	static EventCounter coin_hoarder = new EventCounter( GameCenterManager.CoinHoarder, 1, CounterType.Total_any_level, 50000 );
	
	//Dictionary where the key is the PowerUpType and the value represents the amount of power-ups the player has either purchased or found in the episodes.
	//This inventory is only relevant for consumable power-ups.
	//This is saved as concatenated string. See defaultPowerUpsForNewPlayer as a format example.
	Dictionary<PowerUpType, PowerUpInventory> powerUpInventory = new Dictionary<PowerUpType, PowerUpInventory>(6);
	//Shield is 1, Magnet is 2, ZNuke is 3, MagicBoots is 4, SlowTime is 5, Speed Boost is 6
	//By default, players have three consumable power ups of each
	//Players can upgrade their power ups from a level of 0 to 6
	//For example, the duration of the Shield power up when upgraded to level 3 is 20 seconds (instead of 10).
	//By default, the upgrade level for each is 0.
	//Format is : PowerUpType,quantity,upgrade level
	const string defaultPowerUpsForNewPlayer = "1,0,0,2,0,0,3,3,0,4,3,0,5,3,0,6,3,0";
	
	//Sound volume between 0 and 1f.
	float soundVolume = 1f;

	//The number of times the player was revived for the current run.
	//This is used to calculate the cost to revive since the cost increases by one each time the player is revived.
	//This value is not saved.
	//This value is reset each time the player:
	//1) restarts the level from the last checkpoint
	//2) passes a Checkpoint
	//3) uses a cullis gate
	int timesPlayerRevivedInLevel = 0;

	//This value is saved.
	//The index is the episode number. The value is the number of times the player died for that specific episode. The initial values are 0.
	//The value for a specific episode is reset when the player restarts that episode.
	int[] deathInEpisodesArray = new int[LevelData.NUMBER_OF_EPISODES];

	List<string> treasureKeysFound = new List<string>();

	//For debugging
	//Cheat
	bool hasInfiniteLives = false;
	bool hasInfiniteTreasureIslandKeys = false;

	PowerUpType powerUpSelected = PowerUpType.MagicBoots;
	DifficultyLevel difficultyLevel = DifficultyLevel.Normal;
	//The avatar the player has chosen in the character selection screen.
	//The enum will be converted using toString. The available avatars in the resources/avatar folder must be named
	//exactly like their enum counterpart.
	Avatar avatar = Avatar.None;

	string userName;

	bool ownsStarDoubler = false; //True if the player has purchased the Star Doubler in the store.

	string challenges = String.Empty; //List of Challenge in JSON format. See ChallengeBoard.

	public static PlayerStatsManager Instance
	{
        get
		{
            if (playerStatsManager == null)
			{

                playerStatsManager = new PlayerStatsManager();

            }
            return playerStatsManager;
        }
    } 
				
	public void addToDistanceTravelled( float addDistanceTravelled )
	{
		distanceTravelled = distanceTravelled + addDistanceTravelled;
		//We have an achievement when the player runs 1000 meters for the first time.
		if( distanceTravelled >= novice_runner.valueNeededToSucceed )
		{
			novice_runner.incrementCounter();
		}

	}

	//For debugging
	//Cheat
	public bool getHasInfiniteLives()
	{
		return hasInfiniteLives;
	}

	public bool getHasInfiniteTreasureIslandKeys()
	{
		return hasInfiniteTreasureIslandKeys;
	}

	public DifficultyLevel getDifficultyLevel()
	{
		return difficultyLevel;
	}
	
	public string getDifficultyLevelName()
	{
		string difficultyLevelName = System.String.Empty;

		switch (difficultyLevel)
		{
		case DifficultyLevel.Normal:
			difficultyLevelName = LocalizationManager.Instance.getText("MENU_DIFFICULTY_LEVEL_NORMAL");
			break;
			
		case DifficultyLevel.Heroic:
			difficultyLevelName = LocalizationManager.Instance.getText("MENU_DIFFICULTY_LEVEL_HEROIC");
			break;
			
		case DifficultyLevel.Legendary:
			difficultyLevelName = LocalizationManager.Instance.getText("MENU_DIFFICULTY_LEVEL_LEGENDARY");
			break;
			
		}
		return difficultyLevelName;
	}

	public void setDifficultyLevel( DifficultyLevel difficulty )
	{
		difficultyLevel = difficulty;
		savePlayerStats();
	}
	
	public DifficultyLevel getNextDifficultyLevel()
	{
		int index = (int)difficultyLevel;
		//we want the next one
		index++;
		//Make sure we stay within the range of the Difficulty Levels: Normal = 1, Heroic = 2, Legendary = 3
		if( index > 3 ) index = 1;
		return (DifficultyLevel)index;
		
	}
	
	public DifficultyLevel getPreviousDifficultyLevel()
	{
		int index = (int)difficultyLevel;
		//we want the previous one
		index--;
		//Make sure we stay within the range of the Difficulty Levels: Normal = 1, Heroic = 2, Legendary = 3
		if( index < 1 ) index = 3;
		return (DifficultyLevel)index;
	}
	
	public Avatar getAvatar()
	{
		return avatar;
	}
	
	public string getAvatarName()
	{
		return avatar.ToString();
	}
	
	public void setAvatar( Avatar value )
	{
		avatar = value;
		savePlayerStats();
		Debug.Log("PlayerStatsManager: Avatar is " + avatar );
	}
	
	public Avatar getNextAvatar()
	{
		int index = (int)avatar;
		//we want the next one
		index++;
		//Make sure we stay within the range of the available avatars: None = 0, Male_Hero = 1, Female_Hero =2
		if( index > 2 ) index = 1;
		return (Avatar)index;
		
	}
	
	public Avatar getPreviousAvatar()
	{
		int index = (int)avatar;
		//we want the previous one
		index--;
		//Make sure we stay within the range of the available avatars: None = 0, Male_Hero = 1, Female_Hero =2
		if( index < 1 ) index = 2;
		return (Avatar)index;
	}

	public string getUserName()
	{
		return userName;
	}
	
	public void saveUserName( string value )
	{
		userName = value;
		savePlayerStats();
	}

	public void setPowerUpSelected( PowerUpType value )
	{
		//Only proceed if the value has changed
		if( value != powerUpSelected )
		{
			powerUpSelected = value;
			savePlayerStats();
		}
	}

	public PowerUpType getPowerUpSelected()
	{
		return powerUpSelected;
	}

	public void incrementTimesPlayerRevivedInLevel()
	{
		timesPlayerRevivedInLevel++;
	}

	public int getTimesPlayerRevivedInLevel()
	{
		return timesPlayerRevivedInLevel;
	}

	public void resetTimesPlayerRevivedInLevel()
	{
		timesPlayerRevivedInLevel = 0;
	}

	public int getDistanceTravelled()
	{
		return (int) distanceTravelled;
	}

	public void resetDistanceTravelled()
	{
		distanceTravelled = 0;
	}

	public void setDateLastPlayed()
	{
		dateLastPlayed = DateTime.Now;
	}

	public DateTime getDateLastPlayed()
	{
		return dateLastPlayed;
	}

	//The name of the coin indicates how many coins to give
	//So Coin_1 means one, Coins_25 means 25, etc.
	public void modifyCoinCount( GameObject coin )
	{
		string coinName = coin.name;
		coinName = coinName.Replace("(Clone)","");
		int index = coinName.IndexOf("_") + 1;
		string numberPortion = coinName.Substring(index);
		int numberAsInt;
		int.TryParse(numberPortion, out numberAsInt);
		modifyCurrentCoins( numberAsInt, true, false );
		//Accumulator
		if( coinParent == coin.transform.parent )
		{
			coinAccumulator = coinAccumulator + numberAsInt;
		}
		else
		{
			coinAccumulator = numberAsInt;
			coinParent = coin.transform.parent;
		}

		CoinSeriesInfo coinSeriesInfo = (CoinSeriesInfo) coin.GetComponent( typeof(CoinSeriesInfo) );
		if( coinSeriesInfo != null )
		{
			if( coinSeriesInfo.isLastCoinOfSeries )
			{
				HUDHandler.hudHandler.displayStarPickup( coinAccumulator, coinSeriesInfo.coinColor );
				//We don't want to play the coin pickup sound too often, so we only play it when it is the 
				//last coin of the series.
				CoinManager.coinManager.playCoinPickupSound();
			}
		}
	}

	public void modifyCurrentCoins( int coins, bool incrementLifetimeCoins, bool isPurchase )
	{
		//Player gets twice the amount of Stars if they own the star doubler permanent item and this is not a purchase
		if( ownsStarDoubler && !isPurchase && coins > 0 ) coins = coins * 2;

		currentCoins = currentCoins + coins;

		LevelManager.Instance.incrementScore( coins );

		//Send an event to interested classes
		if(playerInventoryChanged != null) playerInventoryChanged(PlayerInventoryEvent.Score_Changed, LevelManager.Instance.getScore() );
		if(playerInventoryChanged != null) playerInventoryChanged(PlayerInventoryEvent.Star_Changed, currentCoins );

		//Also add to lifetime coins.
		if( incrementLifetimeCoins )
		{
			lifetimeCoins = lifetimeCoins + coins;
			if( lifetimeCoins >= coin_hoarder.valueNeededToSucceed )
			{
				coin_hoarder.incrementCounter();
			}
		}
	}

	public int getCurrentCoins()
	{
		return currentCoins;
	}
	
	public int getLifetimeCoins()
	{
		return lifetimeCoins;
	}
	
	void loadDisplayStars()
	{
		string displayStarsString = PlayerPrefs.GetString("displayStars", "" );
		if( displayStarsString == "" )
		{
			//displayStarsArray just stays with initial values of 0 because this is a new player
		}
		else
		{
			try
			{
				string[] displayStarsStringArray = displayStarsString.Split(',');
				//Debug.Log ("loadDisplayStars " + displayStarsString + " length " + displayStarsStringArray.Length );
				for( int i = 0; i < displayStarsStringArray.Length; i++ )
				{
					int numberOfStarsAsInt;
					int.TryParse(displayStarsStringArray[i], out numberOfStarsAsInt);
					displayStarsArray[i] = numberOfStarsAsInt;
					//Next line for debugging
					//displayStarsArray[i] = (int) UnityEngine.Random.Range(0,4);
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("PlayerStatsManager-exception occured in loadDisplayStars: " + e.Message + " Resetting stats to default values.");
				PlayerPrefs.DeleteAll();
			}
		}
	}

	void saveDisplayStars()
	{
		string result = "";
		for( int i = 0; i < displayStarsArray.Length; i++ )
		{
			result = result + displayStarsArray[i].ToString() + ",";
		}
		result = result.TrimEnd(',');
		//Debug.Log("saveDisplayStars " + result );
		PlayerPrefs.SetString("displayStars", result );
	}

	void resetDisplayStars()
	{
		for( int i = 0; i < displayStarsArray.Length; i++ )
		{
			displayStarsArray[i] = 0;
		}
		saveDisplayStars();
	}

	public int getNumberDisplayStarsForEpisode( int episodeNumber )
	{
		return displayStarsArray[episodeNumber];
	}

	public void setNumberDisplayStarsForEpisode( int numberOfStars )
	{
		displayStarsArray[LevelManager.Instance.getCurrentEpisodeNumber()] = numberOfStars;
	}

	void loadHighScores()
	{
		string highScoresString = PlayerPrefs.GetString("highScores", "" );
		if( highScoresString == "" )
		{
			//highScoreArray just stays with initial values of 0 because this is a new player
		}
		else
		{
			try
			{
				string[] highScoresStringArray = highScoresString.Split(',');
				//Debug.Log ("loadHighScores " + highScoresString + " length " + highScoresStringArray.Length );
				for( int i = 0; i < highScoresStringArray.Length; i++ )
				{
					int highScoreAsInt;
					int.TryParse(highScoresStringArray[i], out highScoreAsInt);
					highScoreArray[i] = highScoreAsInt;
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("PlayerStatsManager-exception occured in loadHighScores: " + e.Message + " Resetting stats to default values.");
				PlayerPrefs.DeleteAll();
			}
		}
	}

	void saveHighScores()
	{
		string result = "";
		for( int i = 0; i < highScoreArray.Length; i++ )
		{
			result = result + highScoreArray[i].ToString() + ",";
		}
		result = result.TrimEnd(',');
		//Debug.Log("saveHighScores " + result );
		PlayerPrefs.SetString("highScores", result );
	}

	void resetHighScores()
	{
		for( int i = 0; i < highScoreArray.Length; i++ )
		{
			highScoreArray[i] = 0;
		}
		saveHighScores();
	}

	public int getHighScoreForEpisode( int episodeNumber )
	{
		return highScoreArray[episodeNumber];
	}

	public bool setHighScoreForEpisode( int highScore )
	{
		if( highScore > highScoreArray[LevelManager.Instance.getCurrentEpisodeNumber()] )
		{
			highScoreArray[LevelManager.Instance.getCurrentEpisodeNumber()] = highScore;
			saveHighScores();
			return true;
		}
		else
		{
			return false;
		}
	}

	void loadDeathInEpisodes()
	{
		string deathInEpisodesString = PlayerPrefs.GetString("deathInEpisodes", "" );
		if( deathInEpisodesString == "" )
		{
			//deathInEpisodesArray just stays with initial values of 0 because this is a new player
		}
		else
		{
			try
			{
				string[] deathInEpisodesStringArray = deathInEpisodesString.Split(',');
				//Debug.Log ("deathInEpisodesStringArray " + deathInEpisodesString + " length " + deathInEpisodesStringArray.Length );
				for( int i = 0; i < deathInEpisodesStringArray.Length; i++ )
				{
					int numberOfDeathsAsInt;
					int.TryParse(deathInEpisodesStringArray[i], out numberOfDeathsAsInt);
					deathInEpisodesArray[i] = numberOfDeathsAsInt;
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("PlayerStatsManager-exception occured in loadDeathInEpisodes: " + e.Message + " Resetting stats to default values.");
				PlayerPrefs.DeleteAll();
			}
		}
	}

	void saveDeathInEpisodes()
	{
		string result = "";
		for( int i = 0; i < deathInEpisodesArray.Length; i++ )
		{
			result = result + deathInEpisodesArray[i].ToString() + ",";
		}
		result = result.TrimEnd(',');
		//Debug.Log("saveDeathInEpisodes " + result );
		PlayerPrefs.SetString("deathInEpisodes", result );
	}

	public string getDeathInEpisodesAsString()
	{
		string result = "";
		for( int i = 0; i < deathInEpisodesArray.Length; i++ )
		{
			result = result + deathInEpisodesArray[i].ToString() + ",";
		}
		result = result.TrimEnd(',');
		return result;
	}

	public void resetDeathInEpisodes()
	{
		for( int i = 0; i < deathInEpisodesArray.Length; i++ )
		{
			deathInEpisodesArray[i] = 0;
		}
		saveDeathInEpisodes();
	}

	public void incrementNumberDeathForEpisode()
	{
		deathInEpisodesArray[LevelManager.Instance.getNextEpisodeToComplete()]++;
	}

	public void resetNumberDeathsStartingAtEpisode( int episodeNumber )
	{
		for( int i = episodeNumber; i < deathInEpisodesArray.Length; i++ )
		{
			deathInEpisodesArray[i] = 0;
		}
		//Debug.Log("PlayerStatsManager-resetNumberDeathsStartingAtEpisode: " + episodeNumber );
	}

	public int getNumberDeathLeadingToEpisode( int episodeNumber )
	{
		int total = 0;
		for( int i = 0; i <= episodeNumber; i++ )
		{
			total = total + deathInEpisodesArray[i];
		}
		return total;
	}

	void loadKeysFoundInEpisode()
	{
		string keysFoundInEpisodeString = PlayerPrefs.GetString("keysFoundInEpisode", "" );
		if( keysFoundInEpisodeString == "" )
		{
			//keysFoundInEpisodeArray just stays with initial values of 0 because this is a new player
		}
		else
		{
			try
			{
				string[] keysFoundInEpisodeStringArray = keysFoundInEpisodeString.Split(',');
				//Debug.Log ("loadKeysFoundInEpisode " + keysFoundInEpisodeString + " length " + keysFoundInEpisodeStringArray.Length );
				for( int i = 0; i < keysFoundInEpisodeStringArray.Length; i++ )
				{
					int numberOfKeysAsInt;
					int.TryParse(keysFoundInEpisodeStringArray[i], out numberOfKeysAsInt);
					keysFoundInEpisodeArray[i] = numberOfKeysAsInt;
					//Next line for debugging
					//keysFoundInEpisodeArray[i] = (int) UnityEngine.Random.Range(0,7);
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("PlayerStatsManager-exception occured in loadKeysFoundInEpisode: " + e.Message + " Resetting stats to default values.");
				PlayerPrefs.DeleteAll();
			}
		}
	}

	void saveKeysFoundInEpisode()
	{
		string result = "";
		for( int i = 0; i < keysFoundInEpisodeArray.Length; i++ )
		{
			result = result + keysFoundInEpisodeArray[i].ToString() + ",";
		}
		result = result.TrimEnd(',');
		//Debug.Log("saveKeysFoundInEpisode " + result );
		PlayerPrefs.SetString("keysFoundInEpisode", result );
	}

	void resetKeysFoundInEpisode()
	{
		for( int i = 0; i < keysFoundInEpisodeArray.Length; i++ )
		{
			keysFoundInEpisodeArray[i] = 0;
		}
		saveKeysFoundInEpisode();
	}

	public int getNumberKeysFoundInEpisode( int episodeNumber )
	{
		return keysFoundInEpisodeArray[episodeNumber];
	}

	public void incrementNumberKeysFoundInEpisode()
	{
		keysFoundInEpisodeArray[LevelManager.Instance.getCurrentEpisodeNumber()]++;
		if(playerInventoryChanged != null) playerInventoryChanged(PlayerInventoryEvent.Key_Found_In_Episode_Changed, keysFoundInEpisodeArray[LevelManager.Instance.getCurrentEpisodeNumber()] );
		increaseTreasureKeysOwned(1);
	}

	void loadTreasureKeysFound()
	{
		string treasureKeysFoundString = PlayerPrefs.GetString("treasureKeysFound", "" );
		treasureKeysFound.Clear();
		if( treasureKeysFoundString == "" )
		{
			//treasureKeysFound just stays empty because this is a new player
		}
		else
		{
			try
			{
				string[] TreasureKeysFoundStringArray = treasureKeysFoundString.Split(',');
				//Debug.Log ("loadTreasureKeysFound " + treasureKeysFoundString + " length " + TreasureKeysFoundStringArray.Length );
				for( int i = 0; i < TreasureKeysFoundStringArray.Length; i++ )
				{
					treasureKeysFound.Add( TreasureKeysFoundStringArray[i] );
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("PlayerStatsManager-exception occured in loadTreasureKeysFound: " + e.Message + " Resetting stats to default values.");
				PlayerPrefs.DeleteAll();
			}
		}
	}

	void saveTreasureKeysFound()
	{
		string result = "";
		for( int i = 0; i < treasureKeysFound.Count; i++ )
		{
			result = result + treasureKeysFound[i].ToString() + ",";
		}
		result = result.TrimEnd(',');
		//Debug.Log("saveTreasureKeysFound " + result );
		PlayerPrefs.SetString("treasureKeysFound", result );
	}

	public void resetTreasureKeysFound()
	{
		treasureKeysFound.Clear();
		PlayerPrefs.SetString("treasureKeysFound", "" );
	}

	public void foundTreasureKey( string treasureKeyID )
	{
		if( treasureKeysFound.Contains( treasureKeyID ) )
		{
			//Ignore. We already have found that key.
			Debug.LogWarning( "PlayerStatsManager-foundTreasureKey: ignoring because we already have key: " + treasureKeyID );
		}
		else
		{
			treasureKeysFound.Add( treasureKeyID );
			saveTreasureKeysFound();
			PlayerStatsManager.Instance.savePlayerStats();
		}
	}

	public bool hasThisTreasureKeyBeenFound( string treasureKeyID )
	{
		return treasureKeysFound.Contains( treasureKeyID );
	}

	//Used to identify if the player is a new user and it is his first time launching the game
	public bool isFirstTimePlaying()
	{
		return firstTimePlaying;
	}

	public void setSharedOnFacebook( bool value )
	{
		sharedOnFacebook = value;
	}

	public bool getSharedOnFacebook()
	{
		return sharedOnFacebook;
	}

	public void setShowDebugInfoOnHUD( bool value )
	{
		showDebugInfoOnHUD = value;
	}

	public bool getShowDebugInfoOnHUD()
	{
		return showDebugInfoOnHUD;
	}

	public void setOwnsStarDoubler( bool value )
	{
		ownsStarDoubler = value;
		if(playerInventoryChanged != null) playerInventoryChanged(PlayerInventoryEvent.Star_Doubler_Changed, 0 );
	}
	
	public bool getOwnsStarDoubler()
	{
		return ownsStarDoubler;
	}

	public float getSoundVolume()
	{
		return soundVolume;
	}
	
	public void setSoundVolume( float volume )
	{
		soundVolume = volume;
		AudioListener.volume = soundVolume;
	}

	public void setUsesFacebook( bool value )
	{
		usesFacebook = value;
	}
	
	public bool getUsesFacebook()
	{
		return usesFacebook;
	}

	void setLives( int value )
	{
		lives = value;
		//Send an event to interested classes
		if(playerInventoryChanged != null) playerInventoryChanged(PlayerInventoryEvent.Life_Changed, lives );
	}
	
	public int getLives()
	{
		return lives;
	}

	public void increaseLives(int value)
	{
		setLives( lives + value );
	}

	public void decreaseLives(int value)
	{
		int newValue = lives - value;
		if( newValue < 0 ) newValue = 0;
		setLives( newValue );
	}

	void setTreasureKeysOwned( int value )
	{
		treasureKeysOwned = value;
		//Send an event to interested classes
		if(playerInventoryChanged != null) playerInventoryChanged(PlayerInventoryEvent.Key_Changed, treasureKeysOwned );
	}
	
	public int getTreasureKeysOwned()
	{
		return treasureKeysOwned;
	}
	
	public void increaseTreasureKeysOwned(int value)
	{
		setTreasureKeysOwned( treasureKeysOwned + value );
	}
	
	public void decreaseTreasureKeysOwned(int value)
	{
		int newValue = treasureKeysOwned - value;
		if( newValue < 0 ) newValue = 0;
		setTreasureKeysOwned( newValue );

	}
	
	public void addToPowerUpInventory( PowerUpType type, int quantity )
	{
		if( powerUpInventory.ContainsKey(type) )
		{
			powerUpInventory[type].quantity = powerUpInventory[type].quantity + quantity;
			if( powerUpInventoryChanged != null ) powerUpInventoryChanged();
			savePlayerStats();
		}
	}

	public void incrementPowerUpInventory( PowerUpType type )
	{
		if( powerUpInventory.ContainsKey(type) )
		{
			powerUpInventory[type].quantity++;
			if( powerUpInventoryChanged != null ) powerUpInventoryChanged();
			savePlayerStats();
		}
	}
	
	public void decrementPowerUpInventory( PowerUpType type )
	{
		if( powerUpInventory.ContainsKey(type) )
		{
			powerUpInventory[type].quantity--;
			if( powerUpInventory[type].quantity < 0 ) powerUpInventory[type].quantity = 0;
			if( powerUpInventoryChanged != null ) powerUpInventoryChanged();
			savePlayerStats();
		}
	}

	void ClearPowerUpInventory()
	{
		powerUpInventory.Clear();
		PlayerPrefs.SetString("powerUpInventory", defaultPowerUpsForNewPlayer );
	}
	
	public int getPowerUpQuantity( PowerUpType type )
	{
		if( powerUpInventory.ContainsKey(type) )
		{
			return powerUpInventory[type].quantity;
		}
		return 0;
	}
	
	public int getPowerUpUpgradeLevel( PowerUpType type )
	{
		if( powerUpInventory.ContainsKey(type) )
		{
			return powerUpInventory[type].upgradeLevel;
		}
		return 0;
	}

	public void setPowerUpUpgradeLevel( PowerUpType type, int value )
	{
		if( powerUpInventory.ContainsKey(type) )
		{
			powerUpInventory[type].upgradeLevel = value;
		}
	}

	void savePowerUpInventory()
	{
		string result = "";
		foreach(KeyValuePair<PowerUpType, PowerUpInventory> pair in powerUpInventory) 
		{
			int powerUpTypeAsInt = (int)pair.Key;
			result = result + powerUpTypeAsInt.ToString() + "," + pair.Value.quantity.ToString() + "," + pair.Value.upgradeLevel.ToString() + ",";
		}
		result = result.TrimEnd(',');
		//Debug.Log("savePowerUpInventory " + result );
		PlayerPrefs.SetString("powerUpInventory", result );
	}

	void loadPowerUpInventory()
	{
		try
		{
			string powerUpInventoryString = PlayerPrefs.GetString("powerUpInventory", defaultPowerUpsForNewPlayer );
			string[] powerUpInventoryArray = powerUpInventoryString.Split(',');
			//Debug.Log ("loadPowerUpInventory " + powerUpInventoryString + " length " + powerUpInventoryArray.Length );
			for( int i = 0; i < powerUpInventoryArray.Length; i = i+3 )
			{
				int powerUpTypeAsInt;
				int.TryParse(powerUpInventoryArray[i], out powerUpTypeAsInt);
				PowerUpType powerUpType = (PowerUpType)powerUpTypeAsInt;

				int quantityAsInt;
				int.TryParse(powerUpInventoryArray[i+1], out quantityAsInt);

				int upgradeLevelAsInt;
				int.TryParse(powerUpInventoryArray[i+2], out upgradeLevelAsInt);

				//Debug.Log("loadPowerUpInventory " + powerUpType + " " + quantityAsInt + " " + upgradeLevelAsInt );
				if( !powerUpInventory.ContainsKey( powerUpType ) )
				{
					PowerUpInventory pi = new PowerUpInventory();
					pi.quantity = quantityAsInt;
					pi.upgradeLevel = upgradeLevelAsInt;
					powerUpInventory.Add( powerUpType, pi );
				}
			}
		}
		catch (Exception e)
		{
			Debug.LogWarning("PlayerStatsManager-exception occured in loadPowerUpInventory: " + e.Message + " Resetting stats to default values.");
			PlayerPrefs.DeleteAll();
		}
	}

	public void setChallenges( string challenges )
	{
		this.challenges = challenges;
	}

	public string getChallenges()
	{
		return challenges;
	}

	public void loadPlayerStats()
	{
		try
		{
			int nextEpisodeToComplete = PlayerPrefs.GetInt("Next Episode To Complete", 0 );
			LevelManager.Instance.setNextEpisodeToComplete( nextEpisodeToComplete );
			GameMode savedGameMode = (GameMode)PlayerPrefs.GetInt("Game Mode", (int)GameMode.Story);
			GameManager.Instance.setGameMode( savedGameMode );
			int highestEpisodeCompleted = PlayerPrefs.GetInt("Highest Episode Completed", 0 );
			LevelManager.Instance.setHighestEpisodeCompleted( highestEpisodeCompleted );
			string playerFinishedTheGameString = PlayerPrefs.GetString("Finished Game", "false" );
			if( playerFinishedTheGameString == "true" )
			{
				LevelManager.Instance.setPlayerFinishedTheGame( true );
			}
			else
			{
				LevelManager.Instance.setPlayerFinishedTheGame( false );
			}
			setLives( PlayerPrefs.GetInt("Lives", INITIAL_NUMBER_LIVES) );
			setTreasureKeysOwned( PlayerPrefs.GetInt("treasureKeysOwned", 0) );
			string firstTimePlayingString = PlayerPrefs.GetString("First Time Playing", "true" );
			if( firstTimePlayingString == "true" )
			{
				firstTimePlaying = true;
			}
			else
			{
				firstTimePlaying = false;	
			}
			string showDebugInfoOnHUDString = PlayerPrefs.GetString("showDebugInfoOnHUD", "false" );
			if( showDebugInfoOnHUDString == "true" )
			{
				showDebugInfoOnHUD = true;
			}
			else
			{
				showDebugInfoOnHUD = false;	
			}
			string sharedOnFacebookString = PlayerPrefs.GetString("sharedOnFacebook", "false" );
			if( sharedOnFacebookString == "true" )
			{
				sharedOnFacebook = true;
			}
			else
			{
				sharedOnFacebook = false;	
			}
			string ownsStarDoublerString = PlayerPrefs.GetString("ownsStarDoubler", "false" );
			if( ownsStarDoublerString == "true" )
			{
				setOwnsStarDoubler( true );
			}
			else
			{
				setOwnsStarDoubler( false );
			}

			string usesFacebookString = PlayerPrefs.GetString("usesFacebook", "false" );
			if( usesFacebookString == "true" )
			{
				usesFacebook = true;
			}
			else
			{
				usesFacebook = false;	
			}
			string dateLastPlayedString = PlayerPrefs.GetString( "dateLastPlayed", "" );
			if( dateLastPlayedString == "" )
			{
				dateLastPlayed = DateTime.Now;
			}
			else
			{
				dateLastPlayed = DateTime.Parse( dateLastPlayedString );
			}

			setSoundVolume( PlayerPrefs.GetFloat("soundVolume", 1f ) );
			currentCoins = PlayerPrefs.GetInt("currentCoins", 0);
			lifetimeCoins = PlayerPrefs.GetInt("lifetimeCoins", 0);
			loadDisplayStars();
			loadHighScores();
			loadDeathInEpisodes();
			loadKeysFoundInEpisode();
			loadTreasureKeysFound();
			powerUpSelected = (PowerUpType)PlayerPrefs.GetInt("powerUpSelected", (int)PowerUpType.SlowTime);
			difficultyLevel = (DifficultyLevel)PlayerPrefs.GetInt("difficultyLevel", (int)DifficultyLevel.Normal);
			avatar = (Avatar)PlayerPrefs.GetInt("avatar", (int)Avatar.None);
			userName = PlayerPrefs.GetString("userName", "" );
			loadPowerUpInventory();
			challenges = PlayerPrefs.GetString("challenges", "" );
			//Debug.Log ("loadPlayerStats-firstTimePlaying: " + firstTimePlaying + " ownsStarDoubler: " + ownsStarDoubler + " Next Episode To Complete: " + nextEpisodeToComplete + " Highest Episode Completed: " + highestEpisodeCompleted + " Finished game: " + LevelManager.Instance.getPlayerFinishedTheGame() + " Lives: " + lives + " Date Last Played: " + dateLastPlayed + " difficultyLevel " + difficultyLevel + " treasureKeysOwned " + treasureKeysOwned );
		}
		catch (Exception e)
		{
			Debug.LogWarning("PlayerStatsManager-exception occured while loading player stats: " + e.Message + " Resetting stats to default values.");
			PlayerPrefs.DeleteAll();
		}
	}
	
	public void savePlayerStats()
	{
		PlayerPrefs.SetInt("Next Episode To Complete", LevelManager.Instance.getNextEpisodeToComplete() );
		PlayerPrefs.SetInt("Game Mode", (int) GameManager.Instance.getGameMode() );
		PlayerPrefs.SetInt("Highest Episode Completed", LevelManager.Instance.getHighestEpisodeCompleted() );
		PlayerPrefs.SetInt("Lives", lives );
		PlayerPrefs.SetInt("treasureKeysOwned", treasureKeysOwned );
		if( LevelManager.Instance.getPlayerFinishedTheGame() )
		{
			PlayerPrefs.SetString( "Finished Game", "true" );
		}
		else
		{
			PlayerPrefs.SetString( "Finished Game", "false" );
		}
		PlayerPrefs.SetString( "First Time Playing", "false" );
		if( showDebugInfoOnHUD )
		{
			PlayerPrefs.SetString( "showDebugInfoOnHUD", "true" );
		}
		else
		{
			PlayerPrefs.SetString( "showDebugInfoOnHUD", "false" );
		}
		if( sharedOnFacebook )
		{
			PlayerPrefs.SetString( "sharedOnFacebook", "true" );
		}
		else
		{
			PlayerPrefs.SetString( "sharedOnFacebook", "false" );
		}
		if( ownsStarDoubler )
		{
			PlayerPrefs.SetString( "ownsStarDoubler", "true" );
		}
		else
		{
			PlayerPrefs.SetString( "ownsStarDoubler", "false" );
		}
		if( usesFacebook )
		{
			PlayerPrefs.SetString( "usesFacebook", "true" );
		}
		else
		{
			PlayerPrefs.SetString( "usesFacebook", "false" );
		}
		PlayerPrefs.SetString( "dateLastPlayed", dateLastPlayed.ToString() );

		PlayerPrefs.SetFloat("soundVolume", soundVolume );

		PlayerPrefs.SetInt("currentCoins", currentCoins );
		PlayerPrefs.SetInt("lifetimeCoins", lifetimeCoins );
		saveDisplayStars();
		saveHighScores();
		saveDeathInEpisodes();
		saveKeysFoundInEpisode();
		saveTreasureKeysFound();
		PlayerPrefs.SetInt("powerUpSelected", (int)powerUpSelected );
		PlayerPrefs.SetInt("difficultyLevel", (int)difficultyLevel );
		PlayerPrefs.SetInt("avatar", (int)avatar );
		PlayerPrefs.SetString( "userName", userName );
		savePowerUpInventory();
		PlayerPrefs.SetString( "challenges", challenges );
		PlayerPrefs.Save();
		//Debug.Log ("savePlayerStats-firstTimePlaying: " + firstTimePlaying + " ownsStarDoubler: " + ownsStarDoubler + " usesFacebook: "  + usesFacebook + " Date Last Played: " + dateLastPlayed );
	}
	
	//Used for debugging
	public void resetPlayerStats()
	{
		PlayerPrefs.SetInt("Next Episode To Complete", 0 );
		LevelManager.Instance.setNextEpisodeToComplete( 0 );
		PlayerPrefs.SetInt("Game Mode", (int) GameMode.Story );
		GameManager.Instance.setGameMode( GameMode.Story );
		PlayerPrefs.SetInt("Highest Episode Completed", 0 );
		LevelManager.Instance.setHighestEpisodeCompleted( 0 );
		PlayerPrefs.SetInt("Lives", INITIAL_NUMBER_LIVES );
		setLives( INITIAL_NUMBER_LIVES );
		PlayerPrefs.SetInt("treasureKeysOwned", 0 );
		setTreasureKeysOwned( 0 );
		PlayerPrefs.SetString( "Finished Game", "false" );
		LevelManager.Instance.setPlayerFinishedTheGame( false );
		PlayerPrefs.SetString( "First Time Playing", "true" );
		firstTimePlaying = true;
		PlayerPrefs.SetString( "showDebugInfoOnHUD", "false" );
		setShowDebugInfoOnHUD( false );
		PlayerPrefs.SetString( "sharedOnFacebook", "false" );
		setSharedOnFacebook( false );
		PlayerPrefs.SetString( "ownsStarDoubler", "false" );
		setOwnsStarDoubler( false );
		PlayerPrefs.SetString( "usesFacebook", "false" );
		usesFacebook = false;
		dateLastPlayed = DateTime.Now;
		PlayerPrefs.SetString( "dateLastPlayed", dateLastPlayed.ToString() );
		PlayerPrefs.SetFloat("soundVolume", 1f );
		soundVolume = 1f;

		PlayerPrefs.SetInt("currentCoins", 0 );
		currentCoins = 0;
		PlayerPrefs.SetInt("lifetimeCoins", 0 );
		lifetimeCoins = 0;
		resetDisplayStars();
		resetHighScores();
		resetDeathInEpisodes();
		resetKeysFoundInEpisode();
		resetTreasureKeysFound();
		PlayerPrefs.SetInt("powerUpSelected", (int)PowerUpType.SlowTime );
		powerUpSelected = PowerUpType.SlowTime;
		PlayerPrefs.SetInt("difficultyLevel", (int)DifficultyLevel.Normal );
		difficultyLevel = DifficultyLevel.Normal;
		PlayerPrefs.SetInt("avatar", (int)Avatar.None);
		avatar = Avatar.None;
		PlayerPrefs.SetString( "userName", "" );
		ClearPowerUpInventory();
		challenges = string.Empty;
		PlayerPrefs.SetString( "challenges", "" );
		PlayerPrefs.Save();
		Debug.Log ("PlayerStatsManager-resetPlayerStats: called.");
	}

	public class PowerUpInventory
	{
		public int quantity;
		//Players can upgrade their powerups from a level of 0 to 6
		//For example, the duration of the Shield powerup when upgraded to level 3 is 20 seconds (instead of 10).
		public int upgradeLevel = 1;
	}

}
