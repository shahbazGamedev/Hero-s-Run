using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour {

	//New UI
	[Header("Main Menu")]
	public Text playButtonText;
	public Button connectButton;
	public Text connectButtonText;

	//Login popup related
	[Header("Login popup")]
	public GameObject loginPopup;
	public Text loginTitleText;
	public Text loginContentText;
	public Text loginConnectButtonText;
	public Text loginGuestButtonText;

	//Connection Status popup related
	[Header("Connection status popup")]
	public GameObject connectionPopup;
	public Text connectionTitleText;
	public Text connectionContentText;
	public Text connectionOkayButtonText;

	//Progression Slider
	public Slider progressionSlider;
	public Text progressionText;

	void Awake ()
	{
		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu
		#endif

		Handheld.StopActivityIndicator();

		//Get the level data. Level data has the parameters for all the levels of the game.
		GameObject levelDataObject = GameObject.Find("LevelData");
		LevelData levelData = (LevelData) levelDataObject.GetComponent("LevelData");
		//Now set it in the LevelManager
		LevelManager.Instance.setLevelData( levelData );

		//Display the daily bonus screen
		MyUpsightManager.requestLocalizedContent( "daily_bonus" );
		
		//Main Menu
		playButtonText.text = LocalizationManager.Instance.getText("MENU_PLAY");
		connectButtonText.text = LocalizationManager.Instance.getText("MENU_CONNECT");

		//Login popup
		loginTitleText.text = LocalizationManager.Instance.getText("MENU_FB_TITLE");
		loginContentText.text = LocalizationManager.Instance.getText("MENU_FB_EXPLAIN");
		loginConnectButtonText.text = LocalizationManager.Instance.getText("MENU_CONNECT");
		loginGuestButtonText.text = LocalizationManager.Instance.getText("MENU_GUEST");

		//Connection popup
		connectionOkayButtonText.text = LocalizationManager.Instance.getText("MENU_OK");

		//If not already connected to Facebook, show the Connect button on the main menu to encourage player to login.
		//connectButton.gameObject.SetActive( !FacebookManager.Instance.isLoggedIn() );

		//If this is a brand new user, show him the login screen where he will choose between Facebook and Guest.
		//If not, show him the main menu.
		if( PlayerStatsManager.Instance.isFirstTimePlaying() )
		{
			//Show the login popup
			loginPopup.gameObject.SetActive( true );
		}

		//Update the progression slider
		float lastLevelCompleted = (float) LevelManager.Instance.getNextLevelToComplete();
		float numberOfLevels = (float) LevelManager.Instance.getNumberOfLevels();
		float percentageCompleted = lastLevelCompleted/numberOfLevels;
		progressionSlider.value = percentageCompleted;
		progressionText.text = ( (int) (percentageCompleted * 100 ) ).ToString() + "%";
	}

	public void handlePlayButton()
	{
		SoundManager.playButtonClick();
		StartCoroutine("loadLevel");
	}
	
	public void handleFacebookLoginButton()
	{
		SoundManager.playButtonClick();
		connectionPopup.gameObject.SetActive( true );
		if( Application.internetReachability != NetworkReachability.NotReachable )
		{
			PlayerStatsManager.Instance.setUsesFacebook( true );
			PlayerStatsManager.Instance.savePlayerStats();
			connectionTitleText.text = LocalizationManager.Instance.getText("MENU_CONNECTING_TITLE");
			connectionContentText.text = LocalizationManager.Instance.getText("MENU_CONNECTING_TEXT");
			FacebookManager.Instance.CallFBInit( updateState );
		}
		else
		{
			connectionTitleText.text = LocalizationManager.Instance.getText("MENU_CONNECTION_FAILED_TITLE");
			connectionContentText.text = LocalizationManager.Instance.getText("MENU_CONNECTION_FAILED_TEXT");
		}
	}

	public void updateState( FacebookState newState )
	{
		if( newState == FacebookState.LoggedIn )
		{
			connectionTitleText.text = LocalizationManager.Instance.getText("MENU_SUCCESS_TITLE");
			connectionContentText.text = LocalizationManager.Instance.getText("MENU_SUCCESS_TEXT");
			//Hide the Connect button since the player successfully connected
			connectButton.gameObject.SetActive( false );
		}
		else if ( newState == FacebookState.Error )
		{
			connectionTitleText.text = LocalizationManager.Instance.getText("MENU_FB_ERROR_TITLE");
			connectionContentText.text = LocalizationManager.Instance.getText("MENU_FB_ERROR_TEXT");
		}
		else if ( newState == FacebookState.Canceled )
		{
			PlayerStatsManager.Instance.setUsesFacebook( false );
			PlayerStatsManager.Instance.savePlayerStats();
			connectionTitleText.text = LocalizationManager.Instance.getText("MENU_CONNECTION_CANCELED_TITLE");
			connectionContentText.text = LocalizationManager.Instance.getText("MENU_CONNECTION_CANCELED_TEXT");
		}
	}

	public void handleGuestLoginButton()
	{
		SoundManager.playButtonClick();
		PlayerStatsManager.Instance.setUsesFacebook( false );
		PlayerStatsManager.Instance.savePlayerStats();
		loginPopup.gameObject.SetActive( false );
	}

	public void handleLoginCloseButton()
	{
		SoundManager.playButtonClick();
		loginPopup.gameObject.SetActive( false );
	}

	public void handleConnectionCloseButton()
	{
		SoundManager.playButtonClick();
		loginPopup.gameObject.SetActive( false );
		connectionPopup.gameObject.SetActive( false );
	}
	
	IEnumerator loadLevel()
	{
		//Handheld.StartActivityIndicator();
		yield return new WaitForSeconds(0);
		if( PlayerStatsManager.Instance.getAvatar() == Avatar.None )
		{
			//Bring the player to the character selection screen
			Application.LoadLevel( (int)GameScenes.CharacterSelection );
		}
		else
		{
			//Player has already selected an avatar, display the world map
			Application.LoadLevel( (int)GameScenes.ComicBook);
		}

	}
}
