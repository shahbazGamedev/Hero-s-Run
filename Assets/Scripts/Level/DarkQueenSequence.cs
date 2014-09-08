using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DarkQueenSequence : MonoBehaviour {

	Transform player;
	PlayerController playerController;
	Transform fairy;
	FairyController fairyController;
	DarkQueenController darkQueenController;

	bool hasBeenTriggered = false;
	

	// Use this for initialization
	void Awake () {

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		player = playerObject.transform;
		playerController = playerObject.GetComponent<PlayerController>();

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairy = fairyObject.transform;
		fairyController = fairyObject.GetComponent<FairyController>();

		GameObject darkQueenObject = GameObject.FindGameObjectWithTag("DarkQueen");
		darkQueenController = darkQueenObject.GetComponent<DarkQueenController>();

	}
	
	void Start()
	{
	}

	public void startSequence()
	{
		//Slowdown player and remove player control
		print ("Start of dark queen sequence");
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(24f, afterPlayerSlowdown ) );
		darkQueenController.walk( true );
		Invoke ("stopWalking", 4f );
	}

	void afterPlayerSlowdown()
	{
		playerController.anim.SetTrigger("Idle_Look");
		//Call fairy
		fairyController.setYRotationOffset( -10f );
		fairyController.Appear ( FairyEmotion.Worried );
	}

	//Fairy tells something to player
	void step1()
	{
		//AchievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText("FAIRY_DRAGON_BRIDGE"), 0.35f, 3.6f );
		//Invoke ("step2", 3.75f );
	}

	void stopWalking()
	{
		darkQueenController.walk( false );
		//fairyController.CastSpell();
		//Invoke ("step3", 4.2f );
	}

	//Spell works and bridge is rebuilt
	//Dragon takes-off
	void step3()
	{
		Invoke ("step4", 2.5f );
	}
	
	//Make the fairy disappear
	//Player starts running again
	void step4()
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

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Kill_Fairy && !hasBeenTriggered )
		{
			hasBeenTriggered = true;

			startSequence();
		}
	}

}
