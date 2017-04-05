using UnityEngine;
using System.Collections;

public class FairyMessage : MonoBehaviour {

	PlayerController playerController;
	FairyController fairyController;

	public string fairyMessageTextId;
	
	public FairyEmotion fairyEmotion = FairyEmotion.Happy;

	// Use this for initialization
	void Start () {

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();
	}

	public void displayFairyMessage()
	{
		if( playerController == null )
		{
			GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
			playerController = playerObject.GetComponent<PlayerController>();
		}

		//Call fairy
		fairyController.Appear ( fairyEmotion );

		playerController.allowRunSpeedToIncrease = false;
		Invoke ("step1", 1f );
	}

	//Fairy tells something to player
	void step1()
	{
		fairyController.speak(fairyMessageTextId, 3.75f, false );
		//Player looks at fairy
		playerController.lookOverShoulder( 0.4f, 2.75f );
		Invoke ("step2", 3.5f );
	}
	
	//Make the fairy disappear
	void step2()
	{
		fairyController.Disappear ();
		playerController.allowRunSpeedToIncrease = true;
	}

	void OnEnable()
	{
		PlayerController.playerStateChanged += PlayerStateChange;
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
	}
	
	void OnDisable()
	{
		PlayerController.playerStateChanged -= PlayerStateChange;
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
	}
	
	void PlayerStateChange( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying )
		{
			CancelInvoke();
		}
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Fairy_Message  )
		{
			displayFairyMessage();
		}
	}


}
