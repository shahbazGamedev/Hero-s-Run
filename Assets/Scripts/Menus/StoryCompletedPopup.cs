using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StoryCompletedPopup : MonoBehaviour {

	[Header("Story Completed Popup")]
	public Text storyCompletedTitleText;
	public Text messageText;
	public Text difficultyLevelLabel;
	public Text changeDifficultyButtonText;
	public Text startOverButtonText;
	public NewWorldMapHandler newWorldMapHandler;

	// Use this for initialization
	void Awake ()
	{
		storyCompletedTitleText.text = LocalizationManager.Instance.getText("STORY_COMPLETED_TITLE");
		if( PlayerStatsManager.Instance.getDifficultyLevel() == DifficultyLevel.Legendary )
		{
			messageText.text = LocalizationManager.Instance.getText("STORY_COMPLETED_MESSAGE_AT_LEGENDARY");
		}
		else
		{
			messageText.text = LocalizationManager.Instance.getText("STORY_COMPLETED_MESSAGE_NOT_LEGENDARY");
		}
		messageText.text = messageText.text.Replace("\\n", System.Environment.NewLine );

		difficultyLevelLabel.text = LocalizationManager.Instance.getText("STORY_COMPLETED_DIFFICULTY_LEVEL_LABEL");
		changeDifficultyButtonText.text = PlayerStatsManager.Instance.getDifficultyLevelName();
		startOverButtonText.text = LocalizationManager.Instance.getText("STORY_COMPLETED_START_OVER");
	}

	public void showStoryCompletedPopup()
	{
		GetComponent<Animator>().Play("Panel Slide In");
	}

	//Player pressed the X button
	public void close()
	{
		SoundManager.soundManager.playButtonClick();
		GetComponent<Animator>().Play("Panel Slide Out");
	}

	//Player changes the difficulty level
	public void changeDifficultyLevel()
	{
		Debug.Log("changeDifficultyLevel");
		SoundManager.soundManager.playButtonClick();
		DifficultyLevel newDifficultyLevel = PlayerStatsManager.Instance.getNextDifficultyLevel();
		//setDifficultyLevel takes care of saving the new value
		PlayerStatsManager.Instance.setDifficultyLevel(newDifficultyLevel);
		changeDifficultyButtonText.text = PlayerStatsManager.Instance.getDifficultyLevelName();
	}

	//Player starts over
	//Reset everything except the number of stars earned
	public void startOver()
	{
		Debug.Log("startOver");
		SoundManager.soundManager.playButtonClick();

		LevelManager.Instance.setLevelChanged( false );
		LevelManager.Instance.setPlayerFinishedTheGame( false );
		LevelManager.Instance.setScore( 0 );
		LevelManager.Instance.setEpisodeCompleted( false );
		LevelManager.Instance.setHighestLevelCompleted( 0 );
		LevelManager.Instance.forceHighestLevelCompleted( 0 );

		PlayerStatsManager.Instance.resetDeathInLevels();
		PlayerStatsManager.Instance.resetTimesPlayerRevivedInLevel();
		PlayerStatsManager.Instance.resetTreasureKeysFound();
		PlayerStatsManager.Instance.savePlayerStats();

		newWorldMapHandler.drawLevelMarkers();

		GetComponent<Animator>().Play("Panel Slide Out");
		
	}

	
}
