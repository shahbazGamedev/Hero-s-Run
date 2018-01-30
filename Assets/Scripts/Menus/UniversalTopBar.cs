using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UniversalTopBar : MonoBehaviour {

	public static UniversalTopBar Instance;
	const float NUMBER_SPIN_DURATION = 1.25f;

	[Header("Top Panel")]
	[SerializeField] GameObject topPanel; //contains everything except settings and close buttons

	[Header("For Store Access")]
	MainMenuManager mainMenuManager;
	const float SOFT_CURRENCY_STORE_VERTICAL_POSITION = 2508f;
	const float HARD_CURRENCY_STORE_VERTICAL_POSITION = 1420f;

	[Header("Level")]
	[SerializeField] TextMeshProUGUI playerLevelText;

	[Header("XP")]
	[SerializeField] Image xpFill;
	[SerializeField] TextMeshProUGUI currentAndNeededXPText;

	[Header("Soft Currency")]
	[SerializeField] TextMeshProUGUI softCurrencyAmountText;

	[Header("Hard Currency")]
	[SerializeField] TextMeshProUGUI hardCurrencyAmountText;

	[Header("Close and Settings buttons")]
	[SerializeField] GameObject buttons; //contains the settings and close buttons
	[SerializeField] Button closeButton;
	[SerializeField] TextMeshProUGUI closeButtonText;
	[SerializeField] Button settingsButton;

	int residualXP = 0;

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
			mainMenuManager = GameObject.FindObjectOfType<MainMenuManager>();
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
				showTopBar( true );
				onlyShowCloseButton( false );
			break;

			case GameScenes.PlayModes:
				showTopBar( true );
				onlyShowCloseButton( true );
			break;

			case GameScenes.Training:
				showTopBar( true );
				onlyShowCloseButton( true );
			break;

			case GameScenes.HeroSelection:
				showTopBar( false );
			break;

			case GameScenes.LootBox:
				showTopBar( false );
			break;

			case GameScenes.Social:
				showTopBar( true );
				onlyShowCloseButton( true );
			break;

			case GameScenes.CareerProfile:
				showTopBar( true );
				onlyShowCloseButton( true );
			break;

			case GameScenes.Options:
				showTopBar( true );
				onlyShowCloseButton( true );
			break;

			case GameScenes.CircuitSelection:
				showTopBar( true );
				onlyShowCloseButton( true );
			break;

			case GameScenes.Matchmaking:
				showTopBar( true );
				onlyShowCloseButton( true );
			break;

			case GameScenes.WorldMap:
			case GameScenes.Level:
				showTopBar( false );
			break;
		}
	}	
	
	void configureUI()
	{
		playerLevelText.text = GameManager.Instance.playerProfile.getLevel().ToString();
		currentAndNeededXPText.text = string.Format( "{0}/{1}", GameManager.Instance.playerProfile.totalXPEarned.ToString("N0"), ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() ).ToString("N0") );
		xpFill.fillAmount = GameManager.Instance.playerProfile.totalXPEarned/ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() );
	
		softCurrencyAmountText.text = GameManager.Instance.playerInventory.getCoinBalance().ToString("N0");
	
		hardCurrencyAmountText.text = GameManager.Instance.playerInventory.getGemBalance().ToString("N0");
	}

	void PlayerInventoryChangedNew( PlayerInventoryEvent eventType, int previousValue, int newValue )
	{
		switch (eventType)
		{
			case PlayerInventoryEvent.Gem_Balance_Changed:
				//You cannot start a coroutine if the object is not active.
				//So, if the object is active start the coroutine, if not, update the text field directly.
				if( topPanel.activeSelf )
				{
					hardCurrencyAmountText.GetComponent<UISpinNumber>().spinNumber( "{0}", previousValue, newValue, NUMBER_SPIN_DURATION, false );
				}
				else
				{
					hardCurrencyAmountText.text = newValue.ToString("N0");
				}
			break;

			case PlayerInventoryEvent.Coin_Changed:
				if( topPanel.activeSelf )
				{
					softCurrencyAmountText.GetComponent<UISpinNumber>().spinNumber( "{0}", previousValue, newValue, NUMBER_SPIN_DURATION, true );
				}
				else
				{
					softCurrencyAmountText.text = newValue.ToString("N0");
				}

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
				xpFill.fillAmount = GameManager.Instance.playerProfile.totalXPEarned/(float)ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() );
			break;

			case PlayerProfileEvent.XP_Changed:
				handleXPChanged( previousValue, newValue, 1.5f );
			break;
		}
	}

	void handleXPChanged( int previousValue, int newValue, float duration )
	{
		if( newValue > ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() ) )
		{
			//Player is leveling up. Bravo!
			residualXP = newValue - ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() );
			animateProgressBar( previousValue, ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() ), duration, animationCompletedCallback );
		}
		else
		{
			//Player is not leveling up.
			animateProgressBar( previousValue, newValue, duration );
		}
	}

	void animateProgressBar( int previousValue, int newValue, float duration, System.Action<int> onFinish = null )
	{
		//Only proceed if the progress bar is displayed. A coroutine cannot be started on an inactive object.
		if( topPanel.activeSelf )
		{
			//Animate Text
			string currentAndNeededXPString = "{0}/" + ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() ).ToString();
			currentAndNeededXPText.GetComponent<UISpinNumber>().spinNumber( currentAndNeededXPString, previousValue, newValue, duration, true, onFinish );
	
			//Animate horizontal image
			float toValue = newValue/(float)ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() );
			xpFill.GetComponent<UIAnimateRadialImage>().animateFillAmount( toValue, duration );
		}
		else
		{
			//Update the values, but don't animate.
			playerLevelText.text = GameManager.Instance.playerProfile.getLevel().ToString();
			currentAndNeededXPText.text = string.Format( "{0}/{1}", GameManager.Instance.playerProfile.totalXPEarned, ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() ) );
			xpFill.fillAmount = GameManager.Instance.playerProfile.totalXPEarned/ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() );
		}
	}

	/// <summary>
	/// Animation completed callback. This is called when the progress bar text has finished animating.
	/// It will trigger a second animation since the player has leveled up in this case.
	/// </summary>
	void animationCompletedCallback( int value )
	{
		GameManager.Instance.playerProfile.incrementLevel();
		xpFill.fillAmount = 0;
		animateProgressBar( 0, residualXP, 3f );
	}

	public void OnClickShowSoftCurrencyStore()
	{
		//The store button only works when you are in the main menu
		if( SceneManager.GetActiveScene().buildIndex == (int)GameScenes.MainMenu )
		{
			mainMenuManager.OnClickShowStore();
			UISoundManager.uiSoundManager.playButtonClick();
			StartCoroutine( scrollToStorePosition( 0.4f, SOFT_CURRENCY_STORE_VERTICAL_POSITION ) );
			showTopBar( true );
		}
		else
		{
			Debug.LogWarning("OnClickShowSoftCurrencyStore: you can only access the store from the main menu."); 
		}
	}

	public void OnClickShowHardCurrencyStore()
	{
		//The store button only works when you are in the main menu
		if( SceneManager.GetActiveScene().buildIndex == (int)GameScenes.MainMenu )
		{
			mainMenuManager.OnClickShowStore();
			UISoundManager.uiSoundManager.playButtonClick();
			StartCoroutine( scrollToStorePosition( 0.4f, HARD_CURRENCY_STORE_VERTICAL_POSITION ) );
			showTopBar( true );
		}
		else
		{
			Debug.LogWarning("OnClickShowHardCurrencyStore: you can only access the store from the main menu."); 
		}
	}

	IEnumerator scrollToStorePosition( float duration, float verticalPosition )
	{
		float elapsedTime = 0;
		//We need to get references to these values every time we load the main menu since when we leave the main menu, the references get lost.
		//storeVerticalContent holds the store
		RectTransform storeVerticalContent = GameObject.FindGameObjectWithTag("Store Vertical Content").GetComponent<RectTransform>();
		
		Vector2 startVerticalPosition = storeVerticalContent.anchoredPosition;
		Vector2 endVerticalPosition = new Vector2( storeVerticalContent.anchoredPosition.x, verticalPosition );

		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			storeVerticalContent.anchoredPosition = Vector2.Lerp( startVerticalPosition, endVerticalPosition, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
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

	public void showTopBar( bool showTopBar )
	{
		topPanel.SetActive( showTopBar );
		buttons.SetActive( showTopBar );
	}

	void onlyShowCloseButton( bool closeButtonOnly )
	{
		topPanel.SetActive( !closeButtonOnly );
	}

}
