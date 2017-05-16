using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainingModes : MonoBehaviour {

	[Header("Training")]
	bool levelLoading = false;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();		
	}
	
	public void OnClickPlaySolo()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayAlone);
		//The race track is chosen by the player.
		//Open circuit selection.
		StartCoroutine( loadScene(GameScenes.CircuitSelection) );
	}

	public void OnClickPlayCampaign()
	{
		GameManager.Instance.setMultiplayerMode( false );
		StartCoroutine( loadScene(GameScenes.WorldMap) );
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
