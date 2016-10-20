using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TutorialDisplay : BaseClass {

	public GUIStyle textStyle;

	public Texture2D rightArrow;
	public Texture2D upArrow;
	Vector2 arrowSize = new Vector2( Screen.width * 0.25f, Screen.width * 0.25f );
	float arrowMargin = Screen.width * 0.03f;
	Rect textureCoordNormal = new Rect( 0, 0, 1, 1 );
	Rect rightTextureCoordFlipped = new Rect( 0, 0, -1, 1 );
	Rect upTextureCoordFlipped = new Rect( 0, 0, 1, -1 );
	float screenCenter = Screen.width * 0.5f;

	public Texture2D pointingFinger;
	public Texture2D device;

	//Generic user message
	bool showTutorialMessage = false;
	GUIContent textContent;
	float userScreenHeightPercentage = 0;
	float userAngle = 0;
	float userMessageDuration = 0;
	bool showMovingArrow = false;

	TutorialEvent activeTutorial;
	TutorialHelpArrow tutorialHelpArrow;
	
	static Vector2 movingArrowSize = new Vector2( Screen.width * 0.4f, Screen.width * 0.4f );
	static LTRect movingArrowRect = new LTRect( -Screen.width, 0.78f * Screen.height, movingArrowSize.x, movingArrowSize.y );
	Vector2 slideEndDest = new Vector2( Screen.width, movingArrowRect.rect.y );
	Vector2 pointingFingerSize = new Vector2( Screen.width * 0.1f, Screen.width * 0.1f );
	float slideDuration = 0.75f;

	static Vector2 tiltDeviceSize = new Vector2( Screen.width * 0.2f, Screen.width * 0.2f );
	static LTRect tiltDeviceRect = new LTRect( -Screen.width, 0.78f * Screen.height, tiltDeviceSize.x, tiltDeviceSize.y );

	void Start () {
		//Reset values
		showTutorialMessage = false;
		showMovingArrow = false;
	}

	public void activateMovingArrow( TutorialHelpArrow tutorialHelpArrow )
	{
		if( showMovingArrow )
		{
			Debug.LogWarning("TutorialDisplay-activateMovingArrow: busy right now. Display is already active.");
			return;
		}
		this.tutorialHelpArrow = tutorialHelpArrow;

		switch (tutorialHelpArrow)
		{
		case TutorialHelpArrow.CHANGE_LANES_LEFT:
			movingArrowRect = new LTRect( Screen.width * 0.9f, 0.4f * Screen.height, movingArrowSize.x, movingArrowSize.y );
			slideEndDest = new Vector2( Screen.width * 0.35f, 0.4f * Screen.height );
			LeanTween.move( movingArrowRect, slideEndDest, slideDuration ).setEase(LeanTweenType.easeOutQuad).setOnComplete(slideInEnded).setOnCompleteParam(gameObject);
			break;
			
		case TutorialHelpArrow.CHANGE_LANES_RIGHT:
			movingArrowRect = new LTRect( Screen.width * 0.1f, 0.4f * Screen.height, movingArrowSize.x, movingArrowSize.y );
			slideEndDest = new Vector2( Screen.width * 0.65f, 0.4f * Screen.height );
			LeanTween.move( movingArrowRect, slideEndDest, slideDuration ).setEase(LeanTweenType.easeOutQuad).setOnComplete(slideInEnded).setOnCompleteParam(gameObject);
			break;
			
		case TutorialHelpArrow.JUMP:
			movingArrowRect = new LTRect( screenCenter - movingArrowRect.width * 0.5f, 0.7f * Screen.height, movingArrowSize.x, movingArrowSize.y );
			slideEndDest = new Vector2( screenCenter - movingArrowRect.width * 0.5f, 0.4f * Screen.height );
			LeanTween.move( movingArrowRect, slideEndDest, slideDuration ).setEase(LeanTweenType.easeOutQuad).setOnComplete(slideInEnded).setOnCompleteParam(gameObject);
			break;
			
		case TutorialHelpArrow.SLIDE:
			movingArrowRect = new LTRect( screenCenter - movingArrowRect.width * 0.5f, 0.4f * Screen.height, movingArrowSize.x, movingArrowSize.y );
			slideEndDest = new Vector2( screenCenter - movingArrowRect.width * 0.5f, 0.7f * Screen.height );
			LeanTween.move( movingArrowRect, slideEndDest, slideDuration ).setEase(LeanTweenType.easeOutQuad).setOnComplete(slideInEnded).setOnCompleteParam(gameObject);
			break;

		case TutorialHelpArrow.TILT_LEFT:
			tiltDeviceRect = new LTRect(screenCenter - tiltDeviceSize.x * 0.5f, 0.4f * Screen.height, tiltDeviceSize.x, tiltDeviceSize.y );
			LeanTween.rotate ( tiltDeviceRect, -12f, 0.75f ).setEase(LeanTweenType.easeOutQuad).setOnComplete(tiltEnded).setOnCompleteParam(gameObject);
			break;

		case TutorialHelpArrow.TILT_RIGHT:
			tiltDeviceRect = new LTRect(screenCenter - tiltDeviceSize.x * 0.5f, 0.4f * Screen.height, tiltDeviceSize.x, tiltDeviceSize.y );
			LeanTween.rotate ( tiltDeviceRect, 12f, 0.75f ).setEase(LeanTweenType.easeOutQuad).setOnComplete(tiltEnded).setOnCompleteParam(gameObject);
			break;

		}
		showMovingArrow = true;
	}

	void displayMovingArrow()
	{
		Rect arrowRect;
		switch (tutorialHelpArrow)
		{
		case TutorialHelpArrow.CHANGE_LANES_LEFT:
			GUI.BeginGroup(movingArrowRect.rect);
			//Draw arrow pointing right
			arrowRect = new Rect( 0, 0, arrowSize.x, arrowSize.y );
			GUI.DrawTextureWithTexCoords(arrowRect, rightArrow, rightTextureCoordFlipped );
			//Draw pointing finger
			arrowRect = new Rect( arrowRect.x, arrowSize.y * 0.5f, pointingFingerSize.x, pointingFingerSize.y );
			GUI.DrawTextureWithTexCoords(arrowRect, pointingFinger, textureCoordNormal );
			GUI.EndGroup();
			break;
			
		case TutorialHelpArrow.CHANGE_LANES_RIGHT:
			GUI.BeginGroup(movingArrowRect.rect);
			//Draw arrow pointing left
			arrowRect = new Rect( 0, 0, arrowSize.x, arrowSize.y );
			GUI.DrawTextureWithTexCoords(arrowRect, rightArrow, textureCoordNormal );
			//Draw pointing finger
			arrowRect = new Rect( arrowSize.x * 0.5f, arrowSize.y * 0.5f, pointingFingerSize.x, pointingFingerSize.y );
			GUI.DrawTextureWithTexCoords(arrowRect, pointingFinger, textureCoordNormal );
			GUI.EndGroup();
			break;
			
		case TutorialHelpArrow.JUMP:
			GUI.BeginGroup(movingArrowRect.rect);
			//Draw arrow pointing up
			arrowRect = new Rect( (movingArrowRect.rect.width - arrowSize.x) * 0.5f , 0, arrowSize.x, arrowSize.y );
			GUI.DrawTextureWithTexCoords(arrowRect, upArrow, textureCoordNormal );
			//Draw pointing finger
			arrowRect = new Rect( (movingArrowRect.rect.width - pointingFingerSize.x) * 0.5f, arrowSize.y * 0.5f, pointingFingerSize.x, pointingFingerSize.y );
			GUI.DrawTextureWithTexCoords(arrowRect, pointingFinger, textureCoordNormal );
			GUI.EndGroup();
			break;

		case TutorialHelpArrow.SLIDE:
			GUI.BeginGroup(movingArrowRect.rect);
			//Draw arrow pointing down
			arrowRect = new Rect( (movingArrowRect.rect.width - arrowSize.x) * 0.5f, 0, arrowSize.x, arrowSize.y );
			GUI.DrawTextureWithTexCoords(arrowRect, upArrow, upTextureCoordFlipped );
			//Draw pointing finger
			arrowRect = new Rect( (movingArrowRect.rect.width - pointingFingerSize.x) * 0.5f, arrowSize.y, pointingFingerSize.x, pointingFingerSize.y );
			GUI.DrawTextureWithTexCoords(arrowRect, pointingFinger, textureCoordNormal );
			GUI.EndGroup();
			break;

		case TutorialHelpArrow.TILT_LEFT:
		case TutorialHelpArrow.TILT_RIGHT:
			GUI.DrawTextureWithTexCoords(tiltDeviceRect.rect, device, textureCoordNormal );
			break;
		}
	}

	void slideInEnded ()
	{
		LeanTween.alpha(movingArrowRect,0,0.6f).setEase(LeanTweenType.easeOutQuad).setOnComplete(fadeOutEnded).setOnCompleteParam(gameObject);
	}

	void fadeOutEnded()
	{
		showMovingArrow = false;
	}

	void tiltEnded()
	{
		LeanTween.alpha(tiltDeviceRect,0,0).setEase(LeanTweenType.easeOutQuad).setOnComplete(fadeOutEnded).setOnCompleteParam(gameObject);
	}

	//Activates a horizontally centered text with a drop-shadow.
	//User texts are only displayed in the Normal game state.
	public void activateUserMessage( string userText, float userScreenHeightPercentage, float userAngle, float userMessageDuration, TutorialEvent activeTutorial )
	{
		this.userScreenHeightPercentage = userScreenHeightPercentage;
		this.userAngle = userAngle;
		this.userMessageDuration = userMessageDuration;
		this.activeTutorial = activeTutorial;
		showTutorialMessage = true;
		textStyle.fixedWidth = Screen.width * 0.8f;
		textContent = new GUIContent( userText );
		StartCoroutine("displayTutorial");
	}

	//Displays a horizontally centered user text with a drop-shadow that was activated with the method: activateUserMessage.
	private void displayUserMessage()
	{
		Rect textRect = GUILayoutUtility.GetRect( textContent, textStyle );
		float textCenterX = (Screen.width-textRect.width) * 0.5f;	
		Rect positionRect = new Rect( textCenterX, Screen.height * userScreenHeightPercentage, textRect.width, textRect.height );

		//Draw the text
		//Save the GUI.matrix so we can restore it once our rotation is done
		Matrix4x4 matrixBackup = GUI.matrix;
		Vector2 pos = new Vector2( positionRect.x, positionRect.y );
		GUIUtility.RotateAroundPivot(userAngle, pos);
		//Utilities.drawLabelWithDropShadow( positionRect, textContent, textStyle );
		GUI.matrix = matrixBackup;

		drawInstructionArrows();
	}

	private void drawInstructionArrows()
	{
		Rect arrowRect;
		switch (activeTutorial)
		{
		case TutorialEvent.CHANGE_LANES:
			//Now draw the arrows
			arrowRect = new Rect( screenCenter + arrowMargin, Screen.height * userScreenHeightPercentage * 1.35f, arrowSize.x, arrowSize.y );
			//Draw arrow pointing right
			GUI.DrawTextureWithTexCoords(arrowRect, rightArrow, textureCoordNormal );
			//Draw arrow pointing left
			arrowRect = new Rect( screenCenter - arrowSize.x - arrowMargin, Screen.height * userScreenHeightPercentage * 1.35f, arrowSize.x, arrowSize.y );
			GUI.DrawTextureWithTexCoords(arrowRect, rightArrow, rightTextureCoordFlipped );
			break;
			
		case TutorialEvent.JUMP:
			//Now draw the arrows
			arrowRect = new Rect( screenCenter - arrowSize.x * 0.5f, Screen.height * userScreenHeightPercentage * 0.6f, arrowSize.x, arrowSize.y );
			//Draw arrow pointing up
			GUI.DrawTextureWithTexCoords(arrowRect, upArrow, textureCoordNormal );
			break;
			
		case TutorialEvent.TURN_CORNERS:
			//Now draw the arrows
			arrowRect = new Rect( screenCenter + arrowMargin, Screen.height * userScreenHeightPercentage * 1.35f, arrowSize.x, arrowSize.y );
			//Draw arrow pointing right
			GUI.DrawTextureWithTexCoords(arrowRect, rightArrow, textureCoordNormal );
			//Draw arrow pointing left
			arrowRect = new Rect( screenCenter - arrowSize.x - arrowMargin, Screen.height * userScreenHeightPercentage * 1.35f, arrowSize.x, arrowSize.y );
			GUI.DrawTextureWithTexCoords(arrowRect, rightArrow, rightTextureCoordFlipped );
			break;
			
		case TutorialEvent.SLIDE:
			//Now draw the arrows
			arrowRect = new Rect( screenCenter - arrowSize.x * 0.5f, Screen.height * userScreenHeightPercentage * 1.1f, arrowSize.x, arrowSize.y );
			//Draw arrow pointing up
			GUI.DrawTextureWithTexCoords(arrowRect, upArrow, upTextureCoordFlipped );
			break;
			
		case TutorialEvent.SLIDE_BREAKABLE:
			//Now draw the arrows
			arrowRect = new Rect( screenCenter - arrowSize.x * 0.5f, Screen.height * userScreenHeightPercentage * 1.1f, arrowSize.x, arrowSize.y );
			//Draw arrow pointing up
			GUI.DrawTextureWithTexCoords(arrowRect, upArrow, upTextureCoordFlipped );
			break;
			
		case TutorialEvent.TILT_CHANGE_LANES:
			break;
			
		}
	}

	private IEnumerator displayTutorial()
	{
		showTutorialMessage = true;
		do
		{
			userMessageDuration = userMessageDuration - Time.deltaTime;
			yield return _sync();
		} while ( userMessageDuration > 0 );
		
		showTutorialMessage = false;
	}

	void OnGUI ()
	{
		if( showTutorialMessage && GameManager.Instance.getGameState() != GameState.Countdown )
		{
			displayUserMessage();
		}
		if( showMovingArrow )
		{
			displayMovingArrow();
		}
	}
}
