using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.GameCenter;
using UnityEngine.SceneManagement;

public class DebugMenu : MonoBehaviour {

	[Header("Debug Menu")]
	[SerializeField] Text toggleShowDebugInfoText;
	[SerializeField] Text toggleOnlyUseUniqueTilesText;
	[SerializeField] Text clearAssetBundleCacheText;
	[SerializeField] Text facebookName;
	[SerializeField] Text allowBotToPlayCardsText;
	[SerializeField] Text autoPilotText;
	[SerializeField] Slider speedOverrideMultiplierSlider;
	[SerializeField] Text speedOverrideMultiplierText;
	[SerializeField] Slider trophyOverrideMultiplierSlider;
	[SerializeField] Text trophyOverrideMultiplierText;

	void Start () {
				
		SceneManager.sceneUnloaded += OnSceneUnloaded;

		speedOverrideMultiplierSlider.value = LevelManager.Instance.speedOverrideMultiplier * 10;
		speedOverrideMultiplierText.text = LevelManager.Instance.speedOverrideMultiplier.ToString();

		trophyOverrideMultiplierSlider.value = GameManager.Instance.playerProfile.getTrophies();
		trophyOverrideMultiplierText.text = GameManager.Instance.playerProfile.getTrophies().ToString("N0");

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
		if( LevelManager.Instance.allowBotToPlayCards )
		{
			allowBotToPlayCardsText.text = "Bot plays cards: On";
		}
		else
		{
			allowBotToPlayCardsText.text = "Bot plays cards: Off";
		}
		if( LevelManager.Instance.autoPilot )
		{
			autoPilotText.text = "Auto-pilot: On";
		}
		else
		{
			autoPilotText.text = "Auto-pilot: Off";
		}

		updateFacebookName();
	}

	public void OnClickResetSavedData()
	{
		Debug.Log("OnClickResetSavedData");
		UISoundManager.uiSoundManager.playButtonClick();
		PlayerStatsManager.Instance.resetPlayerStats();
	}

	public void setSpeedOverride( Slider slider )
	{
		LevelManager.Instance.speedOverrideMultiplier = slider.value/10; //We want 1.1 not 1.123567. Slider uses whole numbers from 0 to 30 converted to 0 to 3.
		speedOverrideMultiplierText.text = (slider.value/10).ToString();
	}

	public void setTrophyOverride( Slider slider )
	{
		GameManager.Instance.playerProfile.setNumberOfTrophies( (int) slider.value );
		trophyOverrideMultiplierText.text = (slider.value).ToString("N0");
	}

	void OnSceneUnloaded ( Scene scene )
	{
		//Save the player profile. The number of trophies may have been saved.
		GameManager.Instance.playerProfile.serializePlayerprofile();
	}

	public void OnClickAllowBotToPlayCards()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		LevelManager.Instance.allowBotToPlayCards = !LevelManager.Instance.allowBotToPlayCards;
		if( LevelManager.Instance.allowBotToPlayCards )
		{
			allowBotToPlayCardsText.text = "Bot plays cards: On";
		}
		else
		{
			allowBotToPlayCardsText.text = "Bot plays cards: Off";
		}
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

	public void OnClickToggleAutoPilot()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		LevelManager.Instance.autoPilot = !LevelManager.Instance.autoPilot;
		if( LevelManager.Instance.autoPilot )
		{
			autoPilotText.text = "Auto-pilot: On";
		}
		else
		{
			autoPilotText.text = "Auto-pilot: Off";
		}
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
