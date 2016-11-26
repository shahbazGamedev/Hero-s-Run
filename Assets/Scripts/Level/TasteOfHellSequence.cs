using UnityEngine;
using System.Collections;

public class TasteOfHellSequence : MonoBehaviour {

	PlayerController playerController;
	FairyController fairyController;
	public MagicalMirror magicalMirror;
	[Tooltip("The game object holding the remote scene that is seen in the mirror. We hold a reference so we can destroy it once the sequence has finished.")]
	public GameObject remoteScene;

	bool hasBeenTriggered = false;

	// Use this for initialization
	void Awake () {

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		playerController = playerObject.GetComponent<PlayerController>();

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();
	
	}
	
	void startSequence( Transform trigger )
	{
		//Slowdown player and remove player control
		print ("Start of Hell Cave sequence");
		playerController.placePlayerInCenterLane();
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(4f, afterPlayerSlowdown, trigger ) );
	}

	void afterPlayerSlowdown()
	{
		playerController.anim.SetTrigger("Idle_Look");
		//Call fairy
		fairyController.setYRotationOffset( -10f );
		fairyController.Appear ( FairyEmotion.Worried );
		Invoke ("step1", 1f );
	}

	//Fairy tells something to player
	void step1()
	{
		fairyController.speak("VO_FA_ELVISH_MIRROR", 4.5f, false );
		Invoke ("step2", 6.5f );
	}

	void step2()
	{
		fairyController.speak("VO_FA_TOUCH_MIRROR", 4.5f, false );
		magicalMirror.allowActivation();
	}

	//Called by MagicalMirror when the vision has been viewed
	public void visionEnded()
	{
		fairyController.speak("VO_FA_AFTER_MIRROR", 4.5f, false );
		Destroy( remoteScene );
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

	void PlayerEnteredTrigger( GameEvent eventType, GameObject trigger )
	{
		if( eventType == GameEvent.Taste_Hell_Sequence && !hasBeenTriggered )
		{
			hasBeenTriggered = true;
			startSequence( trigger.transform );
		}
	}
}
