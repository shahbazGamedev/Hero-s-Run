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
		//The map is chosen by the player.
		//Open map selection.
		StartCoroutine( loadScene(GameScenes.MapSelection) );
	}

	public void OnClickPlayAgainstOneBot()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayAgainstOneBot);
		//The map is chosen by the player.
		//Open map selection.
		StartCoroutine( loadScene(GameScenes.MapSelection) );
	}

	public void OnClickPlayCoopWithOneBot()
	{
		GameManager.Instance.setPlayMode(PlayMode.PlayCoopWithOneBot);
		StartCoroutine( loadScene(GameScenes.Matchmaking) );
	}
}
