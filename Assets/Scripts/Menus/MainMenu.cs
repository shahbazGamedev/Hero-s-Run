using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

	[Header("Main Menu")]
	bool levelLoading = false;
	[Header("Version Number")]
	[SerializeField]  Text versionNumberText;
	[Header("New Player Icons Indicator")]
	[SerializeField] GameObject newPlayerIconsIndicator;
	[Header("User Name Selector")]
	[SerializeField] GameObject userNameCanvas;
	[Header("Trophies, current race track, icon and name Panel")]
	[SerializeField] Text numberOfTrophiesText;
	[SerializeField] Text currentRaceTrackText;
	[SerializeField] Image playerIcon;
	[SerializeField] Text playerNameText;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();
		versionNumberText.text = GameManager.Instance.getVersionNumber();
		updateNumberOfPlayerIcons();
		//If this is a new user, display the user name panel.
		//Entering a user name is mandatory.
		if( PlayerStatsManager.Instance.isFirstTimePlaying() )
		{
			gameObject.SetActive( false ); //To increase legibility, hide the main menu and top bar while the user name panel is played.
			UniversalTopBar.Instance.showTopBar( false );
			userNameCanvas.SetActive(  true );
		}
		numberOfTrophiesText.text = GameManager.Instance.playerProfile.getTrophies().ToString("N0");
		string sectorName = LocalizationManager.Instance.getText( "SECTOR_" + LevelManager.Instance.getLevelData().getRaceTrackByTrophies().circuitInfo.sectorNumber.ToString() );
		currentRaceTrackText.text = sectorName;
		playerIcon.sprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( GameManager.Instance.playerProfile.getPlayerIconId() ).icon;
		playerNameText.text = GameManager.Instance.playerProfile.getUserName();
	}

	public void updateUserName( string newUserName )
	{
		playerNameText.text = newUserName;
	}

	void updateNumberOfPlayerIcons()
	{
		//Next to the Career Profile button, display a NEW indicator if there are newly aacquired player icons
		int newPlayerIcons = GameManager.Instance.playerIcons.getNumberOfNewPlayerIcons();
		newPlayerIconsIndicator.SetActive( newPlayerIcons > 0 );
	}

	#region Menu options
	public void OnClickOpenPlayModes()
	{
		StartCoroutine( loadScene(GameScenes.PlayModes) );
	}

	public void OnClickOpenTraining()
	{
		StartCoroutine( loadScene(GameScenes.Training) );
	}

	public void OnClickOpenHeroSelection()
	{
		StartCoroutine( loadScene(GameScenes.HeroSelection) );
	}

	public void OnClickOpenSocial()
	{
		StartCoroutine( loadScene(GameScenes.Social) );
	}

	public void OnClickShowCareerProfile()
	{
		StartCoroutine( loadScene(GameScenes.CareerProfile) );
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
	#endregion

}
