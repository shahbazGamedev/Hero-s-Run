using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Facebook.Unity;

public class FacebookPortraitHandler : MonoBehaviour {

	public int episodeNumber;
	public Sprite defaultPortrait;
	public Image spinner;
	public bool isPlayerPortrait = false; //True if this the player's portrait, false if it is a friend's portrait.
	private string requestedPortraitID;

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
				requestedPortraitID = userID;
			}
			else
			{
				//Picture will be requested. Draw default portrait with a spinner on top.
				GetComponent<Image>().sprite = defaultPortrait;
				spinner.gameObject.SetActive( true );
				FacebookManager.Instance.getFriendPicture( userID );
				requestedPortraitID = userID;
			}
		}
		else
		{
			Debug.LogError("FacebookPortraitHandler-setPortrait called with an empty user ID.");
		}
	}

	public void setPlayerPortrait()
	{
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
		FacebookManager.facebookStateChanged += FacebookStateChanged;
	}

	void OnDisable()
	{
		FacebookManager.facebookFriendPortraitReceived -= FacebookFriendPortraitReceived;
		FacebookManager.facebookPlayerPortraitReceived -= FacebookPlayerPortraitReceived;
		FacebookManager.facebookStateChanged -= FacebookStateChanged;
	}

	void FacebookFriendPortraitReceived( string facebookID )
	{
		//Make sure it is for us
		if( facebookID.Equals( requestedPortraitID ) ) setPortrait (facebookID);
		requestedPortraitID = string.Empty;
	}

	void FacebookPlayerPortraitReceived()
	{
		setPlayerPortrait ();
	}

	public void FacebookStateChanged( FacebookState newState )
	{
		if( newState == FacebookState.LoggedOut )
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

}
