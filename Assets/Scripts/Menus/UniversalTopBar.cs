using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UniversalTopBar : MonoBehaviour {

	public static UniversalTopBar Instance;
	const float COIN_STORE_VERTICAL_POSITION = 0;
	const float GEM_STORE_VERTICAL_POSITION = 1174f;

	[Header("For Store Access")]
	RectTransform horizontalContent;
	RectTransform storeVerticalContent;
	[Header("Holder Panel")]
	[SerializeField] GameObject holderPanel;
	[SerializeField] GameObject topPanel;
	[SerializeField] GameObject middlePanel;

	[Header("Top Section")]
	[SerializeField] GameObject balanceHolder;
	[SerializeField] GameObject progressHolder;

	[SerializeField] Text playerLevelText;
	[SerializeField] Text currentAndNeededXPText;
	[SerializeField] Slider progressBarSlider;

	[SerializeField] Text numberOfCoinsText;

	[SerializeField] Text numberOfGemsText;

	[SerializeField] Button closeButton;
	[SerializeField] Text closeButtonText;

	[SerializeField] Button settingsButton;

	[Header("Middle Section")]
	[SerializeField] Image playerIcon;
	[SerializeField] Text playerNameText;
	[SerializeField] Image onlineIndicator;

	void Awake ()
	{
		if(Instance)
		{
			DestroyImmediate(gameObject);
		}
		else
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
			SceneManager.sceneLoaded += OnSceneLoaded;
			PlayerProfile.playerProfileChanged += PlayerProfileChanged;
			PlayerInventory.playerInventoryChangedNew += PlayerInventoryChangedNew;
		}
	}

	// Use this for initialization
	void Start ()
	{
		configureUI();
	}	

	void OnSceneLoaded ( Scene scene, LoadSceneMode mode )
	{
		GameScenes gameScene = (GameScenes) scene.buildIndex;

		//In the main menu, the Close button becomes a Settings button
		if( gameScene == GameScenes.MainMenu )
		{
			closeButton.gameObject.SetActive( false );
			settingsButton.gameObject.SetActive( true );
		}
		else
		{
			closeButton.gameObject.SetActive( true );
			settingsButton.gameObject.SetActive( false );
		}

		//In all cases, reset value
		enableCloseButton( true );

		switch( gameScene )
		{
			case GameScenes.MainMenu:
				//We need to get references to these values every time we load the main menu since when we leave the main menu, the references get lost.
				//horizontalContent holds the main menu, the card collection and the store
				horizontalContent = GameObject.FindGameObjectWithTag("Horizontal Content").GetComponent<RectTransform>();
				//storeVerticalContent holds the store
				storeVerticalContent = GameObject.FindGameObjectWithTag("Store Vertical Content").GetComponent<RectTransform>();
				configurePanels( true, true, true );
				onlyShowCloseButton( false );
			break;

			case GameScenes.PlayModes:
				configurePanels( true, true, false );
				onlyShowCloseButton( true );
			break;

			case GameScenes.Training:
				configurePanels( true, true, false );
				onlyShowCloseButton( true );
			break;

			case GameScenes.HeroSelection:
				configurePanels( false );
			break;

			case GameScenes.Social:
				configurePanels( true, true, true );
				onlyShowCloseButton( true );
			break;

			case GameScenes.CareerProfile:
				configurePanels( true, true, true );
				onlyShowCloseButton( true );
			break;

			case GameScenes.Options:
				configurePanels( true, true );
				onlyShowCloseButton( true );
			break;

			case GameScenes.CircuitSelection:
				configurePanels( true, true );
				onlyShowCloseButton( true );
			break;

			case GameScenes.Matchmaking:
				configurePanels( true, true );
				onlyShowCloseButton( true );
			break;

			case GameScenes.WorldMap:
			case GameScenes.Level:
				configurePanels( false );
			break;
		}
	}	
	
	void configureUI()
	{
		playerLevelText.text = GameManager.Instance.playerProfile.getLevel().ToString();
		currentAndNeededXPText.text = string.Format( "{0}/{1}", GameManager.Instance.playerProfile.totalXPEarned, ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() ) );
		progressBarSlider.value = GameManager.Instance.playerProfile.totalXPEarned/ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() );
	
		numberOfCoinsText.text = GameManager.Instance.playerInventory.getCoinBalance().ToString("N0");
	
		numberOfGemsText.text = GameManager.Instance.playerInventory.getGemBalance().ToString("N0");
	
		playerIcon.sprite = ProgressionManager.Instance.getPlayerIconDataByUniqueId( GameManager.Instance.playerProfile.getPlayerIconId() ).icon;
		playerNameText.text = GameManager.Instance.playerProfile.getUserName();

	}

	public void configureChatStatusColor( Color chatStatusColor )
	{
		onlineIndicator.color = chatStatusColor;
	}

	void PlayerInventoryChangedNew( PlayerInventoryEvent eventType, int newValue )
	{
		switch (eventType)
		{
			case PlayerInventoryEvent.Gem_Balance_Changed:
				numberOfGemsText.text = newValue.ToString("N0");
			break;

			case PlayerInventoryEvent.Coin_Changed:
				numberOfCoinsText.text = newValue.ToString("N0");			
			break;
		}
	}

	void PlayerProfileChanged( PlayerProfileEvent eventType, int previousValue, int newValue )
	{
		switch (eventType)
		{
			case PlayerProfileEvent.Level_Changed:
				playerLevelText.text = newValue.ToString();
				currentAndNeededXPText.text = string.Format( "{0}/{1}", GameManager.Instance.playerProfile.totalXPEarned, ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() ) );
				progressBarSlider.value = GameManager.Instance.playerProfile.totalXPEarned/(float)ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() );
			break;

			case PlayerProfileEvent.Player_Icon_Changed:
				playerIcon.sprite = ProgressionManager.Instance.getPlayerIconDataByUniqueId( newValue ).icon;
			break;

			case PlayerProfileEvent.XP_Changed:
				animateProgressBar( previousValue, newValue, 3f );
			break;

			case PlayerProfileEvent.User_Name_Changed:
				playerNameText.text = GameManager.Instance.playerProfile.getUserName();
			break;
		}
	}

	void animateProgressBar( int previousValue, int newValue, float duration )
	{
		//Animate Text
		string currentAndNeededXPString = "{0}/" + ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() ).ToString();
		currentAndNeededXPText.GetComponent<UISpinNumber>().spinNumber( currentAndNeededXPString, previousValue, newValue, duration );

		//Animate Slider
		float toValue = newValue/(float)ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() );
		progressBarSlider.GetComponent<UIAnimateSlider>().animateSlider( toValue, duration );
	}

	public void OnClickShowCoinStore()
	{
		//The store button only works when you are in the main menu
		if( SceneManager.GetActiveScene().buildIndex == (int)GameScenes.MainMenu )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			StartCoroutine( scrollToStorePosition( 0.4f, COIN_STORE_VERTICAL_POSITION ) );
			configurePanels( true, true );
		}
		else
		{
			Debug.LogWarning("OnClickShowCoinStore: you can only access the store from the main menu."); 
		}
	}

	public void OnClickShowGemStore()
	{
		//The store button only works when you are in the main menu
		if( SceneManager.GetActiveScene().buildIndex == (int)GameScenes.MainMenu )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			StartCoroutine( scrollToStorePosition( 0.4f, GEM_STORE_VERTICAL_POSITION ) );
			configurePanels( true, true );
		}
		else
		{
			Debug.LogWarning("OnClickShowCoinStore: you can only access the store from the main menu."); 
		}
	}

	IEnumerator scrollToStorePosition( float duration, float verticalPosition )
	{
		float elapsedTime = 0;
		
		Vector2 startHorizontalPosition = horizontalContent.anchoredPosition;
		Vector2 endHorizontalPosition = new Vector2( 0, horizontalContent.anchoredPosition.y );

		Vector2 startVerticalPosition = storeVerticalContent.anchoredPosition;
		Vector2 endVerticalPosition = new Vector2( storeVerticalContent.anchoredPosition.x, verticalPosition );

		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			horizontalContent.anchoredPosition = Vector2.Lerp( startHorizontalPosition, endHorizontalPosition, elapsedTime/duration );
			storeVerticalContent.anchoredPosition = Vector2.Lerp( startVerticalPosition, endVerticalPosition, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		horizontalContent.anchoredPosition = new Vector2( 0, horizontalContent.anchoredPosition.y );
		storeVerticalContent.anchoredPosition = new Vector2( storeVerticalContent.anchoredPosition.x, verticalPosition );
	}

	public void OnClose()
	{
		StartCoroutine( loadScene(GameScenes.MainMenu) );
	}

	public void OnClickOpenOptionsMenu()
	{
		StartCoroutine( loadScene(GameScenes.Options) );
	}

	IEnumerator loadScene( GameScenes value )
	{
		UISoundManager.uiSoundManager.playButtonClick();
		Handheld.StartActivityIndicator();
		yield return new WaitForSeconds(0);
		SceneManager.LoadScene( (int)value );
	}

	public void enableCloseButton( bool enable )
	{
		if( enable )
		{
			closeButton.interactable = true;
			closeButtonText.color = Color.white;
		}
		else
		{
			closeButton.interactable = false;
			closeButtonText.color = Color.gray;
		}
	}

	public void configurePanels( bool holderPanelVisible, bool topPanelVisible = false, bool middlePanelVisible = false )
	{
		holderPanel.SetActive( holderPanelVisible );
		topPanel.SetActive( topPanelVisible );
		middlePanel.SetActive( middlePanelVisible );
	}

	void onlyShowCloseButton( bool closeButtonOnly )
	{
		balanceHolder.gameObject.SetActive( !closeButtonOnly );
		progressHolder.gameObject.SetActive( !closeButtonOnly );
		topPanel.GetComponent<Image>().enabled = !closeButtonOnly;
	}

}
