﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.GameCenter;

public class DebugMenu : MonoBehaviour {


	[Header("Debug Menu")]
	[SerializeField] Text titleText;
	[SerializeField] Text toggleShowDebugInfoText;
	[SerializeField] Text toggleOnlyUseUniqueTilesText;
	[SerializeField] Text clearAssetBundleCacheText;
	[Header("Player Stats")]
	[SerializeField] Text currentCoins;
	[SerializeField] Text lifetimeCoins;
	[SerializeField] Text ownsCoinDoubler;
	[SerializeField] Text deathPerEpisodeText;
	[SerializeField] Text facebookName;

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
		if( LevelManager.Instance.getOnlyUseUniqueTiles() )
		{
			toggleOnlyUseUniqueTilesText.text = "Only Use Unique Tiles: On";
		}
		else
		{
			toggleOnlyUseUniqueTilesText.text = "Only Use Unique Tiles: Off";
		}

		updatePlayerStats();
	}

	public void resetSavedData()
	{
		Debug.Log("resetSavedData");
		UISoundManager.uiSoundManager.playButtonClick();
		PlayerStatsManager.Instance.resetPlayerStats();
	}

	public void deleteRequests()
	{
		Debug.Log("deleteRequests");
		UISoundManager.uiSoundManager.playButtonClick();
		StartCoroutine( FacebookManager.Instance.deleteAllAppRequests() );
	}

	public void resetAchievements()
	{
		Debug.Log("resetAchievements");
		UISoundManager.uiSoundManager.playButtonClick();
		GameCenterPlatform.ResetAllAchievements( (resetResult) => {
			Debug.Log( (resetResult) ? "Achievement Reset succesfull." : "Achievement Reset failed." );
		});
		GameCenterManager.resetAchievementsCompleted();
	}

	public void giveTreasureChestKeys()
	{
		Debug.Log("Give 25 Treasure Chest Keys");
		UISoundManager.uiSoundManager.playButtonClick();
		PlayerStatsManager.Instance.increaseTreasureKeysOwned( 25 );
		PlayerStatsManager.Instance.savePlayerStats();
	}

	public void toggleShowDebugInfo()
	{
		Debug.Log("toggleShowDebugInfo");
		UISoundManager.uiSoundManager.playButtonClick();
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
		UISoundManager.uiSoundManager.playButtonClick();
		LevelManager.Instance.unlockAllEpisodes();
		PlayerStatsManager.Instance.savePlayerStats();
	}

	public void unlockAllStories()
	{
		Debug.Log("unlockAllStories");
		UISoundManager.uiSoundManager.playButtonClick();
		GameManager.Instance.journalData.unlockAllEntries(); //also takes care of saving
	}

	public void toggleOnlyUseUniqueTiles()
	{
		Debug.Log("toggleOnlyUseUniqueTiles");
		UISoundManager.uiSoundManager.playButtonClick();
		LevelManager.Instance.setOnlyUseUniqueTiles( !LevelManager.Instance.getOnlyUseUniqueTiles() );
		if( LevelManager.Instance.getOnlyUseUniqueTiles() )
		{
			toggleOnlyUseUniqueTilesText.text = "Only Use Unique Tiles: On";
		}
		else
		{
			toggleOnlyUseUniqueTilesText.text = "Only Use Unique Tiles: Off";
		}
	}

	public void clearAssetBundleCache()
	{
		Debug.Log("clearAssetBundleCache");
		UISoundManager.uiSoundManager.playButtonClick();
		bool result = Caching.CleanCache();
		if( result )
		{
			clearAssetBundleCacheText.text = clearAssetBundleCacheText.text + ": Success";
		}
		else
		{
			clearAssetBundleCacheText.text = clearAssetBundleCacheText.text + ": Failure";
		}
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
		currentCoins.text = "Current Coins: " + PlayerStatsManager.Instance.getCurrentCoins();
		lifetimeCoins.text = "Lifetime Coins: " + PlayerStatsManager.Instance.getLifetimeCoins();
		if( PlayerStatsManager.Instance.getOwnsCoinDoubler() )
		{
			ownsCoinDoubler.text = "Owns Coin Doubler: true";
		}
		else
		{
			ownsCoinDoubler.text = "Owns Coin Doubler: false";
		}
		deathPerEpisodeText.text = "Death Per Episode: " + PlayerStatsManager.Instance.getDeathInEpisodesAsString();
	}

	void OnEnable()
	{
		updatePlayerStats();
	}
	
}
