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
	public Canvas settingsMenuCanvas;
	public Text numberOfKeysText;
	public Text numberOfLivesText;
	public Text numberOfStarsText;
	public string inviteFriendsCustomImageUri = "http://i.imgur.com/zkYlB.jpg";
	public Image playerPortrait;
	[Header("Message Center")]
	public GameObject messageCenterPanel;
	public Text numberOfMessages;
	StoreManager storeManager;
	[Header("Facebook Ask Lives")]
	public GameObject facebookAskLivesPanel;
	[Header("Facebook Offer Lives")]
	public GameObject facebookOfferLivesPanel;
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

		playerPortrait.GetComponent<FacebookPortraitHandler>().setPlayerPortrait();

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

		//Get all of the user's outstanding app requests right away and then poll Facebook regularly. AppRequests also get refreshed after an app resume.
		CancelInvoke("getAllAppRequests");
		InvokeRepeating("getAllAppRequests", 0, 60 );
		//The score of the player's friends gets refreshed when loading the scene and if the app resume after being paused.
		getUpdatedScores();
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
		List<LevelData.EpisodeInfo> episodeList = levelData.episodeList;

		for( int i=0; i < episodeList.Count; i++ )
		{
			drawEpisodeLevelMarker( i );
			drawDisplayStars( i );
		}
	}

	public void updateFriendPortraits()
	{
		for( int i=0; i < facebookPortraitList.Count; i++ )
		{
			facebookPortraitList[i].setFriendPortrait();
		}
	}

	public void updatePlayerPortrait()
	{
		for( int i=0; i < episodeStationLocations.Length; i++ )
		{
			RectTransform episodeStationLocationRect = episodeStationLocations[i];
			Button episodeStationButton = episodeStationLocationRect.FindChild("Episode Station(Clone)").GetComponent<Button>();
			if ( i == LevelManager.Instance.getCurrentEpisodeNumber() )
			{
				//This is the current episode. Enable outline.
				episodeStationButton.GetComponent<Outline>().enabled = true;
				nextLevelToPlayGlowingOutline = episodeStationButton.GetComponent<Outline>();
	
				//Position the player portrait on the right-hand side of the level station
				RectTransform buttonRectTransform = episodeStationButton.GetComponent<RectTransform>();
				playerPortrait.rectTransform.SetParent( buttonRectTransform );
				playerPortrait.rectTransform.anchoredPosition = new Vector2( buttonRectTransform.anchoredPosition.x - 57.2f, buttonRectTransform.anchoredPosition.y -14f );
			}
			else
			{
				episodeStationButton.GetComponent<Outline>().enabled = false;
			}
		}
	}

	void episodeButtonClick( int episodeNumber )
	{
		Debug.Log("Episode Station click-Episode: " + episodeNumber );
		UISoundManager.uiSoundManager.playButtonClick();
		if( GameManager.Instance.getGameMode() == GameMode.Story )
		{
			LevelManager.Instance.setCurrentEpisodeNumber( episodeNumber );
			episodePopup.showEpisodePopup( episodeNumber );
		}
		else
		{
			play( episodeNumber );
		}
	}

	void drawEpisodeLevelMarker( int episodeNumber )
	{
		GameObject go = (GameObject)Instantiate(episodeStationPrefab);
		go.transform.SetParent(map.transform,false);
		Button levelStationButton = go.GetComponent<Button>();
		RectTransform levelStationButtonRectTransform = levelStationButton.GetComponent<RectTransform>();
		levelStationButtonRectTransform.SetParent( episodeStationLocations[episodeNumber], false );
		levelStationButtonRectTransform.anchoredPosition = new Vector2( 0, 0 );
		levelStationButton.onClick.AddListener(() => episodeButtonClick(episodeNumber));
		Text[] episodeStationTexts = levelStationButton.GetComponentsInChildren<Text>();
		episodeStationTexts[0].text = (episodeNumber + 1).ToString();
		string levelNumberString = (episodeNumber + 1).ToString();
		episodeStationTexts[1].text = LocalizationManager.Instance.getText("EPISODE_NAME_" + levelNumberString );

		if( episodeNumber > LevelManager.Instance.getHighestEpisodeCompleted() )
		{
			//Level is not unlocked yet. Make button non-interactable and dim the level number text
			levelStationButton.interactable = false;
			episodeStationTexts[0].enabled = false;
			episodeStationTexts[1].enabled = true;
		}
		if ( episodeNumber == LevelManager.Instance.getCurrentEpisodeNumber() )
		{
			//This is the current episode. Enable outline.
			levelStationButton.GetComponent<Outline>().enabled = true;
			nextLevelToPlayGlowingOutline = levelStationButton.GetComponent<Outline>();

			//Position the player portrait on the right-hand side of the level station
			playerPortrait.rectTransform.SetParent( levelStationButtonRectTransform );
			playerPortrait.rectTransform.anchoredPosition = new Vector2( levelStationButtonRectTransform.anchoredPosition.x - 57.2f, levelStationButtonRectTransform.anchoredPosition.y -14f );
		}
		prepareFriendPicture( levelStationButtonRectTransform, episodeNumber );
	}

	//Set up data for friend picture, which sits to the right of the shield
	void prepareFriendPicture( RectTransform levelStationButtonRectTransform, int episodeCounter )
	{
		FacebookPortraitHandler fph = levelStationButtonRectTransform.FindChild("Friend Portrait").GetComponent<FacebookPortraitHandler>();
		fph.episodeNumber = episodeCounter;
		facebookPortraitList.Add( fph );
	}

	void drawDisplayStars( int episodeNumber )
	{
		if( episodeNumber <= LevelManager.Instance.getHighestEpisodeCompleted() )
		{
			//Level is unlocked so show the stars.
			GameObject go = (GameObject)Instantiate(starDisplayPrefab);
			go.transform.SetParent(map.transform,false);
			go.name = "Star Meter " + (episodeNumber + 1).ToString();
			RectTransform goRectTransform = go.GetComponent<RectTransform>();
			goRectTransform.SetParent( episodeStationLocations[episodeNumber], false );
			goRectTransform.anchoredPosition = new Vector2( 0, 55f );
			//Store it so we can easily update the stars later
			starLocations[episodeNumber] = goRectTransform;
			//numberOfStars is between 0 and 3
			int numberOfStars = PlayerStatsManager.Instance.getNumberDisplayStarsForEpisode( episodeNumber );
			updateDisplayStars( episodeNumber, numberOfStars );
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
		UISoundManager.uiSoundManager.playButtonClick();
		storeManager.showStore( StoreTab.Store, StoreReason.None );
	}

	public void showShopScreen()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		storeManager.showStore( StoreTab.Shop, StoreReason.None );
	}

	//Middle Panel
	public void showMessageCenter()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		messageCenterPanel.GetComponent<MessageManager>().refreshMessages();
		messageCenterPanel.GetComponent<Animator>().Play("Panel Slide In");
	}

	//Bottom panel
	public void showInviteFriends()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		FacebookManager.Instance.inviteFriends( inviteFriendsCustomImageUri );
	}

	public void showCharacterGallery()
	{
		UISoundManager.uiSoundManager.playButtonClick();
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
		UISoundManager.uiSoundManager.playButtonClick();
		facebookAskLivesPanel.GetComponent<Animator>().Play("Panel Slide In");
	}

	public void hideAskLivesPopup()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		facebookAskLivesPanel.GetComponent<Animator>().Play("Panel Slide Out");
	}

	public void showOfferLivesPopup()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		facebookOfferLivesPanel.GetComponent<Animator>().Play("Panel Slide In");
	}

	public void hideOfferLivesPopup()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		facebookOfferLivesPanel.GetComponent<Animator>().Play("Panel Slide Out");
	}

	public void showSettingsMenu()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		settingsMenuCanvas.GetComponent<SettingsMenu>().showSettingsMenu();
	}

	//Treasure island
	public void showTreasureIsland()
	{
		UISoundManager.uiSoundManager.playButtonClick();
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
		UISoundManager.uiSoundManager.playButtonClick();
		//postLevelPopupPanel.GetComponent<PostLevelPopup>().showPostLevelPopup(levelData);
		GameObject CoreManagers = GameObject.FindGameObjectWithTag("CoreManagers");
		CoreManagers.GetComponent<NotificationServicesHandler>().sendTestLocalNotification();
		PlayerStatsManager.Instance.resetDeathInEpisodes();
		PlayerStatsManager.Instance.resetTimesPlayerRevivedInLevel();
		PlayerStatsManager.Instance.resetTreasureKeysFound();
		PlayerStatsManager.Instance.setChallenges(string.Empty);
		PlayerStatsManager.Instance.savePlayerStats();
		showOfferLivesPopup();
	}

	public void toggleGameMode()
	{
		UISoundManager.uiSoundManager.playButtonClick();
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
		updateFriendPortraits();
	}

	public void play( int episodeNumber )
	{
		//Reset the level changed value
		LevelManager.Instance.setEpisodeChanged( false );

		//Reset value in case a player who did previously finish the game, replays earlier levels
		LevelManager.Instance.setPlayerFinishedTheGame( false );

		//When you restart an episode, the number of deaths for that episode and all subsequent episodes are reset
		PlayerStatsManager.Instance.resetNumberDeathsStartingAtEpisode( episodeNumber );
		PlayerStatsManager.Instance.resetTimesPlayerRevivedInLevel();
		//We are starting a new run, reset some values
		LevelManager.Instance.setEnableTorches( true );
		LevelManager.Instance.setScore( 0 );
		PlayerStatsManager.Instance.resetDistanceTravelled();
		LevelManager.Instance.setCurrentEpisodeNumber( episodeNumber );
		LevelManager.Instance.setEpisodeCompleted( false );
		LevelManager.Instance.forceNextEpisodeToComplete( episodeNumber );
		LevelManager.Instance.resetNumberOfCheckpointsPassed();
		LevelManager.Instance.resetStarsAtLastCheckpoint();

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

	//When the application resumes, the game will refresh both the friend portraits and the message center.
	void OnApplicationPause( bool pauseStatus )
	{
		if( !pauseStatus  )
		{
			//Get all of the user's outstanding app requests right away
			getAllAppRequests();
			//Get the score of the player's friends right away
			getUpdatedScores();
		}
	}

}
