using System.Collections;
using UnityEngine;

public class PlayModes : Menu {

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();		
	}
	
	public void OnClickPlayAgainstOnePlayer()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayAgainstOnePlayer);
		//The race track is automatically chosen based on the number of trophies.
		//We head directly to matchmaking.
		StartCoroutine( loadScene(GameScenes.Matchmaking) );
	}

	public void OnClickPlayAgainstTwoPlayers()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayAgainstTwoPlayers);
		//The race track is automatically chosen based on the number of trophies.
		//We head directly to matchmaking.
		StartCoroutine( loadScene(GameScenes.Matchmaking) );
	}

	public void OnClickPlayAgainstOneFriend()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayAgainstOneFriend);
		StartCoroutine( loadScene(GameScenes.Social) );
	}

	public void PlayCoopWithOnePlayer()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayCoopWithOnePlayer);
		//A random coop race track will be automatically chosen.
		//We head directly to matchmaking.
		StartCoroutine( loadScene(GameScenes.Matchmaking) );
	}
}
