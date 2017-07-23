using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MultiplayerPauseMenu : MonoBehaviour {

	[SerializeField] GameObject pausePanel;
	[SerializeField] Text titleText;
	[SerializeField] Text resumeButtonText;
	[SerializeField] Text quitButtonText;
	[SerializeField] GameObject pauseButton;

	// Use this for initialization
	void Awake ()
	{
		if( !GameManager.Instance.isMultiplayer() ) Destroy( gameObject );
		titleText.text = LocalizationManager.Instance.getText("MENU_PAUSE");
		resumeButtonText.text = LocalizationManager.Instance.getText("MENU_RESUME");
		quitButtonText.text = LocalizationManager.Instance.getText("MENU_QUIT");
		//Reset in case player paused to quit last game
		Time.timeScale = 1f;
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
			pausePanel.SetActive( true );
			AudioListener.pause = true;
		}
		else if( GameManager.Instance.getGameState() == GameState.Paused )
		{
			//We were paused. Resume game.
			pausePanel.SetActive( false );
			GameManager.Instance.setGameState( GameState.Normal );
			AudioListener.pause = false;
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
		pauseButton.SetActive( false );
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


	void MultiplayerStateChanged( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying )
		{
  			pauseButton.SetActive( false );
		}
	}

	void GameStateChange( GameState previousState, GameState newState )
	{
		if( newState == GameState.Normal )
		{
 			pauseButton.SetActive( true );
		}
	}
}
