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
	public NewWorldMapHandler newWorldMapHandler;

	Animator anim;
	int episodeNumber;
	LevelData levelData;

	// Use this for initialization
	void Awake () {
	
		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu
		#endif

		playButtonText.text = LocalizationManager.Instance.getText("MENU_PLAY");

		anim = GetComponent<Animator>(); //Used to play the slide-in/out animation

		//Get the episode data. Level data has the parameters for all the episodes and levels of the game.
		levelData = LevelManager.Instance.getLevelData();

	}

	public void showEpisodePopup( int episodeNumber )
	{
		this.episodeNumber = episodeNumber;
		loadEpisodeData();
		anim.Play("Panel Slide In");
	}

	private void loadEpisodeData()
	{
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

	}

	public void closeEpisodeMenu()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		anim.Play("Panel Slide Out");
	}

	public void play()
	{
		Debug.Log("EpisodePopup-Play button pressed: Episode: " + episodeNumber );
		UISoundManager.uiSoundManager.playButtonClick();
		newWorldMapHandler.play( episodeNumber );
	}
	
}
