﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class FacebookConnectionHandler : MonoBehaviour {

	[Header("Facebook Connection")]
	[SerializeField] Button connectButton;
	[SerializeField] Text connectButtonText;
	[SerializeField] Image connectionStatusImage;
	[SerializeField] Sprite connectionSuccessSprite;
	[SerializeField] Sprite connectionFailureSprite;
	[SerializeField] TextMeshProUGUI funText;
	const float DELAY_BEFORE_HIDING_STATUS = 5f;

	void Awake ()
	{
		Handheld.StopActivityIndicator();
		funText.text = string.Format( LocalizationManager.Instance.getText("FACEBOOK_MORE_FUN_WITH_FRIENDS"), LocalizationManager.Instance.getText("GAME_TITLE") );
		connectionStatusImage.gameObject.SetActive( false );
		if( FacebookManager.Instance.isLoggedIn() )
		{
			connectButtonText.text = LocalizationManager.Instance.getText("MENU_CONNECTED");
			connectButton.interactable = false;
		}
		else
		{
			connectButtonText.text = LocalizationManager.Instance.getText("MENU_CONNECT");
			connectButton.interactable = true;
		}
	}

	public void OnClickLoginToFacebook()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		Debug.LogWarning("OnClickLoginToFacebook");
		connectButton.interactable = false;
		connectionStatusImage.gameObject.SetActive( false );
		CancelInvoke();
		if( Application.internetReachability != NetworkReachability.NotReachable )
		{
			GameManager.Instance.playerConfiguration.setUsesFacebook( true );
			GameManager.Instance.playerConfiguration.serializePlayerConfiguration( true );
			FacebookManager.Instance.CallFBInit();
		}
		else
		{
			//Player is not connected to the Internet
			MultiPurposePopup.Instance.displayPopup( "MENU_NO_INTERNET" );
		}
	}

	void OnEnable()
	{
		FacebookManager.facebookStateChanged += FacebookStateChanged;
	}

	void OnDisable()
	{
		FacebookManager.facebookStateChanged -= FacebookStateChanged;
	}

	public void FacebookStateChanged( FacebookState newState )
	{
		if( newState == FacebookState.LoggedIn )
		{
			connectionStatusImage.sprite = connectionSuccessSprite;
			connectionStatusImage.gameObject.SetActive( true );
			connectButtonText.text = LocalizationManager.Instance.getText("MENU_CONNECTED");
			Invoke("hideConnectionStatusImage", DELAY_BEFORE_HIDING_STATUS );
		}
		else if ( newState == FacebookState.Error )
		{
			connectionStatusImage.sprite = connectionFailureSprite;
			connectionStatusImage.gameObject.SetActive( true );
			Invoke("hideConnectionStatusImage", DELAY_BEFORE_HIDING_STATUS );
			connectButton.interactable = true;
		}
		else if ( newState == FacebookState.Canceled )
		{
			connectionStatusImage.sprite = connectionFailureSprite;
			connectionStatusImage.gameObject.SetActive( true );
			Invoke("hideConnectionStatusImage", DELAY_BEFORE_HIDING_STATUS );
			connectButton.interactable = true;
		}
	}

	void hideConnectionStatusImage()
	{
		connectionStatusImage.gameObject.SetActive( false );
	}
}
