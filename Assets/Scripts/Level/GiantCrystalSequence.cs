using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GiantCrystalSequence : MonoBehaviour {

	PlayerController playerController;
	FairyController fairyController;
	public float walkDistance = 3f;
	public Vector3 fairyPositionBehindPlayer = new Vector3( 0, 1f, -0.3f );

	bool hasBeenTriggered = false;

	// Use this for initialization
	void Awake () {

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		playerController = playerObject.GetComponent<PlayerController>();

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();
	
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
		if( eventType == GameEvent.Giant_Crystal_Sequence && !hasBeenTriggered )
		{
			hasBeenTriggered = true;

			startSequence();
		}
	}

	//Dark Queen sequence that summon skeletons in the battlefield
	void startSequence()
	{
		print ("Start of Dark Queen summon skeletons sequence.");

		//Slowdown player and remove player control
		playerController.placePlayerInCenterLane();
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(18f, afterPlayerSlowdown ) );
	}
	
	void afterPlayerSlowdown()
	{
		playerController.anim.SetTrigger("Idle_Look");
		//Call fairy
		fairyController.setYRotationOffset( -10f );
		fairyController.Appear ( FairyEmotion.Worried );
		fairyController.speak("VO_FA_CANT_BE_GOOD", 2f, false );
		Invoke("playerStartsRunningAgain", 3f );
	}

	void lookOverEdge()
	{
		StartCoroutine( playerController.walkForDistance( walkDistance, 3.5f, edgeReached ) );
	}

	void edgeReached()
	{
		playerController.sc.playCutscene(CutsceneType.SummonSkeletons );
		Invoke("moveFairyBehindPlayer", 0.5f );
	}

	void moveFairyBehindPlayer()
	{
		StartCoroutine( fairyController.goHere( 1.6f, fairyPositionBehindPlayer, pushPlayer ) );
	}

	void pushPlayer()
	{
		Invoke("playerStartsRunningAgain", 8f );
		Invoke("fairyTalks", 1.2f );
	}

	void fairyTalks()
	{
		fairyController.speak("VO_FA_PORTAL", 2.5f, false );
	}

	void playerStartsRunningAgain()
	{
		//Give player control
		//playerController.sc.reactivateMaincamera();
		playerController.allowPlayerMovement(true );
		playerController.startRunning(false);
		playerController.allowRunSpeedToIncrease = true;
		playerController.enablePlayerControl( true );
		Invoke("fairyDisappears", 1.2f );
	}

	void fairyDisappears()
	{
		fairyController.Disappear ();
		fairyController.resetYRotationOffset();
	}

}
