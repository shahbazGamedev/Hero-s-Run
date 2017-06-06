using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Facebook.Unity;
using UnityEngine.EventSystems;

public class FacebookFriendsHandler : MonoBehaviour {

	[SerializeField] Button inviteButton;
	[SerializeField] string inviteFriendsCustomImageUri = "http://i.imgur.com/zkYlB.jpg";

	// Use this for initialization
	void Start ()
	{
		inviteButton.interactable = FB.IsLoggedIn;
	}
	
	void OnEnable()
	{
		FacebookManager.facebookStateChanged += FacebookStateChanged;
	}

	void OnDisable()
	{
		FacebookManager.facebookStateChanged -= FacebookStateChanged;
	}

	void FacebookStateChanged( FacebookState newState )
	{
		print("FacebookStateChanged " + newState );
		if( newState == FacebookState.LoggedIn )
		{
			inviteButton.interactable = true;
		}
		else
		{
			inviteButton.interactable = false;
		}
	}


	public void OnClickInviteFacebookFriends()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		if( Application.internetReachability != NetworkReachability.NotReachable )
		{
			FacebookManager.Instance.inviteFriends( inviteFriendsCustomImageUri );
		}
		else
		{
			//Player is not connected to the Internet
			MultiPurposePopup.Instance.displayPopup( "MENU_NO_INTERNET" );
		}
	}

	public void OnClickClose()
	{
		gameObject.SetActive( false );
	}

}
