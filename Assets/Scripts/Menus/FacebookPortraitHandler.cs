using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Facebook.Unity;

public class FacebookPortraitHandler : MonoBehaviour {

	public int episodeNumber;
	public Sprite defaultPortrait;
	public Image spinner;
	bool isPlayerPortrait = false; //True if this the player's portrait, false if it is a friend's portrait.
	
	//Used in the world map
	public void setFriendPortrait ()
	{
		//Ignore if this is the player's portrait
		if( !isPlayerPortrait )
		{
			if( GameManager.Instance.getGameMode() == GameMode.Story )
			{
				if( FB.IsLoggedIn )
				{
					string userID = FacebookManager.Instance.getFriendPictureForEpisode( episodeNumber );
					if( userID != null )
					{
						//Yes, a friend has reached that episode
						GetComponent<Image>().gameObject.SetActive( true );
	
						Sprite picture;
						if ( FacebookManager.Instance.friendImages.TryGetValue( userID, out picture)) 
						{
							//We have the friend's picture
							spinner.gameObject.SetActive( false );
							GetComponent<Image>().sprite = picture;
						}
						else if ( FacebookManager.Instance.friendImagesRequested.Contains( userID ) )
						{
							//Picture has been requested but not received yet. Draw default portrait with a spinner on top.
							GetComponent<Image>().sprite = defaultPortrait;
							spinner.gameObject.SetActive( true );
						}
					}
					else
					{
						//We do not have friends that have reached that episode. Hide the portrait.
						GetComponent<Image>().gameObject.SetActive( false );
					}
				}
				else
				{
					//We are not logged in to Facebook. Hide the portrait.
					GetComponent<Image>().gameObject.SetActive( false );
				}
			}
			else
			{
				//We are in Endless mode. Hide the portrait.
				GetComponent<Image>().gameObject.SetActive( false );
			}
		}
	}

	//Used by the messenge center
	public void setPortrait ( string userID )
	{
		if( userID != string.Empty )
		{
			Sprite picture;
			if ( FacebookManager.Instance.friendImages.TryGetValue( userID, out picture)) 
			{
				//We have the friend's picture
				spinner.gameObject.SetActive( false );
				GetComponent<Image>().sprite = picture;
			}
			else if ( FacebookManager.Instance.friendImagesRequested.Contains( userID ) )
			{
				//Picture has been requested but not received yet. Draw default portrait with a spinner on top.
				GetComponent<Image>().sprite = defaultPortrait;
				spinner.gameObject.SetActive( true );
			}
		}
	}

	public void setPlayerPortrait()
	{
		isPlayerPortrait = true;
		if ( FacebookManager.Instance.playerPortrait != null ) 
		{
			//We have the player's picture
			GetComponent<Image>().sprite = FacebookManager.Instance.playerPortrait;
		}
		else
		{
			//Simply draw the default portrait
			GetComponent<Image>().sprite = defaultPortrait;
		}
	}

	void OnEnable()
	{
		FacebookManager.facebookFriendPortraitReceived += FacebookFriendPortraitReceived;
		FacebookManager.facebookPlayerPortraitReceived += FacebookPlayerPortraitReceived;
		FacebookManager.facebookLogout += FacebookLogout;
	}

	void OnDisable()
	{
		FacebookManager.facebookFriendPortraitReceived -= FacebookFriendPortraitReceived;
		FacebookManager.facebookPlayerPortraitReceived -= FacebookPlayerPortraitReceived;
		FacebookManager.facebookLogout -= FacebookLogout;
	}

	void FacebookFriendPortraitReceived( string facebookID )
	{
		setFriendPortrait ();
	}

	void FacebookPlayerPortraitReceived()
	{
		setPlayerPortrait ();
	}

	void FacebookLogout()
	{
		if( isPlayerPortrait )
		{
			//Simply draw the default portrait
			GetComponent<Image>().sprite = defaultPortrait;
		}
		else
		{
			setFriendPortrait();
		}
	}
}
