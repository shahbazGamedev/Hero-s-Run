using UnityEngine;
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
	[Header("Not Enough Time Popup")]
	public GameObject ranOutofTimePopup;
	public Text ranOutofTimeTitleText;
	public Text ranOutofTimeContentText;
	public Text ranOutofTimeButtonText;
	 

	void Awake()
	{
		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		playerController = playerObject.GetComponent<PlayerController>();

		//Normal Save Me
		titleNormalText.text = LocalizationManager.Instance.getText("MENU_SAVE_ME_TITLE");
		saveMeText.text = LocalizationManager.Instance.getText("MENU_SAVE_ME");
		costString = LocalizationManager.Instance.getText("MENU_COST");
		checkpointText.text = LocalizationManager.Instance.getText("MENU_FROM_CHECKPOINT");
		checkpointCostText.text = LocalizationManager.Instance.getText("MENU_FREE");
		quitNormalText.text = LocalizationManager.Instance.getText("MENU_QUIT");
		//Tutorial Save Me
		titleTutorialText.text = LocalizationManager.Instance.getText("TUTORIAL_OOPS");
		//helpText.text gets set at runtime.
		tryAgainText.text = LocalizationManager.Instance.getText("MENU_TRY_AGAIN");
		quitTutorialText.text = LocalizationManager.Instance.getText("MENU_QUIT");
		//Not enough time
		ranOutofTimeTitleText.text = LocalizationManager.Instance.getText("NOT_ENOUGH_TIME_TITLE");
		ranOutofTimeContentText.text = LocalizationManager.Instance.getText("NOT_ENOUGH_TIME_CONTENT");
		ranOutofTimeButtonText.text = LocalizationManager.Instance.getText("NOT_ENOUGH_TIME_BUTTON");
		//Do not show the retry from last checkpoint button when in endless mode
		if( GameManager.Instance.getGameMode() == GameMode.Endless )
		{
			buttonPanel.sizeDelta = new Vector2(buttonPanel.rect.width, buttonPanel.rect.height - checkpointButton.rect.height);
			checkpointButton.gameObject.SetActive( false );
		}
	}

	void OnEnable()
	{
		if( PlayerStatsManager.Instance.getHasInfiniteLives() )
		{
			livesText.text = LocalizationManager.Instance.getText("MENU_LIVES") + " " + PlayerStatsManager.Instance.getLives().ToString("N0") + "*";
		}
		else
		{
			livesText.text = LocalizationManager.Instance.getText("MENU_LIVES") + " " + PlayerStatsManager.Instance.getLives().ToString("N0");
		}
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
			if( ( GameManager.Instance.getGameMode() == GameMode.Story ) && PlayerStatsManager.Instance.getNumberDeathLeadingToLevel( LevelManager.Instance.getNextLevelToComplete() + 1 ) > GameManager.MAX_NUMBER_OF_ATTEMPTS )
			{
				ranOutofTimePopup.SetActive( true );
			}
			else
			{

				activateNormalSaveMe();
			}
		}
	}

	void closeSaveMeMenu()
	{
		SoundManager.soundManager.playButtonClick();
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

	}

	public void tutorialTryAgain()
	{
		Debug.Log("Try Again button pressed.");
		GameManager.Instance.setGameState( GameState.Resurrect );
		playerController.resurrectBegin(false);
		closeSaveMeMenu();
	}

	public void saveMe()
	{
		float costLives = PlayerStatsManager.Instance.getTimesPlayerRevivedInLevel() + 1;
		if( PlayerStatsManager.Instance.getLives() >= costLives || PlayerStatsManager.Instance.getHasInfiniteLives() )
		{
			Debug.Log("Save Me button pressed.");
			GameManager.Instance.setGameState( GameState.Resurrect );
			playerController.resurrectBegin(false);
			PlayerStatsManager.Instance.decreaseLives((int)costLives);
			PlayerStatsManager.Instance.incrementTimesPlayerRevivedInLevel();
			PlayerStatsManager.Instance.incrementNumberDeathForLevel();
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
		SoundManager.soundManager.fadeOutAllAudio( SoundManager.STANDARD_FADE_TIME );
		closeSaveMeMenu();
		PlayerStatsManager.Instance.resetTimesPlayerRevivedInLevel();
		LevelManager.Instance.setEnableTorches( true );
		LevelManager.Instance.setNextLevelToComplete( LevelManager.Instance.getLevelNumberOfLastCheckpoint() );
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
		SoundManager.soundManager.stopMusic();
		SoundManager.soundManager.stopAmbience();
		GameManager.Instance.setGameState(GameState.PostLevelPopup);
		//Report score to Game Center
		GameCenterManager.updateLeaderboard();
		closeSaveMeMenu();
		SceneManager.LoadScene( (int) GameScenes.WorldMap );
	}

}
