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
	public Text soundVolumeText;
	public Slider soundVolumeSlider;
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

		soundVolumeText.text = LocalizationManager.Instance.getText("MENU_SOUND_VOLUME");
		if( FacebookManager.Instance.isLoggedIn() )
		{
			facebookText.text = LocalizationManager.Instance.getText("POPUP_BUTTON_FB_DISCONNECT");
		}
		else
		{
			facebookText.text = LocalizationManager.Instance.getText("MENU_LOGGED_OUT");
		}
		difficultyText.text = PlayerStatsManager.Instance.getDifficultyLevelName();
		achievementsText.text = LocalizationManager.Instance.getText("MENU_ACHIEVEMENTS");
		privacyPolicyText.text = LocalizationManager.Instance.getText("MENU_PRIVACY_POLICY");
		restorePurchasesText.text = LocalizationManager.Instance.getText("MENU_RESTORE_PURCHASES");
		debugMenuText.text = LocalizationManager.Instance.getText("MENU_SHOW_DEBUG");
		versionNumberText.text = "v" + GameManager.Instance.getVersionNumber();

		soundVolumeSlider.value = PlayerStatsManager.Instance.getSoundVolume();
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

	public void setSoundVolume( Slider volume )
	{
		Debug.Log("setSoundVolume " + volume.value );
		PlayerStatsManager.Instance.setSoundVolume(volume.value);
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
		difficultyText.text = PlayerStatsManager.Instance.getDifficultyLevelName();
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
