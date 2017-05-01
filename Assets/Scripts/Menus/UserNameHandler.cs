using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UserNameHandler : MonoBehaviour {

	[Header("User Name")]
	[SerializeField] Text userNameTitleText;
	[SerializeField] InputField userNameText;
	[SerializeField] Text userNamePlaceholderText;
	[SerializeField] Text confirmButtonText;
	[Header("Main Menu")]
	[SerializeField] GameObject mainMenuCanvas;
	public const int MINIMUM_USER_NAME_LENGTH = 4;

	// Use this for initialization
	void Awake ()
	{
		userNameTitleText.text = LocalizationManager.Instance.getText( "USER_NAME_POPUP_TITLE" ).Replace("\\n", System.Environment.NewLine );
		userNamePlaceholderText.text = LocalizationManager.Instance.getText( "USER_NAME_PLACEHOLDER" );
		confirmButtonText.text = LocalizationManager.Instance.getText("MENU_CONFIRM");
	}

	public void OnClickConfirm()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		userNameText.text = userNameText.text.Trim();
		if( userNameText.text.Length < MINIMUM_USER_NAME_LENGTH )
		{
			userNameText.text = string.Empty;
			userNamePlaceholderText.text = LocalizationManager.Instance.getText( "USER_NAME_NOT_LONG_ENOUGH" ).Replace("<MINIMUM_LENGTH>", MINIMUM_USER_NAME_LENGTH.ToString() );
			return;
		}

		Debug.Log("User Name is : " + userNameText.text );
		PlayerStatsManager.Instance.setFirstTimePlaying( false );
		GameManager.Instance.playerProfile.saveUserName( userNameText.text );
		//Now that we have a user name, we can connect to chat.
		ChatManager.Instance.ChatConnect();
		gameObject.SetActive( false );
		mainMenuCanvas.SetActive( true );
	}

}
