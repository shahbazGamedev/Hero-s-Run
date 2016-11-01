﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.GameCenter;

public class DebugMenu : MonoBehaviour {


	[Header("Debug Menu")]
	public NewWorldMapHandler newWorldMapHandler;
	public Text titleText;
	[Header("Player Stats")]
	public Text currentStars;
	public Text lifetimeStars;
	public Text ownsStarDoubler;
	public Text toggleShowDebugInfoText;
	public Text toggleAccessToNormalLevelsText;
	public Text deathPerEpisodeText;
	public Text facebookName;

	// Use this for initialization
	void Start () {
	
		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu
		#endif

		//Text Inititialization
		titleText.text = LocalizationManager.Instance.getText("POPUP_TITLE_DEBUG");
		if( PlayerStatsManager.Instance.getShowDebugInfoOnHUD() )
		{
			toggleShowDebugInfoText.text = "Show Debug Info: On";
		}
		else
		{
			toggleShowDebugInfoText.text = "Show Debug Info: Off";
		}
		if( PlayerStatsManager.Instance.getAllowAccessToNormalLevels() )
		{
			toggleAccessToNormalLevelsText.text = "Access to normal levels: On";
		}
		else
		{
			toggleAccessToNormalLevelsText.text = "Access to normal levels: Off";
		}

		updatePlayerStats();
	}

	public void resetSavedData()
	{
		Debug.Log("resetSavedData");
		SoundManager.soundManager.playButtonClick();
		PlayerStatsManager.Instance.resetPlayerStats();
	}

	public void deleteRequests()
	{
		Debug.Log("deleteRequests");
		SoundManager.soundManager.playButtonClick();
		StartCoroutine( FacebookManager.Instance.deleteAllAppRequests() );
	}

	public void resetAchievements()
	{
		Debug.Log("resetAchievements");
		SoundManager.soundManager.playButtonClick();
		GameCenterPlatform.ResetAllAchievements( (resetResult) => {
			Debug.Log( (resetResult) ? "Achievement Reset succesfull." : "Achievement Reset failed." );
		});
		GameCenterManager.resetAchievementsCompleted();
	}

	public void giveStars()
	{
		Debug.Log("Give 25000 Stars");
		SoundManager.soundManager.playButtonClick();
		PlayerStatsManager.Instance.modifyCurrentCoins( 25000, false, false );
		PlayerStatsManager.Instance.savePlayerStats();
		updatePlayerStats();
	}

	public void giveLives()
	{
		Debug.Log("Give 20 Lives");
		SoundManager.soundManager.playButtonClick();
		PlayerStatsManager.Instance.increaseLives( 20 );
		PlayerStatsManager.Instance.savePlayerStats();
	}

	public void giveTreasureChestKeys()
	{
		Debug.Log("Give 25 Treasure Chest Keys");
		SoundManager.soundManager.playButtonClick();
		PlayerStatsManager.Instance.increaseTreasureKeysOwned( 25 );
		PlayerStatsManager.Instance.savePlayerStats();
	}

	public void toggleShowDebugInfo()
	{
		Debug.Log("toggleShowDebugInfo");
		SoundManager.soundManager.playButtonClick();
		PlayerStatsManager.Instance.setShowDebugInfoOnHUD( !PlayerStatsManager.Instance.getShowDebugInfoOnHUD() );
		if( PlayerStatsManager.Instance.getShowDebugInfoOnHUD() )
		{
			toggleShowDebugInfoText.text = "Show Debug Info: On";
		}
		else
		{
			toggleShowDebugInfoText.text = "Show Debug Info: Off";
		}
		PlayerStatsManager.Instance.savePlayerStats();
	}

	public void unlockAllLevels()
	{
		Debug.Log("unlockAllLevels");
		SoundManager.soundManager.playButtonClick();
		LevelManager.Instance.unlockAllEpisodes();
		PlayerStatsManager.Instance.savePlayerStats();
		newWorldMapHandler.drawLevelMarkers();
		newWorldMapHandler.updateFriendPortraits();

	}

	public void toggleAllowAccessToNormalLevels()
	{
		Debug.Log("toggleAllowAccessToNormalLevels");
		SoundManager.soundManager.playButtonClick();
		PlayerStatsManager.Instance.setAllowAccessToNormalLevels( !PlayerStatsManager.Instance.getAllowAccessToNormalLevels() );
		if( PlayerStatsManager.Instance.getAllowAccessToNormalLevels() )
		{
			toggleAccessToNormalLevelsText.text = "Access to normal levels: On";
		}
		else
		{
			toggleAccessToNormalLevelsText.text = "Access to normal levels: Off";
		}
		PlayerStatsManager.Instance.savePlayerStats();
		newWorldMapHandler.drawLevelMarkers();
	}

	void updatePlayerStats()
	{
		if( FacebookManager.Instance.firstName == null )
		{
			facebookName.text = "Not logged in to Facebook";
		}
		else
		{
			facebookName.text = "First Name: " + FacebookManager.Instance.firstName;
		}
		currentStars.text = "Current Stars: " + PlayerStatsManager.Instance.getCurrentCoins();
		lifetimeStars.text = "Lifetime Stars: " + PlayerStatsManager.Instance.getLifetimeCoins();
		if( PlayerStatsManager.Instance.getOwnsStarDoubler() )
		{
			ownsStarDoubler.text = "Owns Star Doubler: true";
		}
		else
		{
			ownsStarDoubler.text = "Owns Star Doubler: false";
		}
		deathPerEpisodeText.text = "Death Per Levels: " + PlayerStatsManager.Instance.getDeathInLevelsAsString();
	}

	void OnEnable()
	{
		updatePlayerStats();
	}
	
}
