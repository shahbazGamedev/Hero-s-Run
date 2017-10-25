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
		UISoundManager.uiSoundManager.playButtonClick();
		string message = LocalizationManager.Instance.getText( "MESSAGE_ENTRY_TITLE_EXTRA_LIFE" );
		string title = LocalizationManager.Instance.getText( "FB_ASK_LIVES_TITLE" );
		FacebookManager.Instance.CallAppRequestAsFriendSelector( title, message, "Ask_Give_Life,1,-1,N/A", "", "" );
	}

}
