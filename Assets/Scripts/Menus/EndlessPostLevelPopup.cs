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
			challengeButtonText.text = LocalizationManager.Instance.getText("POST_LEVEL_CHALLENGE_BRAG");
		}
	}

	public string getNamesOfBeatenChallenger()
	{
		List<ChallengeBoard.Challenge> completedChallengesList = GameManager.Instance.challengeBoard.getCompletedChallenges( LevelManager.Instance.getCurrentEpisodeNumber() );
		string[] arrayOfBeatenChallengers = new string[completedChallengesList.Count];
		for( int i = 0; i < completedChallengesList.Count; i++ )
		{
			//Populate an array with the names of the beaten challengers
			arrayOfBeatenChallengers[i] = completedChallengesList[i].challengerFirstName;
			//Create a comma separated list of IDs, which we will use if the player presses the Brag button
			directRequestTo = completedChallengesList[i].challengerID + "," + directRequestTo;
		}
		//Delete trailing comma
		directRequestTo = directRequestTo.TrimEnd(',');

		//Update the message text in the popup based on the number of challengers beaten, but only if we ran with at least one challenge active
		string textToDisplay = string.Empty;
		if( GameManager.Instance.challengeBoard.getNumberOfActiveChallenges( LevelManager.Instance.getCurrentEpisodeNumber() ) > 0 )
		{

			//POST_LEVEL_BEAT_ONE_PERSON,Awesome. You just beat <player1>'s score!
			//POST_LEVEL_BEAT_TWO_PERSONS,Awesome. You just beat <player1>'s and <player2>'s score!
			//POST_LEVEL_BEAT_THREE_PERSONS,Awesome. You just beat <player1>'s<comma> <player2>'s<comma> and <player3>'s score!
			//POST_LEVEL_BEAT_MORE_THAN_THREE,Awesome. You just beat the score of <player1><comma> <player2><comma> and <number additional players> other friends!

			switch (completedChallengesList.Count)
			{
				case 0:
					//The player did not beat any of the challengers. Simply keep the normal challenge button and text.
					break;
					
				case 1:
					textToDisplay = LocalizationManager.Instance.getText("POST_LEVEL_BEAT_ONE_PERSON");
					//Replace name of challengers
					textToDisplay = textToDisplay.Replace("<player1>", arrayOfBeatenChallengers[0] ); 
					break;
					
				case 2:
					textToDisplay = LocalizationManager.Instance.getText("POST_LEVEL_BEAT_TWO_PERSONS");
					//Replace name of challengers
					textToDisplay = textToDisplay.Replace("<player1>", arrayOfBeatenChallengers[0] ); 
					textToDisplay = textToDisplay.Replace("<player2>", arrayOfBeatenChallengers[1] ); 
					break;
		
				case 3:
					textToDisplay = LocalizationManager.Instance.getText("POST_LEVEL_BEAT_THREE_PERSONS");
					//Replace name of challengers
					textToDisplay = textToDisplay.Replace("<player1>", arrayOfBeatenChallengers[0] ); 
					textToDisplay = textToDisplay.Replace("<player2>", arrayOfBeatenChallengers[1] ); 
					textToDisplay = textToDisplay.Replace("<player3>", arrayOfBeatenChallengers[2] ); 
					break;

				default:
					textToDisplay = LocalizationManager.Instance.getText("POST_LEVEL_BEAT_MORE_THAN_THREE");
					//Replace name of challengers
					textToDisplay = textToDisplay.Replace("<player1>", arrayOfBeatenChallengers[0] ); 
					textToDisplay = textToDisplay.Replace("<player2>", arrayOfBeatenChallengers[1] );
					//More than 3 challengers were beaten. Just state the number of additional challengers beaten.
					textToDisplay = textToDisplay.Replace("<number additional players>", (arrayOfBeatenChallengers.Length - 2).ToString() ); 
					break;
			}
		}

		//Delete completed challenges
		for( int i = 0; i < completedChallengesList.Count; i++ )
		{
			GameManager.Instance.challengeBoard.removeChallenge( completedChallengesList[i] );
		}
		//Save the updated challenge list
		GameManager.Instance.challengeBoard.serializeChallenges();
	
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
		UISoundManager.uiSoundManager.playButtonClick();
		PlayerStatsManager.Instance.savePlayerStats();
		GetComponent<Animator>().Play("Panel Slide Out");
	}

	public void retry()
	{
		Debug.Log("showEndlessPostLevelPopup-Retry button pressed.");
		UISoundManager.uiSoundManager.playButtonClick();
		PlayerStatsManager.Instance.savePlayerStats();
		newWorldMapHandler.play( LevelManager.Instance.getCurrentEpisodeNumber() );
	}

	public void challengeFriends()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		string title = LocalizationManager.Instance.getText( "POST_LEVEL_CHALLENGE_FB_TITLE" );
		string message = LocalizationManager.Instance.getText( "POST_LEVEL_CHALLENGE_FB_MESSAGE" );
		int playerScore = LevelManager.Instance.getScore() + PlayerStatsManager.Instance.getDistanceTravelled();
		int episodeNumber = LevelManager.Instance.getCurrentEpisodeNumber();
		string passedData = "Challenge," + playerScore.ToString() + "," + episodeNumber.ToString() + ",Rank X";
		//To recap, format of passed data is Challenge,88888,4,Future Use (might be used for the challenger's rank)
		FacebookManager.Instance.CallAppRequestAsFriendSelector( title, message, passedData, "", "" );
	}

	public void bragFriends()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		string title = LocalizationManager.Instance.getText( "POST_LEVEL_CHALLENGE_BEATEN_FB_TITLE" );
		string message = LocalizationManager.Instance.getText( "POST_LEVEL_CHALLENGE_BEATEN_FB_MESSAGE" );
		int playerScore = LevelManager.Instance.getScore() + PlayerStatsManager.Instance.getDistanceTravelled();
		int episodeNumber = LevelManager.Instance.getCurrentEpisodeNumber();
		string passedData = "ChallengeBeaten," + playerScore.ToString() + "," + episodeNumber.ToString() + ",Rank X";
		//To recap, format of passed data is Challenge,88888,4,Future Use (might be used for the challenger's rank)
		Debug.Log("bragFriends pressed directRequestTo " + directRequestTo + " " + passedData );

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
