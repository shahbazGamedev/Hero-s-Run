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
	Lower_Gate = 12,
	Open_Door = 13,
	Raise_Draw_Bridge = 14,
	Activate_Cullis_Gate = 15,
	Fairy_Message = 18,
	Broken_Bridge = 19,
	Kill_Fairy = 20,
	Start_Kraken = 21,
	Stop_Kraken = 22,
	Start_Cemetery_Sequence = 23,
	Stop_Cemetery_Sequence = 24,
	Start_Ghost = 25,
	Stop_Ghost = 26,
	Start_Cerberus_encounter = 27,
	Dim_light = 28,
	Brighten_light = 29,
	Hell_Cave_Sequence = 30,
	Taste_Hell_Sequence = 31,
	Summon_Skeletons_Sequence = 32,
	Dark_Mines_Sequence = 33,
	ExtinguishTorch = 34,
	Giant_Crystal_Sequence = 35,
	Blizzard_End_Sequence = 36,
	Blizzard_Take_Refuge = 37,
	Blizzard_Dark_Queen_Calls_Storm = 38

}

public class PlayerTrigger : MonoBehaviour {

	//Event management used to notify other classes when the player has crossed the trigger zone.
	//This event could trigger an in-game event such as a zombie wave, or get a cow to start walking.
	public delegate void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier );
	public static event PlayerEnteredTrigger playerEnteredTrigger;
	public GameEvent eventType;
	//The optional uniqueGameObjectIdentifier is sent along with the event. The receiver can use it to verify that the event is really destined for him.
	//However, if you are using a player trigger to start a sequence, you should pass itself as the GameObject. The slow down method in PlayerController needs a
	//reference to the trigger to calculate the distance properly.
	public GameObject uniqueGameObjectIdentifier;

	public float percentageChanceEventTriggered = 1f;

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" && Random.value <= percentageChanceEventTriggered )
		{
			//Send an event to interested classes
			if(playerEnteredTrigger != null) playerEnteredTrigger(eventType, uniqueGameObjectIdentifier );
		}
	}
}
