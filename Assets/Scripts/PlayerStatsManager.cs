using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerStatsManager {
	
	private static PlayerStatsManager playerStatsManager = null;
	
	//Coin management
	int coinTotal = 0;
	int coinAccumulator = 0;
	Transform coinParent = null;
	int lifetimeCoins = 0;

	float distanceTravelled = 0;
	int highScore = 0;
	bool firstTimePlaying;

	bool hasMetSuccubus = false;
	int lives = 0;
	int treasureIslandKeys = 0;

	//If the user has logged in with Facebook this value is true, if he is logged in as guest, this value is false.
	bool usesFacebook = false;

	//DateTime when player last started WorldMapHandler
	DateTime dateLastPlayed;

	List <string> unlockFromIDList = new List<string>();
	//This indicates the highest level unlocked (which is not the same as the highest level completed).
	//So if the world has 30 levels and there is a junction at level 10 and another one at level 20 and 3 friends helped you
	//unlock the first junction, the highest level unlocked is 20.
	int highestLevelUnlocked = -1;

	//For stats and achievements
	static EventCounter novice_runner = new EventCounter( GameCenterManager.NoviceRunner, 1, CounterType.Total_any_level, 1000 );
	static EventCounter coin_hoarder = new EventCounter( GameCenterManager.CoinHoarder, 1, CounterType.Total_any_level, 50000 );
	
	//Dictionary where the key is the PowerUpType and the value represents the amount of power-ups the player has either purchased or found in the levels.
	//This inventory is only relevant for consumable power-ups.
	//This is saved as concatenated string. See defaultPowerUpsForNewPlayer as a format example.
	Dictionary<PowerUpType, PowerUpInventory> powerUpInventory = new Dictionary<PowerUpType, PowerUpInventory>(5);
	//Shield is 1, Magnet is 2, ZNuke is 3, MagicBoots is 4, SlowTime is 5
	//By default, players have three consumable power ups of each
	//Players can upgrade their power ups from a level of 0 to 5
	//For example, the duration of the Shield power up when upgraded to level 3 is 20 seconds (instead of 10).
	//By default, the upgrade level for each is 0.
	//Format is : PowerUpType,quantity,upgrade level
	const string defaultPowerUpsForNewPlayer = "1,0,0,2,0,0,3,3,0,4,3,0,5,3,0";
	
	//Sound volume for both effects and music. Value is between 0 and 1f.
	float soundVolume = 1f;

	//The number of times the player was revived in the current level using Energy points.
	//This is used to calculate the Energy cost to revive since the amount of energy required doubles each time the player is revived.
	//This value is not saved.
	//This value is reset each time the player restarts the level.
	int timesPlayerRevivedInLevel = 0;

	//For debugging
	//Cheat
	bool hasInfiniteLives = true;
	bool hasInfiniteTreasureIslandKeys = false;

	PowerUpType powerUpSelected = PowerUpType.MagicBoots;
	DifficultyLevel difficultyLevel = DifficultyLevel.Normal;
	//The avatar the player has chosen in the character selection screen.
	//The enum will be converted using toString. The available avatars in the resources/avatar folder must be named
	//exactly like their enum counterpart.
	Avatar avatar = Avatar.None;

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
		Debug.Log("PlayerStatsManager: Difficulty level is " + difficultyLevel );
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

	public void resetLevelStats()
	{
		resetDistanceTravelled();
		resetTimesPlayerRevivedInLevel();
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
		modifyCoinCount( numberAsInt );

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
				//Is the series complete?
				if( coinAccumulator == coinSeriesInfo.coinSeriesTotal )
				{
					//Yes it is!
					//Tell page hud to display the total number of coins accumulated
					HUDHandler.displayCoinTotal( coinAccumulator, coinSeriesInfo.coinColor, true );
				}
				else
				{
					//No, it isn't
					//Tell page hud to display the total number of coins accumulated
					HUDHandler.displayCoinTotal( coinAccumulator, coinSeriesInfo.coinColor, false );
				}
			}
		}
	}

	public void modifyCoinCount( int coins )
	{
		coinTotal = coinTotal + coins;
		//Also add to lifetime coins.
		lifetimeCoins = lifetimeCoins + coins;
		if( lifetimeCoins >= coin_hoarder.valueNeededToSucceed )
		{
			coin_hoarder.incrementCounter();
		}
	}

	public void resetPlayerCoins()
	{
		coinTotal = 0;
	}

	public int getPlayerCoins()
	{
		return coinTotal;
	}
	
	public int getLifetimeCoins()
	{
		return lifetimeCoins;
	}

	public int getPlayerScore()
	{
		return coinTotal + (int)distanceTravelled;
	}
	
	public int getPlayerHighScore()
	{
		return highScore;
	}

	//Note that a high score message will only be displayed to the player in the Endless game mode, not in the Story game mode. 
	public bool isNewHighScore()
	{
		if( GameManager.Instance.getGameMode() != GameMode.Endless ) return false;

		//Dont highlight a new high score if there was none to begin with
		if( highScore == 0 )
		{
			return false;
		}
		else
		{
			if( getPlayerScore() > highScore &&  getPlayerScore() > 250 )
			{
				highScore = getPlayerScore();
				Debug.Log ("isNewHighScore--we have a new high score: " + highScore );
				return true;
			}
			else
			{
				return false;
			}	
		}
	}

	//Used to identify if the player is a new user and it is his first time launching the game
	public bool isFirstTimePlaying()
	{
		return firstTimePlaying;
	}

	public void setHasMetSuccubus( bool value )
	{
		hasMetSuccubus = value;
	}

	public float getSoundVolume()
	{
		return soundVolume;
	}

	public void setSoundVolume( float volume )
	{
		soundVolume = volume;
	}
	
	public bool getHasMetSuccubus()
	{
		return hasMetSuccubus;
	}

	public void setUsesFacebook( bool value )
	{
		usesFacebook = value;
	}
	
	public bool getUsesFacebook()
	{
		return usesFacebook;
	}

	public void setLives( int value )
	{
		lives = value;
	}
	
	public int getLives()
	{
		return lives;
	}

	public void increaseLives(int value)
	{
		lives = lives + value;
	}

	public void decreaseLives(int value)
	{
		lives = lives - value;
		if( lives < 0 ) lives = 0;
	}

	public void setTreasureIslandKeys( int value )
	{
		treasureIslandKeys = value;
	}
	
	public int getTreasureIslandKeys()
	{
		return treasureIslandKeys;
	}
	
	public void increaseTreasureIslandKeys(int value)
	{
		treasureIslandKeys = treasureIslandKeys + value;
	}
	
	public void decreaseTreasureIslandKeys(int value)
	{
		treasureIslandKeys = treasureIslandKeys - value;
		if( treasureIslandKeys < 0 ) treasureIslandKeys = 0;
	}

	//Store the fromID of a friend who has accepted to unlock the next section of the map if it does not already exist.
	//There is a maximum of 3 fromID in this list since we only need 3 friends to unlock the next section.
	public void addUnlockRequest( string fromID )
	{
		//for debugging
		if( unlockFromIDList.Count <= 3 )
		//if( unlockFromIDList.Count <= 3 && !unlockFromIDList.Contains(fromID ))
		{
			unlockFromIDList.Add(fromID );
		}
	}

	public void ClearSaveUnlockRequests()
	{
		unlockFromIDList.Clear();
	}

	public List<string> getSaveUnlockRequests()
	{
		//for debugging
		//unlockFromIDList.Clear();
		//unlockFromIDList.Add("1378641987");	//Raphael
		//unlockFromIDList.Add("593102300");	//Veronique
		//unlockFromIDList.Add("48302002");		//Gaetano

		return unlockFromIDList;
	}

	string concatenateSaveUnlockRequests()
	{
		string result = "";
		for( int i =0; i < unlockFromIDList.Count; i++ )
		{
			result = result + unlockFromIDList[i] + ",";
		}
		result = result.TrimEnd(',');
		Debug.Log("concatenateSaveUnlock " + result );
		return result;
	}
	
	public void addToPowerUpInventory( PowerUpType type, int quantity )
	{
		if( powerUpInventory.ContainsKey(type) )
		{
			powerUpInventory[type].quantity = powerUpInventory[type].quantity + quantity;
			savePlayerStats();
		}
	}

	public void incrementPowerUpInventory( PowerUpType type )
	{
		if( powerUpInventory.ContainsKey(type) )
		{
			powerUpInventory[type].quantity++;
			Debug.Log("incrementPowerUpInventory new total " + powerUpInventory[type].quantity + " for type " + type );
			savePlayerStats();
		}
	}
	
	public void decrementPowerUpInventory( PowerUpType type )
	{
		if( powerUpInventory.ContainsKey(type) )
		{
			powerUpInventory[type].quantity--;
			if( powerUpInventory[type].quantity < 0 ) powerUpInventory[type].quantity = 0;
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

	void savePowerUpInventory()
	{
		string result = "";
		foreach(KeyValuePair<PowerUpType, PowerUpInventory> pair in powerUpInventory) 
		{
			int powerUpTypeAsInt = (int)pair.Key;
			result = result + powerUpTypeAsInt.ToString() + "," + pair.Value.quantity.ToString() + "," + pair.Value.upgradeLevel.ToString() + ",";
		}
		result = result.TrimEnd(',');
		Debug.Log("savePowerUpInventory " + result );
		PlayerPrefs.SetString("powerUpInventory", result );
	}

	void loadPowerUpInventory()
	{
		try
		{
			string powerUpInventoryString = PlayerPrefs.GetString("powerUpInventory", defaultPowerUpsForNewPlayer );
			string[] powerUpInventoryArray = powerUpInventoryString.Split(',');
			Debug.Log ("loadPowerUpInventory " + powerUpInventoryString + " length " + powerUpInventoryArray.Length );
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
	
	public void setHighestLevelUnlocked( int value )
	{
		highestLevelUnlocked = value;
		Debug.Log("setHighestLevelUnlocked " + highestLevelUnlocked );
	}

	//Reminder levels start at 0 even though 1 is displayed on the map.
	public int getHighestLevelUnlocked()
	{
		return highestLevelUnlocked;
	}

	public void loadPlayerStats()
	{
		try
		{
			int nextLevelToComplete = PlayerPrefs.GetInt("Next Level To Complete");
			LevelManager.Instance.setNextLevelToComplete( nextLevelToComplete );

			string playerFinishedTheGameString = PlayerPrefs.GetString("Finished Game", "false" );
			if( playerFinishedTheGameString == "true" )
			{
				LevelManager.Instance.setPlayerFinishedTheGame( true );
			}
			else
			{
				LevelManager.Instance.setPlayerFinishedTheGame( false );
			}

			highScore = PlayerPrefs.GetInt("High Score");
			lives = PlayerPrefs.GetInt("Lives", 6);
			treasureIslandKeys = PlayerPrefs.GetInt("TreasureIslandKeys", 0);
			//for debugging
			treasureIslandKeys = 5;
			string firstTimePlayingString = PlayerPrefs.GetString("First Time Playing", "true" );
			if( firstTimePlayingString == "true" )
			{
				firstTimePlaying = true;
			}
			else
			{
				firstTimePlaying = false;	
			}
			string hasMetSuccubusString = PlayerPrefs.GetString("Has Met Succubus", "false" );
			if( hasMetSuccubusString == "true" )
			{
				hasMetSuccubus = true;
			}
			else
			{
				hasMetSuccubus = false;	
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
			string list = PlayerPrefs.GetString("unlockFromIDList", "" );
			string[] listArray = list.Split(',');
			for( int i = 0; i < listArray.Length; i++ )
			{
				unlockFromIDList.Add( listArray[i] );
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
			highestLevelUnlocked = PlayerPrefs.GetInt("highestLevelUnlocked",-1);
			soundVolume = PlayerPrefs.GetFloat("soundVolume", 1f );
			lifetimeCoins = PlayerPrefs.GetInt("lifetimeCoins", 0);
			powerUpSelected = (PowerUpType)PlayerPrefs.GetInt("powerUpSelected", (int)PowerUpType.SlowTime);
			difficultyLevel = (DifficultyLevel)PlayerPrefs.GetInt("difficultyLevel", (int)DifficultyLevel.Normal);
			avatar = (Avatar)PlayerPrefs.GetInt("avatar", (int)Avatar.None);
			loadPowerUpInventory();
			Debug.Log ("loadPlayerStats-highScore: " + highScore + " firstTimePlaying: " + firstTimePlaying + " has met succubus: " + hasMetSuccubus + " Next Level To Complete: " + nextLevelToComplete + " Finished game: " + LevelManager.Instance.getPlayerFinishedTheGame() + " Lives: " + lives + " Date Last Played: " + dateLastPlayed + " difficultyLevel " + difficultyLevel + " treasureIslandKeys " + treasureIslandKeys );
		}
		catch (Exception e)
		{
			Debug.LogWarning("PlayerStatsManager-exception occured while loading player stats: " + e.Message + " Resetting stats to default values.");
			PlayerPrefs.DeleteAll();
		}
	}
	
	public void savePlayerStats()
	{
		PlayerPrefs.SetInt("Next Level To Complete", LevelManager.Instance.getNextLevelToComplete() );
		PlayerPrefs.SetInt("Lives", lives );
		PlayerPrefs.SetInt("TreasureIslandKeys", treasureIslandKeys );
		if( LevelManager.Instance.getPlayerFinishedTheGame() )
		{
			PlayerPrefs.SetString( "Finished Game", "true" );
		}
		else
		{
			PlayerPrefs.SetString( "Finished Game", "false" );
		}
		PlayerPrefs.SetString( "First Time Playing", "false" );
		if ( getPlayerScore() > highScore )
		{
			highScore = getPlayerScore();
			PlayerPrefs.SetInt( "High Score", highScore );
		}
		if( hasMetSuccubus )
		{
			PlayerPrefs.SetString( "Has Met Succubus", "true" );
		}
		else
		{
			PlayerPrefs.SetString( "Has Met Succubus", "false" );
		}
		if( usesFacebook )
		{
			PlayerPrefs.SetString( "usesFacebook", "true" );
		}
		else
		{
			PlayerPrefs.SetString( "usesFacebook", "false" );
		}
		string unlockRequests = concatenateSaveUnlockRequests();
		PlayerPrefs.SetString( "unlockFromIDList", unlockRequests );
		PlayerPrefs.SetString( "dateLastPlayed", dateLastPlayed.ToString() );
		PlayerPrefs.SetInt("highestLevelUnlocked", highestLevelUnlocked );
		PlayerPrefs.SetFloat("soundVolume", soundVolume );
		PlayerPrefs.SetInt("lifetimeCoins", lifetimeCoins );
		PlayerPrefs.SetInt("powerUpSelected", (int)powerUpSelected );
		PlayerPrefs.SetInt("difficultyLevel", (int)difficultyLevel );
		PlayerPrefs.SetInt("avatar", (int)avatar );
		savePowerUpInventory();
		PlayerPrefs.Save();
		Debug.Log ("savePlayerStats-highScore: " + highScore + " firstTimePlaying: " + firstTimePlaying + " has met succubus: " + hasMetSuccubus + " usesFacebook: "  + usesFacebook + " unlockFromIDList: " + unlockRequests + " Date Last Played: " + dateLastPlayed );
	}
	
	//Used for debugging
	public void resetPlayerStats()
	{
		PlayerPrefs.SetInt("Next Level To Complete", 0 );
		LevelManager.Instance.setNextLevelToComplete( 0 );
		PlayerPrefs.SetInt("Lives", 6 );
		lives = 6;
		PlayerPrefs.SetInt("TreasureIslandKeys", 0 );
		treasureIslandKeys = 0;
		PlayerPrefs.SetString( "Finished Game", "false" );
		LevelManager.Instance.setPlayerFinishedTheGame( false );
		PlayerPrefs.SetString( "First Time Playing", "true" );
		firstTimePlaying = true;
		PlayerPrefs.SetString( "Has Met Succubus", "false" );
		hasMetSuccubus = false;
		PlayerPrefs.SetString( "usesFacebook", "false" );
		usesFacebook = false;
		PlayerPrefs.SetInt( "High Score", 0 );
		highScore = 0;
		PlayerPrefs.SetString( "unlockFromIDList", "" );
		ClearSaveUnlockRequests();
		dateLastPlayed = DateTime.Now;
		PlayerPrefs.SetString( "dateLastPlayed", dateLastPlayed.ToString() );
		PlayerPrefs.SetInt("highestLevelUnlocked", -1 );
		highestLevelUnlocked = -1;
		PlayerPrefs.SetFloat("soundVolume", 1f );
		soundVolume = 1f;
		PlayerPrefs.SetInt("lifetimeCoins", 0 );
		lifetimeCoins = 0;
		PlayerPrefs.SetInt("powerUpSelected", (int)PowerUpType.SlowTime );
		powerUpSelected = PowerUpType.SlowTime;
		PlayerPrefs.SetInt("difficultyLevel", (int)DifficultyLevel.Normal );
		difficultyLevel = DifficultyLevel.Normal;
		PlayerPrefs.SetInt("avatar", (int)Avatar.None );
		avatar = Avatar.None;
		ClearPowerUpInventory();
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
