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
	public StoryCompletedPopup storyCompletedPopup;
	public ConnectToFacebookPopup connectToFacebookPopup;
	[Header("Score Meter")]
	public GameObject scoreMeter;

	ClockTimeSetter clockTimeSetter;
	const float UPDATE_SEQUENCE_DELAY = 0.9f;

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
				}
				clockTimeSetter.updateTime( episodeNumber, LevelManager.Instance.getNextLevelToComplete(), Level_Progress.LEVEL_END_WITH_PROGRESS );
			}
			else
			{
				//The player did not finish the current level. Encourage him.
				postLevelDescriptionText.text = LocalizationManager.Instance.getText("POST_LEVEL_LUCK_NEXT_TIME");
				clockTimeSetter.updateTime( episodeNumber, LevelManager.Instance.getNextLevelToComplete(), Level_Progress.LEVEL_END_WITH_NO_PROGRESS );
			}
		}
		episodeKeysText.text = PlayerStatsManager.Instance.getNumberKeysFoundInEpisode( episodeNumber ) + "/" + selectedEpisode.numberOfChestKeys;
		StartCoroutine( startUpdateSequence( selectedEpisode ) );
		
	}

	IEnumerator startUpdateSequence( LevelData.EpisodeInfo selectedEpisode )
	{
		//Wait for the post-level popup to have finished sliding in before spinning values
		yield return new WaitForSeconds(UPDATE_SEQUENCE_DELAY);
		StartCoroutine( scoreMeter.GetComponent<ScoreMeterHandler>().spinScoreNumber( LevelManager.Instance.getScore() ) );
	}

	public void closePostLevelPopup()
	{
		SoundManager.soundManager.playButtonClick();
		//Reset the level changed value
		LevelManager.Instance.setLevelChanged( false );
		GetComponent<Animator>().Play("Panel Slide Out");
		GameManager.Instance.setGameState(GameState.Menu);
		if( LevelManager.Instance.getPlayerFinishedTheGame() )
		{
			StartCoroutine( showStoryCompletedPopupThread() );
		}
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

		//So if we are here, it means that the player successfully completed the episode.
		//Before we allow him to continue to the next one, if the user does not use Facebook currently and we have network connectivity,
		//then encourage the player to connect to Facebook.
		//We will encourage him to connect to Facebook after completing episode 2, 4, 6, and 8.
		if ( LevelManager.Instance.getCurrentEpisodeNumber()%2 == 0 && !PlayerStatsManager.Instance.getUsesFacebook() && Application.internetReachability != NetworkReachability.NotReachable )
		{
			connectToFacebookPopup.showConnectToFacebookPopup();
		}
		else
		{
			episodePopup.showEpisodePopup( LevelManager.Instance.getCurrentEpisodeNumber(), LevelManager.Instance.getNextLevelToComplete() );
		}
	}

	public void retry()
	{
		Debug.Log("PostLevelPopup-Retry button pressed.");
		SoundManager.soundManager.playButtonClick();
		newWorldMapHandler.play( LevelManager.Instance.getCurrentEpisodeNumber(), LevelManager.Instance.getNextLevelToComplete() );
	}

	IEnumerator showStoryCompletedPopupThread()
	{
		GetComponent<Animator>().Play("Panel Slide Out");
		yield return new WaitForSeconds(2f);
		storyCompletedPopup.showStoryCompletedPopup();	
	}

}
