using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;



public enum PopupType {
	None = 0,
	FriendsOrPay = 1,
	AskFriendsUnlock = 2,
	ThreeFriends = 3,
	PurchaseUnlockNow = 4,
	MessageCenter = 5,
	AskLife = 6,
	AskFriendsLife = 7,
	UnlockNow = 8,
	DebugPopup = 9,
	OfferLives = 10,
	SaveMe = 11,
	Settings = 12,
	PauseMenu = 13,
	BoostsMenu = 14
}

public class PopupHandler : MonoBehaviour {

	//Dragon Run GUI skin
	public GUISkin dragonRunSkin;

	//Control
	Stack<PopupType> popupStack = new Stack<PopupType>();

	//Close button
	public GUIStyle closeButtonStyle;
	Vector2 closeButtonSize	= new Vector2( Screen.width * 0.12f, Screen.width * 0.12f );

	//Typical button
	public GUIStyle buttonStyle;

	//Icon
	public Texture facebookIcon;
	public Texture soundIcon;
	public Texture informationIcon;
	public Texture lifeIcon;
	public Texture sectionIcon;
	public Texture gameCenterIcon;
	public Texture restoreIcon;
	Vector2 iconSize = new Vector2( Screen.width * 0.15f, Screen.width * 0.15f );
	//Standard icon height accross all popups. This is also use for Facebook portraits.
	float iconHeight;

	//Message
	public GUIStyle messageStyle;
	float messageHeight;

	//Popup display
	Rect popupRect;
	Rect originalPopupRect;
	static Vector2 originalPopupSize = new Vector2( Screen.width * 0.8f, Screen.height * 0.55f );
	Vector2 popupSize = new Vector2( originalPopupSize.x, originalPopupSize.y );

	//Popup background texture
	public Texture popupBackground;

	//Title
	public GUIStyle titleStyle;
	GUIContent titleContent = new GUIContent( System.String.Empty);

	float titleHeight;

	//Button
	Vector2 buttonSize;
	float buttonXOffset;
	float buttonHeight;

	//Spinner
	public Texture spinner;
	float spinSpeed = 240f;

	//Font size (includes all buttons, message and title)
	int fontSize;

	//Three friends
	List<string> unlockFromIDList = new List<string>();

	//Facebook portraits
	public Texture defaultPortrait;
	public Texture portraitFrame;
	Vector2 popupPortraitSize;

	//messageCenterHandler
	MessageCenterHandler messageCenterHandler;

	//Offer Lives
	OfferLivesHandler offerLivesHandler;

	PauseMenu pauseMenu;
	BoostsMenu boostsMenu;


	//For user message
	Rect userMessageRect;
	Vector2 userMessageSize;
	GUIContent userMessageTitleContent = new GUIContent("Title");
	GUIContent textContent = new GUIContent("This is some text.");
	public GUIStyle textStyle;
	bool displayPopupButtons = true;
	bool displayPopup;
	
	void Awake()
	{
		originalPopupRect = new Rect( (Screen.width - popupSize.x)/2, (Screen.height - popupSize.y)/2 , popupSize.x, popupSize.y );
		popupRect = new Rect( originalPopupRect.x, originalPopupRect.y , originalPopupRect.width, originalPopupRect.height );

		buttonSize = new Vector2( popupSize.x * 0.8f, popupSize.y * 0.1f );
		buttonXOffset = (popupRect.width - buttonSize.x)/2f;
		
		popupPortraitSize 	 = new Vector2( Screen.width * 0.15f, Screen.width * 0.15f );

		//Standard heights
		titleHeight = 0.05f * popupRect.height;
		iconHeight = 0.2f * popupRect.height;
		messageHeight = 0.15f * popupRect.height;
		buttonHeight = 0.85f * popupRect.height;

		//Font size - default size is 26
		//Adjust font sizes based on screen resolution
		fontSize = getResolutionBasedFontSize(26);
		closeButtonStyle.fontSize = fontSize;
		buttonStyle.fontSize = fontSize;
		messageStyle.fontSize = fontSize;
		titleStyle.fontSize = fontSize;
		changeFontSizeBasedOnResolution( textStyle );

		//User Message
		userMessageSize = new Vector2( popupSize.x * 0.8f, popupSize.y * 0.8f );
		userMessageRect = new Rect( (popupSize.x - userMessageSize.x)/2, (popupSize.y - userMessageSize.y)/2, userMessageSize.x, userMessageSize.y );

	}

	public void setWorldMapHandler( GameObject worldMapHandlerGameObject )
	{
		//Message Center
		messageCenterHandler = worldMapHandlerGameObject.GetComponent<MessageCenterHandler>();
		//Offer Lives to friends
		offerLivesHandler = worldMapHandlerGameObject.GetComponent<OfferLivesHandler>();
	}

	public void setPauseMenu( PauseMenu pauseMenu )
	{
		this.pauseMenu = pauseMenu;
	}

	public void setBoostsMenu( BoostsMenu boostsMenu )
	{
		this.boostsMenu = boostsMenu;
	}

	void Start()
	{
		popupStack.Clear();
		popupStack.Push( PopupType.None );

	}

	void OnEnable()
	{
		// We must first hook up listeners to Unibill's events.
		Unibiller.onTransactionsRestored += onTransactionsRestored;
	}

	void OnDisable()
	{
		Unibiller.onTransactionsRestored -= onTransactionsRestored;
	}

	public void activatePopup( PopupType popupType )
	{
		popupStack.Push( popupType );
	}

	public void setPopupRect( Rect newPopupRect )
	{
		popupRect = newPopupRect;
	}

	public void resetPopupRect()
	{
		popupRect = originalPopupRect;
	}

	public void setPopupSize( Vector2 newSize )
	{
		popupSize = new Vector2( newSize.x, newSize.y);
		popupRect = new Rect( (Screen.width - popupSize.x)/2, (Screen.height - popupSize.y)/2 , popupSize.x, popupSize.y );
		buttonSize = new Vector2( popupSize.x * 0.8f, popupSize.y * 0.1f );
		buttonXOffset = (popupRect.width - buttonSize.x)/2f;
	}
	
	public void resetPopupSize()
	{
		popupSize = new Vector2( originalPopupSize.x, originalPopupSize.y);
		popupRect = originalPopupRect;
		buttonSize = new Vector2( popupSize.x * 0.8f, popupSize.y * 0.1f );
		buttonXOffset = (popupRect.width - buttonSize.x)/2f;
	}

	public bool isPopupDisplayed()
	{
		if( popupStack.Peek() != PopupType.None )
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public void closePopup()
	{
		SoundManager.playButtonClick();
		popupStack.Clear();
		popupStack.Push( PopupType.None );
	}

	public void closePopupNoSound( )
	{
		popupStack.Clear();
		popupStack.Push( PopupType.None );
	}

	void OnGUI ()
	{
		if( popupStack.Peek() != PopupType.None )
		{
			GUI.ModalWindow(0, popupRect, (GUI.WindowFunction) showPopup, "");
		}
	}

	void showPopup( int windowID )
	{
		PopupType type = popupStack.Peek();

		drawBackground();

		drawTitle( type );

		//Popup specific
		switch (type)
		{
		case PopupType.AskLife:
			renderAskLife();
			break;

		case PopupType.AskFriendsLife:
			renderAskFriendsLife();
			break;

		case PopupType.FriendsOrPay:
			renderFriendsOrPay();
			break;

		case PopupType.ThreeFriends:
			renderThreeFriends();
			break;

		case PopupType.PurchaseUnlockNow:
			renderPurchaseUnlockNow();
			break;

		case PopupType.MessageCenter:
			messageCenterHandler.renderMessageCenter();
			break;

		case PopupType.DebugPopup:
			renderDebugPopup();
			break;

		case PopupType.OfferLives:
			offerLivesHandler.renderOfferLives();
			break;

		case PopupType.Settings:
			renderSettingsMenu();
			break;

		case PopupType.PauseMenu:
			pauseMenu.renderPauseMenu();
			break;

		case PopupType.BoostsMenu:
			boostsMenu.renderBoostsMenu();
			break;
		}
	}

	//Step 1 - asking friends to send you a life
	//Called when player clicks on the Life button in WorldMapHandler
	void renderAskLife()
	{
		//Navigation button (top-right)
		drawCloseButton( closePopup );
		drawIcon( lifeIcon, iconHeight, true );
		Rect area = new Rect( buttonXOffset, buttonHeight, buttonSize.x, popupRect.height );
		GUILayout.BeginArea( area );
		GUILayout.BeginVertical();
		GUIContent buttonText = new GUIContent( LocalizationManager.Instance.getText("POPUP_ASK_FRIENDS") );
		drawButtonWithIcon( buttonText, askFriendsLife, facebookIcon );
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	//Step 2
	void askFriendsLife()
	{
		popupStack.Push(PopupType.AskFriendsLife);
	}

	//Step 3
	void renderAskFriendsLife()
	{
		//Navigation button (top-right)
		drawBackButton();
		drawIcon( facebookIcon, iconHeight, true );
		drawMessage( messageHeight, new GUIContent( LocalizationManager.Instance.getText("POPUP_ASK_FRIENDS_LIFE_MESSAGE") ) );
		Rect area = new Rect( buttonXOffset, buttonHeight, buttonSize.x, popupRect.height );
		GUILayout.BeginArea( area );
		GUILayout.BeginVertical();
		GUIContent buttonText = new GUIContent( LocalizationManager.Instance.getText("POPUP_ASK_FRIENDS") );
		drawButton( buttonText, askLifeFB );
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	//Step 4
	void askLifeFB()
	{
		//We need to close the popup modal window in the Unity Editor before we can open the Facebook window which is also modal.
		#if UNITY_EDITOR
		closePopup();
		#endif
		string message = LocalizationManager.Instance.getText( "APP_REQUEST_MESSAGE_UNLOCK" );
		FacebookManager.Instance.CallAppRequestAsFriendSelector( "App Requests", message, "app_users", "Ask_Give_Life,1", "", "" );
	}

	//-------------------------------------------------------------

	//Step 1 - asking friends to help you unlock a level or pay to unlock it immediately
	//Called when player clicks on a Section button in WorldMapHandler
	void renderFriendsOrPay()
	{
		drawCloseButton( closePopup );
		drawIcon( sectionIcon, iconHeight, true );
		Rect area = new Rect( buttonXOffset, 0.68f * popupRect.height, buttonSize.x, popupRect.height );
		GUILayout.BeginArea( area );
		GUILayout.BeginVertical();
		GUIContent buttonText = new GUIContent( LocalizationManager.Instance.getText("POPUP_ASK_FRIENDS") );
		drawButtonWithIcon( buttonText, askThreeFriends, facebookIcon );
		buttonText = new GUIContent( LocalizationManager.Instance.getText("POPUP_TITLE_UNLOCK_NOW") );
		drawButtonWithIcon( buttonText, purchaseUnlockNow, sectionIcon );
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	//Step 2 - A
	void askThreeFriends()
	{
		unlockFromIDList = PlayerStatsManager.Instance.getSaveUnlockRequests();
		print ("super!! " + unlockFromIDList.Count );
		popupStack.Push( PopupType.ThreeFriends );
	}

	//Step 2 - B
	void purchaseUnlockNow()
	{
		popupStack.Push(PopupType.PurchaseUnlockNow);
	}

	//Step 3 - A
	void renderThreeFriends()
	{
		
		drawBackButton();
		drawMessage( messageHeight, new GUIContent( LocalizationManager.Instance.getText("POPUP_ASK_FRIENDS_UNLOCK_MESSAGE") ) );
		
		//Verify if one or more friends have accepted our app request. If this is the case,
		//display the images of those friends.
		//If no friends have been asked or no friends have responded, show the Facebook icon with a text message instead.
		if( unlockFromIDList.Count > 0 )
		{
			float totalMargin = popupRect.width - ( popupPortraitSize.x * 3 );
			float margin = totalMargin/4f;
			float distanceBetweenPortraits = popupPortraitSize.x + margin;
			Rect friendPictureRect = new Rect( 0, iconHeight, popupPortraitSize.x, popupPortraitSize.y );
			for( int i =0; i < 3; i++ ) 
			{
				Texture picture;
				friendPictureRect.x = margin + (i * distanceBetweenPortraits);
				if ( i < unlockFromIDList.Count && FacebookManager.Instance.friendImages.TryGetValue( unlockFromIDList[i], out picture)) 
				{
					//We have the friend's picture
					drawPortrait( friendPictureRect, picture, false );
				}
				else if ( i < unlockFromIDList.Count && FacebookManager.Instance.friendImagesRequested.Contains( unlockFromIDList[i] ) )
				{
					//Picture has been requested but not received yet. Draw default portrait with a spinner on top.
					drawPortrait( friendPictureRect, defaultPortrait, true );
				}
				else
				{
					//Simply draw the default portrait
					drawPortrait( friendPictureRect, defaultPortrait, false );
				}
			}
		}
		else
		{
			
			drawIcon( facebookIcon, iconHeight, true );
		}
		
		Rect area = new Rect( buttonXOffset, buttonHeight, buttonSize.x, popupRect.height );
		GUILayout.BeginArea( area );
		GUILayout.BeginVertical();
		GUIContent buttonText = new GUIContent( LocalizationManager.Instance.getText("POPUP_ASK_FRIENDS") );
		drawButton( buttonText, askUnlockFB );
		GUILayout.EndVertical();
		GUILayout.EndArea();
		
	}

	//Step 3 - B
	void renderPurchaseUnlockNow()
	{
		
		drawBackButton();
		drawIcon( sectionIcon, iconHeight, true, -12 );
		drawMessage( messageHeight, new GUIContent( LocalizationManager.Instance.getText("POPUP_UNLOCK_NOW_PURCHASE_MESSAGE") ) );
		Rect area = new Rect( buttonXOffset, buttonHeight, buttonSize.x, popupRect.height );
		GUILayout.BeginArea( area );
		GUILayout.BeginVertical();
		GUIContent buttonText = new GUIContent( "0,99$" );
		drawButton( buttonText, askUnlockPurchase );
		GUILayout.EndVertical();
		GUILayout.EndArea();

	}

	//Step 4 - A
	void askUnlockFB()
	{
		#if UNITY_EDITOR
		closePopup();
		#endif
		string message = LocalizationManager.Instance.getText( "APP_REQUEST_MESSAGE_UNLOCK" );
		FacebookManager.Instance.CallAppRequestAsFriendSelector( "App Requests", message, "app_users", "Ask_Section_Unlock," + LevelManager.Instance.getNextSectionToUnlock().ToString(), "", "" );
	}

	//Step 4 - B
	void askUnlockPurchase()
	{
		//To do
		Debug.Log("PopupHandler-askUnlockPurchase: method not implemented yet.");
	}

	//-------------------------------------------------------------

	//Step 1
	void renderDebugPopup()
	{
		drawCloseButton( closePopup );
		Rect area = new Rect( buttonXOffset, 0.15f * popupRect.height, buttonSize.x, popupRect.height );
		GUILayout.BeginArea( area );
		GUILayout.BeginVertical();
		GUIContent buttonText = new GUIContent( "Reset stats" );
		drawButton( buttonText, resetPlayerStats );
		buttonText = new GUIContent( "Delete Requests" );
		drawButton( buttonText, deleteAllAppRequests );
		buttonText = new GUIContent( "Leaderboard" );
		drawButton( buttonText, showLeaderboard );
		buttonText = new GUIContent( "Achievements" );
		drawButton( buttonText, showAchievements );
		buttonText = new GUIContent( "Reset Achievements" );
		drawButton( buttonText, resetAchievements );
		buttonText = new GUIContent( "Unlock All Levels" );
		drawButton( buttonText, unlockAllLevels );
		GUILayout.Label("Total Stars " + PlayerStatsManager.Instance.getLifetimeCoins() );
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	//Step 2 - A
	void resetPlayerStats()
	{
		PlayerStatsManager.Instance.resetPlayerStats();
	}
	
	//Step 2 - B
	void deleteAllAppRequests()
	{
		StartCoroutine( FacebookManager.Instance.deleteAllAppRequests() );
	}

	//Step 2 - C
	void showLeaderboard()
	{
		Social.ShowLeaderboardUI();
	}

	//Step 2 - D
	void showAchievements()
	{
		Social.ShowAchievementsUI();
	}

	//Step 2 - E
	void resetAchievements()
	{
		GameCenterPlatform.ResetAllAchievements( (resetResult) => {
			Debug.Log( (resetResult) ? "Achievement Reset succesfull." : "Achievement Reset failed." );
		});
		GameCenterManager.resetAchievementsCompleted();
	}

	//Step 2 - F
	void unlockAllLevels()
	{
		LevelManager.Instance.unlockAllLevels();
	}

	//-------------------------------------------------------------
	
	//Step 1
	void renderSettingsMenu()
	{
		//Navigation button (top-right)
		drawCloseButton( closeSettingsMenu );
		Rect area = new Rect( buttonXOffset, 0.25f * popupRect.height, buttonSize.x, popupRect.height );
		GUILayout.BeginArea( area );
		GUILayout.BeginVertical();
		GUIContent buttonText;
		if( FacebookManager.Instance.isLoggedIn() )
		{
			buttonText = new GUIContent( LocalizationManager.Instance.getText("POPUP_BUTTON_FB_DISCONNECT") );
			drawButtonWithIcon( buttonText, logout, facebookIcon );
		}
		else
		{
			buttonText = new GUIContent( LocalizationManager.Instance.getText("MENU_LOGGED_OUT") );
			Color originalColor = buttonStyle.normal.textColor;
			buttonStyle.normal.textColor = Color.gray;
			drawButtonWithIcon( buttonText, doNothing, facebookIcon );
			buttonStyle.normal.textColor = originalColor;
		}
		if( PlayerStatsManager.Instance.getSoundVolume() == 0 )
		{
			buttonText = new GUIContent( LocalizationManager.Instance.getText("MENU_OFF") );
			drawButtonWithIcon( buttonText, turnSoundOn, soundIcon );
		}
		else
		{
			buttonText = new GUIContent( LocalizationManager.Instance.getText("MENU_ON") );
			drawButtonWithIcon( buttonText, turnSoundOff, soundIcon );
		}
		buttonText = new GUIContent( LocalizationManager.Instance.getText("MENU_PRIVACY_POLICY") );
		drawButtonWithIcon( buttonText, privacyPolicy, informationIcon );
		buttonText = new GUIContent( LocalizationManager.Instance.getText("MENU_GLOBAL_LEADERBOARD") );
		drawButtonWithIcon( buttonText, showLeaderboard, gameCenterIcon );
		buttonText = new GUIContent( LocalizationManager.Instance.getText("MENU_RESTORE_PURCHASES") );
		drawButtonWithIcon( buttonText, restoreTransactions, restoreIcon );
		//Only dislay the More Games button if the Upsight Opt-out option is false. 
		//If Opt-out is true, the button will do nothing so there is no reason to display it.
		//if( !MyUpsightManager.getUpsightOptOutOption() )
		//{
		//	buttonText = new GUIContent( LocalizationManager.Instance.getText("MENU_MORE_GAMES") );
		//	drawButtonWithIcon( buttonText, moreGames, restoreIcon );
		//}
		buttonText = new GUIContent( PlayerStatsManager.Instance.getDifficultyLevelName() );
		drawButtonWithIcon( buttonText, changeDifficultyLevel, gameCenterIcon );
		GUILayout.EndVertical();
		GUILayout.EndArea();
		Rect versionRect = new Rect( buttonSize.x, 0.9f * popupRect.height, buttonSize.x, buttonSize.y );
		GUI.Label( versionRect, "v" + GameManager.Instance.getVersionNumber() );
	}

	//Step 2A

	public void closeSettingsMenu()
	{
		resetPopupSize();
		closePopup();
	}

	void logout()
	{
		//Coppa - when we log out of Facebook, we put the Upsight Opt-out option back to true to be safe.
		//MyUpsightManager.setUpsightOptOutOption( true );
		FacebookManager.Instance.CallFBLogout();
		PlayerStatsManager.Instance.setUsesFacebook( false );
		PlayerStatsManager.Instance.savePlayerStats();
	}

	//Step 2B
	void doNothing()
	{
	}
	
	//Step 3A
	void turnSoundOff()
	{
		AudioListener.volume = 0;
		PlayerStatsManager.Instance.setSoundVolume(0);
		PlayerStatsManager.Instance.savePlayerStats();
	}

	//Step 3B
	void turnSoundOn()
	{
		AudioListener.volume = 1f;
		PlayerStatsManager.Instance.setSoundVolume(1f);
		PlayerStatsManager.Instance.savePlayerStats();
	}

	//Step 4
	void privacyPolicy()
	{
		string privacyPolicyURL = "http://www.redlondongames.com/";
		Application.OpenURL(privacyPolicyURL);
	}

	//Step 5
	void restoreTransactions()
	{
		Unibiller.restoreTransactions();
	}

	private void onTransactionsRestored (bool success) {
		Debug.Log("Transactions restored: success " + success );
		drawUserMessage();

	}

	void moreGames()
	{
		//Make a content request for the Upsight more games list
		//Upsight.sendContentRequest( "more_games", true );
	}

	void changeDifficultyLevel()
	{
		DifficultyLevel newDifficultyLevel = PlayerStatsManager.Instance.getNextDifficultyLevel();
		//setDifficultyLevel takes care of saving the new value
		PlayerStatsManager.Instance.setDifficultyLevel(newDifficultyLevel);
	}

	//-------------------------------------------------------------

	//Draw the background
	void drawBackground()
	{
		GUI.DrawTexture(new Rect(0, 0, popupRect.width, popupRect.height), popupBackground, ScaleMode.StretchToFill );
	}

	//Title (top-center)
	void drawTitle( PopupType type )
	{
		titleContent.text = getTitle( type );
		Rect textRect = GUILayoutUtility.GetRect( titleContent, titleStyle );
		float textCenterX = (popupRect.width-textRect.width)/2f;
		Rect titleTextRect = new Rect( textCenterX, titleHeight, textRect.width, textRect.height );
		int maxFontSize = getMaximumFontSize( titleTextRect, titleStyle, titleContent, true, Vector2.zero );
		int originalFontSize = titleStyle.fontSize;
		titleStyle.fontSize = maxFontSize;
		Utilities.drawLabelWithDropShadow( titleTextRect, titleContent, titleStyle );
		titleStyle.fontSize = originalFontSize;
	}

	//Draw Close button (top right)
	public void drawCloseButton( System.Action onClick )
	{
		Rect closeButtonRect = new Rect( popupRect.width - closeButtonSize.x, 0, closeButtonSize.x, closeButtonSize.y );
		if( GUI.Button( closeButtonRect, "X", closeButtonStyle )) 
		{
			onClick.Invoke();
		}
	}

	//Draw Back button (top right)
	void drawBackButton()
	{
		Rect closeButtonRect = new Rect( popupRect.width - closeButtonSize.x, 0, closeButtonSize.x, closeButtonSize.y );
		if( GUI.Button( closeButtonRect, "<", closeButtonStyle )) 
		{
			SoundManager.playButtonClick();
			//Display the previous pop-up
			popupStack.Pop();
		}
	}

	//Draw an icon centered in x
	void drawIcon( Texture icon, float yHeight, bool usesTransparency )
	{
		drawIcon( icon, yHeight, usesTransparency, 0 );
	}

	//Draw an icon centered in x
	void drawIcon( Texture icon, float yHeight, bool usesTransparency, int angle )
	{
		Vector2 pivot = Vector2.zero;
		Matrix4x4 matrixBackup = GUI.matrix;	
		Rect iconRect = new Rect( (popupRect.width - iconSize.x)/2, yHeight, iconSize.x, iconSize.y );
		pivot.x = iconRect.center.x; 
		pivot.y = iconRect.center.y; 
		GUIUtility.RotateAroundPivot( angle, pivot );
		GUI.DrawTexture( iconRect, icon, ScaleMode.ScaleToFit, usesTransparency );
		GUI.matrix = matrixBackup;
	}

	//Draw a button. The button text will be centered. 
	public void drawButton( GUIContent buttonText, System.Action onClick )
	{
		buttonStyle.alignment =  TextAnchor.MiddleCenter;
		buttonStyle.contentOffset = Vector2.zero;
		if( GUILayout.Button( buttonText, buttonStyle )) 
		{
			SoundManager.playButtonClick();
			onClick.Invoke ();
		}
	}

	//Draw a button. The button text will be centered. If selected is true, the text
	//color will be yellow otherwise it will be the text color set in buttonStyle.
	public void drawButton( GUIContent buttonText, System.Action onClick, bool selected )
	{
		buttonStyle.alignment =  TextAnchor.MiddleCenter;
		buttonStyle.contentOffset = Vector2.zero;
		Color originalFontColor = buttonStyle.normal.textColor;
		if(selected) buttonStyle.normal.textColor = Color.yellow;
		if( GUILayout.Button( buttonText, buttonStyle )) 
		{
			SoundManager.playButtonClick();
			onClick.Invoke ();
		}
		buttonStyle.normal.textColor = originalFontColor;
	}

	//Draw a label. The text will be centered. The text color will be grey.
	public void drawInactiveButton( GUIContent buttonText )
	{
		buttonStyle.alignment =  TextAnchor.MiddleCenter;
		buttonStyle.contentOffset = Vector2.zero;
		Color originalFontColor = buttonStyle.normal.textColor;
		buttonStyle.normal.textColor = Color.grey;
		GUILayout.Label( buttonText, buttonStyle );
		buttonStyle.normal.textColor = originalFontColor;
	}

	//Draw a button centered in x with a Facebook icon to the left 
	void drawButtonWithIcon( GUIContent buttonText, System.Action onClick, Texture icon )
	{
		buttonStyle.alignment =  TextAnchor.MiddleLeft;
		Rect buttonRect = GUILayoutUtility.GetRect( buttonText, buttonStyle );
		buttonRect.x = 0;
		float iconSize = buttonRect.height * 0.9f;
		//Move the content to the right to make room for the icon
		Vector2 currentOffset = new Vector2(buttonRect.x + buttonRect.width * 0.02f + iconSize, 0);
		buttonStyle.contentOffset = currentOffset;

		int maxFontSize = getMaximumFontSize( buttonRect, buttonStyle, buttonText, false, buttonStyle.contentOffset );
		int originalFontSize = buttonStyle.fontSize;
		buttonStyle.fontSize = maxFontSize;

		if( GUI.Button( buttonRect, buttonText, buttonStyle )) 
		{
			SoundManager.playButtonClick();
			onClick.Invoke ();
		}
		buttonStyle.fontSize = originalFontSize;
		//Draw Facebook icon
		Rect iconRect = new Rect( buttonRect.x + buttonRect.width * 0.02f, buttonRect.y + (buttonRect.height - iconSize)/2f, iconSize, iconSize );
		GUI.DrawTexture( iconRect, icon, ScaleMode.ScaleToFit, true );
	}

	//Draw a text message using the defaut font
	public void drawMessage( float yHeight, GUIContent messageContent )
	{
		messageStyle.fixedWidth = popupRect.width * 0.9f;
		Rect textRect = GUILayoutUtility.GetRect( messageContent, messageStyle );
		Rect messageTextRect = new Rect( (popupRect.width - textRect.width)/2, yHeight, textRect.width, textRect.height );
		Utilities.drawLabelWithDropShadow( messageTextRect, messageContent, messageStyle );
	}

	//Draw a Facebook portrait with a frame around it
	//To do: spinner is not implemented yet
	public void drawPortrait( Rect portraitRect, Texture portrait, bool withSpinner )
	{
		GUI.DrawTexture( portraitRect, portrait, ScaleMode.ScaleToFit, false );
		//Draw a frame around the portrait
		Rect popupPortraitFrameRect = new Rect (0, 0, portraitRect.width * 1.3f, portraitRect.height * 1.3f);
		popupPortraitFrameRect.x = portraitRect.x - ( popupPortraitFrameRect.width  - portraitRect.width)/2f;
		popupPortraitFrameRect.y = portraitRect.y - ( popupPortraitFrameRect.height - portraitRect.height)/2f;
		GUI.DrawTexture( popupPortraitFrameRect, portraitFrame, ScaleMode.ScaleToFit, true );
		if( withSpinner )
		{
			drawSpinner( portraitRect );
		}
	}

	//Draw a the default Facebook portrait with a frame around it
	//To do: spinner is not implemented yet
	public void drawDefaultPortrait( Rect portraitRect, bool withSpinner )
	{
		GUI.DrawTexture( portraitRect, defaultPortrait, ScaleMode.ScaleToFit, false );
		//Draw a frame around the portrait
		Rect popupPortraitFrameRect = new Rect (0, 0, portraitRect.width * 1.3f, portraitRect.height * 1.3f);
		popupPortraitFrameRect.x = portraitRect.x - ( popupPortraitFrameRect.width  - portraitRect.width)/2f;
		popupPortraitFrameRect.y = portraitRect.y - ( popupPortraitFrameRect.height - portraitRect.height)/2f;
		GUI.DrawTexture( popupPortraitFrameRect, portraitFrame, ScaleMode.ScaleToFit, true );
		if( withSpinner )
		{
			drawSpinner( portraitRect );
		}
	}

	public void drawSpinner( Rect portraitRect )
	{
		Rect spinnerRect = new Rect (0, 0, portraitRect.width * 0.5f, portraitRect.height * 0.5f);
		spinnerRect.x = portraitRect.x + ( portraitRect.width  - spinnerRect.width)/2f;
		spinnerRect.y = portraitRect.y + ( portraitRect.height - spinnerRect.height)/2f;

		Vector2 pivot = new Vector2( spinnerRect.center.x, spinnerRect.center.y );
		Matrix4x4 matrixBackup = GUI.matrix;	
		float angle = Time.time * spinSpeed;
		GUIUtility.RotateAroundPivot( angle, pivot );
		GUI.DrawTexture( spinnerRect, spinner, ScaleMode.ScaleToFit, true );
		GUI.matrix = matrixBackup;
	}

	void drawUserMessage()
	{
		
		float popupWidth = 0.8f * Screen.width;
		float popupHeight = 0.5f * Screen.height;
		GUI.BeginGroup( userMessageRect );
		
		//Box
		GUI.Box(new Rect(0, 0, userMessageRect.width, userMessageRect.height), "" );
		
		//Title
		Rect textRect = GUILayoutUtility.GetRect( userMessageTitleContent, titleStyle );
		float textCenterX = (userMessageRect.width-textRect.width)/2f;
		Rect connectingTextRect = new Rect( textCenterX, popupHeight * 0.13f, textRect.width, textRect.height );
		Rect connectingTextRectDropShadow = new Rect( connectingTextRect.x + 1, connectingTextRect.y + 2, textRect.width, textRect.height );
		titleStyle.normal.textColor = Color.black;
		GUI.Label ( connectingTextRectDropShadow, userMessageTitleContent, titleStyle );
		titleStyle.normal.textColor = Color.white;
		GUI.Label ( connectingTextRect, userMessageTitleContent, titleStyle );
		
		//Text
		textStyle.fixedWidth = popupWidth * 0.9f;
		textRect = GUILayoutUtility.GetRect( textContent, textStyle );
		textCenterX = (userMessageRect.width-textRect.width)/2f;
		float textHeight = 0.35f;
		connectingTextRect = new Rect( (popupWidth - textRect.width)/2, popupHeight * textHeight, textRect.width, textRect.height );
		connectingTextRectDropShadow = new Rect( connectingTextRect.x + 1, connectingTextRect.y + 2, textRect.width, textRect.height );
		textStyle.normal.textColor = Color.black;
		GUI.Label ( connectingTextRectDropShadow, textContent, textStyle );
		textStyle.normal.textColor = Color.white;
		GUI.Label ( connectingTextRect, textContent, textStyle );
		
		if( displayPopupButtons )
		{
			//Draw Close button (top right)
			float buttonWidth = Screen.width * 0.10f;
			Rect closeButtonRect = new Rect( userMessageRect.width - (buttonWidth * 1.1f), (buttonWidth * 1.1f), buttonWidth, buttonWidth );
			if( GUI.Button( closeButtonRect, "X", textStyle )) 
			{
				SoundManager.playButtonClick();
				displayPopup = false;
			}
			
			//Draw Okay button (bottom center)
			buttonWidth = Screen.width * 0.13f;
			Rect okayButtonRect = new Rect( (userMessageRect.width - buttonWidth) /2, userMessageRect.height - (buttonWidth * 1.1f), buttonWidth, buttonWidth );
			if( GUI.Button( okayButtonRect, "Okay", textStyle )) 
			{
				SoundManager.playButtonClick();
				displayPopup = false;
			}
		}
		
		
		GUI.EndGroup();
	}

	//Assumes all font size are adjusted for an iPhone 5 which has a width of 640 pixels.
	public static int getResolutionBasedFontSize( int originalFontSize )
	{
		return (int) (originalFontSize * Screen.width/640f);

	}

	//Assumes all font size are adjusted for an iPhone 5 which has a width of 640 pixels.
	public static void changeFontSizeBasedOnResolution( GUIStyle style )
	{
		style.fontSize = (int)(style.fontSize * Screen.width/640f);
	}

	public int getMaximumFontSize( Rect textAreaRect, GUIStyle style, GUIContent content, bool maximizeFontSize, Vector2 contentOffset )
	{
		int ajustedFontSize = 0;
		Vector2 textSize = style.CalcSize( content );
		if( maximizeFontSize )
		{
			//Make the font as big as possible.
			//font ratio needed to ensure text is as big as it can be while fitting in the space
			float ratioW = (textAreaRect.width-contentOffset.x)/textSize.x;
			float ratioH = (textAreaRect.height-contentOffset.y)/textSize.y;
			float ratio = Mathf.Min(ratioW, ratioH);
			ajustedFontSize = (int)(style.fontSize * ratio);
		}
		else
		{
			//Only change the font size if necessary.
			if( textSize.x > textAreaRect.width || textSize.y > textAreaRect.height )
			{
				//Yes, we do need to resize.
				//Adjust font size to ensure text is as big as it can be while fitting in the space.
				float ratioW = (textAreaRect.width-contentOffset.x)/textSize.x;
				float ratioH = (textAreaRect.height-contentOffset.y)/textSize.y;
				float ratio = Mathf.Min(ratioW, ratioH);
				ajustedFontSize = (int)(style.fontSize * ratio);
			}
			else
			{
				//Nothing to do. Return the original font size.
				ajustedFontSize = style.fontSize;
			}
		}
		return ajustedFontSize;
	}

	string getTitle( PopupType popupType )
	{
		string title = "";
		switch (popupType)
		{
		case PopupType.FriendsOrPay:
			title = LocalizationManager.Instance.getText("POPUP_TITLE_UNLOCK_LEVELS");
			break;
			
		case PopupType.ThreeFriends:
		case PopupType.AskFriendsLife:
			title = LocalizationManager.Instance.getText("POPUP_TITLE_ASK_FRIENDS");
			break;
		
		case PopupType.AskLife:
			title = LocalizationManager.Instance.getText("POPUP_TITLE_ASK_LIVES");
			break;

		case PopupType.PurchaseUnlockNow:
		case PopupType.UnlockNow:
			title = LocalizationManager.Instance.getText("POPUP_TITLE_UNLOCK_NOW");
			break;

		case PopupType.MessageCenter:
			title = LocalizationManager.Instance.getText("POPUP_TITLE_MESSAGE_CENTER");
			break;

		case PopupType.DebugPopup:
			title = LocalizationManager.Instance.getText("POPUP_TITLE_DEBUG");
			break;
		case PopupType.OfferLives:
			title = LocalizationManager.Instance.getText("POPUP_TITLE_OFFER_LIVES");
			break;

		case PopupType.Settings:
			title = LocalizationManager.Instance.getText("POPUP_TITLE_SETTINGS");
			break;

		case PopupType.PauseMenu:
			title = LocalizationManager.Instance.getText("MENU_PAUSE");;
			break;

		case PopupType.BoostsMenu:
			title = LocalizationManager.Instance.getText("MENU_BOOSTS");
			break;
		}
		return title;
	}

}
