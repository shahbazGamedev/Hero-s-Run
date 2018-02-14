using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UniversalTopBar : Menu {

	const float NUMBER_SPIN_DURATION = 1.25f;
	public static UniversalTopBar Instance;

	[Header("Top Panel")]
	[SerializeField] GameObject topPanel; //contains everything except settings and close buttons

	[Header("For Store Access")]
	[SerializeField] MainMenuManager mainMenuManager;
	const float SOFT_CURRENCY_STORE_VERTICAL_POSITION = 2488f;
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

	int residualXP = 0;

	void Awake ()
	{
		Instance = this;
	}

	void OnEnable ()
	{
		PlayerProfile.playerProfileChanged += PlayerProfileChanged;
		PlayerInventory.playerInventoryChangedNew += PlayerInventoryChangedNew;
	}

	void OnDisable ()
	{
		PlayerProfile.playerProfileChanged -= PlayerProfileChanged;
		PlayerInventory.playerInventoryChangedNew -= PlayerInventoryChangedNew;
	}

	void Start ()
	{
		configureUI();
	}	

	#if UNITY_EDITOR
	//For testing
	void Update()
	{
		handleKeyboard();
	}

	private void handleKeyboard()
	{
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.Q) ) 
		{
			GameManager.Instance.playerProfile.addToTotalXPEarned( 100, false );
		}
		else if ( Input.GetKeyDown (KeyCode.W) ) 
		{
			GameManager.Instance.playerProfile.addToTotalXPEarned( 2000, false );
		}
	}
	#endif

	void configureUI()
	{
		configureXP();
	
		softCurrencyAmountText.text = GameManager.Instance.playerInventory.getCoinBalance().ToString("N0");
	
		hardCurrencyAmountText.text = GameManager.Instance.playerInventory.getGemBalance().ToString("N0");
	}

	void configureXP()
	{
		int currentPlayerLevel = GameManager.Instance.playerProfile.getLevel();

		if(  currentPlayerLevel == ProgressionManager.MAX_LEVEL )
		{
			playerLevelText.text = currentPlayerLevel.ToString();
			int currentXP = GameManager.Instance.playerProfile.getTotalXPEarned();
			string maxLevelString = LocalizationManager.Instance.getText( "MAIN_MENU_MAX_LEVEL" );
			currentAndNeededXPText.text = string.Format( "{0}/{1}", currentXP.ToString("N0"), maxLevelString );
			xpFill.fillAmount = 1f;
		}
		else
		{
			playerLevelText.text = currentPlayerLevel.ToString();
			int currentXP = GameManager.Instance.playerProfile.getTotalXPEarned();
			int nextPlayerLevel = currentPlayerLevel + 1;
			int xpNeededToReachNextLevel = ProgressionManager.Instance.getTotalXPRequired( nextPlayerLevel );
			currentAndNeededXPText.text = string.Format( "{0}/{1}", currentXP.ToString("N0"), xpNeededToReachNextLevel.ToString("N0") );
			xpFill.fillAmount = currentXP/(float)xpNeededToReachNextLevel;
		}
	}

	void PlayerInventoryChangedNew( PlayerInventoryEvent eventType, int previousValue, int newValue )
	{
		switch (eventType)
		{
			case PlayerInventoryEvent.Gem_Balance_Changed:
				hardCurrencyAmountText.GetComponent<UISpinNumber>().spinNumber( "{0}", previousValue, newValue, NUMBER_SPIN_DURATION, false );
			break;

			case PlayerInventoryEvent.Coin_Changed:
				softCurrencyAmountText.GetComponent<UISpinNumber>().spinNumber( "{0}", previousValue, newValue, NUMBER_SPIN_DURATION, true );
			break;
		}
	}

	void PlayerProfileChanged( PlayerProfileEvent eventType, int previousValue, int newValue )
	{
		switch (eventType)
		{
			case PlayerProfileEvent.XP_Changed:
				handleXPChanged( previousValue, newValue, 1.5f );
			break;
		}
	}

	void handleXPChanged( int previousValue, int newValue, float duration )
	{
		int currentPlayerLevel = ProgressionManager.Instance.getLevel( previousValue );
		int playerLevelBasedOnNewValue = ProgressionManager.Instance.getLevel( newValue );

		if( playerLevelBasedOnNewValue > currentPlayerLevel )
		{
			//Player is leveling up. Bravo! Don't animate. Set the values directly.
			configureXP();
		}
		else
		{
			//Player is not leveling up.
			animateProgressBar( previousValue, newValue, duration );
		}
	}

	void animateProgressBar( int previousValue, int newValue, float duration, System.Action<int> onFinish = null )
	{
		//Animate Text
		int xpRequiredToReachNextLevel = ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() + 1 );
		string currentAndNeededXPString = "{0}/" + xpRequiredToReachNextLevel.ToString();
		currentAndNeededXPText.GetComponent<UISpinNumber>().spinNumber( currentAndNeededXPString, previousValue, newValue, duration, true, onFinish );

		//Animate horizontal image
		float toValue = newValue/(float)ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() + 1 );
		xpFill.GetComponent<UIAnimateRadialImage>().animateFillAmount( toValue, duration );
	}

	#region Store
	public void OnClickShowSoftCurrencyStore()
	{
		mainMenuManager.OnClickShowStore();
		UISoundManager.uiSoundManager.playButtonClick();
		StartCoroutine( scrollToStorePosition( 0.4f, SOFT_CURRENCY_STORE_VERTICAL_POSITION ) );
	}

	public void OnClickShowHardCurrencyStore()
	{
		mainMenuManager.OnClickShowStore();
		UISoundManager.uiSoundManager.playButtonClick();
		StartCoroutine( scrollToStorePosition( 0.4f, HARD_CURRENCY_STORE_VERTICAL_POSITION ) );
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
			yield return new WaitForEndOfFrame();  
			
		} while ( elapsedTime < duration );
		storeVerticalContent.anchoredPosition = new Vector2( storeVerticalContent.anchoredPosition.x, verticalPosition );
	}
	#endregion

	/// <summary>
	/// Opens the options menu.
	/// </summary>
	public void OnClickOpenOptionsMenu()
	{
		StartCoroutine( loadScene(GameScenes.Options) );
	}

	/// <summary>
	/// Shows the top bar.
	/// </summary>
	/// <param name="showTopBar">If set to <c>true</c> show top bar.</param>
	public void showTopBar( bool showTopBar )
	{
		topPanel.SetActive( showTopBar );
	}

}
