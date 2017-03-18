using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

	[Header("Main Menu")]
	bool levelLoading = false;
	[Header("New Player Icons Indicator")]
	[SerializeField] Text newPlayerIconsIndicator;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();		
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
		//Update the number of newly unlocked player icons
		int newPlayerIcons = ProgressionManager.Instance.getNumberOfNewPlayerIcons();
		newPlayerIconsIndicator.text = newPlayerIcons.ToString();
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
