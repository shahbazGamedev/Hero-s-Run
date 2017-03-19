﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.GameCenter;

public class DebugMenu : MonoBehaviour {

	[Header("Debug Menu")]
	[SerializeField] Text toggleShowDebugInfoText;
	[SerializeField] Text toggleOnlyUseUniqueTilesText;
	[SerializeField] Text clearAssetBundleCacheText;
	[SerializeField] Text facebookName;

	void Start () {
	
		//Text Inititialization
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

		updateFacebookName();
	}

	public void OnClickResetSavedData()
	{
		Debug.Log("OnClickResetSavedData");
		UISoundManager.uiSoundManager.playButtonClick();
		PlayerStatsManager.Instance.resetPlayerStats();
	}

	public void OnClickDeleteRequests()
	{
		Debug.Log("OnClickDeleteRequests");
		UISoundManager.uiSoundManager.playButtonClick();
		StartCoroutine( FacebookManager.Instance.deleteAllAppRequests() );
	}

	public void OnClickResetAchievements()
	{
		Debug.Log("OnClickResetAchievements");
		UISoundManager.uiSoundManager.playButtonClick();
		GameCenterPlatform.ResetAllAchievements( (resetResult) => {
			Debug.Log( (resetResult) ? "Achievement Reset succesfull." : "Achievement Reset failed." );
		});
		GameCenterManager.resetAchievementsCompleted();
	}

	public void OnClickGiveTreasureChestKeys()
	{
		Debug.Log("Give 25 Treasure Chest Keys");
		UISoundManager.uiSoundManager.playButtonClick();
		PlayerStatsManager.Instance.increaseTreasureKeysOwned( 25 );
		PlayerStatsManager.Instance.savePlayerStats();
	}

	public void OnClickToggleShowDebugInfo()
	{
		Debug.Log("OnClickToggleShowDebugInfo");
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

	public void OnClickUnlockAllLevels()
	{
		Debug.Log("OnClickUnlockAllLevels");
		UISoundManager.uiSoundManager.playButtonClick();
		LevelManager.Instance.unlockAllEpisodes();
		PlayerStatsManager.Instance.savePlayerStats();
	}

	public void OnClickUnlockAllStories()
	{
		Debug.Log("OnClickUnlockAllStories");
		UISoundManager.uiSoundManager.playButtonClick();
		//I am not sure if I will release with the Journal code. For now, it only gets initialized
		//when entering the world map and not when entering the main menu. Because of this, test for null value.
		if( GameManager.Instance.journalData != null )
		{
			GameManager.Instance.journalData.unlockAllEntries(); //also takes care of saving
		}
	}

	public void OnClickToggleOnlyUseUniqueTiles()
	{
		Debug.Log("OnClickToggleOnlyUseUniqueTiles");
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

	public void OnClickClearAssetBundleCache()
	{
		Debug.Log("OnClickClearAssetBundleCache");
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

	void updateFacebookName()
	{
		if( FacebookManager.Instance.firstName == null )
		{
			facebookName.text = "Not logged in to Facebook";
		}
		else
		{
			facebookName.text = "First Name: " + FacebookManager.Instance.firstName;
		}
	}
	
}
