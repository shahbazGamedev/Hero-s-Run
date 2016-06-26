using UnityEngine;
using System.Collections;

public class TasteOfHellSequence : MonoBehaviour {

	PlayerController playerController;
	FairyController fairyController;
	[Tooltip("Voice over played by fairy upon seeing the mirror with the player. This is an ancient elvish mirror.")]
	public AudioClip VO_FA_ELVISH_MIRROR;
	[Tooltip("Voice over played by fairy a few seconds later. Touch the magical mirror to activate it.")]
	public AudioClip VO_FA_TOUCH_MIRROR;
	[Tooltip("Voice over played by fairy a few seconds later. By the White Tree! This is horrible ...")]
	public AudioClip VO_FA_AFTER_MIRROR;

	bool hasBeenTriggered = false;

	// Use this for initialization
	void Awake () {

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		playerController = playerObject.GetComponent<PlayerController>();

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();
	
	}
	
	void startSequence()
	{
		//Slowdown player and remove player control
		print ("Start of Hell Cave sequence");
		playerController.placePlayerInCenterLane();
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(4f, afterPlayerSlowdown ) );
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
		fairyController.speak("VO_FA_ELVISH_MIRROR", 4.5f, VO_FA_ELVISH_MIRROR );
		Invoke ("step2", 6.5f );
	}

	void step2()
	{
		fairyController.speak("VO_FA_TOUCH_MIRROR", 4.5f, VO_FA_TOUCH_MIRROR );
	}

	//Called by MagicalMirror when the vision has been viewed
	public void visionEnded()
	{
		fairyController.speak("VO_FA_AFTER_MIRROR", 4.5f, VO_FA_AFTER_MIRROR );
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
		if( eventType == GameEvent.Taste_Hell_Sequence && !hasBeenTriggered )
		{
			hasBeenTriggered = true;

			startSequence();
		}
	}
}
