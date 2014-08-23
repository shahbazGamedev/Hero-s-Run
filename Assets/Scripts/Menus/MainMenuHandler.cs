using UnityEngine;
using System.Collections;

public class MainMenuHandler : MonoBehaviour {

	//Main menu background
	public Texture2D backgroundTexture;
	Rect backgroundRect;

	//Styles
	public GUIStyle buttonStyle;
	public GUIStyle textStyle;
	public GUIStyle titleStyle;
	public GUIStyle closeStyle;
	public GUIStyle okayStyle;
	
	//Content
	GUIContent playButtonContent;
	GUIContent fbButtonContent;
	GUIContent explanationTextContent;
	GUIContent guestButtonContent;
	GUIContent popupTitleContent;
	GUIContent popupTextContent;
	GUIContent okayButtonContent;
	GUIContent closeButtonContent;
	
	//control
	bool displayPopup = false;
	bool displayPopupButtons = false;
	bool displayLoginSelection = false;

	void Awake ()
	{
	
		playButtonContent = new GUIContent( LocalizationManager.Instance.getText("MENU_PLAY") );
		fbButtonContent = new GUIContent( LocalizationManager.Instance.getText("MENU_CONNECT") );
		explanationTextContent = new GUIContent( LocalizationManager.Instance.getText("MENU_FB_EXPLAIN") );
		guestButtonContent = new GUIContent( LocalizationManager.Instance.getText("MENU_GUEST") );
		okayButtonContent = new GUIContent( LocalizationManager.Instance.getText("MENU_OK") );
		closeButtonContent = new GUIContent( "X" );
		popupTitleContent = new GUIContent( LocalizationManager.Instance.getText("MENU_CONNECTING_TITLE") );
		popupTextContent = new GUIContent( LocalizationManager.Instance.getText("MENU_CONNECTING_TEXT") );

		//If this is a brand new user, show him the login screen where he will choose between Facebook and Guest.
		//If not, show him the main menu.
		displayLoginSelection = PlayerStatsManager.Instance.isFirstTimePlaying();

		//Main menu background
		backgroundRect = new Rect( 0,0, Screen.width, Screen.height );

		PopupHandler.changeFontSizeBasedOnResolution( buttonStyle );
		PopupHandler.changeFontSizeBasedOnResolution( textStyle );
		PopupHandler.changeFontSizeBasedOnResolution( titleStyle );
		PopupHandler.changeFontSizeBasedOnResolution( closeStyle );
		PopupHandler.changeFontSizeBasedOnResolution( okayStyle );
	}

	void Start()
	{
		//Display the daily bonus screen
		MyUpsightManager.requestLocalizedContent( "daily_bonus" );
	}

	public void updateState( FacebookState newState )
	{
		if( newState == FacebookState.LoggedIn )
		{
			popupTitleContent = new GUIContent( LocalizationManager.Instance.getText("MENU_SUCCESS_TITLE") );
			popupTextContent = new GUIContent( LocalizationManager.Instance.getText("MENU_SUCCESS_TEXT") );

		}
		else if ( newState == FacebookState.Error )
		{
			popupTitleContent = new GUIContent( LocalizationManager.Instance.getText("MENU_FB_ERROR_TITLE") );
			popupTextContent = new GUIContent( LocalizationManager.Instance.getText("MENU_FB_ERROR_TEXT") );
		}
		else if ( newState == FacebookState.Canceled )
		{
			PlayerStatsManager.Instance.setUsesFacebook( false );
			PlayerStatsManager.Instance.savePlayerStats();
			popupTitleContent = new GUIContent( LocalizationManager.Instance.getText("MENU_CONNECTION_CANCELED_TITLE") );
			popupTextContent = new GUIContent( LocalizationManager.Instance.getText("MENU_CONNECTION_CANCELED_TEXT") );
		}
		displayPopupButtons = true;
		displayLoginSelection = false;
	}


	void OnGUI ()
	{
		//Draw background
		GUI.DrawTexture( backgroundRect, backgroundTexture );

		if( displayPopup )
		{
			showPopup();			
		}
		else
		{
			if( displayLoginSelection )
			{
				drawLoginSelectionScreen();
			}
			else
			{
				drawMainMenu();
			}
		}
	}

	void drawMainMenu ()
	{
		//Draw centered Play! button	
		Rect textRect = GUILayoutUtility.GetRect( playButtonContent, buttonStyle );
		float textCenterX = (Screen.width-textRect.width)/2f;
		Rect playButtonRect = new Rect( textCenterX, Screen.height * 0.4f, textRect.width, textRect.height );
		if( GUI.Button( playButtonRect, playButtonContent, buttonStyle )) 
		{
			SoundManager.playButtonClick();
			StartCoroutine("loadLevel");
		}
	
		//Encourage player to connect to Facebook by leaving Connect button
		if( !FacebookManager.Instance.isLoggedIn() )
		{
			textRect = GUILayoutUtility.GetRect( fbButtonContent, buttonStyle );
			textCenterX = (Screen.width-textRect.width)/2f;
			Rect fbButtonRect = new Rect( textCenterX, Screen.height * 0.6f, textRect.width, textRect.height );
			if( GUI.Button( fbButtonRect, fbButtonContent, buttonStyle )) 
			{
				SoundManager.playButtonClick();
				if( Application.internetReachability != NetworkReachability.NotReachable )
				{
					popupTitleContent = new GUIContent( LocalizationManager.Instance.getText("MENU_CONNECTING_TITLE") );
					popupTextContent = new GUIContent( LocalizationManager.Instance.getText("MENU_CONNECTING_TEXT") );
					FacebookManager.Instance.CallFBInit( updateState );
				}
				else
				{
					popupTitleContent = new GUIContent( LocalizationManager.Instance.getText("MENU_CONNECTION_FAILED_TITLE") );
					popupTextContent = new GUIContent( LocalizationManager.Instance.getText("MENU_CONNECTION_FAILED_TEXT") );
					displayPopupButtons = true;
				}
				displayPopup = true;
			}
		}

		//Welcome user by first name (used for debugging Facebook)
		if( FacebookManager.Instance.isLoggedIn() && FacebookManager.Instance.Username != null )
		{
			GUIContent userNameContent = new GUIContent( LocalizationManager.Instance.getText("MENU_WELCOME") + " " + FacebookManager.Instance.Username );
			textStyle.fixedWidth = Screen.width * 0.5f;
			textRect = GUILayoutUtility.GetRect( userNameContent, textStyle );
			textCenterX = (Screen.width-textRect.width)/2f;
			float textHeight = 0.85f;
			Rect userNameTextRect = new Rect( textCenterX, Screen.height * textHeight, textRect.width, textRect.height );
			Rect userNameTextRectDropShadow = new Rect( textCenterX + 1, (Screen.height * textHeight) + 2, textRect.width, textRect.height );
			textStyle.normal.textColor = Color.black;
			GUI.Label ( userNameTextRectDropShadow, userNameContent, textStyle );
			textStyle.normal.textColor = Color.white;
			GUI.Label ( userNameTextRect, userNameContent, textStyle );
		}
	}

	void drawLoginSelectionScreen()
	{

		//Draw Connect with Facebook button. It needs to be centered.
		Rect textRect = GUILayoutUtility.GetRect( fbButtonContent, buttonStyle );
		float textCenterX = (Screen.width-textRect.width)/2f;
		Rect fbButtonRect = new Rect( textCenterX, Screen.height * 0.35f, textRect.width, textRect.height );
		if( GUI.Button( fbButtonRect, fbButtonContent, buttonStyle )) 
		{
			SoundManager.playButtonClick();
			if( Application.internetReachability != NetworkReachability.NotReachable )
			{
				PlayerStatsManager.Instance.setUsesFacebook( true );
				PlayerStatsManager.Instance.savePlayerStats();
				popupTitleContent = new GUIContent( LocalizationManager.Instance.getText("MENU_CONNECTING_TITLE") );
				popupTextContent = new GUIContent( LocalizationManager.Instance.getText("MENU_CONNECTING_TEXT") );
				FacebookManager.Instance.CallFBInit( updateState );
			}
			else
			{
				popupTitleContent = new GUIContent( LocalizationManager.Instance.getText("MENU_CONNECTION_FAILED_TITLE") );
				popupTextContent = new GUIContent( LocalizationManager.Instance.getText("MENU_CONNECTION_FAILED_TEXT") );
				displayPopupButtons = true;
			}
			displayPopup = true;
		}

		//Draw box with text explaining advantages of connecting with Facebook
		textStyle.fixedWidth = Screen.width * 0.7f;
		textRect = GUILayoutUtility.GetRect( explanationTextContent, textStyle );
		textCenterX = (Screen.width-textRect.width)/2f;
		float textHeight = 0.45f;
		Rect explanationTextRect = new Rect( textCenterX, Screen.height * textHeight, textRect.width, textRect.height );
		Rect explanationTextRectDropShadow = new Rect( textCenterX + 1, (Screen.height * textHeight) + 2, textRect.width, textRect.height );
		textStyle.normal.textColor = Color.black;
		GUI.Label ( explanationTextRectDropShadow, explanationTextContent, textStyle );
		textStyle.normal.textColor = Color.white;
		GUI.Label ( explanationTextRect, explanationTextContent, textStyle );

		//Draw Guest connection button
		textRect = GUILayoutUtility.GetRect( guestButtonContent, buttonStyle );
		textCenterX = (Screen.width-textRect.width)/2f;
		Rect guestButtonRect = new Rect( textCenterX, Screen.height * 0.6f, textRect.width, textRect.height );
		if( GUI.Button( guestButtonRect, guestButtonContent, buttonStyle )) 
		{
			SoundManager.playButtonClick();
			PlayerStatsManager.Instance.setUsesFacebook( false );
			PlayerStatsManager.Instance.savePlayerStats();
			displayLoginSelection = false;
		}

	}

	void showPopup()
	{

		float popupWidth = 0.8f * Screen.width;
		float popupHeight = 0.5f * Screen.height;
		Rect popupRect = new Rect( (Screen.width - popupWidth)/2, (Screen.height - popupHeight)/2 , popupWidth, popupHeight );
		GUI.BeginGroup( popupRect );

		//Box
		GUI.Box(new Rect(0, popupHeight * 0.1f, popupWidth, popupHeight * 0.9f), "" );

		//Title
		Rect textRect = GUILayoutUtility.GetRect( popupTitleContent, titleStyle );
		float textCenterX = (popupRect.width-textRect.width)/2f;
		Rect connectingTextRect = new Rect( textCenterX, popupHeight * 0.13f, textRect.width, textRect.height );
		Rect connectingTextRectDropShadow = new Rect( connectingTextRect.x + 1, connectingTextRect.y + 2, textRect.width, textRect.height );
		titleStyle.normal.textColor = Color.black;
		GUI.Label ( connectingTextRectDropShadow, popupTitleContent, titleStyle );
		titleStyle.normal.textColor = Color.white;
		GUI.Label ( connectingTextRect, popupTitleContent, titleStyle );

		//Text
		textStyle.fixedWidth = popupWidth * 0.9f;
		textRect = GUILayoutUtility.GetRect( popupTextContent, textStyle );
		textCenterX = (Screen.width-textRect.width)/2f;
		float textHeight = 0.35f;
		connectingTextRect = new Rect( (popupWidth - textRect.width)/2, popupHeight * textHeight, textRect.width, textRect.height );
		connectingTextRectDropShadow = new Rect( connectingTextRect.x + 1, connectingTextRect.y + 2, textRect.width, textRect.height );
		textStyle.normal.textColor = Color.black;
		GUI.Label ( connectingTextRectDropShadow, popupTextContent, textStyle );
		textStyle.normal.textColor = Color.white;
		GUI.Label ( connectingTextRect, popupTextContent, textStyle );

		if( displayPopupButtons )
		{
			//Draw Close button (top right)
			float buttonWidth = Screen.width * 0.10f;
			Rect closeButtonRect = new Rect( popupRect.width - (buttonWidth * 1.1f), (buttonWidth * 1.1f), buttonWidth, buttonWidth );
			if( GUI.Button( closeButtonRect, closeButtonContent, closeStyle )) 
			{
				SoundManager.playButtonClick();
				displayPopup = false;
			}

			//Draw Okay button (bottom center)
			buttonWidth = Screen.width * 0.13f;
			Rect okayButtonRect = new Rect( (popupRect.width - buttonWidth) /2, popupRect.height - (buttonWidth * 1.1f), buttonWidth, buttonWidth );
			if( GUI.Button( okayButtonRect, okayButtonContent, okayStyle )) 
			{
				SoundManager.playButtonClick();
				displayPopup = false;
			}
		}


		GUI.EndGroup();
	}

	IEnumerator loadLevel()
	{
		Handheld.StartActivityIndicator();
		yield return new WaitForSeconds(0);
		//Load world map
		Application.LoadLevel( 3 );

	}
}
