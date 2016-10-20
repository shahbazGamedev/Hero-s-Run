using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class NewTutorialManager : MonoBehaviour {

	[Header("Post Level Popup")]
	public bool isTutorialActive = false;
	public Text instructionText;
	public Image pointingFinger;
	Dictionary<NewTutorialEvent,string> tutorialTexts = new Dictionary<NewTutorialEvent, string>(8);
	Dictionary<NewTutorialEvent,string> tutorialTextsIfFailed = new Dictionary<NewTutorialEvent, string>(5);
	NewTutorialEvent activeTutorial;
	PowerUpType powerUpUsedInTutorial = PowerUpType.SpeedBoost;
	const float SLIDE_DURATION = 0.9f;
	const float END_OF_SLIDE_PAUSE = 0.3f;
	Vector2 slideStartDest;
	Vector2 slideEndDest;
	int loopCounter = 0;

	void Awake ()
	{
		//Populate tutorialTexts
		tutorialTexts.Clear();
		tutorialTexts.Add(NewTutorialEvent.Change_Lane_Left, LocalizationManager.Instance.getText("TUTORIAL_CHANGE_LANES"));
		tutorialTexts.Add(NewTutorialEvent.Change_Lane_Right, LocalizationManager.Instance.getText("TUTORIAL_CHANGE_LANES"));
		tutorialTexts.Add(NewTutorialEvent.Turn_Corner, LocalizationManager.Instance.getText("TUTORIAL_TURN_CORNERS"));
		tutorialTexts.Add(NewTutorialEvent.Jump, LocalizationManager.Instance.getText("TUTORIAL_JUMP"));
		tutorialTexts.Add(NewTutorialEvent.Slide, LocalizationManager.Instance.getText("TUTORIAL_SLIDE"));
		tutorialTexts.Add(NewTutorialEvent.Slide_Breakable, LocalizationManager.Instance.getText("TUTORIAL_SLIDE_BREAKABLE"));
		tutorialTexts.Add(NewTutorialEvent.Tilt_Left, LocalizationManager.Instance.getText("TUTORIAL_TILT_CHANGE_LANES"));
		tutorialTexts.Add(NewTutorialEvent.Tilt_Right, LocalizationManager.Instance.getText("TUTORIAL_TILT_CHANGE_LANES"));
		tutorialTexts.Add(NewTutorialEvent.Activate_Power_Up, LocalizationManager.Instance.getText("TUTORIAL_ACTIVATE_POWER_UP"));

		//Text used in the Save Me screen if the player failed a tutorial
		tutorialTextsIfFailed.Clear();
		tutorialTextsIfFailed.Add(NewTutorialEvent.Change_Lane_Left, LocalizationManager.Instance.getText("TUTORIAL_CHANGE_LANES_FAIL"));
		tutorialTextsIfFailed.Add(NewTutorialEvent.Change_Lane_Right, LocalizationManager.Instance.getText("TUTORIAL_TURN_CORNERS_FAIL"));
		tutorialTextsIfFailed.Add(NewTutorialEvent.Jump, LocalizationManager.Instance.getText("TUTORIAL_JUMP_FAIL"));
		tutorialTextsIfFailed.Add(NewTutorialEvent.Slide, LocalizationManager.Instance.getText("TUTORIAL_SLIDE_FAIL"));
		tutorialTextsIfFailed.Add(NewTutorialEvent.Slide_Breakable, LocalizationManager.Instance.getText("TUTORIAL_SLIDE_BREAKABLE_FAIL"));

		slideStartDest = new Vector2( 50f, pointingFinger.GetComponent<RectTransform>().anchoredPosition.y );
		slideEndDest = new Vector2( pointingFinger.GetComponent<RectTransform>().anchoredPosition.x + 500f, pointingFinger.GetComponent<RectTransform>().anchoredPosition.y );

	}

	void handleTutorialEvent( NewTutorialEvent tutorialEvent )
	{
		Debug.LogWarning("TutorialEventTriggered " + tutorialEvent );
		activeTutorial = tutorialEvent;

		switch (tutorialEvent)
		{
		case NewTutorialEvent.Tutorial_Start:
			isTutorialActive = true;
			//Make sure we have at least one power-up
			int numberOfPowerUps = PlayerStatsManager.Instance.getPowerUpQuantity(powerUpUsedInTutorial);
			if( numberOfPowerUps == 0 ) PlayerStatsManager.Instance.addToPowerUpInventory(powerUpUsedInTutorial,1);
			PlayerStatsManager.Instance.setPowerUpSelected(powerUpUsedInTutorial);
			break;
			
		case NewTutorialEvent.Tutorial_End:
			isTutorialActive = false;
			break;
			
		case NewTutorialEvent.Change_Lane_Left:
			//LeanTween.move( movingArrowRect, slideEndDest, slideDuration ).setEase(LeanTweenType.easeOutQuad).setOnComplete(slideInEnded).setOnCompleteParam(gameObject);
			break;
			
		case NewTutorialEvent.Change_Lane_Right:
			pointingFinger.gameObject.SetActive( true );
			pointingFinger.GetComponent<RectTransform>().anchoredPosition = slideStartDest;
			LeanTween.moveX( pointingFinger.GetComponent<RectTransform>(),pointingFinger.GetComponent<RectTransform>().anchoredPosition.x + 500f, SLIDE_DURATION ).setOnComplete(slideEnded).setOnCompleteParam(gameObject).setLoopType(LeanTweenType.easeInOutQuad).setLoopCount(3);
			break;
			
		case NewTutorialEvent.Jump:
			//LeanTween.move( movingArrowRect, slideEndDest, slideDuration ).setEase(LeanTweenType.easeOutQuad).setOnComplete(slideInEnded).setOnCompleteParam(gameObject);
			break;
			
		case NewTutorialEvent.Slide:
			//LeanTween.move( movingArrowRect, slideEndDest, slideDuration ).setEase(LeanTweenType.easeOutQuad).setOnComplete(slideInEnded).setOnCompleteParam(gameObject);
			break;

		case NewTutorialEvent.Tilt_Left:
			//tiltDeviceRect = new LTRect(screenCenter - tiltDeviceSize.x * 0.5f, 0.4f * Screen.height, tiltDeviceSize.x, tiltDeviceSize.y );
			//LeanTween.rotate ( tiltDeviceRect, -12f, 0.75f ).setEase(LeanTweenType.easeOutQuad).setOnComplete(tiltEnded).setOnCompleteParam(gameObject);
			break;

		case NewTutorialEvent.Tilt_Right:
			//tiltDeviceRect = new LTRect(screenCenter - tiltDeviceSize.x * 0.5f, 0.4f * Screen.height, tiltDeviceSize.x, tiltDeviceSize.y );
			//LeanTween.rotate ( tiltDeviceRect, 12f, 0.75f ).setEase(LeanTweenType.easeOutQuad).setOnComplete(tiltEnded).setOnCompleteParam(gameObject);
			break;

		}
	}

	void slideEnded()
	{
		instructionText.gameObject.SetActive( false );
		pointingFinger.gameObject.SetActive( false );
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
		NewTutorialTrigger.tutorialEventTriggered += TutorialEventTriggered;
	}

	void OnDisable()
	{
		NewTutorialTrigger.tutorialEventTriggered -= TutorialEventTriggered;
	}

	void TutorialEventTriggered( NewTutorialEvent tutorialEvent )
	{
		handleTutorialEvent( tutorialEvent );
	}
}
