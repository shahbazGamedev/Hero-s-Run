using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ConnectToFacebookPopup : MonoBehaviour {

	[Header("Connect to Facebook Popup")]
	public Text titleText;
	public Text messageText;
	public Text buttonText;
	public NewWorldMapHandler newWorldMapHandler;
	public EpisodePopup episodePopup;
	[Header("Connection Status Popup")]
	public GameObject connectionPopup;
	public Text connectionStatusTitleText;
	public Text connectionStatusMessageText;
	public Text connectionStatusButtonText;

	// Use this for initialization
	void Awake ()
	{
		titleText.text = LocalizationManager.Instance.getText("MENU_TITLE_CONNECT_TO_FACEBOOK");
		messageText.text = LocalizationManager.Instance.getText("MENU_CONNECT_TO_FACEBOOK_REASON");
		messageText.text = messageText.text.Replace("\\n", System.Environment.NewLine );
		buttonText.text = LocalizationManager.Instance.getText("MENU_CONNECT");

		//Connection status popup (connecting, connected, failure)
		connectionStatusButtonText.text = LocalizationManager.Instance.getText("MENU_OK");
	}

	public void showConnectToFacebookPopup()
	{
		GetComponent<Animator>().Play("Panel Slide In");
	}

	//Player pressed the X button
	public void close()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		StartCoroutine( showEpisodedPopupThread() );
	}

	public void connectToFacebook()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		connectionPopup.gameObject.SetActive( true );
		if( Application.internetReachability != NetworkReachability.NotReachable )
		{
			PlayerStatsManager.Instance.setUsesFacebook( true );
			PlayerStatsManager.Instance.savePlayerStats();
			connectionStatusTitleText.text = LocalizationManager.Instance.getText("MENU_CONNECTING_TITLE");
			connectionStatusMessageText.text = LocalizationManager.Instance.getText("MENU_CONNECTING_TEXT");
			FacebookManager.Instance.CallFBInit( updateState );
		}
		else
		{
			connectionStatusTitleText.text = LocalizationManager.Instance.getText("MENU_CONNECTION_FAILED_TITLE");
			connectionStatusMessageText.text = LocalizationManager.Instance.getText("MENU_CONNECTION_FAILED_TEXT");
		}
	}

	public void closeConnectionPopup()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		connectionPopup.gameObject.SetActive( false );
		StartCoroutine( showEpisodedPopupThread() );
	}

	public void updateState( FacebookState newState )
	{
		if( newState == FacebookState.LoggedIn )
		{
			connectionStatusTitleText.text = LocalizationManager.Instance.getText("MENU_SUCCESS_TITLE");
			connectionStatusMessageText.text = LocalizationManager.Instance.getText("MENU_SUCCESS_TEXT");
		}
		else if ( newState == FacebookState.Error )
		{
			connectionStatusTitleText.text = LocalizationManager.Instance.getText("MENU_FB_ERROR_TITLE");
			connectionStatusMessageText.text = LocalizationManager.Instance.getText("MENU_FB_ERROR_TEXT");
		}
		else if ( newState == FacebookState.Canceled )
		{
			connectionStatusTitleText.text = LocalizationManager.Instance.getText("MENU_CONNECTION_CANCELED_TITLE");
			connectionStatusMessageText.text = LocalizationManager.Instance.getText("MENU_CONNECTION_CANCELED_TEXT");
		}
	}

	IEnumerator showEpisodedPopupThread()
	{
		GetComponent<Animator>().Play("Panel Slide Out");
		yield return new WaitForSeconds(2f);
		episodePopup.showEpisodePopup( LevelManager.Instance.getCurrentEpisodeNumber() );
	}


	
}
