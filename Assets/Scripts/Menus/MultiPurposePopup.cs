using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiPurposePopup : MonoBehaviour {

	[Header("Multi-purpose Popup")]
	public Text titleText;
	public Text contentText;
	public Text buttonText;

	public void configurePopup( string titleID, string contentID, string buttonID )
	{
		titleText.text = LocalizationManager.Instance.getText( titleID );
		contentText.text = LocalizationManager.Instance.getText( contentID );
		buttonText.text = LocalizationManager.Instance.getText( buttonID );
	}

	public void display()
	{
		gameObject.SetActive( true );
	}

	public void hide()
	{
		gameObject.SetActive( false );
	}

}
