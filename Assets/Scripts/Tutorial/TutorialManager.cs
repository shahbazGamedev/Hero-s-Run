using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TutorialManager : MonoBehaviour {
	
	Transform player;
	TutorialDisplay tutorialDisplay;
	
	Dictionary<TutorialEvent,string> tutorialTexts = new Dictionary<TutorialEvent, string>(8);
	static Dictionary<TutorialEvent,string> tutorialTextsIfFailed = new Dictionary<TutorialEvent, string>(5);
	public static TutorialEvent activeTutorial = TutorialEvent.CHANGE_LANES;

	void Awake ()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
		
		tutorialDisplay = player.gameObject.GetComponent<TutorialDisplay>();

		//Populate tutorialTexts
		tutorialTexts.Clear();
		tutorialTexts.Add(TutorialEvent.CHANGE_LANES, LocalizationManager.Instance.getText("TUTORIAL_CHANGE_LANES"));
		tutorialTexts.Add(TutorialEvent.TURN_CORNERS, LocalizationManager.Instance.getText("TUTORIAL_TURN_CORNERS"));
		tutorialTexts.Add(TutorialEvent.JUMP, LocalizationManager.Instance.getText("TUTORIAL_JUMP"));
		tutorialTexts.Add(TutorialEvent.SLIDE, LocalizationManager.Instance.getText("TUTORIAL_SLIDE"));
		tutorialTexts.Add(TutorialEvent.SLIDE_BREAKABLE, LocalizationManager.Instance.getText("TUTORIAL_SLIDE_BREAKABLE"));
		tutorialTexts.Add(TutorialEvent.TILT_CHANGE_LANES, LocalizationManager.Instance.getText("TUTORIAL_TILT_CHANGE_LANES"));
		tutorialTexts.Add(TutorialEvent.ACTIVATE_POWER_UP, LocalizationManager.Instance.getText("TUTORIAL_ACTIVATE_POWER_UP"));

		//Text used in the Save Me screen if the player failed a tutorial
		tutorialTextsIfFailed.Clear();
		tutorialTextsIfFailed.Add(TutorialEvent.CHANGE_LANES, LocalizationManager.Instance.getText("TUTORIAL_CHANGE_LANES_FAIL"));
		tutorialTextsIfFailed.Add(TutorialEvent.TURN_CORNERS, LocalizationManager.Instance.getText("TUTORIAL_TURN_CORNERS_FAIL"));
		tutorialTextsIfFailed.Add(TutorialEvent.JUMP, LocalizationManager.Instance.getText("TUTORIAL_JUMP_FAIL"));
		tutorialTextsIfFailed.Add(TutorialEvent.SLIDE, LocalizationManager.Instance.getText("TUTORIAL_SLIDE_FAIL"));
		tutorialTextsIfFailed.Add(TutorialEvent.SLIDE_BREAKABLE, LocalizationManager.Instance.getText("TUTORIAL_SLIDE_BREAKABLE_FAIL"));

	}

	public void tutorialEventTriggered( TutorialEvent tutorialEvent )
	{
		if( tutorialEvent == TutorialEvent.NEXT_TUTORIAL )
		{
			activeTutorial++;
			displayExcellentMessage();
			print ("Tutorial completed - next tutorial is " + activeTutorial );
		}
		else if( tutorialEvent == TutorialEvent.SUCCESS )
		{
			displayGoodMessage();
		}
		else
		{
			activeTutorial = tutorialEvent;
			tutorialDisplay.activateUserMessage( tutorialTexts[tutorialEvent], 0.3f, 0, 3f, activeTutorial );
			print ("Tutorial started - current tutorial is " + activeTutorial );
		}
	}

	public void displayHelpArrow( TutorialHelpArrow tutorialHelpArrow )
	{
		if( tutorialHelpArrow == TutorialHelpArrow.DOUBLE_TAP )
		{
			//Simply display text
			HUDHandler.activateUserMessage( LocalizationManager.Instance.getText("TUTORIAL_DOUBLE_TAP_NOW"), 0.4f, -7f, 1.5f );
		}
		else
		{
			tutorialDisplay.activateMovingArrow( tutorialHelpArrow );
		}
	}

	public static string getFailedTutorialText()
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

	void displayGoodMessage()
	{
		HUDHandler.activateUserMessage( LocalizationManager.Instance.getText("TUTORIAL_GOOD"), 0.42f, -7f, 1.5f );
	}

	void displayExcellentMessage()
	{
		HUDHandler.activateUserMessage( LocalizationManager.Instance.getText("TUTORIAL_EXCELLENT"), 0.42f, -7f, 1.5f );
	}
	
}
