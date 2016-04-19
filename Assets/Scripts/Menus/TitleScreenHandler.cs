using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScreenHandler : MonoBehaviour {

	[Header("Title Screen")]
	public Text gameTitle;
	public Button playButton;
	public Text playButtonText;
	public Button connectButton;
	public Text connectButtonText;

	[Header("Connection Status Popup")]
	public GameObject connectionPopup;
	public Text connectionTitleText;
	public Text connectionContentText;
	public Text connectionOkayButtonText;


	void Awake ()
	{
		Handheld.StopActivityIndicator();

		initialiseGame();

		//Title Screen
		gameTitle.text = LocalizationManager.Instance.getText("GAME_TITLE");
		playButtonText.text = LocalizationManager.Instance.getText("MENU_PLAY");
		connectButtonText.text = LocalizationManager.Instance.getText("MENU_CONNECT");

		//Connection status popup (connecting, connected, failure)
		connectionOkayButtonText.text = LocalizationManager.Instance.getText("MENU_OK");

		//If not already connected to Facebook, show the Connect button on the main menu to encourage player to login.
		connectButton.gameObject.SetActive( !FacebookManager.Instance.isLoggedIn() );

		//Get the level data. Level data has the parameters for all the levels of the game.
		GameObject levelDataObject = GameObject.Find("LevelData");
		LevelData levelData = (LevelData) levelDataObject.GetComponent("LevelData");
		//Now set it in the LevelManager
		LevelManager.Instance.setLevelData( levelData );		

	}

	void initialiseGame()
	{
		Application.targetFrameRate = 60;

		#if UNITY_IPHONE
		Handheld.SetActivityIndicatorStyle(UnityEngine.iOS.ActivityIndicatorStyle.WhiteLarge);
		#elif UNITY_ANDROID
		Handheld.SetActivityIndicatorStyle(AndroidActivityIndicatorStyle.Large;
		#endif

		LocalizationManager.Instance.initialize();

		GameManager.Instance.loadVersionNumber();
		#if UNITY_EDITOR
		if( UnityEditor.PlayerSettings.bundleVersion != GameManager.Instance.getVersionNumber() )
		{
			Debug.LogError("TitleScreenHandler: Version number mismatch. The version indicated in version.txt (" + GameManager.Instance.getVersionNumber() + ") does not match the bundle version in PlayerSettings (" + UnityEditor.PlayerSettings.bundleVersion + "). This needs to be corrected before submitting the build.");
		}
		#endif

		PlayerStatsManager.Instance.loadPlayerStats();

		Debug.Log("TitleScreenHandler: Start : uses Facebook " + PlayerStatsManager.Instance.getUsesFacebook() );
		//If the player agreed to use Facebook, auto-login for him
		if( PlayerStatsManager.Instance.getUsesFacebook() && Application.internetReachability != NetworkReachability.NotReachable )
		{
			FacebookManager.Instance.CallFBInit( updateState );
		}
	}

	public void play()
	{
		SoundManager.playButtonClick();
		StartCoroutine("loadWorldMap");
	}
	
	IEnumerator loadWorldMap()
	{
		Handheld.StartActivityIndicator();
		yield return new WaitForSeconds(0);
		if( PlayerStatsManager.Instance.getAvatar() == Avatar.None )
		{
			//Bring the player to the character selection screen
			SceneManager.LoadScene( (int)GameScenes.CharacterSelection );
		}
		else
		{
			//Player has already selected an avatar, display the world map
			SceneManager.LoadScene( (int)GameScenes.WorldMap);
		}
	}

	public void loginWithFacebook()
	{
		SoundManager.playButtonClick();
		//Hide Play and Connect buttons while showing the popup
		playButton.gameObject.SetActive( false );
		connectButton.gameObject.SetActive( false );

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

	public void closeConnectionPopup()
	{
		SoundManager.playButtonClick();
		connectionPopup.gameObject.SetActive( false );
		//Show Play and Connect buttons since we are dismissing the popup
		playButton.gameObject.SetActive( true );
		//If not already connected to Facebook, show the Connect button on the main menu to encourage player to login.
		connectButton.gameObject.SetActive( !FacebookManager.Instance.isLoggedIn() );
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
			connectionTitleText.text = LocalizationManager.Instance.getText("MENU_CONNECTION_CANCELED_TITLE");
			connectionContentText.text = LocalizationManager.Instance.getText("MENU_CONNECTION_CANCELED_TEXT");
		}
	}
	
}
