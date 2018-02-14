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
		StartCoroutine( loadScene(GameScenes.Matchmaking) );
	}
}
