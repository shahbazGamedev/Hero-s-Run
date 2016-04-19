using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class WorldMapHandler : MonoBehaviour {

	//World
	Rect worldRect;

	//Map
	public Texture2D mapTexture;
	Rect mapRect;
	float mapScale = 1.2f;
	Vector2 screenTouchPosition;

	//Styles
	public GUIStyle titleStyle;
	public GUIStyle levelNumberStyle;
	public GUIStyle messageCenterOpenStyle;
	public GUIStyle lifeStyle;
	public GUIStyle debugStyle;
	public GUIStyle settingsStyle;
	public GUIStyle playButtonStyle;

	//Stars
	public Texture starTexture;
	public Color oneStar = Color.red;
	public Color twoStar = Color.green;
	public Color threeStar = Color.yellow;

	//control
	//Set to true when the level starts loading to avoid the user pressing the load level button multiple times.
	int levelForPortrait = -1; //zero indexed
	bool levelLoading = false;

	//Shield with level number
	Rect shieldButtonRect;
	Rect shieldButtonGlowRect;
	public GUIStyle shieldStyle;
	public GUIStyle shieldGlowStyle;
	float flashSpeed = 3f;
	Color shieldDropShadowColor = new Color(0.24f,0.07f,0.24f, 1f);
	Color shieldColorLocked = new Color(0.5f,0.5f,0.5f, 1f);

	//Section end icon
	Rect sectionEndButtonRect;
	public GUIStyle sectionEndStyle;

	//User portrait
	public Texture defaultPortrait;
	Texture playerPortrait;
	LTRect playerPortraitRect;
	Rect friendPortraitRect;
	Hashtable options1 = new Hashtable();
	Hashtable options2 = new Hashtable();

	private LevelData levelData;
	List<LevelData.LevelInfo> levelList;
	List<Rect> levelButtonList = new List<Rect>();

	//Sizes
	Vector2 shieldButtonSize;
	Vector2 sectionEndButtonSize;
	Vector2 shieldButtonGlowSize;
	Vector2 shieldPortraitMargin;

	Vector2 facebookPortraitSize;
	Vector2 playerPortraitOffset;
	Vector2 friendPortraitOffset;

	Vector2 starSize;
	
	PopupHandler popupHandler;

	public Canvas settingsMenuCanvas;

	void Awake ()
	{
		//Get the level data. Level data has the parameters for all the levels of the game.
		levelData = LevelManager.Instance.getLevelData();
		levelList = levelData.getLevelList();
		levelButtonList.Capacity = levelList.Count;
		//The Map texture is 1136x640 (size of iPhone5). This corresponds to a scale of 1. If the screen resolution is different, we need to scale up or down proportionaly.

		float heightRatio = Screen.height / (float)mapTexture.height;
		float widthRatio = Screen.width / (float)mapTexture.width;

		worldRect = new Rect( 0,Screen.height - ( mapTexture.height * mapScale * heightRatio ), mapTexture.width * mapScale * widthRatio, mapTexture.height * mapScale * heightRatio );
		mapRect = new Rect( 0, 0, worldRect.width, worldRect.height );

		Handheld.StopActivityIndicator();

		shieldButtonSize 	 = new Vector2( Screen.width * 0.08f, Screen.width * 0.08f );
		shieldButtonGlowSize = new Vector2( shieldButtonSize.x, shieldButtonSize.y );
		sectionEndButtonSize = new Vector2( Screen.width * 0.1f, Screen.width * 0.1f );
		shieldPortraitMargin = new Vector2( shieldButtonSize.x * 0.1f, shieldButtonSize.y * 0.1f );
		facebookPortraitSize = new Vector2( shieldButtonSize.x * 0.85f, shieldButtonSize.y * 0.85f );
		starSize 	 		 = new Vector2( Screen.width * 0.07f, Screen.width * 0.07f );

		//Important: Everything is relative to the level coordinates.
		//The shield with the level number is centered on the those coordinates.
		shieldButtonRect 	 = new Rect( 0,0, shieldButtonSize.x, shieldButtonSize.y );
		shieldButtonGlowRect = new Rect( 0,0, shieldButtonGlowSize.x, shieldButtonGlowSize.y );
		sectionEndButtonRect = new Rect( 0,0, sectionEndButtonSize.x, sectionEndButtonSize.y );
		playerPortraitRect 	 = new LTRect( 0, 0, facebookPortraitSize.x, facebookPortraitSize.y );
		friendPortraitRect   = new Rect( 0, 0, facebookPortraitSize.x, facebookPortraitSize.y );
		playerPortraitOffset = new Vector2 ( - shieldButtonSize.x/2f - facebookPortraitSize.x + shieldPortraitMargin.x, shieldPortraitMargin.y );
		friendPortraitOffset = new Vector2 (   shieldButtonSize.x/2f - shieldPortraitMargin.x, shieldPortraitMargin.y );

		Transform CoreManagers = GameObject.FindGameObjectWithTag("CoreManagers").transform;
		popupHandler = CoreManagers.GetComponent<PopupHandler>();

		PopupHandler.changeFontSizeBasedOnResolution( titleStyle );
		PopupHandler.changeFontSizeBasedOnResolution( levelNumberStyle );


	}

	void Start()
	{
		levelLoading = false;
		//For debugging
		//Display localized test announcement
		//MyUpsightManager.requestLocalizedContent( "announcement_01" );

		//If we quit from the pause menu, the audio listener was paused.
		//We could set AudioListener.pause = false in the goToHomeMenu() but
		//that causes the level sound to play for a very brief moment before getting killed.
		//This is why we do it here instead.
		AudioListener.pause = false;

		popupHandler.setWorldMapHandler( gameObject );
		popupHandler.closePopupNoSound();

		options1.Add("ease", LeanTweenType.easeOutQuad);
		options1.Add("onUpdate", "moveMapWithPortrait");
		options1.Add("onUpdateTarget", gameObject );
		options1.Add("onComplete", "arrivedAtLevelMarker");
		options1.Add("onCompleteTarget", gameObject );

		options2.Add("ease", LeanTweenType.easeOutQuad);
		options2.Add("onUpdate", "moveMapWithPortrait");
		options2.Add("onUpdateTarget", gameObject );
		options2.Add("onComplete", "arrivedAtJunctionMarker");
		options2.Add("onCompleteTarget", gameObject );

		//Get all of the user's outstanding app requests right away and then poll Facebook every 30 seconds
		InvokeRepeating("getAllAppRequests", 0.25f, 30 );


		//if we have a Facebook user portrait, use it, or else, use the default one.
		if( FacebookManager.Instance.UserPortrait == null )
		{
			//playerPortrait = defaultPortrait;
		}
		else
		{
			//playerPortrait = FacebookManager.Instance.UserPortrait;
		}

		if( LevelManager.Instance.getLevelChanged() )
		{
			//The user has completed a new level.
			//Move his portrait from the current level he has just finished to the new one.
			LevelManager.Instance.setLevelChanged( false );
			//Because the nextLevelToComplete value was immediately updated when the player reached the checkpoint, substract one since we want the previous value.
			levelForPortrait = LevelManager.Instance.getNextLevelToComplete() - 1;
			StartCoroutine( moveUserPortrait(1.8f) );
		}
		else
		{
			levelForPortrait = LevelManager.Instance.getNextLevelToComplete();
		}

		//Center map around shield indicating next level to complete
		LevelData.LevelInfo levelInfo = levelList[levelForPortrait];
		playerPortraitRect._rect.x = levelInfo.MapCoordinates.x * worldRect.width  +  playerPortraitOffset.x;
		playerPortraitRect._rect.y = levelInfo.MapCoordinates.y * worldRect.height +  playerPortraitOffset.y;
		worldRect.x = -playerPortraitRect.rect.center.x  + Screen.width/2;
		worldRect.y = -playerPortraitRect.rect.center.y  + Screen.height/2;
		enforceMapBoundaries();

		//For debugging
		TimeSpan timeSpan = DateTime.Now - PlayerStatsManager.Instance.getDateLastPlayed();
		Debug.Log("WorldMapHandler-Start: minutes elapsed since last played: " + timeSpan.Minutes );

	}

	void getAllAppRequests()
	{
		FacebookManager.Instance.getAllAppRequests();
	}
	
	void moveMapWithPortrait()
	{
		//Center map around moving user portrait
		worldRect.x = -playerPortraitRect.rect.center.x  + Screen.width/2;
		worldRect.y = -playerPortraitRect.rect.center.y  + Screen.height/2;
		enforceMapBoundaries();


	}
	void arrivedAtLevelMarker()
	{
		levelForPortrait = LevelManager.Instance.getNextLevelToComplete();
		print ("arrivedAtLevelMarker");
	}

	void arrivedAtJunctionMarker()
	{
		print ("arrivedAtJunctionMarker");

	}

	IEnumerator moveUserPortrait( float waitPeriod )
	{
		//Give time to the player to understand what is going on before starting to move the portrait
		yield return new WaitForSeconds( waitPeriod );

		LevelData.LevelInfo levelInfo = levelList[ LevelManager.Instance.getNextLevelToComplete() - 1 ];
		if( levelInfo.isSectionEnd )
		{
			//Move the user portrait to the junction marker associated to the level just completed.
			Vector2 dest = new Vector2( levelInfo.sectionIconMapCoordinates.x * worldRect.width + playerPortraitOffset.x, levelInfo.sectionIconMapCoordinates.y * worldRect.height + playerPortraitOffset.y );
			LeanTween.move( playerPortraitRect, dest, 3.8f, options2 );
		}
		else
		{
			//Move the user portrait to the next level shield.
			levelInfo = levelList[ LevelManager.Instance.getNextLevelToComplete() ];
			Vector2 dest = new Vector2( levelInfo.MapCoordinates.x * worldRect.width + playerPortraitOffset.x, levelInfo.MapCoordinates.y * worldRect.height + playerPortraitOffset.y );
			LeanTween.move( playerPortraitRect, dest, 3.8f, options1 );
		}

	}

	IEnumerator moveUserPortrait2( float waitPeriod )
	{
		//Give time to the player to understand what is going on before starting to move the portrait
		yield return new WaitForSeconds( waitPeriod );
		
		//Move the user portrait to the next level shield.
		LevelData.LevelInfo levelInfo = levelList[ LevelManager.Instance.getNextLevelToComplete() ];
		Vector2 dest = new Vector2( levelInfo.MapCoordinates.x * worldRect.width + playerPortraitOffset.x, levelInfo.MapCoordinates.y * worldRect.height + playerPortraitOffset.y );
		LeanTween.move( playerPortraitRect, dest, 3.8f, options1 );
	}

	void displayOfferLivesPopup()
	{
		//Once a day, display the Offer Lives to friends popup if the player is logged in to Facebook and if there are no other popup displayed
		if( !popupHandler.isPopupDisplayed() && FacebookManager.Instance.isLoggedIn() )
		{
			TimeSpan timeSpan = DateTime.Now - PlayerStatsManager.Instance.getDateLastPlayed();
			if( timeSpan.Minutes > 1440 )
			{
				//Yes, the required elapsed time has gone by.
				//Reset saved date to Now
				PlayerStatsManager.Instance.setDateLastPlayed();
				MessageCenterHandler.allowAutomaticOpening = false;
				popupHandler.activatePopup(PopupType.OfferLives );
			}
		}

	}
	void OnGUI ()
	{
		//Once a day, display the Offer Lives to friends popup if the player is logged in to Facebook and if there are no other popup displayed.
		displayOfferLivesPopup();

		//If there is no popup currently displayed and the player has messages and the message center allows automatic opening, automatically
		//open the Message Center.
		if( MessageCenterHandler.allowAutomaticOpening && !popupHandler.isPopupDisplayed() && FacebookManager.Instance.AppRequestDataList.Count > 0 )
		{
			popupHandler.activatePopup( PopupType.MessageCenter );
		}

		#if UNITY_EDITOR
		handleMouseMovement();
		#endif
		detectGesture();

		GUI.BeginGroup( worldRect );

		//Draw map
		GUI.DrawTexture( mapRect, mapTexture, ScaleMode.StretchToFill, false );

		//Draw all level markers
		drawLevelMarkers();

		GUI.EndGroup();

	}



	//Draw button to open message center
	void drawMessageCenterButton()
	{
		if( !popupHandler.isPopupDisplayed() )
		{
			int numberMessages = FacebookManager.Instance.AppRequestDataList.Count;
			//Only display message center button if there are messages
			if( numberMessages > -1 )
			{
				//Draw button
				float marginX = Screen.width * 0.025f;
				float marginY = Screen.width * 0.2f;
				float buttonSize = Screen.width * 0.075f;
				Rect buttonRect = new Rect( marginX, marginY, messageCenterOpenStyle.fixedWidth, messageCenterOpenStyle.fixedHeight );
				if( GUI.Button( buttonRect, "", messageCenterOpenStyle )) 
				{
					SoundManager.playButtonClick();
					popupHandler.setPopupSize(new Vector2( Screen.width * 0.8f, Screen.height * 0.55f ));
					popupHandler.activatePopup( PopupType.MessageCenter );
				}
				
				//Draw number of messages
				float yOffset = (buttonRect.height - buttonSize)/2;
				GUIContent numberMessagesContent = new GUIContent( numberMessages.ToString() );
				Rect numberMessagesRect = new Rect( buttonRect.x + buttonRect.width , buttonRect.y + yOffset, buttonSize, buttonSize );
				Utilities.drawLabelWithDropShadow( numberMessagesRect, numberMessagesContent, titleStyle, Color.red );
			}
		}
	}

	void drawLevelMarkers()
	{
		levelButtonList.Clear();
		LevelData.LevelInfo levelInfo;
		for( int i=0; i < levelList.Count; i++ )
		{
			levelInfo = levelList[i];
			//the map coordinates correspond to the exact center of the shield
			drawLevelMarker( levelInfo.MapCoordinates, i );

			//A section is composed of many levels, typically around 10.
			//To unlock the next section you must either ask 3 friends or pay in the App store.
			//Is this the end of a section?
			if( levelInfo.isSectionEnd )
			{
				//Yes it is.
				drawSectionMarker( levelInfo.sectionIconMapCoordinates, i );
			}
		}
		//Draw the user picture to the left of the shield after everything else.
		//Since it moves, we always want it drawn last so it appears on top.
		//If the user is logged in to FB, show his picture, if not, show the default portrait.
		if( FacebookManager.Instance.isLoggedIn() )
		{
			popupHandler.drawPortrait( playerPortraitRect.rect, playerPortrait, false );
		}
		else
		{
			popupHandler.drawDefaultPortrait( playerPortraitRect.rect, false );
		}
	}

	void drawLevelMarker( Vector2 coord, int levelNumber )
	{
		//Draw shield with level number
		shieldButtonRect.x = ( coord.x * worldRect.width  ) - shieldButtonSize.x/2f;
		shieldButtonRect.y = ( coord.y * worldRect.height ) - shieldButtonSize.y/2f;
		levelButtonList.Add( shieldButtonRect );
		GUIContent shieldButtonContent = new GUIContent( levelNumber.ToString() );
		shieldButtonGlowRect.x = shieldButtonRect.x + 3;
		shieldButtonGlowRect.y = shieldButtonRect.y + 3;
		if( levelNumber == levelForPortrait )
		{
			//The shield correspondonding to the next level to play will blink
			GUI.backgroundColor = new Color(1f,1f,1f,(Mathf.Sin(Time.time * flashSpeed) + 1f)/ 2f);
		}
		else
		{
			//Others will have a drop shadow
			GUI.backgroundColor = shieldDropShadowColor;

		}
		//Create the drop shadow effect
		GUI.Label( shieldButtonGlowRect, shieldButtonContent, shieldGlowStyle );

		//Has this level been unlocked yet?
		//HAck if( levelNumber > levelForPortrait )
		if( LevelManager.Instance.isLevelLocked( levelNumber ) )
		{
			//No, it is locked
			GUI.backgroundColor = shieldColorLocked;
			GUI.Label( shieldButtonRect, shieldButtonContent, shieldStyle );
		}
		else
		{
			//Yes, it is unlocked
			//Set the color of the shield
			GUI.backgroundColor = Color.white;
			if( GUI.Button( shieldButtonRect, shieldButtonContent, shieldStyle ) ) 
			{
				//For debugging only
				SoundManager.playButtonClick();
				int levelToLoad = levelButtonList.IndexOf(shieldButtonRect);
				LevelManager.Instance.forceNextLevelToComplete( levelToLoad );
				initiateLevelLoading();
			}
		}

		//Reset the color
		GUI.backgroundColor = Color.white;

		//Draw the level number on top of the shield
		Rect LevelLabelRect = GUILayoutUtility.GetRect( shieldButtonContent, levelNumberStyle );
		LevelLabelRect.x = shieldButtonRect.x + (shieldButtonRect.width - LevelLabelRect.width)/2f + 3f;
		LevelLabelRect.y = shieldButtonRect.y + (shieldButtonRect.height - LevelLabelRect.height)/2f;
		Utilities.drawLabelWithDropShadow( LevelLabelRect, shieldButtonContent, levelNumberStyle );

		//Draw friend picture to the right of the shield
		drawFriendPicture( coord, levelNumber );

	}

	void initiateLevelLoading()
	{
		if( !levelLoading )
		{
			levelLoading = true;
			StartCoroutine( loadLevel() );
		}
		
	}

	IEnumerator loadLevel()
	{
		Handheld.StartActivityIndicator();
		yield return new WaitForSeconds(0);
		//Load level scene
		SceneManager.LoadScene( (int) GameScenes.Level );
		
	}

	//Draw friend picture to the right of the shield but only if we are logged in to Facebook
	void drawFriendPicture( Vector2 coord,int levelNumber )
	{
		if( FacebookManager.Instance.isLoggedIn() )
		{
			string userID = FacebookManager.Instance.getFriendPictureForLevel( levelNumber );
			if( userID != null )
			{
				//Yes, a friend has reached that level
				friendPortraitRect.x =  coord.x * worldRect.width  + friendPortraitOffset.x;
				friendPortraitRect.y =  coord.y * worldRect.height + friendPortraitOffset.y;
				
				Sprite picture;
				if (FacebookManager.Instance.friendImages.TryGetValue( userID, out picture)) 
				{
					//We have the friend's picture
					//popupHandler.drawPortrait( friendPortraitRect, picture, false );
				}
				else if ( FacebookManager.Instance.friendImagesRequested.Contains( userID ) )
				{
					//Picture has been requested but not received yet. Draw default portrait with a spinner on top.
					popupHandler.drawDefaultPortrait( friendPortraitRect, true );
				}
			}
		}
	}
	
	//A junction marker has 3 states:
	//a) Locked (not clickable and darkened)
	//b) Active (in full color, click to ask Facebook friends to help you unlock)
	//c) Unlocked (no longer clickable, but in full color)
	void drawSectionMarker( Vector2 coord, int levelNumber )
	{

		//Establish coordinates and content for section marker
		sectionEndButtonRect.x = ( coord.x * worldRect.width  ) - sectionEndButtonSize.x/2f;
		sectionEndButtonRect.y = ( coord.y * worldRect.height ) - sectionEndButtonSize.y/2f;
		GUIContent shieldButtonContent = new GUIContent( "" );

		//Verify in which state we are
		int nextSectionToUnlock = LevelManager.Instance.getNextSectionToUnlock();

		if( levelNumber == LevelManager.Instance.getNextLevelToComplete() - 1 && levelNumber == nextSectionToUnlock )
		{
			//ACTIVE state
			if( GUI.Button( sectionEndButtonRect, shieldButtonContent, sectionEndStyle ) ) 
			{
				SoundManager.playButtonClick();
				print ( "level " + levelNumber + " next lvl " + LevelManager.Instance.getNextLevelToComplete() + " nextSectionToUnlock " + nextSectionToUnlock  );
				//for debugging
				//popupHandler.activatePopup( PopupType.FriendsOrPay );
				NextSectionNowUnlocked();
			}
		}
		else
		{
			//NOT ACTIVE
			if( levelNumber < nextSectionToUnlock )
			{
				//UNLOCKED state
				GUI.Label( sectionEndButtonRect, shieldButtonContent, sectionEndStyle );
			}
			else
			{
				//LOCKED state
				GUI.backgroundColor = Color.gray;
				GUI.Label( sectionEndButtonRect, shieldButtonContent, sectionEndStyle );
				GUI.backgroundColor = Color.white;
			}
		}
	}

	void handleMouseMovement()
	{
		if( worldRect.Contains(Event.current.mousePosition))
		{
			if(Event.current.type == EventType.MouseDrag)
			{
				moveMap( Event.current.delta, false );
			}
			else if(Event.current.type == EventType.MouseUp)
			{
				float  hiddenW = worldRect.x;
				float coordW = Event.current.mousePosition.x - hiddenW;
				float  hiddenH = worldRect.y;
				float coordH = Event.current.mousePosition.y - hiddenH;
				Debug.Log("Mouse coordinates-Percentage Width: " + (coordW/worldRect.width).ToString("N3") +  " Percentage Height: " + (coordH/worldRect.height).ToString("N3") );
			}
		}
	}

	float timeTouchPhaseEnded;
	Vector2 scrollVelocity = Vector2.zero;
	float inertiaDuration = 1.25f;
	void detectGesture()
	{
		//Dont allow map movement if a popup is displayed or if we asked a level to load
		if ( !popupHandler.isPopupDisplayed() )
		{
			//Don't allow the user to move the map while the Facebook portrait is moving.
			//Also, do not use touch.deltaPosition to move the map. It does not work well.
			if( LeanTween.isTweening( playerPortraitRect ) == false )
			{
				if ( scrollVelocity.x != 0.0f || scrollVelocity.y != 0.0f )
				{
					// slow down over time
					float t = (Time.time - timeTouchPhaseEnded) / inertiaDuration;
					Vector2 frameVelocity = Vector2.Lerp(scrollVelocity, Vector2.zero, t);
					worldRect.x += frameVelocity.x * Time.deltaTime;
					worldRect.y -= frameVelocity.y * Time.deltaTime;
					
					// after N seconds, we've stopped
					if (t >= 1.1f) scrollVelocity = Vector2.zero;
					enforceMapBoundaries();
				}

				if ( Input.touchCount > 0 )
				{
					Touch touch = Input.GetTouch(0);
					if( touch.phase == TouchPhase.Began  )
					{
						scrollVelocity = Vector2.zero;
						screenTouchPosition = touch.position;
					}
					else if( touch.phase == TouchPhase.Moved  )
					{
						moveMap( touch.position - screenTouchPosition, true );
						screenTouchPosition = touch.position;
					}
					else if (touch.phase == TouchPhase.Ended)
					{
						// impart momentum, using last delta as the starting velocity
						// ignore delta < 10; precision issues can cause ultra-high velocity
						Vector2 deltaPosition = touch.position - screenTouchPosition;
						
						if ( Mathf.Abs(deltaPosition.x) >= 10|| Mathf.Abs(deltaPosition.y) >= 10 )
						{
							scrollVelocity = (deltaPosition / touch.deltaTime)/3f;
							//Cap the scroll speed
							if (Mathf.Abs(scrollVelocity.y) > 700f )
							{
								scrollVelocity.y = 700f * Mathf.Sign(scrollVelocity.y);
							}
							if (Mathf.Abs(scrollVelocity.x) > 700f )
							{
								scrollVelocity.x = 700f * Mathf.Sign(scrollVelocity.x);
							}
						}
						timeTouchPhaseEnded = Time.time;
					}
				}
			}
		}
	}

	void moveMap( Vector2 delta, bool isMobile )
	{
		worldRect.x += delta.x;
		if( isMobile )
		{
			worldRect.y -= delta.y;
		}
		else
		{
			worldRect.y += delta.y;
		}
		enforceMapBoundaries();
	}

	void enforceMapBoundaries()
	{
		if( worldRect.x > 0 )
		{
			worldRect.x = 0;
		}
		else if( worldRect.x < -(worldRect.width - Screen.width) )
		{
			worldRect.x = -(worldRect.width - Screen.width);
		}
		if( worldRect.y < -worldRect.height + Screen.height )
		{
			worldRect.y = -worldRect.height + Screen.height;
		}
		else if( worldRect.y > 0 )
		{
			worldRect.y = 0;
		}
	}

	void OnEnable()
	{
		FacebookManager.nextSectionNowUnlocked += NextSectionNowUnlocked;
	}

	void OnDisable()
	{
		FacebookManager.nextSectionNowUnlocked -= NextSectionNowUnlocked;
	}
	
	void NextSectionNowUnlocked()
	{
		//Levels are now unlocked up to highestLevelUnlocked
		int highestLevelUnlocked = LevelManager.Instance.getNextSectionToUnlock();
		PlayerStatsManager.Instance.setHighestLevelUnlocked(highestLevelUnlocked);
		PlayerStatsManager.Instance.savePlayerStats();
		print ("*******Super! WorldMapHandler-NextSectionNowUnlocked new: " + highestLevelUnlocked);
		StartCoroutine( moveUserPortrait2(0.5f) );
	}

}
