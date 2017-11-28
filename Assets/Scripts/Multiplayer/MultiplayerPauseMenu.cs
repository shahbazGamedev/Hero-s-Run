using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MultiplayerPauseMenu : MonoBehaviour {

	[SerializeField] GameObject pausePanel;
	[SerializeField] GameObject pauseButton;

	// Use this for initialization
	void Awake ()
	{
		if( !GameManager.Instance.isMultiplayer() ) Destroy( gameObject );
  		pauseButton.GetComponent<CanvasGroup>().alpha = 0;
		pauseButton.GetComponent<CanvasGroup>().interactable = false;
		pausePanel.GetComponent<CanvasGroup>().interactable = false;
 		pausePanel.GetComponent<CanvasGroup>().alpha = 0;
	}

	//If the device is paused by pressing the Home button, because of a low battery warning or a phone call, the game will automatically display the pause menu.
	void OnApplicationPause( bool pauseStatus )
	{
		if( pauseStatus && GameManager.Instance.getGameState() == GameState.Normal ) togglePause();
	}

	public void OnClickPause()
	{
		togglePause();
	}
	
	public void OnClickResume()
	{
		togglePause();
	}

	public void OnClickQuit()
	{
		quit();
	}

	void togglePause()
	{
		if( GameManager.Instance.getGameState() == GameState.Normal )
		{
			//Pause game.
			GameManager.Instance.setGameState( GameState.Paused );
			//Take this opportunity to do a garbage collection
			System.GC.Collect();
			pausePanel.GetComponent<CanvasGroup>().interactable = true;
			pausePanel.GetComponent<FadeInCanvasGroup>().fadeIn();
		}
		else if( GameManager.Instance.getGameState() == GameState.Paused )
		{
			//We were paused. Resume game.
			pausePanel.GetComponent<CanvasGroup>().interactable = false;
			pausePanel.GetComponent<FadeInCanvasGroup>().fadeOut();
			GameManager.Instance.setGameState( GameState.Normal );
		}
	}

	/// <summary>
	/// Quit and return to matchmaking.
	/// </summary>
	void quit()
	{
		if( PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.IN_PROGRESS )
		{
			PlayerRaceManager.Instance.playerAbandonedRace();
		}
		GameManager.Instance.setGameState( GameState.Matchmaking );
		PhotonNetwork.LeaveRoom();
	}

	public void hidePauseButton()
	{
		pauseButton.GetComponent<CanvasGroup>().interactable = false;
		pauseButton.GetComponent<FadeInCanvasGroup>().fadeOut();
	}

	void OnEnable()
	{
		PlayerControl.multiplayerStateChanged += MultiplayerStateChanged;
		GameManager.gameStateEvent += GameStateChange;
	}
	
	void OnDisable()
	{
		PlayerControl.multiplayerStateChanged -= MultiplayerStateChanged;
		GameManager.gameStateEvent -= GameStateChange;
	}


	void MultiplayerStateChanged( Transform player, PlayerCharacterState newState )
	{
		if( player.GetComponent<PlayerAI>() == null )
		{
			if( newState == PlayerCharacterState.Dying )
			{
				pauseButton.GetComponent<CanvasGroup>().interactable = false;
	  			pauseButton.GetComponent<FadeInCanvasGroup>().fadeOut();
			}
		}
	}

	void GameStateChange( GameState previousState, GameState newState )
	{
		if( newState == GameState.Normal )
		{
			pauseButton.GetComponent<CanvasGroup>().interactable = true;
 			pauseButton.GetComponent<FadeInCanvasGroup>().fadeIn();
		}
	}
}
