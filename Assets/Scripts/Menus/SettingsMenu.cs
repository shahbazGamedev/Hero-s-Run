using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.GameCenter;
using UnityEngine.Audio;

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
		string difficultyLevel = LocalizationManager.Instance.getText("MENU_DIFFICULTY_LEVEL"); 
		difficultyText.text = difficultyLevel + "\n" + PlayerStatsManager.Instance.getDifficultyLevelName();
		achievementsText.text = LocalizationManager.Instance.getText("MENU_ACHIEVEMENTS");
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

	public void showSettingsMenu()
	{
		SoundManager.soundManager.playButtonClick();
		settingsMenuCanvas.gameObject.SetActive( true );
	}

	public void closeSettingsMenu()
	{
		SoundManager.soundManager.playButtonClick();
		PlayerStatsManager.Instance.savePlayerStats();
		settingsMenuCanvas.gameObject.SetActive( false );
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
		SoundManager.soundManager.playButtonClick();
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

	public void changeDifficultyLevel()
	{
		Debug.Log("changeDifficultyLevel");
		SoundManager.soundManager.playButtonClick();
		DifficultyLevel newDifficultyLevel = PlayerStatsManager.Instance.getNextDifficultyLevel();
		//setDifficultyLevel takes care of saving the new value
		PlayerStatsManager.Instance.setDifficultyLevel(newDifficultyLevel);
		string difficultyLevel = LocalizationManager.Instance.getText("MENU_DIFFICULTY_LEVEL"); 
		difficultyText.text = difficultyLevel + "\n" + PlayerStatsManager.Instance.getDifficultyLevelName();
	}

	public void showAchievements()
	{
		Debug.Log("showAchievements");
		SoundManager.soundManager.playButtonClick();
		Social.ShowAchievementsUI();
	}

	public void showPrivacyPolicy()
	{
		Debug.Log("showPrivacyPolicy");
		SoundManager.soundManager.playButtonClick();
		Application.OpenURL(privacyPolicyURL);
	}

	public void restorePurchases()
	{
		Debug.LogWarning("restorePurchases - Not implemented.");
		SoundManager.soundManager.playButtonClick();
	}

	public void showDebugMenu()
	{
		Debug.Log("showDebugMenu");
		SoundManager.soundManager.playButtonClick();
		settingsMenuCanvas.gameObject.SetActive( false );
		debugMenuCanvas.gameObject.SetActive( true );
	}

	public void closeDebugMenu()
	{
		Debug.Log("closeDebugMenu");
		SoundManager.soundManager.playButtonClick();
		settingsMenuCanvas.gameObject.SetActive( true );
		debugMenuCanvas.gameObject.SetActive( false );
	}
	
}
