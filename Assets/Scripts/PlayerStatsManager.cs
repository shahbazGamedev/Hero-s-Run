using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class PlayerStatsManager {
	
	private static PlayerStatsManager playerStatsManager = null;
	
	//Coin management
	int currentCoins = 0;			//The amount of coins the player currently has. This number always goes up, unless the player purchases something.
	int coinAccumulator = 0;
	Transform coinParent = null;
	int lifetimeCoins = 0;			//The amount of coins the player has earned over time. This is a statistic. It is used by the achievement system.
	//Assume there are 9 episodes for now. For each episode, the player has received between 0 and 3 stars. The number of stars is displayed on the world map.
	//The index is the episode number. The value is the number of stars between 0 and 3. The initial values are 0.
	int[] displayStarsArray = new int[LevelData.NUMBER_OF_EPISODES];

	//In each episode, a limited number of treasure chest keys have been hidden.
	//The index is the episode number. The value is the number of keys found by the player. The initial values are 0.
	int[] keysFoundInEpisodeArray = new int[LevelData.NUMBER_OF_EPISODES];
	//Number of treasure keys owned by the player
	int treasureKeysOwned = 0;

	//Delegate used to communicate to other classes when the number of keys, lives, coins or coin doubler changes
	public delegate void PlayerInventoryChanged( PlayerInventoryEvent eventType, int newValue );
	public static event PlayerInventoryChanged playerInventoryChanged;

	//Delegate used to communicate to other classes when the power-up quantity changes
	public delegate void PowerUpInventoryChanged();
	public static event PowerUpInventoryChanged powerUpInventoryChanged;

	//High-score per episode.
	//The index is the episode number. The value is the score for that episode (sum of coins collected and distance run). The initial values are 0.
	int[] highScoreArray = new int[LevelData.NUMBER_OF_EPISODES];
	float distanceTravelled = 0;
	bool sharedOnFacebook = false;	//Has the player shared an image on Facebook?

	int lives = 0;
	const int INITIAL_NUMBER_LIVES = 6;
	
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

	PowerUpType powerUpSelected = PowerUpType.MagicBoots;
	//The avatar the player has chosen in the character selection screen. 
	//The enum will be converted using toString. The available avatars in the resources/avatar folder must be named
	//exactly like their enum counterpart.
	//NOTE: There is no more character selection screen. Default to Hero.
	Avatar avatar = Avatar.Hero;

	bool ownsCoinDoubler = false; //True if the player has purchased the Coin Doubler in the store.

	string challenges = String.Empty; //List of Challenge in JSON format. See ChallengeBoard.
	string journalEntries = String.Empty; //List of Journal Entries in JSON format. See JournalData.
	string playerProfile = String.Empty; //Player profile data such as current XP amount
	string playerStatistics = String.Empty; //Player statistics data such as win/loss ratio, current win streak, best win streak, etc.
	string playerDeck = String.Empty; //Player cards. Specifies the card's level, whether it is part of the battle deck or not, etc.
	string playerFriends = String.Empty; //List of player's friends.
	string recentPlayers = String.Empty; //List of the last 8 players the player played against.
	string playerInventory = String.Empty; //List of the player's inventory including gem balance.
	string playerIcons = String.Empty; //List of the player icons available in the game. Include both locked and unlocked icons.
	string voiceLines = String.Empty; //List of the voice lines available in the game. Include both locked and unlocked voice lines.
	string playerConfiguration = String.Empty; //Player configuration such as sound FX volume, music volume, etc.
	string debugConfiguration = String.Empty; //Debug configuration. See Debug menu for the various options.

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

	public bool isAvatarMale()
	{
		if( avatar == Avatar.Hero )
		{
			return true;
		}
		else
		{
			return false;
		}
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
				HUDHandler.hudHandler.displayCoinPickup( coinAccumulator );
			}
		}
	}

	public void modifyCurrentCoins( int coins, bool incrementLifetimeCoins, bool isPurchase )
	{
		//Player gets twice the amount of Coins if they own the coin doubler permanent item and this is not a purchase
		if( ownsCoinDoubler && !isPurchase && coins > 0 ) coins = coins * 2;

		currentCoins = currentCoins + coins;

		LevelManager.Instance.incrementScore( coins );

		//Send an event to interested classes
		if(playerInventoryChanged != null) playerInventoryChanged(PlayerInventoryEvent.Score_Changed, LevelManager.Instance.getScore() );
		if(playerInventoryChanged != null) playerInventoryChanged(PlayerInventoryEvent.Coin_Changed, currentCoins );

		//Also add to lifetime coins.
		if( incrementLifetimeCoins )
		{
			lifetimeCoins = lifetimeCoins + coins;
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
		if( GameManager.Instance.getGameMode() == GameMode.Story )
		{
			deathInEpisodesArray[LevelManager.Instance.getNextEpisodeToComplete()]++;
		}
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

	public void setSharedOnFacebook( bool value )
	{
		sharedOnFacebook = value;
	}

	public bool getSharedOnFacebook()
	{
		return sharedOnFacebook;
	}

	public void setOwnsCoinDoubler( bool value )
	{
		ownsCoinDoubler = value;
		if(playerInventoryChanged != null) playerInventoryChanged(PlayerInventoryEvent.Coin_Doubler_Changed, 0 );
	}
	
	public bool getOwnsCoinDoubler()
	{
		return ownsCoinDoubler;
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
			if( string.IsNullOrEmpty(powerUpInventoryString) ) powerUpInventoryString = defaultPowerUpsForNewPlayer;
			string[] powerUpInventoryArray = powerUpInventoryString.Split(',');
			//Debug.Log ("loadPowerUpInventory " + powerUpInventoryString + " length " + powerUpInventoryArray.Length + " default " + defaultPowerUpsForNewPlayer + " powerUpInventoryString " + powerUpInventoryString);
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

	public void setJournalEntries( string journalEntries )
	{
		this.journalEntries = journalEntries;
	}

	public string getJournalEntries()
	{
		return journalEntries;
	}

	public void setPlayerProfile( string playerProfile )
	{
		this.playerProfile = playerProfile;
	}

	public string getPlayerProfile()
	{
		return playerProfile;
	}

	public void setPlayerStatistics( string playerStatistics )
	{
		this.playerStatistics = playerStatistics;
	}

	public string getPlayerStatistics()
	{
		return playerStatistics;
	}

	public void setPlayerDeck( string playerDeck )
	{
		this.playerDeck = playerDeck;
	}

	public string getPlayerDeck()
	{
		return playerDeck;
	}

	public void setPlayerFriends( string playerFriends )
	{
		this.playerFriends = playerFriends;
	}

	public string getPlayerFriends()
	{
		return playerFriends;
	}

	public void setRecentPlayers( string recentPlayers )
	{
		this.recentPlayers = recentPlayers;
	}

	public string getRecentPlayers()
	{
		return recentPlayers;
	}

	public void setPlayerInventory( string playerInventory )
	{
		this.playerInventory = playerInventory;
	}

	public string getPlayerInventory()
	{
		return playerInventory;
	}

	public void setPlayerIcons( string playerIcons )
	{
		this.playerIcons = playerIcons;
	}

	public string getPlayerIcons()
	{
		return playerIcons;
	}

	public void setVoiceLines( string voiceLines )
	{
		this.voiceLines = voiceLines;
	}

	public string getVoiceLines()
	{
		return voiceLines;
	}

	public void setPlayerConfiguration( string playerConfiguration )
	{
		this.playerConfiguration = playerConfiguration;
	}

	public string getPlayerConfiguration()
	{
		return playerConfiguration;
	}

	public void setDebugConfiguration( string debugConfiguration )
	{
		this.debugConfiguration = debugConfiguration;
	}

	public string getDebugConfiguration()
	{
		return debugConfiguration;
	}

	public void loadPlayerStats()
	{
		try
		{
			playerProfile = PlayerPrefs.GetString("playerProfile", "" );
			playerStatistics = PlayerPrefs.GetString("playerStatistics", "" );
			playerDeck = PlayerPrefs.GetString("playerDeck", "" );
			playerFriends = PlayerPrefs.GetString("playerFriends", "" );
			recentPlayers = PlayerPrefs.GetString("recentPlayers", "" );
			playerInventory = PlayerPrefs.GetString("playerInventory", "" );
			playerIcons = PlayerPrefs.GetString("playerIcons", "" );
			voiceLines = PlayerPrefs.GetString("voiceLines", "" );
			playerConfiguration = PlayerPrefs.GetString("playerConfiguration", "" );
			debugConfiguration = PlayerPrefs.GetString("debugConfiguration", "" );
		}
		catch (Exception e)
		{
			Debug.LogWarning("PlayerStatsManager-exception occured while loading player stats: " + e.Message + " Resetting stats to default values.");
			PlayerPrefs.DeleteAll();
		}
	}
	
	public void savePlayerStats()
	{
		PlayerPrefs.SetString( "playerProfile", playerProfile );
		PlayerPrefs.SetString( "playerStatistics", playerStatistics );
		PlayerPrefs.SetString( "playerDeck", playerDeck );
		PlayerPrefs.SetString( "playerFriends", playerFriends );
		PlayerPrefs.SetString( "recentPlayers", recentPlayers );
		PlayerPrefs.SetString( "playerInventory", playerInventory );
		PlayerPrefs.SetString( "playerIcons", playerIcons );
		PlayerPrefs.SetString( "voiceLines", voiceLines );
		PlayerPrefs.SetString( "playerConfiguration", playerConfiguration );
		PlayerPrefs.SetString( "debugConfiguration", debugConfiguration );
		PlayerPrefs.Save();
	}
	
	//Used for debugging
	public void resetPlayerStats()
	{
		try
		{
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
			Debug.Log ("PlayerStatsManager-delete all called." );
		}
		catch (Exception e)
		{
			Debug.LogWarning("PlayerStatsManager-exception occured while deleting all data: " + e.Message );
		}
	}

	public class PowerUpInventory
	{
		public int quantity;
		//Players can upgrade their powerups from a level of 0 to 6
		//For example, the duration of the Shield powerup when upgraded to level 3 is 20 seconds (instead of 10).
		public int upgradeLevel = 1;
	}

}
