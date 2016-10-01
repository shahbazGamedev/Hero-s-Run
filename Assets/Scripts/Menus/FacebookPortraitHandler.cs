using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FacebookPortraitHandler : MonoBehaviour {

	public Sprite defaultPortrait;
	public Sprite spinner;
	public Image facebookPortrait;
	
	public void setPortrait ( string facebookID )
	{
		//Facebook portrait
		Sprite picture;
		if ( FacebookManager.Instance.friendImages.TryGetValue( facebookID, out picture)) 
		{
			//We have the friend's picture
			facebookPortrait.sprite = picture;
		}
		else if ( FacebookManager.Instance.friendImagesRequested.Contains( facebookID ) )
		{
			//Picture has been requested but not received yet. Draw default portrait with a spinner on top.
			facebookPortrait.sprite = defaultPortrait;
		}
		else
		{
			//Simply draw the default portrait
			facebookPortrait.sprite = defaultPortrait;
		}
	
	}

	void OnEnable()
	{
		FacebookManager.facebookPortraitReceived += FacebookPortraitReceived;
	}

	void OnDisable()
	{
		FacebookManager.facebookPortraitReceived -= FacebookPortraitReceived;
	}

	void FacebookPortraitReceived( string facebookID )
	{
		setPortrait ( facebookID );
	}

}
