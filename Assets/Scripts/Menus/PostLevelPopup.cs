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
	public Text retryButtonText;
	[Header("Star Meter")]
	public GameObject starMeter;

	bool levelLoading = false;

	// Use this for initialization
	void Awake () {

		retryButtonText.text = LocalizationManager.Instance.getText("POST_LEVEL_RETRY");
	}

	public void showPostLevelPopup(LevelData levelData)
	{	
		GetComponent<Animator>().Play("Panel Slide In");	
		loadEpisodeData(levelData);
	}

	private void loadEpisodeData(LevelData levelData)
	{
		int episodeNumber = LevelManager.Instance.EpisodeCurrentlyBeingPlayed;

		LevelData.EpisodeInfo selectedEpisode = levelData.getEpisodeInfo( episodeNumber );
		string levelNumberString = (episodeNumber + 1).ToString();

		string episodeNumberString = LocalizationManager.Instance.getText("EPISODE_NUMBER");

		//Replace the string <number> by the number of the episode
		episodeNumberString = episodeNumberString.Replace( "<number>", levelNumberString );

		episodeNumberText.text = episodeNumberString;
		episodeNameText.text = LocalizationManager.Instance.getText("EPISODE_NAME_" + levelNumberString );
		if( LevelManager.Instance.getLevelChanged() )
		{
			//The player has passed at least one checkpoint. Congratulate him.
			postLevelDescriptionText.text = LocalizationManager.Instance.getText("POST_LEVEL_MADE_PROGRESS");
		}
		else
		{
			//The player did not finish the current level. Encourage him.
			postLevelDescriptionText.text = LocalizationManager.Instance.getText("POST_LEVEL_LUCK_NEXT_TIME");
		}
		episodeKeysText.text = PlayerStatsManager.Instance.getNumberKeysFoundInEpisode( episodeNumber ) + "/" + selectedEpisode.numberOfChestKeys;

		starMeter.GetComponent<StarMeterHandler>().updateValues( selectedEpisode );
		
	}

	public void closePostLevelPopup()
	{
		SoundManager.playButtonClick();
		GameManager.Instance.setGameState(GameState.Menu);
		GetComponent<Animator>().Play("Panel Slide Out");	
	}

	public void retry()
	{
		Debug.Log("Retry button pressed: ");
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
