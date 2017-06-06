using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Facebook.Unity;
using UnityEngine.EventSystems;

public class FacebookFriendsHandler : MonoBehaviour {

	[SerializeField] Button inviteButton;

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

	public void OnClickClose()
	{
		gameObject.SetActive( false );
	}

}
