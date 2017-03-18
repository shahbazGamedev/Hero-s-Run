using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.GameCenter;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour {

	[Header("Settings Menu")]
	[SerializeField] Canvas settingsMenuCanvas;
	[SerializeField] Text titleText;
	[Header("Sound")]
	[SerializeField] AudioMixer mainMixer;
	[SerializeField] Text soundFxVolumeText;
	[SerializeField] Slider soundFxVolumeSlider;
	[SerializeField] Text musicVolumeText;
	[SerializeField] Slider musicVolumeSlider;
	[Header("Facebook")]
	[SerializeField] Text facebookText;
	[Header("Difficulty")]
	[SerializeField] Text difficultyText;
	[Header("Achievements")] //Game Center
	[SerializeField] Text achievementsText;
	[Header("Flip Camera")]
	[SerializeField] Text flipCameraText;
	[Header("Privacy Policy")]
	[SerializeField] Text privacyPolicyText;
	[SerializeField] string privacyPolicyURL = "http://www.google.com/";
	[Header("Restore Purchases")]
	[SerializeField] Text restorePurchasesText;
	[Header("Debug Menu")]
	[SerializeField] Button debugMenuButton;
	[SerializeField] Text debugMenuText;
	[SerializeField] Canvas debugMenuCanvas;
	bool levelLoading = false;

	// Use this for initialization
	void Start () {

		Handheld.StopActivityIndicator();

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
