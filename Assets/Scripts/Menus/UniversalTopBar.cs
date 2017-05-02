using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UniversalTopBar : MonoBehaviour {

	public static UniversalTopBar Instance;

	[Header("Holder Panel")]
	[SerializeField] GameObject holderPanel;
	[SerializeField] GameObject topPanel;
	[SerializeField] GameObject middlePanel;

	[Header("Top Section")]
	[SerializeField] Text playerLevelText;
	[SerializeField] Text currentAndNeededXPText;
	[SerializeField] Slider progressBarSlider;

	[SerializeField] Text numberOfCoinsText;

	[SerializeField] Text numberOfGemsText;

	[SerializeField] Button closeButton;
	[SerializeField] Text closeButtonText;

	[Header("Middle Section")]
	[SerializeField] Image playerIcon;
	[SerializeField] Text playerNameText;
	[SerializeField] Image onlineIndicator;

	bool isBrowsingStore = false;

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
		switch( gameScene )
		{
			case GameScenes.MainMenu:
				configurePanels( true, true, true );
			break;

			case GameScenes.PlayModes:
				configurePanels( true, true, true );
			break;

			case GameScenes.Training:
				configurePanels( true, true, true );
			break;

			case GameScenes.HeroSelection:
				configurePanels( false );
			break;

			case GameScenes.Social:
				configurePanels( true, true, true );
			break;

			case GameScenes.CareerProfile:
				configurePanels( true, true, true );
			break;

			case GameScenes.Options:
				configurePanels( false );
			break;

			case GameScenes.CircuitSelection:
				configurePanels( true, true );
			break;

			case GameScenes.Matchmaking:
				configurePanels( true, true );
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
	
		numberOfCoinsText.text = PlayerStatsManager.Instance.getCurrentCoins().ToString("N0");
	
		numberOfGemsText.text = GameManager.Instance.playerInventory.getGemBalance().ToString("N0");
	
		playerIcon.sprite = ProgressionManager.Instance.getPlayerIconDataByUniqueId( GameManager.Instance.playerProfile.getPlayerIconId() ).icon;
		playerNameText.text = GameManager.Instance.playerProfile.getUserName();

	}

	public void configureChatStatusColor( Color chatStatusColor )
	{
		onlineIndicator.color = chatStatusColor;
	}

	void OnEnable()
	{
		PlayerStatsManager.playerInventoryChanged += PlayerInventoryChanged;
		PlayerInventory.playerInventoryChangedNew += PlayerInventoryChangedNew;
	}
	
	void OnDisable()
	{
		PlayerStatsManager.playerInventoryChanged -= PlayerInventoryChanged;
	}

	void PlayerInventoryChanged( PlayerInventoryEvent eventType, int newValue )
	{
		switch (eventType)
		{
			case PlayerInventoryEvent.Coin_Changed:
				numberOfCoinsText.text = newValue.ToString("N0");			
			break;
		}
	}

	void PlayerInventoryChangedNew( PlayerInventoryEvent eventType, int newValue )
	{
		switch (eventType)
		{
			case PlayerInventoryEvent.Gem_Balance_Changed:
				numberOfGemsText.text = newValue.ToString("N0");
			break;
		}
	}

	void PlayerProfileChanged( PlayerProfileEvent eventType, int newValue )
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
				currentAndNeededXPText.text = string.Format( "{0}/{1}", GameManager.Instance.playerProfile.totalXPEarned, ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() ) );
				progressBarSlider.value = GameManager.Instance.playerProfile.totalXPEarned/(float)ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() );
			break;

			case PlayerProfileEvent.User_Name_Changed:
				playerNameText.text = GameManager.Instance.playerProfile.getUserName();
			break;
		}
	}

	public void OnClickShowStore()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		StoreManager.Instance.showStore( StoreTab.Store, StoreReason.None );
		configurePanels( true, true );
		isBrowsingStore = true;
	}

	public void OnClose()
	{
		if( isBrowsingStore )
		{
			StoreManager.Instance.closeStore();
			isBrowsingStore = false;
		}
		else
		{
			StartCoroutine( loadScene() );
		}
	}

	IEnumerator loadScene()
	{
		//From the main menu, you cannot go to another scene via the close button.
		if( SceneManager.GetActiveScene().buildIndex != (int) GameScenes.MainMenu )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)GameScenes.MainMenu );
		}
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

}
