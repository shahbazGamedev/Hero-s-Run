using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.GameCenter;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


#if UNITY_IOS
using UnityEngine.iOS;
using UnityEngine.Apple.ReplayKit;
#endif

public class SettingsMenu : MonoBehaviour {


	[Header("Settings Menu")]
	public Canvas settingsMenuCanvas;
	public Text titleText;
	[Header("Sound")]
	public AudioMixer mainMixer;
	public Text soundFxVolumeText;
	public Slider soundFxVolumeSlider;
	public Text musicVolumeText;
	public Slider musicVolumeSlider;
	[Header("Facebook")]
	public Text facebookText;
	[Header("Difficulty")]
	public Text difficultyText;
	[Header("Achievements")] //Game Center
	public Text achievementsText;
	[Header("Recording Button")]
	public GameObject showRecordButtonPanel;
	public Text showRecordButtonText;
	[Header("Flip Camera")]
	public Text flipCameraText;
	[Header("Privacy Policy")]
	public Text privacyPolicyText;
	public string privacyPolicyURL = "http://www.google.com/";
	[Header("Restore Purchases")]
	public Text restorePurchasesText;
	[Header("Debug Menu")]
	public Button debugMenuButton;
	public Text debugMenuText;
	public Canvas debugMenuCanvas;
	[Header("Version Number")]
	public Text versionNumberText;
	bool levelLoading = false;

	// Use this for initialization
	void Start () {
	
		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu
		#endif

		//Text Inititialization
		titleText.text = LocalizationManager.Instance.getText("POPUP_TITLE_SETTINGS");

		soundFxVolumeText.text = LocalizationManager.Instance.getText("MENU_SOUND_FX_VOLUME");
		musicVolumeText.text = LocalizationManager.Instance.getText("MENU_MUSIC_VOLUME");
		if( FacebookManager.Instance.isLoggedIn() )
		{
			facebookText.text = LocalizationManager.Instance.getText("POPUP_BUTTON_FB_DISCONNECT");
		}
		else
		{
			facebookText.text = LocalizationManager.Instance.getText("MENU_LOGGED_OUT");
		}
		achievementsText.text = LocalizationManager.Instance.getText("MENU_ACHIEVEMENTS");
		if( PlayerStatsManager.Instance.getCameraFlipped() )
		{
			flipCameraText.text = LocalizationManager.Instance.getText("MENU_CAMERA_FACES_FRONT_HERO");
		}
		else
		{
			flipCameraText.text = LocalizationManager.Instance.getText("MENU_CAMERA_FACES_BACK_HERO");
		}
		showRecordButtonPanel.gameObject.SetActive( false );
		#if UNITY_IOS
		if( ReplayKit.APIAvailable )
		{
			showRecordButtonPanel.SetActive( true );
			if( PlayerStatsManager.Instance.getShowRecordButton() )
			{
				showRecordButtonText.text = LocalizationManager.Instance.getText("MENU_SHOW_RECORD_BUTTON");
			}
			else
			{
				showRecordButtonText.text = LocalizationManager.Instance.getText("MENU_HIDE_RECORD_BUTTON");
			}
		}
		#endif
		privacyPolicyText.text = LocalizationManager.Instance.getText("MENU_PRIVACY_POLICY");
		restorePurchasesText.text = LocalizationManager.Instance.getText("MENU_RESTORE_PURCHASES");
		debugMenuText.text = LocalizationManager.Instance.getText("MENU_SHOW_DEBUG");
		if( Debug.isDebugBuild )
		{
			debugMenuButton.gameObject.SetActive( true );
		}
		else
		{
			debugMenuButton.gameObject.SetActive( false );
		}
		versionNumberText.text = "v" + GameManager.Instance.getVersionNumber();

		soundFxVolumeSlider.value = PlayerStatsManager.Instance.getSoundFxVolume();
		musicVolumeSlider.value = PlayerStatsManager.Instance.getMusicVolume();
	}

	public void closeSettingsMenu()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		PlayerStatsManager.Instance.savePlayerStats();
		StartCoroutine( loadScene(GameScenes.MainMenu) );
	}

	public void setSoundFxVolume( Slider volume )
	{
		PlayerStatsManager.Instance.setSoundFxVolume(volume.value);
		mainMixer.SetFloat( "SoundEffectsVolume", volume.value );
	}

	public void setMusicVolume( Slider volume )
	{
		PlayerStatsManager.Instance.setMusicVolume(volume.value);
		mainMixer.SetFloat( "MusicVolume", volume.value );
	}

	public void handleFacebookConnect()
	{
		Debug.Log("handleFacebookConnect");
		UISoundManager.uiSoundManager.playButtonClick();
		if( FacebookManager.Instance.isLoggedIn() )
		{
			//Logout
			FacebookManager.Instance.CallFBLogout();
			PlayerStatsManager.Instance.setUsesFacebook( false );
		}
		else
		{
			//Login
		}
	}

	public void showAchievements()
	{
		Debug.Log("showAchievements");
		UISoundManager.uiSoundManager.playButtonClick();
		Social.ShowAchievementsUI();
	}

	public void toggleShowRecordButton()
	{
		Debug.Log("toggleShowRecordButton");
		UISoundManager.uiSoundManager.playButtonClick();
		PlayerStatsManager.Instance.setShowRecordButton( !PlayerStatsManager.Instance.getShowRecordButton() );
		if( PlayerStatsManager.Instance.getShowRecordButton() )
		{
			showRecordButtonText.text = LocalizationManager.Instance.getText("MENU_SHOW_RECORD_BUTTON");
		}
		else
		{
			showRecordButtonText.text = LocalizationManager.Instance.getText("MENU_HIDE_RECORD_BUTTON");
		}
	}

	public void flipCamera()
	{
		Debug.Log("flipCamera");
		UISoundManager.uiSoundManager.playButtonClick();
		PlayerStatsManager.Instance.setCameraFlipped( !PlayerStatsManager.Instance.getCameraFlipped() );
		if( PlayerStatsManager.Instance.getCameraFlipped() )
		{
			flipCameraText.text = LocalizationManager.Instance.getText("MENU_CAMERA_FACES_FRONT_HERO");
		}
		else
		{
			flipCameraText.text = LocalizationManager.Instance.getText("MENU_CAMERA_FACES_BACK_HERO");
		}
	}

	public void showPrivacyPolicy()
	{
		Debug.Log("showPrivacyPolicy");
		UISoundManager.uiSoundManager.playButtonClick();
		Application.OpenURL(privacyPolicyURL);
	}

	public void restorePurchases()
	{
		Debug.LogWarning("restorePurchases - Not implemented.");
		UISoundManager.uiSoundManager.playButtonClick();
	}

	public void showDebugMenu()
	{
		Debug.Log("showDebugMenu");
		UISoundManager.uiSoundManager.playButtonClick();
		settingsMenuCanvas.gameObject.SetActive( false );
		debugMenuCanvas.gameObject.SetActive( true );
	}

	public void closeDebugMenu()
	{
		Debug.Log("closeDebugMenu");
		UISoundManager.uiSoundManager.playButtonClick();
		settingsMenuCanvas.gameObject.SetActive( true );
		debugMenuCanvas.gameObject.SetActive( false );
	}

	IEnumerator loadScene(GameScenes value)
	{
		if( !levelLoading )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)value );
		}
	}
	
}
