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
		GameManager.Instance.setPlayMode(PlayMode.PlayTwoPlayers);
		//The race track is automatically chosen based on the number of trophies.
		//We head directly to matchmaking.
		StartCoroutine( loadScene(GameScenes.Matchmaking) );
	}

	public void OnClickThreePlayerRace()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayThreePlayers);
		//The race track is automatically chosen based on the number of trophies.
		//We head directly to matchmaking.
		StartCoroutine( loadScene(GameScenes.Matchmaking) );
	}

	public void OnClickRaceWithFriend()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayWithFriends);
		StartCoroutine( loadScene(GameScenes.Social) );
	}

	public void OnClickPlayAgainstAI()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayAgainstEnemy);
		//The race track is chosen by the player.
		//Open circuit selection.
		StartCoroutine( loadScene(GameScenes.CircuitSelection) );
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
