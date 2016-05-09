using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;

public class MessageCenterHandler : MonoBehaviour {

	GUIContent messageCenterCheckAllButton;
	GUIContent messageCenterAcceptButton;
	
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

	MessageEntryData AskSectionUnlock;
	MessageEntryData AcceptSectionUnlock;
	MessageEntryData AskGiveLife;
	MessageEntryData AcceptGiveLife;
	MessageEntryData UnknownRequestDataType;

	//Scrolling
	Vector2 screenTouchPosition;

	//Entry background texture
	public Texture entryBackground;
	
	//For testing only
	AppRequestData appRequestData0 = new AppRequestData();
	AppRequestData appRequestData1 = new AppRequestData();
	AppRequestData appRequestData2 = new AppRequestData();
	AppRequestData appRequestData3 = new AppRequestData();
	AppRequestData appRequestData4 = new AppRequestData();

	//Check All button state
	bool isCheckAllSelected = false;

	PopupHandler popupHandler;

	public static bool allowAutomaticOpening = true;

	//List of AppRequestData objects to process
	Stack<AppRequestData> appRequestsToProcessList = new Stack<AppRequestData>();

	// Use this for initialization
	void Awake () {
		
		Transform CoreManagers = GameObject.FindGameObjectWithTag("CoreManagers").transform;
		popupHandler = CoreManagers.GetComponent<PopupHandler>();

		//Message center button text
		messageCenterCheckAllButton = new GUIContent( LocalizationManager.Instance.getText("POPUP_BUTTON_CHECK_ALL") );
		messageCenterAcceptButton = new GUIContent( LocalizationManager.Instance.getText("POPUP_BUTTON_ACCEPT") );
		
		//Message center other
		messageCenterPopupSize	= new Vector2( Screen.width * 0.8f, Screen.height * 0.55f );
		popupRect 				= new Rect( (Screen.width - messageCenterPopupSize.x)/2, (Screen.height - messageCenterPopupSize.y)/2 , messageCenterPopupSize.x, messageCenterPopupSize.y );
		rowSize 				= new Vector2( messageCenterPopupSize.x * 0.95f, messageCenterPopupSize.y * 0.16f );
		scrollComplete			= new Rect(0, 0, rowSize.x * 0.95f, rowSize.y);
		messageCenterMargin 	= (messageCenterPopupSize.x - rowSize.x)/2f;
		scrollView 				= new Rect(messageCenterMargin, messageCenterPopupSize.y * 0.2f, rowSize.x, 3.5f * rowSize.y);
		//The scrollViewTouchZone is extended up and down by rowSize.y/2. When we scroll, our finger tends to go outside the scroll area but we still want to be able to scroll normally. 
		scrollViewTouchZone 	= new Rect(popupRect.x + messageCenterMargin, popupRect.y + messageCenterPopupSize.y * 0.16f - rowSize.y/2, rowSize.x, 3.5f * rowSize.y + rowSize.y/2);
		iconSize 				= new Vector2( Screen.width * 0.07f, Screen.width * 0.07f );
		
		//Message entry data
		AskGiveLife = new MessageEntryData( RequestDataType.Ask_Give_Life );
		AcceptGiveLife = new MessageEntryData( RequestDataType.Accept_Give_Life );
		UnknownRequestDataType = new MessageEntryData( RequestDataType.Unknown );

		facebookPortraitSize = new Vector2( Screen.width * 0.08f, Screen.width * 0.08f );
		toggleSize = new Vector2( facebookPortraitSize.x * 0.7f, facebookPortraitSize.y * 0.7f );

	}
	
	public void renderMessageCenter()
	{
		popupHandler.drawCloseButton( closeMessageCenter );

		GUI.skin = popupHandler.dragonRunSkin;

		//Draw message entries
		if( FacebookManager.Instance.isLoggedIn() )
		{
			drawMessageScrollWindow();
		}
		else
		{
			drawMessageScrollWindowDebug();
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
			print ("ACCEPT BUTTON PRESSED");
			allowAutomaticOpening = false;
			popupHandler.closePopup();
			copyAppRequestsToProcess();
			processNextAppRequest();
		}

		GUI.skin = null;

	}

	public void closeMessageCenter()
	{
		allowAutomaticOpening = false;
		popupHandler.closePopup();
		//Reset values
		isCheckAllSelected = false;
		foreach(AppRequestData appRequestData in FacebookManager.Instance.AppRequestDataList) 
		{
			appRequestData.isSelected = false;
		}
		//For debugging
		appRequestData0.isSelected = false;
		appRequestData1.isSelected = false;
		appRequestData2.isSelected = false;
		appRequestData3.isSelected = false;
		appRequestData4.isSelected = false;

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
			foreach(AppRequestData appRequestData in FacebookManager.Instance.AppRequestDataList) 
			{
				appRequestData.isSelected = isCheckAllSelected;
			}

			//For debugging
			appRequestData0.isSelected = isCheckAllSelected;
			appRequestData1.isSelected = isCheckAllSelected;
			appRequestData2.isSelected = isCheckAllSelected;
			appRequestData3.isSelected = isCheckAllSelected;
			appRequestData4.isSelected = isCheckAllSelected;

		}
	}

	void copyAppRequestsToProcess()
	{
		//Reset stack
		appRequestsToProcessList.Clear();
		AppRequestData appRequestData;
		for ( int i =0; i < FacebookManager.Instance.AppRequestDataList.Count; i++ ) 
		{
			appRequestData = (AppRequestData) FacebookManager.Instance.AppRequestDataList[i];
			
			//Process request since it is selected 
			if( appRequestData.isSelected )
			{
				appRequestsToProcessList.Push( appRequestData );
				print ("copying " + appRequestData.dataType );
			}
		}

		//After the copy we can now delete the entries that will be processed
		for ( int i = FacebookManager.Instance.AppRequestDataList.Count-1; i >=0; i--)
		{
			appRequestData = (AppRequestData) FacebookManager.Instance.AppRequestDataList[i];
			if( appRequestData.isSelected )
			{
				//Delete from main Facebook list
				FacebookManager.Instance.AppRequestDataList.RemoveAt(i);
				print ("deleting " + appRequestData.dataType );
			}
		}
	}
	
	void processNextAppRequest()
	{
		print ("processNextAppRequest count " + appRequestsToProcessList.Count );

		if( appRequestsToProcessList.Count > 0 )
		{
			AppRequestData appRequestData = appRequestsToProcessList.Peek();

			switch (appRequestData.dataType)
			{
			case RequestDataType.Ask_Give_Life:
				FacebookManager.Instance.CallAppRequestAsDirectRequest("App Requests", LocalizationManager.Instance.getText("FB_HAVE_A_LIFE_MESSAGE"), appRequestData.fromID, "Accept_Give_Life," + appRequestData.dataNumber.ToString(), MCHCallback, appRequestData.appRequestID );
				break;
			case RequestDataType.Accept_Give_Life:
				PlayerStatsManager.Instance.increaseLives(1);
				PlayerStatsManager.Instance.savePlayerStats();
				appRequestsToProcessList.Pop();
				//Now that it is successfully processed, delete the app request on Facebook
				FacebookManager.Instance.deleteAppRequest( appRequestData.appRequestID );
				processNextAppRequest();
				break;
			default:
				Debug.LogWarning("MessageCenterHandler-processNextAppRequest: unknown data type specified: " + appRequestData.dataType );
				break;
			}
		}
		else
		{
			//We have processed all of the requests.
			//Give control back to the user.
		}
	}

	public void MCHCallback(IAppRequestResult result, string appRequestIDToDelete )
	{
		if (result.Error != null)
		{
			Debug.Log ("MessageCenterHandler-MCHCallback error:\n" + result.Error );
		}
		else
		{
			Debug.Log ("MessageCenterHandler-MCHCallback success:\n" + result.RawResult );
			if( !result.RawResult.Contains("cancelled") )
			{
				//Now that it is successfully processed, delete the app request on Facebook
				FacebookManager.Instance.deleteAppRequest( appRequestIDToDelete );
			}
			else
			{
				Debug.Log ("MessageCenterHandler-MCHCallback user cancelled." );
			}
		}
		//Remove processed entry
		appRequestsToProcessList.Pop();
		processNextAppRequest();
	}

	void drawMessageScrollWindow() 
	{
		int numRows = FacebookManager.Instance.AppRequestDataList.Count;
		scrollComplete.height = numRows * rowSize.y;
		
		scrollPosition = GUI.BeginScrollView (scrollView, scrollPosition, scrollComplete, false, false);

		Rect rBtn = new Rect(0, 0, scrollComplete.width, rowSize.y);
		
		for (int iRow = 0; iRow < numRows; iRow++)
		{
			// draw call optimization: don't actually draw the row if it is not visible
			if ( rBtn.yMax >= scrollPosition.y && rBtn.yMin <= (scrollPosition.y + scrollView.height) )
			{
				AppRequestData appRequestData = (AppRequestData) FacebookManager.Instance.AppRequestDataList[iRow];
				drawMessageEntry( iRow, rBtn.y, appRequestData );
			}
			rBtn.y += rowSize.y;
		}
		GUI.EndScrollView();
		//Count number of selected entries. If the player manually selected all the entries, we want to automatically turn on the Check All toggle.
		int totalSelected = 0;
		for (int iRow = 0; iRow < numRows; iRow++)
		{
			AppRequestData appRequestData = (AppRequestData) FacebookManager.Instance.AppRequestDataList[iRow];
			if( appRequestData.isSelected ) totalSelected++;
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
	
	void drawMessageScrollWindowDebug() 
	{
		int numRows = 3;
		scrollComplete.height = numRows * rowSize.y;
		
		scrollPosition = GUI.BeginScrollView (scrollView, scrollPosition, scrollComplete, false, false);

		Rect rBtn = new Rect(0, 0, scrollComplete.width, rowSize.y);
		
		for (int iRow = 0; iRow < numRows; iRow++)
		{
			// draw call optimization: don't actually draw the row if it is not visible
			if ( rBtn.yMax >= scrollPosition.y && rBtn.yMin <= (scrollPosition.y + scrollView.height) )
			{
				if( iRow == 0 )
				{
					appRequestData0.dataType = RequestDataType.Accept_Give_Life;
					appRequestData0.fromFirstName = "Mary";
					drawMessageEntry( iRow, rBtn.y, appRequestData0 );

				}
				else if( iRow == 1 )
				{
					appRequestData2.dataType = RequestDataType.Ask_Give_Life;
					appRequestData2.fromFirstName = "Régis";
					drawMessageEntry( iRow, rBtn.y, appRequestData2 );
				}
				else if( iRow == 2 )
				{
					appRequestData4.dataType = RequestDataType.Unknown;
					appRequestData4.fromFirstName = "";
					drawMessageEntry( iRow, rBtn.y, appRequestData4 );
				}

			}
			
			rBtn.y += rowSize.y;
		}
		GUI.EndScrollView();
		
		handleTouchScroll();

	}
	
	void drawMessageEntry( int iRow, float yPos, AppRequestData appRequestData )
	{
		MessageEntryData med = getMessageEntryData( appRequestData.dataType );
		float gap = 14;

		//checkbox, button background, facebook portrait, facebook frame, entry title, entry text, entry type icon
		//If you get help a request, the title is "Help your friend!" and the background is pink.
		//If a friend helped you, the title is "You got help! and the background is green.
		Rect entryRect = new Rect(0, yPos, scrollComplete.width, rowSize.y);
		GUI.BeginGroup( entryRect );
		
		//Entry background
		GUI.color = med.entryColor;
		Rect rBtn = new Rect(0, 0, scrollComplete.width, rowSize.y);
		GUI.DrawTexture( rBtn, entryBackground, ScaleMode.StretchToFill, true );
		GUI.color = Color.white;

		//Toggle button
		Rect toggleRect = new Rect(gap, 0, toggleSize.x, toggleSize.y);
		float yOffset = (rowSize.y - toggleRect.height)/2f;
		toggleRect.y = yOffset;
		appRequestData.isSelected = GUI.Toggle(toggleRect,appRequestData.isSelected,"", "Toggle");

		//Facebook portrait
		string userID = appRequestData.fromID;
		float xOffset = toggleRect.xMax + gap;
		yOffset = (rowSize.y - facebookPortraitSize.y)/2f;
		Rect friendPictureRect = new Rect( xOffset, yOffset, facebookPortraitSize.x, facebookPortraitSize.y );
		
		Sprite picture;
		if ( FacebookManager.Instance.friendImages.TryGetValue( userID, out picture)) 
		{
			//We have the friend's picture
		//popupHandler.drawPortrait( friendPictureRect, picture, false );
		}
		else if ( FacebookManager.Instance.friendImagesRequested.Contains( userID ) )
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
		GUI.Label(entryTitleRect, med.entryTitle, "Entry Title" );
		
		//Draw entry text label
		xOffset = friendPictureRect.xMax + gap;
		yOffset = 30;
		length = scrollComplete.width - xOffset - facebookPortraitSize.x - gap;
		Rect entryTextRect = new Rect( xOffset, yOffset, length, rowSize.y );
		//Verify if we need to insert the first name into the text or if it simply goes at the beginning of the text
		string entryText = "";
		if( med.entryText.Contains("<first_name>") )
		{
			//Yes, insert it into the text
			entryText = med.entryText;
			entryText = entryText.Replace("<first_name>", appRequestData.fromFirstName );
		}
		else
		{
			entryText = appRequestData.fromFirstName + " " + med.entryText;
		}

		GUI.Label(entryTextRect, entryText, "Entry Message");
		
		//Draw request type icon (Give life is a heart for example)
		xOffset = scrollComplete.width - iconSize.x - gap;
		yOffset = (rowSize.y - iconSize.y)/2f;
		Rect iconRect = new Rect( xOffset, yOffset, iconSize.x, iconSize.y );
		GUI.DrawTexture( iconRect, med.entryIcon, ScaleMode.ScaleToFit, true );
		
		
		GUI.EndGroup();
		
	}
	
	MessageEntryData getMessageEntryData( RequestDataType requestDataType )
	{
		switch (requestDataType)
		{
		case RequestDataType.Ask_Give_Life:
			return AskGiveLife;
		case RequestDataType.Accept_Give_Life:
			return AcceptGiveLife;
		default:
			return UnknownRequestDataType;
		}
		
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
