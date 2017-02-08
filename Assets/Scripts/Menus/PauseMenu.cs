using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

	float timeScaleBeforePause;
	bool isPlayerControlEnabledBeforePause = true;
	PlayerController playerController;

	//Sound to play every second during countdown
	public AudioClip beep;

	public GameObject pauseMenuPopup;

	public Text titleText;
	public Text explanationText;
	public Image powerupImage;
	public Text powerupQuantity;
	public Text powerupName;
	public Text tutorialText;
	public Text resumeButtonText;
	public Text homeButtonText;

	public Text goText;


	// Use this for initialization
	void Awake ()
	{
		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu
		#endif

		titleText.text = LocalizationManager.Instance.getText("MENU_PAUSE");

		PowerUpType powerUpSelected = PlayerStatsManager.Instance.getPowerUpSelected();
		updatePowerupData( powerUpSelected );

		tutorialText.text = LocalizationManager.Instance.getText("POWER_UP_HOW_TO");

		resumeButtonText.text = LocalizationManager.Instance.getText("MENU_RESUME");
		homeButtonText.text = LocalizationManager.Instance.getText("MENU_QUIT");

		goText.text = LocalizationManager.Instance.getText("GO"); //Used to display 3,2,1 Go!
	}
	
	private void updatePowerupData( PowerUpType powerUpSelected )
	{
		powerupQuantity.text = PlayerStatsManager.Instance.getPowerUpQuantity( powerUpSelected ).ToString();
		powerupImage.sprite = PowerUpDisplayData.getPowerUpSprite( powerUpSelected );
		powerupName.text = PowerUpDisplayData.getPowerUpName( powerUpSelected );
		explanationText.text = LocalizationManager.Instance.getText("POWER_UP_" + powerUpSelected.ToString().ToUpper() + "_EXPLANATION" );

	}

	//Right arrow
	public void next()
	{
		PowerUpType newPowerUp = PowerUpDisplayData.getNextPowerUp();
		PlayerStatsManager.Instance.setPowerUpSelected( newPowerUp );
		updatePowerupData( newPowerUp );
	}

	//Left arrow
	public void previous()
	{
		PowerUpType newPowerUp = PowerUpDisplayData.getPreviousPowerUp();
		PlayerStatsManager.Instance.setPowerUpSelected( newPowerUp );
		updatePowerupData( newPowerUp );
	}

	public void resumeGame()
	{
		pauseGame();
	}
	
	//Returns the player to the World Map
	public void quit()
	{
		Debug.Log("Quit button pressed");
		//Save before going to the world map in particular so player does not lose coins he picked up
		PlayerStatsManager.Instance.savePlayerStats();
		//We might have the slow down power-up still active, so just to be sure
		//we will reset the timescale back to 1.
		Time.timeScale = 1f;
		//Report score to Game Center
		GameCenterManager.updateLeaderboard();
		if( GameManager.Instance.isMultiplayer() )
		{
			//Clean-up matches and connections on exit
			if( MPNetworkLobbyManager.Instance != null ) MPNetworkLobbyManager.Instance.cleanUpOnExit();
			SceneManager.LoadScene( (int) GameScenes.CircuitSelection );
		}
		else
		{
			GameManager.Instance.setGameState(GameState.PostLevelPopup);
			SceneManager.LoadScene( (int) GameScenes.WorldMap );
		}
	}

	//If the device is paused by pressing the Home button, because of a low battery warning or a phone call, the game will automatically display the pause menu.
	void OnApplicationPause( bool pauseStatus )
	{
		if( pauseStatus && GameManager.Instance.getGameState() != GameState.Paused && playerController.getCharacterState() != PlayerCharacterState.Dying ) pauseGame();
	}
	
	public void pauseGame()
	{
		GameState gameState = GameManager.Instance.getGameState();
		
		if( gameState == GameState.Normal )
		{
			//Pause game
			GameManager.Instance.setGameState( GameState.Paused );
			timeScaleBeforePause = Time.timeScale;
			Time.timeScale = 0;
			AudioListener.pause = true;
			//Take this opportunity to do a garbage collection
			System.GC.Collect();
			updatePowerupData( PlayerStatsManager.Instance.getPowerUpSelected() );
			pauseMenuPopup.gameObject.SetActive( true );
			isPlayerControlEnabledBeforePause = playerController.isPlayerControlEnabled();
			playerController.enablePlayerControl(false);
		}
		else if( gameState == GameState.Paused )
		{
			//We were paused. Start the resume game countdown
			GameManager.Instance.setGameState( GameState.Countdown );
			pauseMenuPopup.gameObject.SetActive( false );
			if( playerController.getCharacterState() != PlayerCharacterState.Dying )
			{
				//In case we pause and unpause quickly, make sure we stop the countdown routine
				StopCoroutine( "StartCountdown" );
				CancelInvoke ();
				StartCoroutine( "StartCountdown" );
			}
			else
			{
				//Resume game but without the countdown
				AudioListener.pause = false;
				GameManager.Instance.setGameState( GameState.Normal );
				if( isPlayerControlEnabledBeforePause ) playerController.enablePlayerControl(true);
			}
		}
	}
	
	IEnumerator StartCountdown()
	{
		//For the 3,2,1 countdown displayed when resuming game after pausing it
		goText.rectTransform.eulerAngles = new Vector3( 0,0,0 );
		goText.gameObject.SetActive( true );
		int countdown = 3;
		while (countdown > 0)
		{
			goText.text = countdown.ToString();
			UISoundManager.uiSoundManager.playAudioClip( beep );
			yield return new WaitForSecondsRealtime( 1.0f );
			countdown --;
			if( countdown == 0 )
			{
				//Resume game
				Time.timeScale = timeScaleBeforePause;
				AudioListener.pause = false;
				GameManager.Instance.setGameState( GameState.Normal );
				if( isPlayerControlEnabledBeforePause ) playerController.enablePlayerControl(true);
				//Display a Go! message and hide after a few seconds
				goText.rectTransform.eulerAngles = new Vector3( 0,0,4 );
				goText.text = LocalizationManager.Instance.getText("GO");
				Invoke ("hideGoText", 1.5f );
			}
		}
	}

	void hideGoText()
	{
		goText.gameObject.SetActive( false );
	}

	void OnEnable()
	{
		PlayerController.localPlayerCreated += LocalPlayerCreated;
	}
	
	void OnDisable()
	{
		PlayerController.localPlayerCreated -= LocalPlayerCreated;
	}

	void LocalPlayerCreated( Transform playerTransform, PlayerController playerController )
	{
		this.playerController = playerController;
	}

}
