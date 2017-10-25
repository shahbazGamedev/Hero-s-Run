using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class NewTutorialManager : MonoBehaviour {

	[Header("Tutorial Manager")]
	public bool isTutorialActive = false;
	public GameObject panel;
	public Text instructionText;
	public Image pointingFinger;
	public Image tiltDevice;
	public Image tapDevice;
	Dictionary<NewTutorialEvent,string> tutorialTexts = new Dictionary<NewTutorialEvent, string>(9);
	Dictionary<NewTutorialEvent,string> tutorialTextsIfFailed = new Dictionary<NewTutorialEvent, string>(9);
	NewTutorialEvent activeTutorial;
	PowerUpType powerUpUsedInTutorial = PowerUpType.SpeedBoost;
	const float HORIZONTAL_DURATION = 0.9f;
	const float VERTICAL_DURATION = 0.8f;
	const float TILT_DURATION = 0.9f;
	const float FADE_IN_DURATION = 0.25f;
	const float PAUSE_DELAY = 0.5f;
	Vector2 leftArrow;
	Vector2 rightArrow;
	Vector2 middleText;
	Vector2 highText;
	Vector2 lowArrow;
	Vector2 highArrow;
	int loopCounter = 0;
	string methodToInvoke;

	void Awake ()
	{
		//Populate tutorialTexts
		tutorialTexts.Clear();
		tutorialTexts.Add(NewTutorialEvent.Change_Lane_Left, LocalizationManager.Instance.getText("TUTORIAL_CHANGE_LANES"));
		tutorialTexts.Add(NewTutorialEvent.Change_Lane_Right, LocalizationManager.Instance.getText("TUTORIAL_CHANGE_LANES"));
		tutorialTexts.Add(NewTutorialEvent.Turn_Corner_Left, LocalizationManager.Instance.getText("TUTORIAL_TURN_CORNERS"));
		tutorialTexts.Add(NewTutorialEvent.Turn_Corner_Right, LocalizationManager.Instance.getText("TUTORIAL_TURN_CORNERS"));
		tutorialTexts.Add(NewTutorialEvent.Jump, LocalizationManager.Instance.getText("TUTORIAL_JUMP"));
		tutorialTexts.Add(NewTutorialEvent.Slide, LocalizationManager.Instance.getText("TUTORIAL_SLIDE"));
		tutorialTexts.Add(NewTutorialEvent.Slide_Breakable, LocalizationManager.Instance.getText("TUTORIAL_SLIDE_BREAKABLE"));
		tutorialTexts.Add(NewTutorialEvent.Tilt, LocalizationManager.Instance.getText("TUTORIAL_TILT_CHANGE_LANES"));
		tutorialTexts.Add(NewTutorialEvent.Activate_Power_Up, LocalizationManager.Instance.getText("TUTORIAL_ACTIVATE_POWER_UP"));
		tutorialTexts.Add(NewTutorialEvent.Attach_to_Zipline, LocalizationManager.Instance.getText("TUTORIAL_ATTACH_TO_ZIPLINE"));

		//Text used in the Save Me screen if the player failed a tutorial
		tutorialTextsIfFailed.Clear();
		tutorialTextsIfFailed.Add(NewTutorialEvent.Tutorial_Start, LocalizationManager.Instance.getText("TUTORIAL_TRY_AGAIN"));
		tutorialTextsIfFailed.Add(NewTutorialEvent.Change_Lane_Left, LocalizationManager.Instance.getText("TUTORIAL_CHANGE_LANES_FAIL"));
		tutorialTextsIfFailed.Add(NewTutorialEvent.Change_Lane_Right, LocalizationManager.Instance.getText("TUTORIAL_CHANGE_LANES_FAIL"));
		tutorialTextsIfFailed.Add(NewTutorialEvent.Turn_Corner_Left, LocalizationManager.Instance.getText("TUTORIAL_TURN_CORNERS"));
		tutorialTextsIfFailed.Add(NewTutorialEvent.Turn_Corner_Right, LocalizationManager.Instance.getText("TUTORIAL_TURN_CORNERS"));
		tutorialTextsIfFailed.Add(NewTutorialEvent.Jump, LocalizationManager.Instance.getText("TUTORIAL_JUMP_FAIL"));
		tutorialTextsIfFailed.Add(NewTutorialEvent.Slide, LocalizationManager.Instance.getText("TUTORIAL_SLIDE_FAIL"));
		tutorialTextsIfFailed.Add(NewTutorialEvent.Slide_Breakable, LocalizationManager.Instance.getText("TUTORIAL_SLIDE_BREAKABLE_FAIL"));
		tutorialTextsIfFailed.Add(NewTutorialEvent.Tilt, LocalizationManager.Instance.getText("TUTORIAL_TRY_AGAIN"));
		tutorialTextsIfFailed.Add(NewTutorialEvent.Activate_Power_Up, LocalizationManager.Instance.getText("TUTORIAL_TRY_AGAIN"));
		tutorialTextsIfFailed.Add(NewTutorialEvent.Attach_to_Zipline, LocalizationManager.Instance.getText("TUTORIAL_ATTACH_TO_ZIPLINE"));

		leftArrow = new Vector2( -100f, -130f );
		rightArrow = new Vector2( 400f, -130f );

		lowArrow = new Vector2( 150f, -357 );
		highArrow = new Vector2( 150f, -67f );

		middleText = new Vector2( 150f, -230f );
		highText = new Vector2( 150f, 28f );

	}

	void handleTutorialEvent( NewTutorialEvent tutorialEvent )
	{
		Debug.Log("TutorialEventTriggered " + tutorialEvent );
		activeTutorial = tutorialEvent;

		switch (tutorialEvent)
		{
		case NewTutorialEvent.Tutorial_Start:
			isTutorialActive = true;
			break;
			
		case NewTutorialEvent.Tutorial_End:
			isTutorialActive = false;
			break;
			
		case NewTutorialEvent.Change_Lane_Left:
			methodToInvoke = "startLeftSlide";
			instructionText.GetComponent<RectTransform>().anchoredPosition = middleText;
			instructionText.text = tutorialTexts[NewTutorialEvent.Change_Lane_Left];
			instructionText.gameObject.SetActive( true );
			instructionText.color = new Color(1f,1f,1f, 0 );
			LeanTween.colorText(instructionText.GetComponent<RectTransform>(), new Color(1f,1f,1f, 1f ), FADE_IN_DURATION ).setOnComplete(startLeftSlide).setOnCompleteParam(gameObject);
			break;
			
		case NewTutorialEvent.Change_Lane_Right:
			methodToInvoke = "startRightSlide";
			instructionText.GetComponent<RectTransform>().anchoredPosition = middleText;
			instructionText.text = tutorialTexts[NewTutorialEvent.Change_Lane_Right];
			instructionText.gameObject.SetActive( true );
			instructionText.color = new Color(1f,1f,1f, 0 );
			LeanTween.colorText(instructionText.GetComponent<RectTransform>(), new Color(1f,1f,1f, 1f ), FADE_IN_DURATION ).setOnComplete(startRightSlide).setOnCompleteParam(gameObject);
			break;
			
		case NewTutorialEvent.Turn_Corner_Left:
			methodToInvoke = "startLeftSlide";
			instructionText.GetComponent<RectTransform>().anchoredPosition = middleText;
			instructionText.text = tutorialTexts[NewTutorialEvent.Turn_Corner_Left];
			instructionText.gameObject.SetActive( true );
			instructionText.color = new Color(1f,1f,1f, 0 );
			LeanTween.colorText(instructionText.GetComponent<RectTransform>(), new Color(1f,1f,1f, 1f ), FADE_IN_DURATION ).setOnComplete(startLeftSlide).setOnCompleteParam(gameObject);
			break;
			
		case NewTutorialEvent.Turn_Corner_Right:
			methodToInvoke = "startRightSlide";
			instructionText.GetComponent<RectTransform>().anchoredPosition = middleText;
			instructionText.text = tutorialTexts[NewTutorialEvent.Turn_Corner_Right];
			instructionText.gameObject.SetActive( true );
			instructionText.color = new Color(1f,1f,1f, 0 );
			LeanTween.colorText(instructionText.GetComponent<RectTransform>(), new Color(1f,1f,1f, 1f ), FADE_IN_DURATION ).setOnComplete(startRightSlide).setOnCompleteParam(gameObject);
			break;

		case NewTutorialEvent.Jump:
			methodToInvoke = "startUpSlide";
			instructionText.GetComponent<RectTransform>().anchoredPosition = highText;
			instructionText.text = tutorialTexts[NewTutorialEvent.Jump];
			instructionText.gameObject.SetActive( true );
			instructionText.color = new Color(1f,1f,1f, 0 );
			LeanTween.colorText(instructionText.GetComponent<RectTransform>(), new Color(1f,1f,1f, 1f ), FADE_IN_DURATION ).setOnComplete(startUpSlide).setOnCompleteParam(gameObject);
			break;
			
		case NewTutorialEvent.Slide:
			methodToInvoke = "startDownSlide";
			instructionText.GetComponent<RectTransform>().anchoredPosition = highText;
			instructionText.text = tutorialTexts[NewTutorialEvent.Slide];
			instructionText.gameObject.SetActive( true );
			instructionText.color = new Color(1f,1f,1f, 0 );
			LeanTween.colorText(instructionText.GetComponent<RectTransform>(), new Color(1f,1f,1f, 1f ), FADE_IN_DURATION ).setOnComplete(startDownSlide).setOnCompleteParam(gameObject);
			break;

		case NewTutorialEvent.Slide_Breakable:
			methodToInvoke = "startDownSlide";
			instructionText.GetComponent<RectTransform>().anchoredPosition = highText;
			instructionText.text = tutorialTexts[NewTutorialEvent.Slide_Breakable];
			instructionText.gameObject.SetActive( true );
			instructionText.color = new Color(1f,1f,1f, 0 );
			LeanTween.colorText(instructionText.GetComponent<RectTransform>(), new Color(1f,1f,1f, 1f ), FADE_IN_DURATION ).setOnComplete(startDownSlide).setOnCompleteParam(gameObject);
			break;

		case NewTutorialEvent.Tilt:
			instructionText.GetComponent<RectTransform>().anchoredPosition = middleText;
			instructionText.text = tutorialTexts[NewTutorialEvent.Tilt];
			instructionText.gameObject.SetActive( true );
			instructionText.color = new Color(1f,1f,1f, 0 );
			tiltDevice.GetComponent<RectTransform>().localEulerAngles = Vector3.zero;
			tiltDevice.gameObject.SetActive( true );
			tiltDevice.color = new Color(1f,1f,1f, 1f );
			LeanTween.colorText(instructionText.GetComponent<RectTransform>(), new Color(1f,1f,1f, 1f ), FADE_IN_DURATION ).setOnComplete(startTilt).setOnCompleteParam(gameObject);
			break;

		case NewTutorialEvent.Activate_Power_Up:
			//Make sure we have at least one power-up
			int numberOfPowerUps = PlayerStatsManager.Instance.getPowerUpQuantity(powerUpUsedInTutorial);
			if( numberOfPowerUps == 0 ) PlayerStatsManager.Instance.addToPowerUpInventory(powerUpUsedInTutorial,1);
			PlayerStatsManager.Instance.setPowerUpSelected(powerUpUsedInTutorial);

			instructionText.GetComponent<RectTransform>().anchoredPosition = middleText;
			instructionText.text = tutorialTexts[NewTutorialEvent.Activate_Power_Up];
			instructionText.gameObject.SetActive( true );
			instructionText.color = new Color(1f,1f,1f, 0 );
			LeanTween.colorText(instructionText.GetComponent<RectTransform>(), new Color(1f,1f,1f, 1f ), FADE_IN_DURATION ).setOnComplete(startActivatePowerUp).setOnCompleteParam(gameObject);

			tapDevice.gameObject.SetActive( true );
			tapDevice.color = new Color(1f,1f,1f, 0 );
			LeanTween.color(tapDevice.GetComponent<RectTransform>(), new Color(1f,1f,1f, 1f ), FADE_IN_DURATION );
			break;

		case NewTutorialEvent.Attach_to_Zipline:
			methodToInvoke = "startUpSlide";
			instructionText.GetComponent<RectTransform>().anchoredPosition = highText;
			instructionText.text = tutorialTexts[NewTutorialEvent.Attach_to_Zipline];
			instructionText.gameObject.SetActive( true );
			instructionText.color = new Color(1f,1f,1f, 0 );
			LeanTween.colorText(instructionText.GetComponent<RectTransform>(), new Color(1f,1f,1f, 1f ), FADE_IN_DURATION ).setOnComplete(startUpSlide).setOnCompleteParam(gameObject);
			break;
		}
	}

	void startLeftSlide()
	{
		pointingFinger.GetComponent<RectTransform>().localEulerAngles = Vector3.zero;
		pointingFinger.GetComponent<RectTransform>().anchoredPosition = leftArrow;
		pointingFinger.gameObject.SetActive( true );
		LeanTween.moveX( pointingFinger.GetComponent<RectTransform>(),rightArrow.x, HORIZONTAL_DURATION ).setOnComplete(slideEnded).setOnCompleteParam(gameObject).setEase(LeanTweenType.easeOutQuad);
	}

	void startRightSlide()
	{
		pointingFinger.GetComponent<RectTransform>().localEulerAngles = Vector3.zero;
		pointingFinger.GetComponent<RectTransform>().anchoredPosition = rightArrow;
		pointingFinger.gameObject.SetActive( true );
		LeanTween.moveX( pointingFinger.GetComponent<RectTransform>(),leftArrow.x, HORIZONTAL_DURATION ).setOnComplete(slideEnded).setOnCompleteParam(gameObject).setEase(LeanTweenType.easeOutQuad);
	}

	void startUpSlide()
	{
		pointingFinger.GetComponent<RectTransform>().localEulerAngles = new Vector3( 0,0,90f);
		pointingFinger.GetComponent<RectTransform>().anchoredPosition = lowArrow;
		pointingFinger.gameObject.SetActive( true );
		LeanTween.moveY( pointingFinger.GetComponent<RectTransform>(),highArrow.y, VERTICAL_DURATION ).setOnComplete(slideEnded).setOnCompleteParam(gameObject).setEase(LeanTweenType.easeOutQuad);
	}

	void startDownSlide()
	{
		pointingFinger.GetComponent<RectTransform>().localEulerAngles = new Vector3( 0,0,90f);
		pointingFinger.GetComponent<RectTransform>().anchoredPosition = highArrow;
		pointingFinger.gameObject.SetActive( true );
		LeanTween.moveY( pointingFinger.GetComponent<RectTransform>(),lowArrow.y, VERTICAL_DURATION ).setOnComplete(slideEnded).setOnCompleteParam(gameObject).setEase(LeanTweenType.easeOutQuad);
	}

	void slideEnded()
	{
		if( loopCounter < 2 ) //it loops 3 times, but we already called the method once.
		{
			loopCounter++;
			Invoke( methodToInvoke, PAUSE_DELAY );
		}
		else
		{
			loopCounter = 0;
			instructionText.gameObject.SetActive( false );
			pointingFinger.gameObject.SetActive( false );
		}
	}

	void startTilt()
	{
		tiltDevice.GetComponent<RectTransform>().localEulerAngles = Vector3.zero;
		tiltDevice.gameObject.SetActive( true );
		LeanTween.rotate( tiltDevice.GetComponent<RectTransform>(), 12f, TILT_DURATION ).setEase(LeanTweenType.easeOutQuad).setOnComplete(firstTiltEnded).setOnCompleteParam(gameObject);
	}

	void firstTiltEnded()
	{
		LeanTween.rotate( tiltDevice.GetComponent<RectTransform>(), -24f, TILT_DURATION ).setEase(LeanTweenType.easeOutQuad).setOnComplete(secondTiltEnded).setOnCompleteParam(gameObject);
	}

	void secondTiltEnded()
	{
		LeanTween.rotate( tiltDevice.GetComponent<RectTransform>(), 24f, TILT_DURATION ).setEase(LeanTweenType.easeOutQuad).setOnComplete(thirdTiltEnded).setOnCompleteParam(gameObject);
	}

	void thirdTiltEnded()
	{
		LeanTween.rotate( tiltDevice.GetComponent<RectTransform>(), 12f, TILT_DURATION ).setEase(LeanTweenType.easeOutQuad).setOnComplete(tiltEnded).setOnCompleteParam(gameObject);
	}
	
	void tiltEnded()
	{
		tiltDevice.gameObject.SetActive( false );
		instructionText.gameObject.SetActive( false );
	}

	void startActivatePowerUp()
	{
		Invoke("endActivatePowerUp", 5f );
	}

	void endActivatePowerUp()
	{
		instructionText.gameObject.SetActive( false );
		tapDevice.gameObject.SetActive( false );
	}

	public string getFailedTutorialText()
	{
		if( tutorialTextsIfFailed.ContainsKey(activeTutorial) )
		{
			return tutorialTextsIfFailed[activeTutorial];
		}
		else
		{
			return "FAILED TUTORIAL TEXT NOT FOUND";
		}
	}

	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
		NewTutorialTrigger.tutorialEventTriggered += TutorialEventTriggered;
		PlayerController.playerStateChanged += PlayerStateChange;
	}

	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		NewTutorialTrigger.tutorialEventTriggered -= TutorialEventTriggered;
		PlayerController.playerStateChanged -= PlayerStateChange;
	}

	void TutorialEventTriggered( NewTutorialEvent tutorialEvent )
	{
		handleTutorialEvent( tutorialEvent );
	}

	void GameStateChange( GameState previousState, GameState newState )
	{
		if( newState == GameState.Normal )
		{
			panel.SetActive( true );
		}
		else
		{
			//Hide any active tutorial
			panel.SetActive( false );
		}
	}

	void PlayerStateChange( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying )
		{
			//Hide any active tutorial
			panel.SetActive( false );
		}
		else
		{
			panel.SetActive( true );
		}
	}

}
