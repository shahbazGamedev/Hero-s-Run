using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PostLevelPopup : MonoBehaviour {


	[Header("Post Level Popup")]
	public Text episodeNumberText;
	public Text episodeNameText;
	public Image episodeImage;
	public Text postLevelDescriptionText;
	public Text episodeKeysText; //format found/total e.g. 1/3
	public Text retryButtonText;
	[Tooltip("If a sprite for the selected episode is not specified in LevelData, this sprite will be used instead.")]
	public Sprite defaultEpisodeSprite;

	bool levelLoading = false;

	// Use this for initialization
	void Awake () {

		retryButtonText.text = LocalizationManager.Instance.getText("MENU_RETRY");
	}

	public void showPostLevelPopup(LevelData levelData)
	{	
		loadEpisodeData(levelData);
		gameObject.SetActive(true);	
	}

	private void loadEpisodeData(LevelData levelData)
	{
		int levelNumber = LevelManager.Instance.getNextLevelToComplete();

		LevelData.EpisodeInfo selectedEpisode = levelData.getEpisodeInfo( levelNumber );
		string levelNumberString = (levelNumber + 1).ToString();

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
		postLevelDescriptionText.text = LocalizationManager.Instance.getText("MENU_BETTER_LUCK_NEXT_TIME");
		episodeKeysText.text = "0" + "/" + selectedEpisode.numberOfChestKeys;
	}

	public void closePostLevelPopup()
	{
		SoundManager.playButtonClick();
		GameManager.Instance.setGameState(GameState.Menu);
		gameObject.SetActive(false);	
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
