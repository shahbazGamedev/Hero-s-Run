using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;



public enum PopupType {
	None = 0,
	OfferLives = 2
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

	//Message
	public GUIStyle messageStyle;

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

	//Spinner
	public Texture spinner;
	float spinSpeed = 240f;

	//Font size (includes all buttons, message and title)
	int fontSize;

	//Facebook portraits
	public Texture defaultPortrait;
	public Texture portraitFrame;

	//Offer Lives
	OfferLivesHandler offerLivesHandler;
	
	//For user message
	Rect userMessageRect;
	Vector2 userMessageSize;
	GUIContent userMessageTitleContent = new GUIContent("Title");
	GUIContent textContent = new GUIContent("This is some text.");
	public GUIStyle textStyle;
	bool displayPopupButtons = true;
	
	void Awake()
	{
		originalPopupRect = new Rect( (Screen.width - popupSize.x)/2, (Screen.height - popupSize.y)/2 , popupSize.x, popupSize.y );
		popupRect = new Rect( originalPopupRect.x, originalPopupRect.y , originalPopupRect.width, originalPopupRect.height );
		
		//Standard heights
		titleHeight = 0.05f * popupRect.height;

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
		//Offer Lives to friends
		offerLivesHandler = worldMapHandlerGameObject.GetComponent<OfferLivesHandler>();
	}

	void Start()
	{
		popupStack.Clear();
		popupStack.Push( PopupType.None );

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
	}
	
	public void resetPopupSize()
	{
		popupSize = new Vector2( originalPopupSize.x, originalPopupSize.y);
		popupRect = originalPopupRect;
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
		SoundManager.soundManager.playButtonClick();
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

		case PopupType.OfferLives:
			offerLivesHandler.renderOfferLives();
			break;

		}
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
			SoundManager.soundManager.playButtonClick();
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
			SoundManager.soundManager.playButtonClick();
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
			SoundManager.soundManager.playButtonClick();
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
			SoundManager.soundManager.playButtonClick();
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
				SoundManager.soundManager.playButtonClick();
			}
			
			//Draw Okay button (bottom center)
			buttonWidth = Screen.width * 0.13f;
			Rect okayButtonRect = new Rect( (userMessageRect.width - buttonWidth) /2, userMessageRect.height - (buttonWidth * 1.1f), buttonWidth, buttonWidth );
			if( GUI.Button( okayButtonRect, "Okay", textStyle )) 
			{
				SoundManager.soundManager.playButtonClick();
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
		
		case PopupType.OfferLives:
			title = LocalizationManager.Instance.getText("POPUP_TITLE_OFFER_LIVES");
			break;

		}
		return title;
	}

}
