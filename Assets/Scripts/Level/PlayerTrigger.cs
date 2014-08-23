using UnityEngine;
using System.Collections;

public enum GameEvent {
	Start_Raining = 0,
	Stop_Raining = 1,
	Call_Succubus = 2,
	Dismiss_Succubus = 3,
	Start_Moving = 4,
	Stop_Moving = 5,
	Start_Lightning = 6,
	Stop_Lightning = 7,
	Lightning_Flash = 8,
	Start_Fog = 9,
	Stop_Fog = 10,
	Give_Powerup = 11, //Used for tutorial
	Lower_Gate = 12,
	Open_Door = 13,
	Raise_Draw_Bridge = 14,
	Activate_Cullis_Gate = 15,
	Start_Snowing = 16,
	Stop_Snowing = 17,
	Fairy_Message = 18,
	Falling_Tree = 19
}

public class PlayerTrigger : MonoBehaviour {

	//Event management used to notify other classes when the player has crossed the trigger zone.
	//This event could trigger an in-game event such as a zombie wave, or get a cow to start walking.
	public delegate void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier );
	public static event PlayerEnteredTrigger playerEnteredTrigger;
	public GameEvent eventType;
	//The optional uniqueGameObjectIdentifier is sent along with the event. The receiver can use it to verify that the event is really destined for him.
	public GameObject uniqueGameObjectIdentifier;

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" )
		{
			//Send an event to interested classes
			if(playerEnteredTrigger != null) playerEnteredTrigger(eventType, uniqueGameObjectIdentifier );
		}
	}
}
