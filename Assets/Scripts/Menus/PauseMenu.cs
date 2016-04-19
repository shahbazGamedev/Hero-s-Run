using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

	float timeScaleBeforePause;
	bool isPlayerControlEnabledBeforePause = true;
	PlayerController playerController;

	//For the 3,2,1 countdown displayed when resuming game after pausing it
	//Countdown 3,2,1
	int countdown = -1;
	//Sound to play every second during countdown
	public AudioClip beep;

	public GameObject pauseMenuPopup;

	public Text titleText;
	public Text levelDescriptionText;
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

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		playerController = playerObject.GetComponent<PlayerController>();

		titleText.text = LocalizationManager.Instance.getText("MENU_PAUSE");

		PowerUpType powerUpSelected = PlayerStatsManager.Instance.getPowerUpSelected();
		updatePowerupData( powerUpSelected );

		tutorialText.text = LocalizationManager.Instance.getText("POWER_UP_HOW_TO");

		resumeButtonText.text = LocalizationManager.Instance.getText("MENU_RESUME");
		homeButtonText.text = LocalizationManager.Instance.getText("MENU_QUIT");

		goText.text = LocalizationManager.Instance.getText("GO"); //Used to display 3,2,1 Go!
	}
	
	private void setLevelToLoad( int levelToLoad )
	{
		//Now that we know what level to load, we can retrieve the level description text.
		levelDescriptionText.text = LevelManager.Instance.getLevelDescription(levelToLoad);

	}

	private void updatePowerupData( PowerUpType powerUpSelected )
	{
		powerupQuantity.text = PlayerStatsManager.Instance.getPowerUpQuantity( powerUpSelected ).ToString();
		powerupImage.sprite = PowerUpDisplayData.getPowerUpSprite( powerUpSelected );
		powerupName.text = PowerUpDisplayData.getPowerUpName( powerUpSelected );
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
	public void goToHomeMenu()
	{
		Debug.Log("Home button pressed");
		//Save before going to home menu in particular so player does not lose stars he picked up
		PlayerStatsManager.Instance.savePlayerStats();
		playerController.enablePlayerControl(false);
		//We might have the slow down power-up still active, so just to be sure
		//we will reset the timescale back to 1.
		Time.timeScale = 1f;
		SoundManager.stopMusic();
		SoundManager.stopAmbience();
		playerController.resetLevel();
		//Go back to world map
		SceneManager.LoadScene( (int) GameScenes.WorldMap );
	}

	//If the device is paused by pressing the Home button, because of a low battery warning or a phone call, the game will automatically display the pause menu.
	void OnApplicationPause( bool pauseStatus )
	{
		if( pauseStatus && GameManager.Instance.getGameState() != GameState.Paused ) pauseGame();
	}
	
	public void pauseGame()
	{
		GameState gameState = GameManager.Instance.getGameState();
		
		if( gameState == GameState.Normal )
		{
			//Pause game
			GameManager.Instance.setGameState( GameState.Paused );
			SoundManager.pauseMusic();
			timeScaleBeforePause = Time.timeScale;
			Time.timeScale = 0;
			AudioListener.pause = true;
			updatePowerupData( PlayerStatsManager.Instance.getPowerUpSelected() );
			setLevelToLoad( LevelManager.Instance.getNextLevelToComplete() );
			pauseMenuPopup.gameObject.SetActive( true );
			isPlayerControlEnabledBeforePause = playerController.isPlayerControlEnabled();
			playerController.enablePlayerControl(false);
		}
		else if( gameState == GameState.Paused )
		{
			//We were paused. Start the resume game countdown
			GameManager.Instance.setGameState( GameState.Countdown );
			//We need to set the time scale back to 1 or else our coroutine will not execute.
			Time.timeScale = timeScaleBeforePause;
			pauseMenuPopup.gameObject.SetActive( false );
			if( playerController.getCharacterState() != CharacterState.Dying )
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
				SoundManager.playMusic();
				if( isPlayerControlEnabledBeforePause ) playerController.enablePlayerControl(true);
			}
		}
	}
	
	IEnumerator StartCountdown()
	{
		int startValue = 3;
		goText.rectTransform.eulerAngles = new Vector3( 0,0,0 );
		goText.gameObject.SetActive( true );
		countdown = startValue;
		while (countdown > 0)
		{
			goText.text = countdown.ToString();
			SoundManager.playGUISound( beep );
			//Slow time may be happening. We still want the countdown to be one second between count
			yield return new WaitForSeconds(1.0f * Time.timeScale);
			countdown --;
			if( countdown == 0 )
			{
				//Resume game
				AudioListener.pause = false;
				GameManager.Instance.setGameState( GameState.Normal );
				SoundManager.playMusic();
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
}
