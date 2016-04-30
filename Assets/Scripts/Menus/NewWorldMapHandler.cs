using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewWorldMapHandler : MonoBehaviour {

	[Header("World Map Handler")]
	PopupHandler popupHandler;
	public RectTransform map;
	bool levelLoading = false;
	private LevelData levelData;
	List<LevelData.LevelInfo> levelList;
	public Canvas settingsMenuCanvas;
	public Text numberOfKeysText;
	public Text numberOfLivesText;
	public Text numberOfStarsText;
	public string inviteFriendsCustomImageUri = "http://i.imgur.com/zkYlB.jpg";
	public Image playerPortrait;
	public Sprite defaultPortrait;
	[Header("Message Center")]
	public Text numberOfMessages;
	StoreManager storeManager;
	[Header("Facebook Ask Lives")]
	public GameObject  facebookAskLivesPanel;
	[Header("Episode Popup")]
	public GameObject episodePopupPanel;
	EpisodePopup episodePopup;
	[Header("Post-Level Popup")]
	public GameObject postLevelPopupPanel;
	[Header("Star Meter")]
	public Color32 starReceivedColor;
	[Header("Level Station Locations")]
	public RectTransform[] levelStationLocations = new RectTransform[15];



	void Awake ()
	{
		SceneManager.LoadScene( (int)GameScenes.Store, LoadSceneMode.Additive );

		GameObject CoreManagers = GameObject.FindGameObjectWithTag("CoreManagers");
		popupHandler = CoreManagers.GetComponent<PopupHandler>();

		episodePopup = episodePopupPanel.GetComponent<EpisodePopup>();

		//Get the level data. Level data has the parameters for all the levels of the game.
		levelData = LevelManager.Instance.getLevelData();
		levelList = levelData.getLevelList();
	}

	// Use this for initialization
	void Start ()
	{
		GameObject storeManagerObject = GameObject.FindGameObjectWithTag("Store");
		storeManager = storeManagerObject.GetComponent<StoreManager>();

		Handheld.StopActivityIndicator();

		//If we quit from the pause menu, the audio listener was paused.
		//We could set AudioListener.pause = false in the goToHomeMenu() but
		//that causes the level sound to play for a very brief moment before getting killed.
		//This is why we do it here instead.
		AudioListener.pause = false;

		levelLoading = false;

		numberOfMessages.text = (FacebookManager.Instance.AppRequestDataList.Count).ToString();
		//if we have a Facebook user portrait, use it, or else, use the default one.
		if( FacebookManager.Instance.UserPortrait == null )
		{
			playerPortrait.sprite = defaultPortrait;
		}
		else
		{
			playerPortrait.sprite = FacebookManager.Instance.UserPortrait;
		}

		drawLevelMarkers();

		if( GameManager.Instance.getGameState() == GameState.PostLevelPopup )
		{
			postLevelPopupPanel.GetComponent<PostLevelPopup>().showPostLevelPopup(levelData);
		}
	}

	void drawLevelMarkers()
	{
		GameObject levelStationPrefab = Resources.Load( "Menu/Level Button") as GameObject;
		GameObject episodeStationPrefab = Resources.Load( "Menu/Episode Button") as GameObject;
		GameObject starDisplayPrefab = Resources.Load( "Menu/Star Display") as GameObject;
		int episodeCounter = 0;
		LevelData.LevelInfo levelInfo;

		for( int i=0; i < levelList.Count; i++ )
		{
			levelInfo = levelList[i];
			if( levelInfo.levelType == LevelType.Episode )
			{
								drawEpisodeLevelMarker( episodeStationPrefab, Vector2.zero, i, episodeCounter );
				drawDisplayStars( starDisplayPrefab, Vector2.zero, i, episodeCounter );
				episodeCounter++;
			}
			else if( levelInfo.levelType == LevelType.Normal )
			{
				drawNormalLevelMarker( levelStationPrefab, Vector2.zero, i, episodeCounter );
			}
		}
	}

	void drawNormalLevelMarker( GameObject levelStationPrefab, Vector2 coord, int levelNumber, int episodeCounter )
	{
		GameObject go = (GameObject)Instantiate(levelStationPrefab);
		go.transform.SetParent(map.transform,false);
		go.name = "Level Station " + (levelNumber + 1).ToString();
		Button levelStationButton = go.GetComponent<Button>();
		RectTransform levelStationButtonRectTransform = levelStationButton.GetComponent<RectTransform>();
		levelStationButtonRectTransform.parent = levelStationLocations[levelNumber];
		levelStationButtonRectTransform.anchoredPosition = new Vector2( 0, 0 );
		levelStationButton.onClick.AddListener(() => levelButtonClick(episodeCounter-1, levelNumber));
		Text levelStationText = levelStationButton.GetComponentInChildren<Text>();
		levelStationText.text = (levelNumber + 1).ToString();
	}

	void levelButtonClick( int episodeNumber, int levelNumber )
	{
		Debug.Log("Level Station click-Episode: " + episodeNumber + " Level: " + levelNumber );
		LevelManager.Instance.EpisodeCurrentlyBeingPlayed = episodeNumber;
		SoundManager.playButtonClick();
		episodePopup.showEpisodePopup( episodeNumber, levelNumber );
	}

	void drawEpisodeLevelMarker( GameObject episodeStationPrefab, Vector2 coord, int levelNumber, int episodeCounter )
	{
		LevelData.EpisodeInfo episodeInfo = levelData.getEpisodeInfo( episodeCounter );
		GameObject go = (GameObject)Instantiate(episodeStationPrefab);
		go.transform.SetParent(map.transform,false);
		go.name = "Episode Station " + (episodeCounter + 1).ToString();
		Button levelStationButton = go.GetComponent<Button>();
		RectTransform levelStationButtonRectTransform = levelStationButton.GetComponent<RectTransform>();
		levelStationButtonRectTransform.parent = levelStationLocations[levelNumber];
		levelStationButtonRectTransform.anchoredPosition = new Vector2( 0, 0 );
		levelStationButton.onClick.AddListener(() => levelButtonClick(episodeCounter, levelNumber));
		Text[] episodeStationTexts = levelStationButton.GetComponentsInChildren<Text>();
		episodeStationTexts[0].text = (levelNumber + 1).ToString();
		episodeStationTexts[1].text = getEpisodeDifficultyText(  episodeInfo.episodeDifficulty );
	}

	void drawDisplayStars(GameObject starDisplayPrefab, Vector2 coord, int levelNumber, int episodeCounter )
	{
		GameObject go = (GameObject)Instantiate(starDisplayPrefab);
		go.transform.SetParent(map.transform,false);
		go.name = "Star Meter " + (episodeCounter + 1).ToString();
		RectTransform goRectTransform = go.GetComponent<RectTransform>();
		goRectTransform.parent = levelStationLocations[levelNumber];
		goRectTransform.anchoredPosition = new Vector2( 0, 55f );
		Image[] stars = go.GetComponentsInChildren<Image>();
		//numberOfStars is between 0 and 3
		int numberOfStars = PlayerStatsManager.Instance.getNumberDisplayStarsForEpisode( episodeCounter );
 		
		switch (numberOfStars)
		{
			case 0:
			    //Do nothing
				break;
			case 1:
				stars[0].color = starReceivedColor;
				break;
			case 2:
				stars[0].color = starReceivedColor;
				stars[1].color = starReceivedColor;
				break;
			case 3:
				stars[0].color = starReceivedColor;
				stars[1].color = starReceivedColor;
				stars[2].color = starReceivedColor;
				break;
		}
 	}

	public string getEpisodeDifficultyText( EpisodeDifficulty episodeDifficulty )
	{
		return LocalizationManager.Instance.getText( "EPISODE_DIFFICULTY_" + episodeDifficulty.ToString().ToUpper() );
	}

	public void showTitleScreen()
	{
		SoundManager.playButtonClick();
		StartCoroutine( loadTitleScreen() );
	}

	IEnumerator loadTitleScreen()
	{
		if( !levelLoading )
		{
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)GameScenes.TitleScreen );
		}	
	}

	public void showStoreScreen()
	{
		SoundManager.playButtonClick();
		storeManager.showStore( StoreTab.Store );
	}

	public void showShopScreen()
	{
		SoundManager.playButtonClick();
		storeManager.showStore( StoreTab.Shop );
	}

	//Middle Panel
	public void showMessageCenter()
	{
		SoundManager.playButtonClick();
		popupHandler.setPopupSize(new Vector2( Screen.width * 0.8f, Screen.height * 0.55f ));
		popupHandler.activatePopup( PopupType.MessageCenter );
	}

	//Bottom panel
	public void showInviteFriends()
	{
		SoundManager.playButtonClick();
		FacebookManager.Instance.inviteFriends( inviteFriendsCustomImageUri );
	}

	public void showCharacterGallery()
	{
		SoundManager.playButtonClick();
		StartCoroutine( loadCharacterGallery() );
	}

	IEnumerator loadCharacterGallery()
	{
		if( !levelLoading )
		{
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)GameScenes.CharacterGallery );
		}	
	}

	public void showAskLivesPopup()
	{
		SoundManager.playButtonClick();
		facebookAskLivesPanel.GetComponent<Animator>().Play("Panel Slide In");
	}

	public void hideAskLivesPopup()
	{
		SoundManager.playButtonClick();
		facebookAskLivesPanel.GetComponent<Animator>().Play("Panel Slide Out");
	}

	public void showSettingsMenu()
	{
		SoundManager.playButtonClick();
		settingsMenuCanvas.GetComponent<SettingsMenu>().showSettingsMenu();
	}

	//Treasure island
	public void showTreasureIsland()
	{
		SoundManager.playButtonClick();
		StartCoroutine( loadTreasureIsland() );
	}

	IEnumerator loadTreasureIsland()
	{
		if( !levelLoading )
		{
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)GameScenes.TreasureIsland );
		}	
	}

	public void cheatShowPostLevelPopup()
	{
		postLevelPopupPanel.GetComponent<PostLevelPopup>().showPostLevelPopup(levelData);
	}


}
