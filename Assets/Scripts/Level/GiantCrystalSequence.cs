using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GiantCrystalSequence : MonoBehaviour {

	PlayerController playerController;
	FairyController fairyController;
	public float walkDistance = 3f;
	public Vector3 fairyPositionBehindPlayer = new Vector3( 0, 1f, -0.3f );
	public Transform sneakyWraith;
	public Vector3 wraithPositionBehindPlayer = new Vector3( 0, 1f, -0.3f );

	bool hasBeenTriggered = false;

	// Use this for initialization
	void Awake () {

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
	
	void PlayerStateChange( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying )
		{
			CancelInvoke();
		}
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject trigger )
	{
		if( eventType == GameEvent.Giant_Crystal_Sequence && !hasBeenTriggered )
		{
			hasBeenTriggered = true;
			startSequence( trigger.transform );
		}
	}

	void startSequence( Transform trigger )
	{
		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		playerController = playerObject.GetComponent<PlayerController>();
		//Slowdown player and remove player control
		playerController.placePlayerInCenterLane();
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(18f, afterPlayerSlowdown, trigger ) );
	}
	
	void afterPlayerSlowdown()
	{
		playerController.anim.SetTrigger("Idle_Look");
		//Call fairy
		fairyController.setYRotationOffset( -10f );
		fairyController.Appear ( FairyEmotion.Worried );
		fairyController.speak("VO_FA_MINES_BIG_CRYSTAL", 3.25f, false );
		Invoke("moveWraithBehindPlayer", 4.5f );
	}

	void moveWraithBehindPlayer()
	{
		sneakyWraith.gameObject.SetActive( true );
		LeanTween.moveLocal( sneakyWraith.gameObject, wraithPositionBehindPlayer, 3f ).setOnComplete(wraithLookAtPlayer).setOnCompleteParam(gameObject);
	}

	void wraithLookAtPlayer()
	{
		fairyController.speak("VO_FA_MINES_BIG_CRYSTAL_COLD", 3.25f, false );
		LeanTween.rotateLocal( sneakyWraith.gameObject, new Vector3( sneakyWraith.localEulerAngles.x, -49f, sneakyWraith.localEulerAngles.z ) , 3f );
		Invoke( "wraithStartsAttackingPlayer", 5f );
	}

	void wraithStartsAttackingPlayer()
	{
		sneakyWraith.GetComponent<Animator>().speed = 0.21f;
		sneakyWraith.GetComponent<Animator>().CrossFadeInFixedTime( "Sneak Attack", 0.2f );
		Invoke( "fairySpinsAroundAndSeesWraith", 0.3f );
	}

	void fairySpinsAroundAndSeesWraith()
	{
		fairyController.setFairyState( FairyController.FairyState.None );
		LeanTween.rotateLocal( fairyController.gameObject, new Vector3( fairyController.transform.localEulerAngles.x, 188.2f, fairyController.transform.localEulerAngles.z ) , 0.6f ).setEase(LeanTweenType.easeOutQuint).setOnComplete(fairyYellsRun).setOnCompleteParam(gameObject);
	}

	void fairyYellsRun()
	{
		fairyController.speak("VO_FA_MINES_BIG_CRYSTAL_RUN", 1.8f, false );
		Invoke( "playerStartsRunningAgain", 0.8f );
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
