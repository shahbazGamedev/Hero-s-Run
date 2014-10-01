using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HUDSaveMe : MonoBehaviour {

	//Control
	Stack<PopupType> popupStack = new Stack<PopupType>();
	Vector2 iconSize = new Vector2( Screen.width * 0.1f, Screen.width * 0.1f );
	Vector2 saveMePopupSize = new Vector2( Screen.width * 0.6f, Screen.height * 0.3f );
	Vector2 saveMeButtonSize;
	Rect saveMeModalRect;
	Rect saveMePopupRect;
	Rect saveMeButtonRect;
	Rect tryAgainButtonRect;
	GUIContent saveMeButtonContent = new GUIContent("");
	GUIContent tryAgainContent;
	GUIContent tutorialTitleContent;
	public GUIStyle saveMeStyle;
	public GUIStyle titleStyle;
	public GUIStyle helpTextStyle;
	float margin;
	Rect skipButtonRect;
	GUIContent skipButtonContent;
	GUIContent helpTextContent = new GUIContent( System.String.Empty );

	void Start()
	{
		margin = saveMePopupSize.x * 0.1f;
		saveMeButtonSize = new Vector2( saveMePopupSize.x * 0.75f, saveMePopupSize.y * 0.21f );
		popupStack.Clear();
		popupStack.Push( PopupType.None );
		float posX = (Screen.width - saveMePopupSize.x)/2;
		float posY = (Screen.height - saveMePopupSize.y)/4;
		saveMeModalRect = new Rect( posX, posY, saveMePopupSize.x, saveMePopupSize.y );
		saveMePopupRect = new Rect( 0, 0, saveMePopupSize.x, saveMePopupSize.y );
		saveMeButtonRect = new Rect( margin, saveMeButtonSize.y + margin, saveMeButtonSize.x, saveMeButtonSize.y );
		tryAgainButtonRect = new Rect( margin, saveMePopupSize.y - saveMeButtonSize.y - margin, saveMeButtonSize.x, saveMeButtonSize.y );
		tryAgainContent = new GUIContent ( LocalizationManager.Instance.getText("TUTORIAL_TRY_AGAIN") );
		tutorialTitleContent = new GUIContent ( LocalizationManager.Instance.getText("TUTORIAL_OOPS") );
		skipButtonRect = new Rect( margin, saveMeButtonRect.y + saveMeButtonRect.height + 5f, saveMeButtonRect.width, saveMeButtonRect.height );
		skipButtonContent = new GUIContent ( LocalizationManager.Instance.getText("MENU_SKIP") );
		helpTextStyle.fixedWidth = saveMePopupRect.width * 0.9f;

		PopupHandler.changeFontSizeBasedOnResolution( saveMeStyle );
		PopupHandler.changeFontSizeBasedOnResolution( titleStyle );
		PopupHandler.changeFontSizeBasedOnResolution( helpTextStyle );

	}
	
	public void activatePopup( PopupType popupType )
	{
		popupStack.Push( popupType );
		if( LevelManager.Instance.isTutorialActive() )
		{
			//Set the help text in case the player failed a tutorial.
			helpTextContent.text = TutorialManager.getFailedTutorialText();
		}
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
	
	void OnGUI ()
	{
		if( popupStack.Peek() != PopupType.None )
		{
			GUI.ModalWindow(0, saveMeModalRect, (GUI.WindowFunction) showPopup, "");
		}
	}
	
	void showPopup( int windowID )
	{
		PopupType type = popupStack.Peek();

		//Popup specific
		switch (type)
		{
			case PopupType.SaveMe:
			if( LevelManager.Instance.isTutorialActive() )
			{
				renderTutorialSaveMe();
			}
			else
			{
				renderNormalSaveMe();
			}
			break;			
		}
		
	}

	void renderTutorialSaveMe()
	{
		drawTitle();
		drawHelpText();
		GUI.BeginGroup(saveMePopupRect);
		if(GUI.Button( tryAgainButtonRect, tryAgainContent, saveMeStyle ))
		{
			Debug.Log("Resurrect button pressed");
			GameManager.Instance.setGameState( GameState.Resurrect );
			SendMessage("resurrectBegin" );
			closePopup();
		}
		GUI.EndGroup();
		
	}

	//Title (top-center)
	void drawTitle()
	{
		Rect textRect = GUILayoutUtility.GetRect( tutorialTitleContent, titleStyle );
		float textCenterX = (saveMePopupRect.width-textRect.width)/2f;
		float titleHeight = 0.05f * saveMePopupRect.height;
		Rect titleTextRect = new Rect( textCenterX, titleHeight, textRect.width, textRect.height );
		Utilities.drawLabelWithDropShadow( titleTextRect, tutorialTitleContent, titleStyle );
	}

	//Title (top-center)
	void drawHelpText()
	{
		Rect textRect = GUILayoutUtility.GetRect( helpTextContent, helpTextStyle );
		float textCenterX = (saveMePopupRect.width-textRect.width)/2f;
		float titleHeight = 0.3f * saveMePopupRect.height;
		Rect titleTextRect = new Rect( textCenterX, titleHeight, textRect.width, textRect.height );
		Utilities.drawLabelWithDropShadow( titleTextRect, helpTextContent, helpTextStyle );
	}

	void renderNormalSaveMe()
	{
		Rect numberLivesRect = new Rect( margin,saveMePopupSize.x * 0.1f, saveMePopupSize.x * 0.36f, saveMePopupSize.x * 0.1f );
		int currentNumberOfLives = PlayerStatsManager.Instance.getLives();
		if( PlayerStatsManager.Instance.getHasInfiniteLives() )
		{
			saveMeButtonContent.text = " Lives: " + currentNumberOfLives.ToString() + "*";
		}
		else
		{
			saveMeButtonContent.text = " Lives: " + currentNumberOfLives.ToString();
		}

		GUI.BeginGroup(saveMePopupRect);

		TextAnchor currentAnchor = saveMeStyle.alignment;
		saveMeStyle.alignment = TextAnchor.MiddleLeft;
		GUI.Label( numberLivesRect, saveMeButtonContent, saveMeStyle );

		//Calculate the energy cost to revive.
		saveMeStyle.alignment = currentAnchor;
		float costLives = PlayerStatsManager.Instance.getTimesPlayerRevivedInLevel() + 1;
		int currentFontSize = saveMeStyle.fontSize;
		saveMeStyle.fontSize = 	(int)(saveMeStyle.fontSize * 0.8f);

		saveMeButtonContent.text = "Save Me!" + "\n" + "Cost: " + costLives.ToString() + "  ";

		if(GUI.Button( saveMeButtonRect, saveMeButtonContent, saveMeStyle ))
		{
			if( PlayerStatsManager.Instance.getLives() >= costLives || PlayerStatsManager.Instance.getHasInfiniteLives() )
			{
				Debug.Log("Resurrect button pressed");
				GameManager.Instance.setGameState( GameState.Resurrect );
				SendMessage("resurrectBegin" );
				PlayerStatsManager.Instance.decreaseLives((int)costLives);
				PlayerStatsManager.Instance.incrementTimesPlayerRevivedInLevel();
				closePopup();
			}
			else
			{
				//Show buy popup
			}
		}
		//Skip button
		if(GUI.Button( skipButtonRect, skipButtonContent, saveMeStyle ))
		{
			SoundManager.playButtonClick();
			fadeOutAllAudio( SoundManager.STANDARD_FADE_TIME );
			Debug.Log("Skip button pressed");
			GameManager.Instance.setGameState( GameState.StatsScreen );
			HUDHandler.showUserMessage = false;
			closePopup();
		}
		GUI.EndGroup();
		//Reset
		saveMeStyle.fontSize = currentFontSize;
	}

	void fadeOutAllAudio( float duration )
	{
		AudioSource[] allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
		foreach(AudioSource audioSource in allAudioSources )
		{
			//Don't fade out GUI sounds
			if( !audioSource.ignoreListenerPause )
			{
				if( audioSource.clip != null && audioSource.isPlaying )
				{
					StartCoroutine( SoundManager.fadeOutClip( audioSource, audioSource.clip, duration ) );
				}
			}
		}
	}


}
