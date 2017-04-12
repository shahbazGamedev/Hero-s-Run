using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FacebookConnectionHandler : MonoBehaviour {

	[Header("Facebook Connection")]
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

		//connectButtonText.text = LocalizationManager.Instance.getText("MENU_CONNECT");

		//Connection status popup (connecting, connected, failure)
		connectionOkayButtonText.text = LocalizationManager.Instance.getText("MENU_OK");

		//If not already connected to Facebook, show the Connect button on the main menu to encourage player to login.
		//connectButton.gameObject.SetActive( !FacebookManager.Instance.isLoggedIn() );

	}

	public void OnClickLoginToFacebook()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		//Hide Connect buttons while showing the popup
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

	public void OnClickCloseConnectionPopup()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		connectionPopup.gameObject.SetActive( false );
		//Show Connect buttons since we are dismissing the popup
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
