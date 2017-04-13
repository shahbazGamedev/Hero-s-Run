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
	[Header("User Name")]
	[SerializeField] GameObject userNameCanvas;

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
			gameObject.SetActive( false ); //To increase legibility, hide the main menu while the user name panel is played.
			userNameCanvas.SetActive(  true );
		}
	}

	public void OnClickOpenPlayModes()
	{
		StartCoroutine( loadScene(GameScenes.PlayModes) );
	}

	public void OnClickOpenTraining()
	{
		StartCoroutine( loadScene(GameScenes.Training) );
	}

	public void OnClickOpenSocial()
	{
		StartCoroutine( loadScene(GameScenes.Social) );
	}

	public void OnClickOpenHeroSelection()
	{
		StartCoroutine( loadScene(GameScenes.HeroSelection) );
	}

	public void OnClickOpenOptionsMenu()
	{
		StartCoroutine( loadScene(GameScenes.Options) );
	}

	public void OnClickPlayAlone()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayAlone);
		StartCoroutine( loadScene(GameScenes.CircuitSelection) );
	}

	public void OnClickShowCareerProfile()
	{
		StartCoroutine( loadScene(GameScenes.CareerProfile) );
	}

	void updateNumberOfPlayerIcons()
	{
		//Next to the Career Profile button, display a NEW indicator if there are newly aacquired player icons
		int newPlayerIcons = ProgressionManager.Instance.getNumberOfNewPlayerIcons();
		newPlayerIconsIndicator.SetActive( newPlayerIcons > 0 );
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
