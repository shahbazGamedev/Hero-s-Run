using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PostLevelPopup : MonoBehaviour {


	[Header("Post Level Popup")]
	public Text episodeNumberText;
	public Text episodeNameText;
	public Text postLevelDescriptionText;
	public Text episodeKeysText; //format found/total e.g. 1/3
	public Button postLevelButton;
	public Text postLevelButtonText;
	public NewWorldMapHandler newWorldMapHandler;
	public EpisodePopup episodePopup;
	[Header("Score Meter")]
	public GameObject scoreMeter;

	bool levelLoading = false;
	ClockTimeSetter clockTimeSetter;

	// Use this for initialization
	void Awake () {

		postLevelButtonText.text = LocalizationManager.Instance.getText("POST_LEVEL_RETRY");
		clockTimeSetter = GetComponentInChildren<ClockTimeSetter>();
	}

	public void showPostLevelPopup(LevelData levelData)
	{
		int starsEarned = calculateStarsEarned();
		Debug.Log( "showPostLevelPopup-score: " + LevelManager.Instance.getScore() + " stars previous " + PlayerStatsManager.Instance.getNumberDisplayStarsForEpisode( LevelManager.Instance.getCurrentEpisodeNumber() ) + " new stars earned " + starsEarned );
		if( starsEarned > PlayerStatsManager.Instance.getNumberDisplayStarsForEpisode( LevelManager.Instance.getCurrentEpisodeNumber() ) )
		{
			//For this episode, we have more stars than we had before
			//Save the value
			PlayerStatsManager.Instance.setNumberDisplayStarsForEpisode( starsEarned );
			PlayerStatsManager.Instance.savePlayerStats();
			newWorldMapHandler.updateDisplayStars( LevelManager.Instance.getCurrentEpisodeNumber(), starsEarned );
			
		}
		//Remove the existing events
    	postLevelButton.onClick.RemoveAllListeners();
		postLevelButton.onClick.AddListener( retry );
		GetComponent<Animator>().Play("Panel Slide In");	
		loadEpisodeData(levelData);
	}

	int calculateStarsEarned()
	{
		LevelData.EpisodeInfo currentEpisode = LevelManager.Instance.getCurrentEpisodeInfo();
		int score = LevelManager.Instance.getScore();

		int numberOfStars = 0;

		if ( score >= currentEpisode.starsRequired.x && score < currentEpisode.starsRequired.y )
		{
			numberOfStars = 1;
		}
		else if ( score >= currentEpisode.starsRequired.y && score < currentEpisode.starsRequired.z )
		{
			numberOfStars = 2;
		}
		else if ( score >= currentEpisode.starsRequired.z )
		{
			numberOfStars = 3;
		}
		return numberOfStars;
	}

	private void loadEpisodeData(LevelData levelData)
	{
		int episodeNumber = LevelManager.Instance.getCurrentEpisodeNumber();

		LevelData.EpisodeInfo selectedEpisode = levelData.getEpisodeInfo( episodeNumber );
		string levelNumberString = (episodeNumber + 1).ToString();

		string episodeNumberString = LocalizationManager.Instance.getText("EPISODE_NUMBER");

		//Replace the string <number> by the number of the episode
		episodeNumberString = episodeNumberString.Replace( "<number>", levelNumberString );

		episodeNumberText.text = episodeNumberString;
		episodeNameText.text = LocalizationManager.Instance.getText("EPISODE_NAME_" + levelNumberString );
		if( LevelManager.Instance.getPlayerFinishedTheGame() )
		{
			postLevelDescriptionText.text = LocalizationManager.Instance.getText("POST_LEVEL_COMPLETED_STORY");
			clockTimeSetter.updateTime( episodeNumber, LevelManager.Instance.getNextLevelToComplete(), Level_Progress.LEVEL_END_WITH_GAME_COMPLETED );
		}
		else
		{
			if( LevelManager.Instance.getLevelChanged() )
			{
				//The player has passed at least one checkpoint. Congratulate him.
				postLevelDescriptionText.text = LocalizationManager.Instance.getText("POST_LEVEL_MADE_PROGRESS");
				if( LevelManager.Instance.wasEpisodeCompleted() )
				{
					postLevelButtonText.text = LocalizationManager.Instance.getText("POST_LEVEL_CONTINUE");
					//Remove the existing events
    				postLevelButton.onClick.RemoveAllListeners();
					postLevelButton.onClick.AddListener( showNextEpisodePopup );
					clockTimeSetter.updateTime( episodeNumber, LevelManager.Instance.getNextLevelToComplete(), Level_Progress.LEVEL_END_WITH_PROGRESS );
				}
			}
			else
			{
				//The player did not finish the current level. Encourage him.
				postLevelDescriptionText.text = LocalizationManager.Instance.getText("POST_LEVEL_LUCK_NEXT_TIME");
				clockTimeSetter.updateTime( episodeNumber, LevelManager.Instance.getNextLevelToComplete(), Level_Progress.LEVEL_END_WITH_NO_PROGRESS );
			}
		}
		episodeKeysText.text = PlayerStatsManager.Instance.getNumberKeysFoundInEpisode( episodeNumber ) + "/" + selectedEpisode.numberOfChestKeys;
		StartCoroutine( scoreMeter.GetComponent<ScoreMeterHandler>().startUpdateSequence( selectedEpisode ) );
		
	}

	public void closePostLevelPopup()
	{
		SoundManager.playButtonClick();
		//Reset the level changed value
		LevelManager.Instance.setLevelChanged( false );
		GameManager.Instance.setGameState(GameState.Menu);
		GetComponent<Animator>().Play("Panel Slide Out");	
	}

	public void showNextEpisodePopup()
	{
		StartCoroutine( showNextEpisodePopupThread() );
	}

	IEnumerator showNextEpisodePopupThread()
	{
		Debug.Log("Continue button pressed: ");
		//Reset the level changed value
		LevelManager.Instance.setLevelChanged( false );
		LevelManager.Instance.incrementCurrentEpisodeNumber();
		GameManager.Instance.setGameState(GameState.Menu);
		GetComponent<Animator>().Play("Panel Slide Out");
		yield return new WaitForSeconds(2f);
		episodePopup.showEpisodePopup( LevelManager.Instance.getCurrentEpisodeNumber(), LevelManager.Instance.getNextLevelToComplete() );	
	}

	public void retry()
	{
		Debug.Log("Retry button pressed: ");
		//Reset the level changed value
		LevelManager.Instance.setLevelChanged( false );
		//We are starting a new run, reset the episode stars
		LevelManager.Instance.setScore( 0 );
		SoundManager.playButtonClick();
		StartCoroutine( loadLevel() );
	}

	IEnumerator loadLevel()
	{
		if( !levelLoading )
		{
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int) GameScenes.Level );
		}
	}
	
}
