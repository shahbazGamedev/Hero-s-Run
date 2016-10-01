using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AskForLivesManager : MonoBehaviour {

	[Header("Ask For Lives")]
	public Text title;
	public Text contentText;
	public Text askFriendsButtonText;

	// Use this for initialization
	void Start ()
 	{
		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu
		#endif
		title.text = LocalizationManager.Instance.getText("POPUP_ASK_LIVES_TITLE");
		contentText.text = LocalizationManager.Instance.getText("POPUP_ASK_LIVES_CONTENT");
		askFriendsButtonText.text = LocalizationManager.Instance.getText("POPUP_ASK_LIVES_BUTTON");
	}
	
	public void showFacebookAskLives()
	{
		SoundManager.soundManager.playButtonClick();
		string message = LocalizationManager.Instance.getText( "MESSAGE_ENTRY_TITLE_EXTRA_LIFE" );
		FacebookManager.Instance.CallAppRequestAsFriendSelector( "App Requests", message, "Ask_Give_Life,1", "", "" );
	}

}
