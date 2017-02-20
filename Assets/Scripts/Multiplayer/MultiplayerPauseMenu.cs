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

	// Use this for initialization
	void Awake ()
	{
		titleText.text = LocalizationManager.Instance.getText("MENU_PAUSE");
		resumeButtonText.text = LocalizationManager.Instance.getText("MENU_RESUME");
		quitButtonText.text = LocalizationManager.Instance.getText("MENU_QUIT");
	}

	//If the device is paused by pressing the Home button, because of a low battery warning or a phone call, the game will automatically display the pause menu.
	void OnApplicationPause( bool pauseStatus )
	{
		if( GameManager.Instance.isMultiplayer() && pauseStatus && GameManager.Instance.getGameState() != GameState.Paused ) togglePause();
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
		if( !GameManager.Instance.isMultiplayer() ) return;
		
		if( GameManager.Instance.getGameState() == GameState.Normal )
		{
			//Pause game.
			GameManager.Instance.setGameState( GameState.Paused );
			//Take this opportunity to do a garbage collection
			System.GC.Collect();
			pausePanel.SetActive( true );
		}
		else if( GameManager.Instance.getGameState() == GameState.Paused )
		{
			//We were paused. Resume game.
			pausePanel.SetActive( false );
			GameManager.Instance.setGameState( GameState.Normal );
		}
	}

	/// <summary>
	/// Quit and return to matchmaking.
	/// </summary>
	void quit()
	{
		GameManager.Instance.setGameState( GameState.Matchmaking );
		PhotonNetwork.LeaveRoom();
	}

}
