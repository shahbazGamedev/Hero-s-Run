using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.GameCenter;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour {

	[Header("Settings Menu")]
	[Header("Sound")]
	[SerializeField] AudioMixer mainMixer;
	[SerializeField] Text soundFxVolumeText;
	[SerializeField] Slider soundFxVolumeSlider;
	[SerializeField] Text musicVolumeText;
	[SerializeField] Slider musicVolumeSlider;
	[Header("Facebook")]
	[SerializeField] Text facebookText;
	[Header("Achievements")] //Game Center
	[SerializeField] Text achievementsText;
	[Header("Enable tilt to turn or switch lanes")]
	[SerializeField] Text enableTiltText;
	[Header("Privacy Policy")]
	[SerializeField] Text privacyPolicyText;
	[SerializeField] string privacyPolicyURL = "http://www.google.com/";
	[Header("Debug Menu")]
	[SerializeField] ScrollRect  optionsScrollView; 
	[SerializeField] GameObject  dotsPanel; 
	bool levelLoading = false;

	void OnEnable ()
	{
		SceneManager.sceneUnloaded += OnSceneUnloaded;
	}

	void Start () {

		Handheld.StopActivityIndicator();

		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu
		#endif

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
		if( GameManager.Instance.playerConfiguration.getTiltEnabled() )
		{
			enableTiltText.text = LocalizationManager.Instance.getText("MENU_TILT_ENABLED");
		}
		else
		{
			enableTiltText.text = LocalizationManager.Instance.getText("MENU_TILT_DISABLED");
		}
		privacyPolicyText.text = LocalizationManager.Instance.getText("MENU_PRIVACY_POLICY");
		
		//Important: Disable horizontal scrolling when not in a Development Build to prevent access to debug options.
		optionsScrollView.horizontal = Debug.isDebugBuild;
		dotsPanel.SetActive( Debug.isDebugBuild );

		soundFxVolumeSlider.value = GameManager.Instance.playerConfiguration.getSoundFxVolume();
		musicVolumeSlider.value = GameManager.Instance.playerConfiguration.getMusicVolume();
	}

	public void OnClickReturnToMainMenu()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		PlayerStatsManager.Instance.savePlayerStats();
		StartCoroutine( loadScene(GameScenes.MainMenu) );
	}

	public void setSoundFxVolume( Slider volume )
	{
		GameManager.Instance.playerConfiguration.setSoundFxVolume(volume.value);
		mainMixer.SetFloat( "SoundEffectsVolume", volume.value );
	}

	public void setMusicVolume( Slider volume )
	{
		GameManager.Instance.playerConfiguration.setMusicVolume(volume.value);
		mainMixer.SetFloat( "MusicVolume", volume.value );
	}

	public void OnClickHandleFacebookConnect()
	{
		Debug.Log("OnClickHandleFacebookConnect");
		UISoundManager.uiSoundManager.playButtonClick();
		if( FacebookManager.Instance.isLoggedIn() )
		{
			//Logout
			FacebookManager.Instance.CallFBLogout();
		}
		else
		{
			//Login
		}
	}

	public void OnClickShowAchievements()
	{
		Debug.Log("OnClickShowAchievements");
		UISoundManager.uiSoundManager.playButtonClick();
		Social.ShowAchievementsUI();
	}

	public void OnClickToggleTilt()
	{
		Debug.Log("OnClickToggleTilt");
		UISoundManager.uiSoundManager.playButtonClick();
		GameManager.Instance.playerConfiguration.setEnableTilt( !GameManager.Instance.playerConfiguration.getTiltEnabled() );
		if( GameManager.Instance.playerConfiguration.getTiltEnabled() )
		{
			enableTiltText.text = LocalizationManager.Instance.getText("MENU_TILT_ENABLED");
		}
		else
		{
			enableTiltText.text = LocalizationManager.Instance.getText("MENU_TILT_DISABLED");
		}
	}

	public void OnClickShowPrivacyPolicy()
	{
		Debug.Log("OnClickShowPrivacyPolicy");
		UISoundManager.uiSoundManager.playButtonClick();
		Application.OpenURL(privacyPolicyURL);
	}

	void OnSceneUnloaded( Scene scene )
	{
		GameManager.Instance.playerConfiguration.serializePlayerConfiguration( false );
		PlayerStatsManager.Instance.savePlayerStats();
		SceneManager.sceneUnloaded -= OnSceneUnloaded;
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
