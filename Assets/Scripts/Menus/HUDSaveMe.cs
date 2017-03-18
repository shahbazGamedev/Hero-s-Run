﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HUDSaveMe : MonoBehaviour {

	[Header("Save Me Menu")]
	public GameObject saveMeCanvas;
	PlayerController playerController;
	public MiniStoreHandler miniStoreHandler;
	[Header("Normal Save Me")]
	public GameObject normalPanel;
	public Text titleNormalText;
	public Text livesText;
	public Text saveMeText;
	public Text saveMeCostText;
	string costString;
	public Text checkpointText;
	public Text checkpointCostText;
	public Text quitNormalText;
	public RectTransform buttonPanel;
	public RectTransform checkpointButton;
	[Header("Tutorial Save Me")]
	public NewTutorialManager newTutorialManager;
	public GameObject tutorialPanel;
	public Text titleTutorialText;
	public Text helpText;
	public Text tryAgainText;
	public Text quitTutorialText;
	[Header("Episode Progress Indicator")]
	public GameObject progressBarPanel;
	 

	void Awake()
	{
		if( GameManager.Instance.isMultiplayer() ) Destroy( gameObject );
		//Normal Save Me
		titleNormalText.text = LocalizationManager.Instance.getText("MENU_SAVE_ME_TITLE");
		saveMeText.text = LocalizationManager.Instance.getText("MENU_SAVE_ME");
		costString = LocalizationManager.Instance.getText("MENU_COST");
		checkpointText.text = LocalizationManager.Instance.getText("MENU_FROM_CHECKPOINT");
		checkpointCostText.text = LocalizationManager.Instance.getText("MENU_FREE");
		quitNormalText.text = LocalizationManager.Instance.getText("MENU_QUIT");
		progressBarPanel.SetActive( false );
		//Tutorial Save Me
		titleTutorialText.text = LocalizationManager.Instance.getText("TUTORIAL_OOPS");
		//helpText.text gets set at runtime.
		tryAgainText.text = LocalizationManager.Instance.getText("MENU_TRY_AGAIN");
		quitTutorialText.text = LocalizationManager.Instance.getText("MENU_QUIT");
		//Do not show the retry from last checkpoint button when in endless mode
		if( GameManager.Instance.getGameMode() == GameMode.Endless )
		{
			buttonPanel.sizeDelta = new Vector2(buttonPanel.rect.width, buttonPanel.rect.height - checkpointButton.rect.height);
			checkpointButton.gameObject.SetActive( false );
		}
	}

	void OnEnable()
	{
		PlayerController.localPlayerCreated += LocalPlayerCreated;
		if( PlayerStatsManager.Instance.getHasInfiniteLives() )
		{
			livesText.text = LocalizationManager.Instance.getText("MENU_LIVES") + " " + PlayerStatsManager.Instance.getLives().ToString("N0") + "*";
		}
		else
		{
			livesText.text = LocalizationManager.Instance.getText("MENU_LIVES") + " " + PlayerStatsManager.Instance.getLives().ToString("N0");
		}
	}
	
	void OnDisable()
	{
		PlayerController.localPlayerCreated -= LocalPlayerCreated;
	}

	void LocalPlayerCreated( Transform playerTransform, PlayerController playerController )
	{
		this.playerController = playerController;
	}

	public void showSaveMeMenu()
	{
		saveMeCanvas.SetActive ( true );
		if( newTutorialManager.isTutorialActive )
		{
			activateTutorialSaveMe();
		}
		else
		{
			activateNormalSaveMe();
		}
	}

	void closeSaveMeMenu()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		saveMeCanvas.SetActive ( false );
	}

	void activateTutorialSaveMe()
	{
		//Set the help text in case the player failed a tutorial.
		helpText.text = newTutorialManager.getFailedTutorialText();
		tutorialPanel.SetActive( true );
		normalPanel.SetActive( false );
	}

	void activateNormalSaveMe()
	{
		if( PlayerStatsManager.Instance.getHasInfiniteLives() )
		{
			livesText.text = LocalizationManager.Instance.getText("MENU_LIVES") + " " + PlayerStatsManager.Instance.getLives().ToString("N0") + "*";
		}
		else
		{
			livesText.text = LocalizationManager.Instance.getText("MENU_LIVES") + " " + PlayerStatsManager.Instance.getLives().ToString("N0");
		}

		//Calculate the energy cost to revive.
		float costLives = PlayerStatsManager.Instance.getTimesPlayerRevivedInLevel() + 1;
		saveMeCostText.text = costString + " " + costLives.ToString();

		tutorialPanel.SetActive( false );
		normalPanel.SetActive( true );
		if( GameManager.Instance.getGameMode() == GameMode.Story )
		{
			progressBarPanel.SetActive( true );
			progressBarPanel.GetComponent<EpisodeProgressIndicator>().updatePlayerIconPosition();
		}

	}

	public void tutorialTryAgain()
	{
		Debug.Log("Try Again button pressed.");
		closeSaveMeMenu();
		SceneManager.LoadScene( (int) GameScenes.Level );
	}

	public void saveMe()
	{
		float costLives = PlayerStatsManager.Instance.getTimesPlayerRevivedInLevel() + 1;
		if( PlayerStatsManager.Instance.getLives() >= costLives || PlayerStatsManager.Instance.getHasInfiniteLives() )
		{
			Debug.Log("Save Me button pressed.");
			playerController.resurrectBegin(false);
			PlayerStatsManager.Instance.decreaseLives((int)costLives);
			PlayerStatsManager.Instance.incrementTimesPlayerRevivedInLevel();
			PlayerStatsManager.Instance.incrementNumberDeathForEpisode();
			closeSaveMeMenu();
		}
		else
		{
			//Show buy popup
			miniStoreHandler.showMiniStore();
		}
	}

	public void retryFromLastCheckpoint()
	{
		Debug.Log("Retry from last checkpoint button pressed");
		closeSaveMeMenu();
		PlayerStatsManager.Instance.resetTimesPlayerRevivedInLevel();
		LevelManager.Instance.setEnableTorches( true );
		LevelManager.Instance.setNextEpisodeToComplete( LevelManager.Instance.getCurrentEpisodeNumber() );
		playerController.resetSharedLevelData(false);
		SceneManager.LoadScene( (int) GameScenes.Level );
	}

	public void quit()
	{
		Debug.Log("Quit button pressed");
		//Save before going to the world map in particular so player does not lose stars he picked up
		PlayerStatsManager.Instance.savePlayerStats();
		//We might have the slow down power-up still active, so just to be sure
		//we will reset the timescale back to 1.
		Time.timeScale = 1f;
		//Report score to Game Center
		GameCenterManager.updateLeaderboard();
		closeSaveMeMenu();
		GameManager.Instance.setGameState(GameState.PostLevelPopup);
		SceneManager.LoadScene( (int) GameScenes.WorldMap );
	}

}
