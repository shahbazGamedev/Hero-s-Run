using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OfferLivesHandler : MonoBehaviour {

	GUIContent messageCenterCheckAllButton;
	GUIContent messageCenterAcceptButton;
	string haveLifeMessage;
	
	//Message center other
	Vector2 messageCenterPopupSize;
	float messageCenterMargin;
	Rect scrollView;
	Rect scrollViewTouchZone;
	Rect scrollComplete;
	Vector2 scrollPosition;
	Vector2 rowSize;
	Vector2 facebookPortraitSize;

	Rect popupRect;

	Vector2 iconSize;
	Vector2 toggleSize;
	
	//Scrolling
	Vector2 screenTouchPosition;

	//Entry background texture
	public Texture entryBackground;

	//Check All button state
	bool isCheckAllSelected = false;

	PopupHandler popupHandler;


	// Use this for initialization
	void Awake () {
		
		Transform CoreManagers = GameObject.FindGameObjectWithTag("CoreManagers").transform;
		popupHandler = CoreManagers.GetComponent<PopupHandler>();

		//Message center button text
		messageCenterCheckAllButton = new GUIContent( LocalizationManager.Instance.getText("POPUP_BUTTON_CHECK_ALL") );
		messageCenterAcceptButton 	= new GUIContent( LocalizationManager.Instance.getText("POPUP_BUTTON_ACCEPT") );
		haveLifeMessage 			= LocalizationManager.Instance.getText("FB_HAVE_A_LIFE_MESSAGE");

		//Message center other
		messageCenterPopupSize	= new Vector2( Screen.width * 0.8f, Screen.height * 0.55f );
		popupRect 				= new Rect( (Screen.width - messageCenterPopupSize.x)/2, (Screen.height - messageCenterPopupSize.y)/2 , messageCenterPopupSize.x, messageCenterPopupSize.y );
		rowSize 				= new Vector2( messageCenterPopupSize.x * 0.95f, messageCenterPopupSize.y * 0.16f );
		scrollComplete			= new Rect(0, 0, rowSize.x * 0.95f, rowSize.y);
		messageCenterMargin 	= (messageCenterPopupSize.x - rowSize.x)/2f;
		scrollView 				= new Rect(messageCenterMargin, messageCenterPopupSize.y * 0.16f, rowSize.x, 3.5f * rowSize.y);
		//The scrollViewTouchZone is extended up and down by rowSize.y/2. When we scroll, our finger tends to go outside the scroll area but we still want to be able to scroll normally. 
		scrollViewTouchZone 	= new Rect(popupRect.x + messageCenterMargin, popupRect.y + messageCenterPopupSize.y * 0.16f - rowSize.y/2, rowSize.x, 3.5f * rowSize.y + rowSize.y/2);
		iconSize 				= new Vector2( Screen.width * 0.07f, Screen.width * 0.07f );

		facebookPortraitSize = new Vector2( Screen.width * 0.08f, Screen.width * 0.08f );
		toggleSize = new Vector2( facebookPortraitSize.x * 0.7f, facebookPortraitSize.y * 0.7f );

	}
	
	public void renderOfferLives()
	{
		popupHandler.drawCloseButton( closeOfferLives );

		GUI.skin = popupHandler.dragonRunSkin;

		//Draw message entries
		if( FacebookManager.Instance.isLoggedIn() )
		{
			drawMessageScrollWindow();
		}

		//Draw Check All button (bottom left)
		Vector2 buttonSize = new Vector2( Screen.width * 0.36f, Screen.width * 0.12f );
		Rect checkAllButtonRect = new Rect( messageCenterMargin, popupRect.height - buttonSize.y - messageCenterMargin, buttonSize.x, buttonSize.y );
		drawCheckAllButton( checkAllButtonRect );

		//Draw Accept button (bottom right)
		//buttonSize = acceptStyle.CalcSize( messageCenterAcceptButton );
		Rect acceptAllButtonRect = new Rect( popupRect.width - buttonSize.x  - messageCenterMargin, checkAllButtonRect.y, buttonSize.x, buttonSize.y );
		if( GUI.Button( acceptAllButtonRect, messageCenterAcceptButton, "Accept Button" )) 
		{
			print ("ACCEPT BUTTON PRESSED " + FacebookManager.Instance.friendsList.Count);
			popupHandler.closePopup();
			//Create a comma separated list of IDs when want to sent the life to
			string DirectRequestTo = "";
			foreach(KeyValuePair<string, FriendData> pair in FacebookManager.Instance.friendsList) 
			{
				if( pair.Value.isSelected )
				{
						DirectRequestTo = pair.Key + "," + DirectRequestTo;
				}
			}
			//Delete trailing comma
			DirectRequestTo = DirectRequestTo.TrimEnd(',');
			FacebookManager.Instance.CallAppRequestAsDirectRequest( "App Requests", haveLifeMessage, DirectRequestTo, "Accept_Give_Life,1", OLHCallback, null );
			resetEntries();
		}

		GUI.skin = null;
		
	}

	public void closeOfferLives()
	{
		popupHandler.closePopup();
		resetEntries();
	}

	void resetEntries()
	{
		//Reset values
		isCheckAllSelected = false;
		foreach(KeyValuePair<string, FriendData> pair in FacebookManager.Instance.friendsList) 
		{
			pair.Value.isSelected = false;
		}
	}

	void drawCheckAllButton( Rect buttonRect )
	{
		//Button background
		GUI.DrawTexture( buttonRect, entryBackground, ScaleMode.StretchToFill, true );

		//Toggle button
		Rect toggleRect = new Rect(buttonRect.x + 6, 0, toggleSize.x, toggleSize.y);
		float yOffset = buttonRect.y + (buttonRect.height - toggleRect.height)/2f;
		toggleRect.y = yOffset;
		Vector2 offset = new Vector2(toggleRect.width + 3,0);
		GUI.skin.FindStyle("Toggle").contentOffset = offset;
		bool newValue = GUI.Toggle(toggleRect,isCheckAllSelected, messageCenterCheckAllButton, "Toggle");

		//Did the value change?
		if( isCheckAllSelected != newValue )
		{
			SoundManager.playButtonClick();
			isCheckAllSelected = newValue;
			//Select all the entries
			foreach(KeyValuePair<string, FriendData> pair in FacebookManager.Instance.friendsList) 
			{
				pair.Value.isSelected = isCheckAllSelected;
			}
		}
	}

	public void OLHCallback(FBResult result, string appRequestIDToDelete )
	{
		if (result.Error != null)
		{
			Debug.Log ("OfferLivesHandler-Callback error:\n" + result.Error );
		}
		else
		{
			Debug.Log ("OfferLivesHandler-Callback success:\n" + result.Text );
			//OfferLivesHandler-Callback success: {"cancelled":true}
		}
	}

	void drawMessageScrollWindow() 
	{
		int numRows = FacebookManager.Instance.friendsList.Count;
		int iRow = 0;
		scrollComplete.height = numRows * rowSize.y;
		
		scrollPosition = GUI.BeginScrollView (scrollView, scrollPosition, scrollComplete, false, false);

		Rect rBtn = new Rect(0, 0, scrollComplete.width, rowSize.y);
		foreach(KeyValuePair<string, FriendData> pair in FacebookManager.Instance.friendsList) 
		{
			// draw call optimization: don't actually draw the row if it is not visible
			if ( rBtn.yMax >= scrollPosition.y && rBtn.yMin <= (scrollPosition.y + scrollView.height) )
			{
				drawMessageEntry( iRow, rBtn.y, pair.Key, pair.Value );
			}
			rBtn.y += rowSize.y;
			iRow++;

		}
		GUI.EndScrollView();
		//Count number of selected entries. If the player manually selected all the entries, we want to automatically turn on the Check All toggle.
		int totalSelected = 0;
		foreach(KeyValuePair<string, FriendData> pair in FacebookManager.Instance.friendsList) 
		{
			if( pair.Value.isSelected ) totalSelected++;
		}
		if(totalSelected == numRows)
		{
			isCheckAllSelected = true;
		}
		else
		{
			isCheckAllSelected = false;
		}

		handleTouchScroll();
		
	}

	void drawMessageEntry( int iRow, float yPos, string friendID, FriendData friendData )
	{
		float gap = 14;

		//checkbox, button background, facebook portrait, facebook frame, entry title, entry text, entry type icon
		//If you get help a request, the title is "Help your friend!" and the background is pink.
		//If a friend helped you, the title is "You got help! and the background is green.
		Rect entryRect = new Rect(0, yPos, scrollComplete.width, rowSize.y);
		GUI.BeginGroup( entryRect );
		
		//Entry background
		GUI.color = Color.red;
		Rect rBtn = new Rect(0, 0, scrollComplete.width, rowSize.y);
		GUI.DrawTexture( rBtn, entryBackground, ScaleMode.StretchToFill, true );
		GUI.color = Color.white;

		//Toggle button
		Rect toggleRect = new Rect(gap, 0, toggleSize.x, toggleSize.y);
		float yOffset = (rowSize.y - toggleRect.height)/2f;
		toggleRect.y = yOffset;
		friendData.isSelected = GUI.Toggle(toggleRect,friendData.isSelected,"", "Toggle");

		//Facebook portrait
		float xOffset = toggleRect.xMax + gap;
		yOffset = (rowSize.y - facebookPortraitSize.y)/2f;
		Rect friendPictureRect = new Rect( xOffset, yOffset, facebookPortraitSize.x, facebookPortraitSize.y );
		
		Texture picture;
		if (FacebookManager.Instance.friendImages.TryGetValue( friendID, out picture)) 
		{
			//We have the friend's picture
			popupHandler.drawPortrait( friendPictureRect, picture, false );
		}
		else if ( FacebookManager.Instance.friendImagesRequested.Contains( friendID ) )
		{
			//Picture has been requested but not received yet. Draw default portrait with a spinner on top.
			popupHandler.drawDefaultPortrait( friendPictureRect, true );
		}
		else
		{
			//Simply draw the default portrait
			popupHandler.drawDefaultPortrait( friendPictureRect, false );
		}
		
		//Draw entry title label
		xOffset = friendPictureRect.xMax + gap;
		yOffset = 7;
		float length = scrollComplete.width - xOffset - facebookPortraitSize.x - gap;
		Rect entryTitleRect = new Rect( xOffset, yOffset, length, rowSize.y );
		GUI.Label(entryTitleRect, LocalizationManager.Instance.getText( "MESSAGE_ENTRY_TITLE_EXTRA_LIFE" ), "Entry Title" );
		
		//Draw entry text label
		xOffset = friendPictureRect.xMax + gap;
		yOffset = 30;
		length = scrollComplete.width - xOffset - facebookPortraitSize.x - gap;
		Rect entryTextRect = new Rect( xOffset, yOffset, length, rowSize.y );

		string entryText = LocalizationManager.Instance.getText( "MESSAGE_ENTRY_TEXT_EXTRA_LIFE" );
		entryText = entryText.Replace("<first_name>", friendData.first_name );
		GUI.Label(entryTextRect, entryText, "Entry Message");
		
		//Draw request type icon (Give life is a heart for example)
		xOffset = scrollComplete.width - iconSize.x - gap;
		yOffset = (rowSize.y - iconSize.y)/2f;
		Rect iconRect = new Rect( xOffset, yOffset, iconSize.x, iconSize.y );
		GUI.DrawTexture( iconRect, popupHandler.lifeIcon, ScaleMode.ScaleToFit, true );
		
		
		GUI.EndGroup();
		
	}


	float timeTouchPhaseEnded;
	float scrollVelocity;
	float inertiaDuration = 1.25f;
	
	void handleTouchScroll()
	{
		if ( scrollVelocity != 0.0f )
		{
			// slow down over time
			float t = (Time.time - timeTouchPhaseEnded) / inertiaDuration;
			if (scrollPosition.y <= 0 || scrollPosition.y >= (scrollComplete.height - scrollView.height))
			{
				// bounce back if top or bottom reached
				scrollVelocity = -scrollVelocity/10f;
			}
			float frameVelocity = Mathf.Lerp(scrollVelocity, 0, t);
			scrollPosition.y += frameVelocity * Time.deltaTime;
			
			// after N seconds, we've stopped
			if (t >= 1.0f) scrollVelocity = 0.0f;
		}
		
		if ( Input.touchCount > 0 )
		{
			Touch touch = Input.touches[0];
			//Only scroll if user touches the screen inside the scroll zone
			if( scrollViewTouchZone.Contains(touch.position ) )
			{
				if (touch.phase == TouchPhase.Began)
				{
					scrollVelocity = 0.0f;
					screenTouchPosition = touch.position;
					
				}
				else if (touch.phase == TouchPhase.Moved )
				{
					// dragging
					scrollPosition.y = scrollPosition.y + ( touch.position.y - screenTouchPosition.y );
					screenTouchPosition = touch.position;
				}
				else if (touch.phase == TouchPhase.Ended)
				{
					// impart momentum, using last delta as the starting velocity
					// ignore delta < 10; precision issues can cause ultra-high velocity
					float deltaPosition = touch.position.y - screenTouchPosition.y;
					
					if (Mathf.Abs(deltaPosition) >= 10)
					{
						scrollVelocity = (deltaPosition / touch.deltaTime)/3f;
						//Cap the scroll speed
						if (scrollVelocity > 800f )
						{
							scrollVelocity = 800f;
						}
						
					}
					timeTouchPhaseEnded = Time.time;
				}
			}
		}
	}
}
