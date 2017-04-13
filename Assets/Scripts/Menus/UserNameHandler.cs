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
		Debug.Log("User Name is : " + userNameText.text );
		PlayerStatsManager.Instance.saveUserName( userNameText.text );
		PlayerStatsManager.Instance.setFirstTimePlaying( false );
		gameObject.SetActive( false );
		mainMenuCanvas.SetActive( true );
	}

}
