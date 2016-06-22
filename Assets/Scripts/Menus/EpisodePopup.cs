using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EpisodePopup : MonoBehaviour {


	[Header("Episode Popup")]
	public Text episodeNumberText;
	public Text episodeNameText;
	public Image episodeImage;
	public Text episodeDescriptionText;
	public Text episodeKeysText; //format found/total e.g. 1/3
	public Text playButtonText;
	[Tooltip("If a sprite for the selected episode is not specified in LevelData, this sprite will be used instead.")]
	public Sprite defaultEpisodeSprite;

	Animator anim;
	bool levelLoading = false;
	int episodeNumber;
	int levelNumber;
	LevelData levelData;
	ClockTimeSetter clockTimeSetter;


	// Use this for initialization
	void Awake () {
	
		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu
		#endif

		playButtonText.text = LocalizationManager.Instance.getText("MENU_PLAY");

		anim = GetComponent<Animator>(); //Used to play the slide-in/out animation

		//Get the episode data. Level data has the parameters for all the episodes and levels of the game.
		levelData = LevelManager.Instance.getLevelData();

		clockTimeSetter = GetComponentInChildren<ClockTimeSetter>();

	}

	public void showEpisodePopup( int episodeNumber, int levelNumber )
	{
		SoundManager.playButtonClick();
		this.episodeNumber = episodeNumber;
		this.levelNumber = levelNumber;
		loadEpisodeData();
		anim.Play("Panel Slide In");
	}

	private void loadEpisodeData()
	{
		//Reset value in case a player who did previously finish the game, replays earlier levels
		LevelManager.Instance.setPlayerFinishedTheGame( false );

		LevelData.EpisodeInfo selectedEpisode = levelData.getEpisodeInfo( episodeNumber );
		string levelNumberString = (episodeNumber + 1).ToString();

		string episodeNumberString = LocalizationManager.Instance.getText("EPISODE_NUMBER");

		//Replace the string <number> by the number of the episode
		episodeNumberString = episodeNumberString.Replace( "<number>", levelNumberString );

		episodeNumberText.text = episodeNumberString;
		episodeNameText.text = LocalizationManager.Instance.getText("EPISODE_NAME_" + levelNumberString );
		if( selectedEpisode.preLevelSprite == null )
		{
			episodeImage.sprite = defaultEpisodeSprite;
		}
		else
		{
			episodeImage.sprite = selectedEpisode.preLevelSprite;
		}
		episodeDescriptionText.text = LocalizationManager.Instance.getText("EPISODE_DESCRIPTION_" + levelNumberString);
		episodeKeysText.text = PlayerStatsManager.Instance.getNumberKeysFoundInEpisode( episodeNumber ) + "/" + selectedEpisode.numberOfChestKeys;
		//When you restart an episode, the number of deaths for that episode and all subsequent episodes are reset
		LevelData.LevelInfo level = LevelManager.Instance.getLevelInfo( levelNumber );
		if( level.levelType == LevelType.Episode )
		{
			PlayerStatsManager.Instance.resetNumberDeathsStartingAtEpisode( episodeNumber );
		}

		//Update pocket watch and Time Left
		clockTimeSetter.updateTime( episodeNumber, levelNumber, Level_Progress.LEVEL_START );
	}

	public void closeEpisodeMenu()
	{
		SoundManager.playButtonClick();
		anim.Play("Panel Slide Out");
	}

	public void play()
	{
		Debug.Log("Play button pressed: Episode: " + episodeNumber + " Level: " + levelNumber );
		SoundManager.playButtonClick();
		//We are starting a new run, reset some values
		LevelManager.Instance.setScore( 0 );
		LevelManager.Instance.setEpisodeCompleted( false );
		LevelManager.Instance.forceNextLevelToComplete( levelNumber );
		PlayerStatsManager.Instance.resetTimesPlayerRevivedInLevel();

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
