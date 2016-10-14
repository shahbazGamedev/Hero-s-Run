using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Facebook.Unity;

public class FacebookPortraitHandler : MonoBehaviour {

	public Sprite defaultPortrait;
	public Image spinner;
	
	public void setPortrait ( string facebookID )
	{
		spinner.gameObject.SetActive( false );
		if( FB.IsLoggedIn )
		{

			Sprite picture;
			if ( FacebookManager.Instance.friendImages.TryGetValue( facebookID, out picture)) 
			{
				//We have the friend's picture
				GetComponent<Image>().sprite = picture;
			}
			else if ( FacebookManager.Instance.friendImagesRequested.Contains( facebookID ) )
			{
				//Picture has been requested but not received yet. Draw default portrait with a spinner on top.
				GetComponent<Image>().sprite = defaultPortrait;
				spinner.gameObject.SetActive( true );
			}
			else
			{
				//Simply draw the default portrait
				GetComponent<Image>().sprite = defaultPortrait;
			}
		}
		else
		{
			//Simply draw the default portrait
			GetComponent<Image>().sprite = defaultPortrait;
		}
	}

	public void setPlayerPortrait()
	{
		if ( FacebookManager.Instance.UserPortrait != null ) 
		{
			//We have the player's picture
			GetComponent<Image>().sprite = FacebookManager.Instance.UserPortrait;
		}
		else
		{
			//Simply draw the default portrait
			GetComponent<Image>().sprite = defaultPortrait;
		}
	}

	void OnEnable()
	{
		FacebookManager.facebookPortraitReceived += FacebookPortraitReceived;
		FacebookManager.facebookLogout += FacebookLogout;
	}

	void OnDisable()
	{
		FacebookManager.facebookPortraitReceived -= FacebookPortraitReceived;
		FacebookManager.facebookLogout -= FacebookLogout;
	}

	void FacebookPortraitReceived( string facebookID )
	{
		setPortrait ( facebookID );
	}

	void FacebookLogout()
	{
		//Player logged out of Facebook, so replace the portrait by the default one
		GetComponent<Image>().sprite = defaultPortrait;
	}

}
