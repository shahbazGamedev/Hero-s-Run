using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StoryCompletedPopup : MonoBehaviour {

	[Header("Story Completed Popup")]
	public Text storyCompletedTitleText;
	public Text messageText;
	public Text startOverButtonText;
	public NewWorldMapHandler newWorldMapHandler;

	// Use this for initialization
	void Awake ()
	{
		storyCompletedTitleText.text = LocalizationManager.Instance.getText("STORY_COMPLETED_TITLE");
		messageText.text = LocalizationManager.Instance.getText("STORY_COMPLETED_MESSAGE");
		messageText.text = messageText.text.Replace("\\n", System.Environment.NewLine );
		startOverButtonText.text = LocalizationManager.Instance.getText("STORY_COMPLETED_START_OVER");
	}

	public void showStoryCompletedPopup()
	{
		GetComponent<Animator>().Play("Panel Slide In");
	}

	//Player pressed the X button
	public void close()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		GetComponent<Animator>().Play("Panel Slide Out");
	}

	//Player starts over
	//Reset everything except the number of stars earned
	public void startOver()
	{
		Debug.Log("startOver");
		UISoundManager.uiSoundManager.playButtonClick();

		LevelManager.Instance.setEpisodeChanged( false );
		LevelManager.Instance.setPlayerFinishedTheGame( false );
		LevelManager.Instance.setScore( 0 );
		LevelManager.Instance.setEpisodeCompleted( false );
		LevelManager.Instance.setHighestEpisodeCompleted( 0 );
		FacebookManager.Instance.postHighScore( 0 );

		PlayerStatsManager.Instance.resetDeathInEpisodes();
		PlayerStatsManager.Instance.resetTimesPlayerRevivedInLevel();
		PlayerStatsManager.Instance.resetTreasureKeysFound();
		PlayerStatsManager.Instance.savePlayerStats();

		newWorldMapHandler.drawLevelMarkers();

		GetComponent<Animator>().Play("Panel Slide Out");
		
	}

	
}
