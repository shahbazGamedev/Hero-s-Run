using UnityEngine;
using System.Collections;

public enum TutorialHelpArrow {
	NONE = -1,
	CHANGE_LANES_LEFT = 0,
	CHANGE_LANES_RIGHT = 1,
	JUMP = 2,
	SLIDE = 3,
	DOUBLE_TAP = 4,
	TILT_LEFT = 5,
	TILT_RIGHT = 6
}


public class TutorialArrowTrigger : MonoBehaviour {

	public TutorialHelpArrow tutorialHelpArrow;
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
			tutorialManager.displayHelpArrow(tutorialHelpArrow);
		}
	}
}
