using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Facebook.Unity;

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
	[Header("Challenge Friends")]
	public Text challengeDescriptionText;
	public Button challengeButton;
	public Text challengeButtonText;

	const float UPDATE_SEQUENCE_DELAY = 0.9f;
	string directRequestTo = string.Empty;

	// Use this for initialization
	void Awake () {

		postLevelButtonText.text = LocalizationManager.Instance.getText("POST_LEVEL_RETRY");
		personalBestText.text = LocalizationManager.Instance.getText("MENU_PERSONAL_BEST");

		challengeDescriptionText.text = LocalizationManager.Instance.getText("POST_LEVEL_CHALLENGE_DESCRIPTION");
		challengeButtonText.text = LocalizationManager.Instance.getText("POST_LEVEL_CHALLENGE");
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
		string challengeResults = getNamesOfBeatenChallenger();
		if( challengeResults != string.Empty )
		{
			challengeDescriptionText.text = challengeResults;
			challengeButton.onClick.RemoveAllListeners();
			challengeButton.onClick.AddListener(() => bragFriends() );
			challengeButtonText.text = "Brag";
		}
	}

	public string getNamesOfBeatenChallenger()
	{
		List<ChallengeBoard.Challenge> completedChallengesList = GameManager.Instance.challengeBoard.getCompletedChallenges( LevelManager.Instance.getCurrentEpisodeNumber() );
		string[] arrayOfBeatenChallengers = new string[completedChallengesList.Count];
		for( int i = 0; i < completedChallengesList.Count; i++ )
		{
			arrayOfBeatenChallengers[i] = completedChallengesList[i].challengerFirstName;
			//Create a comma separated list of IDs when want to sent the brag to
			directRequestTo = completedChallengesList[i].challengerID + "," + directRequestTo;
			//GameManager.Instance.challengeBoard.removeChallenge( completedChallengesList[i] );
		}
		//Delete trailing comma
		directRequestTo = directRequestTo.TrimEnd(',');

		GameManager.Instance.challengeBoard.serializeChallenges();
		string textToDisplay = string.Empty;
		if( GameManager.Instance.challengeBoard.getNumberOfActiveChallenges( LevelManager.Instance.getCurrentEpisodeNumber() ) > 0 )
		{
			switch (completedChallengesList.Count)
			{
				case 0:
					//difficultyLevelName = LocalizationManager.Instance.getText("MENU_DIFFICULTY_LEVEL_NORMAL");
					textToDisplay = "You suck. You were not able to beat anyone's high score.";
					break;
					
				case 1:
					textToDisplay = "Awesome. You just beat " + arrayOfBeatenChallengers[0] +"'s high score!";
					break;
					
				case 2:
					textToDisplay = "Awesome. You just beat " + arrayOfBeatenChallengers[0] + " and " + arrayOfBeatenChallengers[1] +"'s high score!";
					break;
		
				case 3:
					textToDisplay = "Awesome. You just beat " + arrayOfBeatenChallengers[0] + ", " + arrayOfBeatenChallengers[1] + " and " + arrayOfBeatenChallengers[2] + "'s high score!";
					break;

				default:
					textToDisplay = "Awesome. You just beat " + arrayOfBeatenChallengers[0] + ", " + arrayOfBeatenChallengers[1] + " and " + (arrayOfBeatenChallengers.Length - 2).ToString() + " other friends's high score!";
					break;
				
			}
		}
		
		return textToDisplay;
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
		PlayerStatsManager.Instance.savePlayerStats();
		GetComponent<Animator>().Play("Panel Slide Out");
		GameManager.Instance.setGameState(GameState.Menu);
	}

	public void retry()
	{
		Debug.Log("showEndlessPostLevelPopup-Retry button pressed.");
		SoundManager.soundManager.playButtonClick();
		PlayerStatsManager.Instance.savePlayerStats();
		newWorldMapHandler.play( LevelManager.Instance.getCurrentEpisodeNumber(), LevelManager.Instance.getNextLevelToComplete() );
	}

	public void challengeFriends()
	{
		SoundManager.soundManager.playButtonClick();
		string title = LocalizationManager.Instance.getText( "POST_LEVEL_CHALLENGE_FB_TITLE" );
		string message = LocalizationManager.Instance.getText( "POST_LEVEL_CHALLENGE_FB_MESSAGE" );
		int playerScore = LevelManager.Instance.getScore() + PlayerStatsManager.Instance.getDistanceTravelled();
		int episodeNumber = LevelManager.Instance.getCurrentEpisodeNumber();
		string passedData = "Challenge," + playerScore.ToString() + "," + episodeNumber.ToString();
		//To recap, format of passed data is Challenge,88888,4
		FacebookManager.Instance.CallAppRequestAsFriendSelector( title, message, passedData, "", "" );
	}

	public void bragFriends()
	{
		SoundManager.soundManager.playButtonClick();
		string title = LocalizationManager.Instance.getText( "POST_LEVEL_CHALLENGE_BEATEN_FB_TITLE" );
		string message = LocalizationManager.Instance.getText( "POST_LEVEL_CHALLENGE_BEATEN_FB_MESSAGE" );
		int playerScore = LevelManager.Instance.getScore() + PlayerStatsManager.Instance.getDistanceTravelled();
		int episodeNumber = LevelManager.Instance.getCurrentEpisodeNumber();
		string passedData = "ChallengeBeaten," + playerScore.ToString() + "," + episodeNumber.ToString();
		//To recap, format of passed data is Challenge,88888,4
		Debug.LogWarning("bragFriends pressed directRequestTo " + directRequestTo + " " + passedData );

		FacebookManager.Instance.CallAppRequestAsDirectRequest( title, message, directRequestTo, passedData, bragFriendsCallback, null );
	}

	public void bragFriendsCallback(IAppRequestResult result, string appRequestIDToDelete )
	{
		if (result.Error != null)
		{
			Debug.Log ("bragFriendsCallback-Callback error:\n" + result.Error );
		}
		else
		{
			Debug.Log ("bragFriendsCallback-Callback success:\n" + result.RawResult );
			//bragFriendsCallback-Callback success: {"cancelled":true}
		}
	}
}
