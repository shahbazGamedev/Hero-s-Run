using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

	[Header("Main Menu")]
	bool levelLoading = false;
	[Header("Version Number")]
	public Text versionNumberText;
	[Header("New Player Icons Indicator")]
	[SerializeField] GameObject newPlayerIconsIndicator;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();
		versionNumberText.text = GameManager.Instance.getVersionNumber();
		updateNumberOfPlayerIcons();		
	}
	
	public void OnClickOpenPlayModes()
	{
		StartCoroutine( loadScene(GameScenes.PlayModes) );
	}

	public void OnClickOpenTraining()
	{
		StartCoroutine( loadScene(GameScenes.Training) );
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

	public void OnClickShowPlayerIconSelection()
	{
		StartCoroutine( loadScene(GameScenes.PlayerIconSelection) );
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
