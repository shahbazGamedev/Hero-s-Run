using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EndlessPostLevelPopup : MonoBehaviour {


	[Header("Endless Mode - Post Level Popup")]
	public Text episodeNumberText;
	public Text episodeNameText;
	public Text postLevelDescriptionText;
	public Button postLevelButton;
	public Text postLevelButtonText;
	public NewWorldMapHandler newWorldMapHandler;
	[Header("Meters")]
	public GameObject starMeter;
	public GameObject distanceMeter;
	public GameObject scoreMeter;
	public Text personalBestText;

	const float UPDATE_SEQUENCE_DELAY = 0.9f;

	// Use this for initialization
	void Awake () {

		postLevelButtonText.text = LocalizationManager.Instance.getText("POST_LEVEL_RETRY");
		personalBestText.text = LocalizationManager.Instance.getText("MENU_PERSONAL_BEST");
	}

	public void showEndlessPostLevelPopup(LevelData levelData)
	{
		int starsEarned = calculateStarsEarned();
		Debug.Log( "showEndlessPostLevelPopup-score: " + LevelManager.Instance.getScore() + " stars previous " + PlayerStatsManager.Instance.getNumberDisplayStarsForEpisode( LevelManager.Instance.getCurrentEpisodeNumber() ) + " new stars earned " + starsEarned );
		if( starsEarned > PlayerStatsManager.Instance.getNumberDisplayStarsForEpisode( LevelManager.Instance.getCurrentEpisodeNumber() ) )
		{
			//For this episode, we have more stars than we had before
			//Save the value
			PlayerStatsManager.Instance.setNumberDisplayStarsForEpisode( starsEarned );
			PlayerStatsManager.Instance.savePlayerStats();
			newWorldMapHandler.updateDisplayStars( LevelManager.Instance.getCurrentEpisodeNumber(), starsEarned );
			
		}
		//Reset values before sliding out
		starMeter.GetComponent<ScoreMeterHandler>().resetScore();
		distanceMeter.GetComponent<ScoreMeterHandler>().resetScore();
		scoreMeter.GetComponent<ScoreMeterHandler>().resetScore();
		personalBestText.gameObject.SetActive ( false );
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

		StartCoroutine( startUpdateSequence( selectedEpisode ) );
		
	}

	IEnumerator startUpdateSequence( LevelData.EpisodeInfo selectedEpisode )
	{
		//Wait for the endless post-level popup to have finished sliding in before spinning values
		yield return new WaitForSeconds(UPDATE_SEQUENCE_DELAY);
		//Spin star counter first
		StartCoroutine( starMeter.GetComponent<ScoreMeterHandler>().spinScoreNumber( LevelManager.Instance.getScore(), spinDistance ) );
	}

	void spinDistance()
	{
		StartCoroutine( distanceMeter.GetComponent<ScoreMeterHandler>().spinScoreNumber( PlayerStatsManager.Instance.getDistanceTravelled(), spinScore ) );
	}
	
	void spinScore()
	{
		StartCoroutine( scoreMeter.GetComponent<ScoreMeterHandler>().spinScoreNumber( LevelManager.Instance.getScore() +  PlayerStatsManager.Instance.getDistanceTravelled(), updateHighScore ) );
	}

	void updateHighScore()
	{
		if( PlayerStatsManager.Instance.setHighScoreForEpisode( PlayerStatsManager.Instance.getDistanceTravelled() + LevelManager.Instance.getScore() ) )
		{
			//New high score - add a tag
			personalBestText.gameObject.SetActive ( true );
		}
	}

	public void closePostLevelPopup()
	{
		SoundManager.soundManager.playButtonClick();
		PlayerStatsManager.Instance.resetDistanceTravelled();
		PlayerStatsManager.Instance.savePlayerStats();
		GetComponent<Animator>().Play("Panel Slide Out");
		GameManager.Instance.setGameState(GameState.Menu);
	}

	public void retry()
	{
		Debug.Log("showEndlessPostLevelPopup-Retry button pressed.");
		SoundManager.soundManager.playButtonClick();
		PlayerStatsManager.Instance.resetDistanceTravelled();
		PlayerStatsManager.Instance.savePlayerStats();
		newWorldMapHandler.play( LevelManager.Instance.getCurrentEpisodeNumber(), LevelManager.Instance.getNextLevelToComplete() );
	}


}
