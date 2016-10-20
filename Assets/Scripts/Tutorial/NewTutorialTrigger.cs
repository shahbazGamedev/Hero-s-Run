using UnityEngine;
using System.Collections;

public enum NewTutorialEvent {
	Not_Set = 0,
	Tutorial_Start = 1,
	Tutorial_End = 2,
	Change_Lane_Left = 3,
	Change_Lane_Right = 4,
	Turn_Corner = 5,
	Jump = 6,
	Slide = 7,
	Slide_Breakable = 8,
	Tilt_Left = 9,
	Tilt_Right = 10,
	Activate_Power_Up = 11
}

public class NewTutorialTrigger : MonoBehaviour {

	public NewTutorialEvent tutorialEvent = NewTutorialEvent.Not_Set;
	
	//Delegate used to communicate to other classes when a tutorial has been triggered
	public delegate void TutorialEventTriggered( NewTutorialEvent tutorialEvent );
	public static event TutorialEventTriggered tutorialEventTriggered;

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" )
		{
			if( tutorialEventTriggered != null ) tutorialEventTriggered( tutorialEvent );
		}
	}
}
