using System.Collections;
using UnityEngine;

public class TrainingModes : Menu {

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

	public void OnClickPlayAgainstOneBot()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayAgainstOneBot);
		//The race track is chosen by the player.
		//Open circuit selection.
		StartCoroutine( loadScene(GameScenes.CircuitSelection) );
	}

	public void OnClickPlayAgainstTwoBots()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayAgainstTwoBots);
		//The race track is chosen by the player.
		//Open circuit selection.
		StartCoroutine( loadScene(GameScenes.CircuitSelection) );
	}

	public void OnClickPlayCoopWithOneBot()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayCoopWithOneBot);
		//A random coop race track will be automatically chosen.
		//We head directly to matchmaking.
		StartCoroutine( loadScene(GameScenes.Matchmaking) );
	}
}
