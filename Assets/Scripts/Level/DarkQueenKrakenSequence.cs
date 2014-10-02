using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DarkQueenKrakenSequence : MonoBehaviour {

	Transform player;
	PlayerController playerController;
	Transform fairy;
	FairyController fairyController;
	DarkQueenController darkQueenController;

	public AudioClip VO_FA_Oh_no;
	public AudioClip VO_DQ_not_keep_waiting;
	public AudioClip VO_DQ_rise_from_the_deep;

	public ParticleSystem poisonMist;

	bool hasBeenTriggered = false;
	GameEventManager gem;
	GameObject darkQueenObject;

	// Use this for initialization
	void Awake () {

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		player = playerObject.transform;
		playerController = playerObject.GetComponent<PlayerController>();

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairy = fairyObject.transform;
		fairyController = fairyObject.GetComponent<FairyController>();

		darkQueenObject = GameObject.FindGameObjectWithTag("DarkQueen");
		darkQueenController = darkQueenObject.GetComponent<DarkQueenController>();

	}
	
	void Start()
	{
		
		GameObject gameEventManagerObject = GameObject.FindGameObjectWithTag("GameEventManager");
		gem = gameEventManagerObject.GetComponent<GameEventManager>();
	}

	public void startSequence()
	{
		if( gem.isDarkQueenSequenceActive ) return;
		GenerateLevel.enableSurroundingPlane( false );
		//Slowdown player and remove player control
		print ("Start of dark queen sequence");
		gem.isDarkQueenSequenceActive = true;
		playerController.placePlayerInCenterLane();
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(19f, afterPlayerSlowdown ) );
		darkQueenController.arriveAndCastSpell();
		AchievementDisplay.activateDisplayFairy( "Oh no! It's the dark Queen ...", 0.35f, 1.8f );
		fairy.audio.PlayOneShot( VO_FA_Oh_no );

	}
	
	void afterPlayerSlowdown()
	{
		playerController.anim.SetTrigger("Idle_Look");
		//Call fairy
		fairyController.setYRotationOffset( -10f );
		fairyController.Appear ( FairyEmotion.Worried );

	}

	//Make the fairy disappear
	//Player starts running again
	public void step4()
	{
		darkQueenController.Disappear();
		fairyController.Disappear ();
		playerController.allowRunSpeedToIncrease = true;
		playerController.startRunning(false);
		fairyController.resetYRotationOffset();
		Invoke ("step5", 2f );
	}

	void step5()
	{
		gem.playTentaclesSequence();
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
