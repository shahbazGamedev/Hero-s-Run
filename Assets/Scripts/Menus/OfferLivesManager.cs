using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OfferLivesManager : MonoBehaviour {

	[Header("Offer Lives")]
	public Text title;
	public Text contentText;
	public Text buttonText;

	// Use this for initialization
	void Start ()
 	{
		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu
		#endif
		title.text = LocalizationManager.Instance.getText("POPUP_TITLE_OFFER_LIVES");
		contentText.text = LocalizationManager.Instance.getText("POPUP_CONTENT_OFFER_LIVES");
		buttonText.text = LocalizationManager.Instance.getText("POPUP_BUTTON_SEND");
	}
	
	public void showFacebookOfferLives()
	{
		SoundManager.soundManager.playButtonClick();
		string message = LocalizationManager.Instance.getText("FB_HAVE_A_LIFE_MESSAGE");
		string title = LocalizationManager.Instance.getText( "POPUP_TITLE_OFFER_LIVES" );
		FacebookManager.Instance.CallAppRequestAsFriendSelector( title, message, "Accept_Give_Life,1,-1", "", "" );
	}
}
