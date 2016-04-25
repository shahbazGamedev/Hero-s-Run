using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewWorldMapHandler : MonoBehaviour {

	[Header("World Map Handler")]
	PopupHandler popupHandler;
	public Canvas worldCanvas;
	public RectTransform wc;
	public RectTransform map;
	public float mapWidth;
	public float mapHeight;
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

		mapWidth = map.rect.width;
		mapHeight = map.rect.height;
		levelLoading = false;

		updateTopPanelValues();

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
		GameObject starDisplayPrefab = Resources.Load( "Menu/Star Display") as GameObject;

		LevelData.LevelInfo levelInfo;
		for( int i=0; i < levelList.Count; i++ )
		{
			levelInfo = levelList[i];
			//the map coordinates correspond to the exact center of the shield
			drawLevelMarker( levelStationPrefab, levelInfo.MapCoordinates, i );
			drawDisplayStars( starDisplayPrefab, levelInfo.MapCoordinates, i );
		}
	}

	void drawLevelMarker( GameObject levelStationPrefab, Vector2 coord, int levelNumber )
	{
		GameObject go = (GameObject)Instantiate(levelStationPrefab);
		go.transform.SetParent(map.transform,false);
		go.name = "Level Station " + (levelNumber + 1).ToString();
		Button levelStationButton = go.GetComponent<Button>();
		RectTransform levelStationButtonRectTransform = levelStationButton.GetComponent<RectTransform>();
		levelStationButtonRectTransform.anchoredPosition = new Vector2( wc.rect.width * coord.x, mapHeight * (1f - coord.y) );
		levelStationButton.onClick.AddListener(() => levelButtonClick(levelNumber));
		Text levelStationText = levelStationButton.GetComponentInChildren<Text>();
		levelStationText.text = (levelNumber + 1).ToString();
	}

	void levelButtonClick( int levelNumber )
	{
		Debug.Log("Level Station click: " + levelNumber );
		SoundManager.playButtonClick();
		episodePopup.showEpisodePopup( levelNumber );
	}

	void drawDisplayStars(GameObject starDisplayPrefab, Vector2 coord, int episodeNumber)
	{
		GameObject go = (GameObject)Instantiate(starDisplayPrefab);
		go.transform.SetParent(map.transform,false);
		go.name = "Star Meter " + (episodeNumber + 1).ToString();
		RectTransform goRectTransform = go.GetComponent<RectTransform>();
		goRectTransform.anchoredPosition = new Vector2( wc.rect.width * coord.x, mapHeight * (1f - coord.y) + 50f);
		Image[] stars = go.GetComponentsInChildren<Image>();
		//numberOfStars is between 0 and 3
		int numberOfStars = PlayerStatsManager.Instance.getNumberDisplayStarsForEpisode( episodeNumber );
 		
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

	void OnGUI()
	{
		#if UNITY_EDITOR
		handleMouseMovement();
		#endif
	}

	//Top panel

	void updateTopPanelValues()
	{
		numberOfKeysText.text = PlayerStatsManager.Instance.getTreasureIslandKeys().ToString();
		numberOfLivesText.text = PlayerStatsManager.Instance.getLives().ToString();
		numberOfStarsText.text = PlayerStatsManager.Instance.getCurrentCoins().ToString("N0");

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

	void handleMouseMovement()
	{
		if(Event.current.type == EventType.MouseUp)
		{
			Debug.Log("Mouse Pos " +Event.current.mousePosition.x + " " +  Event.current.mousePosition.y + " " + wc.localScale.x + " " + wc.localScale.y );
			float coordW = Event.current.mousePosition.x;
			float coordH = Event.current.mousePosition.y;
			Debug.Log("Mouse coordinates-Percentage Width: " + ((coordW/wc.rect.width)/wc.localScale.x).ToString("N3") +  " Percentage Height: " + ( 1f - coordH/(mapHeight * wc.localScale.y) ).ToString("N3") );
		}
	}

	public void cheatShowPostLevelPopup()
	{
		postLevelPopupPanel.GetComponent<PostLevelPopup>().showPostLevelPopup(levelData);
	}


}
