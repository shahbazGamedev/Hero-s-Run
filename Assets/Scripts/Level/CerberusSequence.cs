﻿using UnityEngine;
using System.Collections;

public class CerberusSequence : MonoBehaviour {

	PlayerController playerController;
	FairyController fairyController;
	CerberusController cerberusController;

	bool hasBeenTriggered = false;

	// Use this for initialization
	void Awake () {

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		playerController = playerObject.GetComponent<PlayerController>();

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();
	
		GameObject cerberusObject = GameObject.FindGameObjectWithTag("Cerberus");
		cerberusController = cerberusObject.GetComponent<CerberusController>();
	}
	
		void startSequence( Transform trigger )
	{
		//Slowdown player and remove player control
		print ("Start of Cerberus sequence");
		playerController.placePlayerInCenterLane();
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(4f, afterPlayerSlowdown, trigger ) );
		cerberusController.walk();
		Invoke ("stopWalking", 3.5f );
	}

	void afterPlayerSlowdown()
	{
		playerController.anim.SetTrigger("Idle_Look");
		//Call fairy
		fairyController.setYRotationOffset( -10f );
		fairyController.Appear ( FairyEmotion.Worried );
		Invoke ("step1", 1f );
	}

	void stopWalking()
	{
		cerberusController.idle();
	}

	//Fairy tells something to player
	void step1()
	{
		fairyController.speak("FAIRY_CERBERUS_UH_OH", 1.4f, false );
		Invoke ("step2", 3f );
		Invoke ("step3", 7f );
	}

	void step2()
	{
		fairyController.speak("FAIRY_CERBERUS_GOOD_BOY", 3f, false );
	}

	//Make the fairy disappear
	//Player starts running again
	void step3()
	{
		fairyController.Disappear ();
		playerController.allowRunSpeedToIncrease = true;
		playerController.startRunning(false);
		fairyController.resetYRotationOffset();
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
	
	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
		{
			CancelInvoke();
		}
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject trigger )
	{
		if( eventType == GameEvent.Start_Cerberus_encounter && !hasBeenTriggered )
		{
			hasBeenTriggered = true;
			startSequence( trigger.transform );
		}
	}
}
