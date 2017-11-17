using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayModes : MonoBehaviour {

	[Header("Play Menu")]
	bool levelLoading = false;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();		
	}
	
	public void OnClickTwoPlayerRace()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayAgainstOnePlayer);
		//The race track is automatically chosen based on the number of trophies.
		//We head directly to matchmaking.
		StartCoroutine( loadScene(GameScenes.Matchmaking) );
	}

	public void OnClickThreePlayerRace()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayAgainstTwoPlayers);
		//The race track is automatically chosen based on the number of trophies.
		//We head directly to matchmaking.
		StartCoroutine( loadScene(GameScenes.Matchmaking) );
	}

	public void OnClickRaceWithFriend()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayAgainstOneFriend);
		StartCoroutine( loadScene(GameScenes.Social) );
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
