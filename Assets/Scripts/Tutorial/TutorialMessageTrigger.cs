using UnityEngine;
using System.Collections;

public enum TutorialEvent {
	NONE = -1,
	CHANGE_LANES = 0,
	TURN_CORNERS = 1,
	JUMP = 2,
	SLIDE = 3,
	SLIDE_BREAKABLE = 4,
	ACTIVATE_POWER_UP = 5,
	TILT_CHANGE_LANES = 6,
	SUCCESS = 8,
	NEXT_TUTORIAL = 9
}


public class TutorialMessageTrigger : MonoBehaviour {

	public TutorialEvent tutorialEvent;
	TutorialManager tutorialManager;

	void Awake()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		
		tutorialManager = player.GetComponent<TutorialManager>();
	}

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" )
		{
			tutorialManager.tutorialEventTriggered( tutorialEvent );
		}
	}
}
