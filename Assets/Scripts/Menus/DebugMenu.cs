using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.GameCenter;
using UnityEngine.SceneManagement;
using System;

public class DebugMenu : MonoBehaviour {

	[Header("Debug Menu")]
	[SerializeField] Dropdown debugInfoTypeDropdown;
	[SerializeField] Text toggleOnlyUseUniqueTilesText;
	[SerializeField] Text clearAssetBundleCacheText;
	[SerializeField] Text facebookName;
	[SerializeField] Text allowBotToPlayCardsText;
	[SerializeField] Text autoPilotText;
	[SerializeField] Slider speedOverrideMultiplierSlider;
	[SerializeField] Text speedOverrideMultiplierText;
	[SerializeField] Slider trophyOverrideMultiplierSlider;
	[SerializeField] Text trophyOverrideMultiplierText;
	[SerializeField] Text trophyOverrideSectorText;
	[SerializeField] Dropdown regionOverrideDropdown;
	[SerializeField] Dropdown mapOverrideDropdown;

	int competitivePoints;

	void Start () {
				
		SceneManager.sceneUnloaded += OnSceneUnloaded;

		speedOverrideMultiplierSlider.value = GameManager.Instance.playerDebugConfiguration.getSpeedOverrideMultiplier() * 10;
		speedOverrideMultiplierText.text = GameManager.Instance.playerDebugConfiguration.getSpeedOverrideMultiplier().ToString();

		competitivePoints = GameManager.Instance.playerProfile.getCompetitivePoints();
		trophyOverrideMultiplierSlider.value = competitivePoints;
		trophyOverrideMultiplierText.text = competitivePoints.ToString("N0");
		trophyOverrideSectorText.text = SectorManager.Instance.getSectorByPoints( competitivePoints ).ToString("N0");

		if( LevelManager.Instance.getOnlyUseUniqueTiles() )
		{
			toggleOnlyUseUniqueTilesText.text = "Only Use Unique Tiles: On";
		}
		else
		{
			toggleOnlyUseUniqueTilesText.text = "Only Use Unique Tiles: Off";
		}
		if( GameManager.Instance.playerDebugConfiguration.getAllowBotToPlayCards() )
		{
			allowBotToPlayCardsText.text = "Bot plays cards: On";
		}
		else
		{
			allowBotToPlayCardsText.text = "Bot plays cards: Off";
		}
		if( GameManager.Instance.playerDebugConfiguration.getAutoPilot() )
		{
			autoPilotText.text = "Auto-pilot: On";
		}
		else
		{
			autoPilotText.text = "Auto-pilot: Off";
		}

		updateFacebookName();
		populatePhotonCloudRegionDropdown();
		populateMapDropdown();
		populateDebugInfoTypeDropdown();
	}

	public void OnClickResetSavedData()
	{
		Debug.Log("OnClickResetSavedData");
		UISoundManager.uiSoundManager.playButtonClick();
		PlayerStatsManager.Instance.resetPlayerStats();
	}

	public void setSpeedOverride( Slider slider )
	{
		GameManager.Instance.playerDebugConfiguration.setSpeedOverrideMultiplier( slider.value/10 ); //We want 1.1 not 1.123567. Slider uses whole numbers from 0 to 30 converted to 0 to 3.
		speedOverrideMultiplierText.text = (slider.value/10).ToString();
	}

	public void setTrophyOverride( Slider slider )
	{
		competitivePoints = (int) slider.value;
		trophyOverrideMultiplierText.text = competitivePoints.ToString("N0");
		trophyOverrideSectorText.text = SectorManager.Instance.getSectorByPoints( competitivePoints ).ToString("N0");
		GameManager.Instance.playerProfile.setCompetitivePoints( competitivePoints );
	}

	void OnSceneUnloaded ( Scene scene )
	{
		//Only proceed if the unloaded scene is Options
		if( scene.buildIndex == (int) GameScenes.Options )
		{
			GameManager.Instance.playerDebugConfiguration.serializeDebugConfiguration( false );
			GameManager.Instance.playerProfile.serializePlayerprofile();
		}
	}

	public void OnClickAllowBotToPlayCards()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		GameManager.Instance.playerDebugConfiguration.setAllowBotToPlayCards( !GameManager.Instance.playerDebugConfiguration.getAllowBotToPlayCards() );
		if( GameManager.Instance.playerDebugConfiguration.getAllowBotToPlayCards() )
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
		GameManager.Instance.playerDebugConfiguration.setAutoPilot( !GameManager.Instance.playerDebugConfiguration.getAutoPilot() );
		if( GameManager.Instance.playerDebugConfiguration.getAutoPilot() )
		{
			autoPilotText.text = "Auto-pilot: On";
		}
		else
		{
			autoPilotText.text = "Auto-pilot: Off";
		}
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
		bool result = Caching.ClearCache();
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

	#region Photon Cloud Region override
	void populatePhotonCloudRegionDropdown()
	{
		string[] enumRegions = Enum.GetNames( typeof( CloudRegionCode ) );
		List<string> regions = new List<string>( enumRegions);
		regionOverrideDropdown.AddOptions( regions );
		regionOverrideDropdown.value = (int) GameManager.Instance.playerDebugConfiguration.getOverrideCloudRegionCode();
	}

	public void OnRegionDropdownValueChanged()
	{
		GameManager.Instance.playerDebugConfiguration.setOverrideCloudRegionCode( (CloudRegionCode) regionOverrideDropdown.value );
		if( PhotonNetwork.connected) PhotonNetwork.Disconnect();
	}
	#endregion

	#region Map override
	void populateMapDropdown()
	{
		List<string> maps = new List<string>();
		string mapName;
		for( int i = -1; i < LevelManager.Instance.getLevelData().getNumberOfMultiplayerLevels(); i++ )
		{
			if( i == -1 )
			{
				maps.Add( "Don't override" );
			}
			else
			{
		 		mapName = LocalizationManager.Instance.getText( "MAP_" + i.ToString() );
				maps.Add( mapName );
			}
		}
		mapOverrideDropdown.AddOptions( maps );
		//A value of -1 (None) means do not override
		mapOverrideDropdown.value = GameManager.Instance.playerDebugConfiguration.getOverrideMap() + 1; //we are starting from -1
	}

	public void OnMapDropdownValueChanged()
	{
		GameManager.Instance.playerDebugConfiguration.setOverrideMap( mapOverrideDropdown.value - 1 ); //we are starting from -1
	}
	#endregion

	#region Debug Info Type
	void populateDebugInfoTypeDropdown()
	{
		string[] enumDebugInfoTypes = Enum.GetNames( typeof( DebugInfoType ) );
		List<string> debugInfoTypes = new List<string>( enumDebugInfoTypes);
		debugInfoTypeDropdown.AddOptions( debugInfoTypes );
		debugInfoTypeDropdown.value = (int) GameManager.Instance.playerDebugConfiguration.getDebugInfoType();
	}

	public void OnDebugInfoTypeDropdownValueChanged()
	{
		GameManager.Instance.playerDebugConfiguration.setDebugInfoType( ( (DebugInfoType) debugInfoTypeDropdown.value ) );
	}
	#endregion
	
}
