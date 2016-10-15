using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewWorldMapHandler : MonoBehaviour {

	[Header("World Map Handler")]
	public RectTransform map;
	bool levelLoading = false;
	private LevelData levelData;
	List<LevelData.LevelInfo> levelList;
	public Canvas settingsMenuCanvas;
	public Text numberOfKeysText;
	public Text numberOfLivesText;
	public Text numberOfStarsText;
	public string inviteFriendsCustomImageUri = "http://i.imgur.com/zkYlB.jpg";
	[Header("Message Center")]
	public GameObject messageCenterPanel;
	public Text numberOfMessages;
	StoreManager storeManager;
	[Header("Facebook Ask Lives")]
	public GameObject facebookAskLivesPanel;
	[Header("Episode Popup")]
	public GameObject episodePopupPanel;
	EpisodePopup episodePopup;
	[Header("Post-Level Popup")]
	public GameObject postLevelPopupPanel;
	[Header("Endless Mode - Post-Level Popup")]
	public GameObject endlessPostLevelPopupPanel;
	[Header("Social Media Popup")]
	public GameObject socialMediaPopupPanel;
	[Header("Game Mode Button")]
	public Text gameModeButtonText;
	[Header("Star Meter")]
	public Color32 starReceivedColor;
	[Header("Episode Station Locations")]
	public RectTransform[] episodeStationLocations = new RectTransform[LevelData.NUMBER_OF_EPISODES];
	[Header("Star Locations")]
	//Array of the RectTransforms holding the three stars. Index is the episode number
	public RectTransform[] starLocations = new RectTransform[LevelData.NUMBER_OF_EPISODES];
	[Header("Menu Prefabs")]
	public GameObject episodeStationPrefab;
	public GameObject starDisplayPrefab;

	public List<FacebookPortraitHandler> facebookPortraitList = new List<FacebookPortraitHandler>( LevelData.NUMBER_OF_EPISODES );
	private Outline nextLevelToPlayGlowingOutline;
	private const float OUTLINE_FLASH_SPEED = 2.25f;


	void Awake ()
	{
		SceneManager.LoadScene( (int)GameScenes.Store, LoadSceneMode.Additive );

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

		if( GameManager.Instance.getGameMode() == GameMode.Story )
		{
			gameModeButtonText.text = LocalizationManager.Instance.getText("MENU_GAME_MODE_STORY");
		}
		else
		{
			gameModeButtonText.text = LocalizationManager.Instance.getText("MENU_GAME_MODE_ENDLESS");
		}

		drawLevelMarkers();

		updateFriendPortraits();

		if( TakeScreenshot.selfieTaken )
		{
			socialMediaPopupPanel.GetComponent<SocialMediaPopup>().showSocialMediaPopup();
		}
		else
		{
			if( GameManager.Instance.getGameState() == GameState.PostLevelPopup )
			{
				if( GameManager.Instance.getGameMode() == GameMode.Story )
				{
					postLevelPopupPanel.GetComponent<PostLevelPopup>().showPostLevelPopup(levelData);
				}
				else
				{
					endlessPostLevelPopupPanel.GetComponent<EndlessPostLevelPopup>().showEndlessPostLevelPopup(levelData);
				}
			}
		}

		//Get all of the user's outstanding app requests right away and then poll Facebook regularly
		CancelInvoke("getAllAppRequests");
		InvokeRepeating("getAllAppRequests", 0, 30 );
		//Get the score of the player's friends on a regular basis
		CancelInvoke("getUpdatedScores");
		InvokeRepeating("getUpdatedScores", 5, 20 );

	}

	void getAllAppRequests()
	{
		FacebookManager.Instance.getAllAppRequests();
	}

	void getUpdatedScores()
	{
		FacebookManager.Instance.QueryScores();
	}

	void Update()
	{
		//Outline color is white.
		if( nextLevelToPlayGlowingOutline != null ) nextLevelToPlayGlowingOutline.effectColor = new Color(1f,1f,1f,(Mathf.Sin(Time.time * OUTLINE_FLASH_SPEED ) + 1f)/ 2f);

	}

	public void drawLevelMarkers()
	{
		int episodeCounter = 0;
		LevelData.LevelInfo levelInfo;

		for( int i=0; i < levelList.Count; i++ )
		{
			levelInfo = levelList[i];
			if( levelInfo.levelType == LevelType.Episode )
			{
				drawEpisodeLevelMarker( i, episodeCounter );
				drawDisplayStars( i, episodeCounter );
				episodeCounter++;
			}
		}
	}

	public void updateFriendPortraits()
	{
		for( int i=0; i < facebookPortraitList.Count; i++ )
		{
			facebookPortraitList[i].setPortrait();
		}
	}

	void levelButtonClick( int episodeNumber, int levelNumber )
	{
		Debug.Log("Level Station click-Episode: " + episodeNumber + " Level: " + levelNumber );
		SoundManager.soundManager.playButtonClick();
		if( GameManager.Instance.getGameMode() == GameMode.Story )
		{
			LevelManager.Instance.setCurrentEpisodeNumber( episodeNumber );
			episodePopup.showEpisodePopup( episodeNumber, levelNumber );
		}
		else
		{
			play( episodeNumber, levelNumber );
		}
	}

	void drawEpisodeLevelMarker( int levelNumber, int episodeCounter )
	{
		GameObject go = (GameObject)Instantiate(episodeStationPrefab);
		go.transform.SetParent(map.transform,false);
		go.name = "Episode Station " + (episodeCounter + 1).ToString();
		Button levelStationButton = go.GetComponent<Button>();
		RectTransform levelStationButtonRectTransform = levelStationButton.GetComponent<RectTransform>();
		levelStationButtonRectTransform.SetParent( episodeStationLocations[episodeCounter], false );
		levelStationButtonRectTransform.anchoredPosition = new Vector2( 0, 0 );
		levelStationButton.onClick.AddListener(() => levelButtonClick(episodeCounter, levelNumber));
		Text[] episodeStationTexts = levelStationButton.GetComponentsInChildren<Text>();
		episodeStationTexts[0].text = (episodeCounter + 1).ToString();
		string levelNumberString = (episodeCounter + 1).ToString();
		episodeStationTexts[1].text = LocalizationManager.Instance.getText("EPISODE_NAME_" + levelNumberString );

		//Get a reference to the object holding the player portrait.
		//Remember that each episode station has a player portrait component.
		Transform playerPortrait = levelStationButtonRectTransform.FindChild("Player Portrait");
		//Let's hide it in case this is not the episode the player is currently playing.
		playerPortrait.gameObject.SetActive( false );
		if( levelNumber > LevelManager.Instance.getHighestLevelCompleted() )
		{
			//Level is not unlocked yet. Make button non-interactable and dim the level number text
			levelStationButton.interactable = false;
			episodeStationTexts[0].enabled = false;
			episodeStationTexts[1].enabled = true;
		}
		if ( episodeCounter == LevelManager.Instance.getCurrentEpisodeNumber() )
		{
			//This is the current episode. Enable outline.
			levelStationButton.GetComponent<Outline>().enabled = true;
			nextLevelToPlayGlowingOutline = levelStationButton.GetComponent<Outline>();

			//Display the player portrait on the right-hand side of the episode station
			playerPortrait.gameObject.SetActive( true );
			FacebookPortraitHandler fph = playerPortrait.GetComponent<FacebookPortraitHandler>();
			fph.setPlayerPortrait();
		}
		prepareFriendPicture( levelStationButtonRectTransform, levelNumber );
	}

	//Set up data for friend picture, which sits to the right of the shield
	void prepareFriendPicture( RectTransform levelStationButtonRectTransform, int episodeCounter )
	{
		FacebookPortraitHandler fph = levelStationButtonRectTransform.FindChild("Friend Portrait").GetComponent<FacebookPortraitHandler>();
		fph.episodeNumber = episodeCounter;
		facebookPortraitList.Add( fph );
	}

	void drawDisplayStars( int levelNumber, int episodeCounter )
	{
		if( levelNumber <= LevelManager.Instance.getHighestLevelCompleted() )
		{
			//Level is unlocked so show the stars.
			GameObject go = (GameObject)Instantiate(starDisplayPrefab);
			go.transform.SetParent(map.transform,false);
			go.name = "Star Meter " + (episodeCounter + 1).ToString();
			RectTransform goRectTransform = go.GetComponent<RectTransform>();
			goRectTransform.SetParent( episodeStationLocations[episodeCounter], false );
			goRectTransform.anchoredPosition = new Vector2( 0, 55f );
			//Store it so we can easily update the stars later
			starLocations[episodeCounter] = goRectTransform;
			//numberOfStars is between 0 and 3
			int numberOfStars = PlayerStatsManager.Instance.getNumberDisplayStarsForEpisode( episodeCounter );
			updateDisplayStars( episodeCounter, numberOfStars );
		}
	}

	public void updateDisplayStars( int episodeNumber, int numberOfStars )
	{
		Image[] stars = starLocations[episodeNumber].GetComponentsInChildren<Image>();
	
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

	public void showStoreScreen()
	{
		SoundManager.soundManager.playButtonClick();
		storeManager.showStore( StoreTab.Store, StoreReason.None );
	}

	public void showShopScreen()
	{
		SoundManager.soundManager.playButtonClick();
		storeManager.showStore( StoreTab.Shop, StoreReason.None );
	}

	//Middle Panel
	public void showMessageCenter()
	{
		SoundManager.soundManager.playButtonClick();
		messageCenterPanel.GetComponent<MessageManager>().refreshMessages();
		messageCenterPanel.GetComponent<Animator>().Play("Panel Slide In");
	}

	//Bottom panel
	public void showInviteFriends()
	{
		SoundManager.soundManager.playButtonClick();
		FacebookManager.Instance.inviteFriends( inviteFriendsCustomImageUri );
	}

	public void showCharacterGallery()
	{
		SoundManager.soundManager.playButtonClick();
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
		SoundManager.soundManager.playButtonClick();
		facebookAskLivesPanel.GetComponent<Animator>().Play("Panel Slide In");
	}

	public void hideAskLivesPopup()
	{
		SoundManager.soundManager.playButtonClick();
		facebookAskLivesPanel.GetComponent<Animator>().Play("Panel Slide Out");
	}

	public void showSettingsMenu()
	{
		SoundManager.soundManager.playButtonClick();
		settingsMenuCanvas.GetComponent<SettingsMenu>().showSettingsMenu();
	}

	//Treasure island
	public void showTreasureIsland()
	{
		SoundManager.soundManager.playButtonClick();
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

	void OnEnable()
	{
		FacebookManager.appRequestsReceived += AppRequestsReceived;
		FacebookManager.facebookScoresReceived += FacebookScoresReceived;
	}

	void OnDisable()
	{
		FacebookManager.appRequestsReceived -= AppRequestsReceived;
		FacebookManager.facebookScoresReceived -= FacebookScoresReceived;
	}

	void AppRequestsReceived( int appRequestsCount )
	{
		numberOfMessages.text = appRequestsCount.ToString();
	}

	void FacebookScoresReceived()
	{
		updateFriendPortraits();
	}

	public void cheatButton()
	{
		Debug.Log("cheatButton called.");
		//postLevelPopupPanel.GetComponent<PostLevelPopup>().showPostLevelPopup(levelData);
		GameObject CoreManagers = GameObject.FindGameObjectWithTag("CoreManagers");
		CoreManagers.GetComponent<NotificationServicesHandler>().sendTestLocalNotification();
		PlayerStatsManager.Instance.resetDeathInLevels();
		PlayerStatsManager.Instance.resetTimesPlayerRevivedInLevel();
		PlayerStatsManager.Instance.resetTreasureKeysFound();
		PlayerStatsManager.Instance.setChallenges(string.Empty);
		PlayerStatsManager.Instance.savePlayerStats();
	}

	public void toggleGameMode()
	{
		SoundManager.soundManager.playButtonClick();
		if( GameManager.Instance.getGameMode() == GameMode.Story )
		{
			gameModeButtonText.text = LocalizationManager.Instance.getText("MENU_GAME_MODE_ENDLESS");
			GameManager.Instance.setGameMode(GameMode.Endless);
		}
		else
		{
			gameModeButtonText.text = LocalizationManager.Instance.getText("MENU_GAME_MODE_STORY");
			GameManager.Instance.setGameMode(GameMode.Story);
		}
		PlayerStatsManager.Instance.savePlayerStats();
	}

	public void play( int episodeNumber, int levelNumber )
	{
		//Reset the level changed value
		LevelManager.Instance.setLevelChanged( false );

		//Reset value in case a player who did previously finish the game, replays earlier levels
		LevelManager.Instance.setPlayerFinishedTheGame( false );

		//When you restart an episode, the number of deaths for that episode and all subsequent episodes are reset
		LevelData.LevelInfo level = LevelManager.Instance.getLevelInfo( levelNumber );
		if( level.levelType == LevelType.Episode )
		{
			PlayerStatsManager.Instance.resetNumberDeathsStartingAtLevel( levelNumber );
		}
		PlayerStatsManager.Instance.resetTimesPlayerRevivedInLevel();
		//We are starting a new run, reset some values
		LevelManager.Instance.setEnableTorches( true );
		LevelManager.Instance.setScore( 0 );
		PlayerStatsManager.Instance.resetDistanceTravelled();
		LevelManager.Instance.setCurrentEpisodeNumber( episodeNumber );
		LevelManager.Instance.setEpisodeCompleted( false );
		LevelManager.Instance.forceHighestLevelCompleted( levelNumber );
		LevelManager.Instance.forceNextLevelToComplete( levelNumber );

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
