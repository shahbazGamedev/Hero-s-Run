using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DarkQueenSequence : MonoBehaviour {

	Transform player;
	PlayerController playerController;
	Transform fairy;
	FairyController fairyController;
	DarkQueenController darkQueenController;
	public AudioClip fairyVO;
	public AudioClip darkQueenVO;

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
		//gem.setOpeningSequence( this );
	}

	public void startSequence()
	{
		if( gem.isDarkQueenSequenceActive ) return;

		//Slowdown player and remove player control
		print ("Start of dark queen sequence");
		gem.isDarkQueenSequenceActive = true;
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(22f, afterPlayerSlowdown ) );
		//darkQueenController.walk( true );
		darkQueenController.floatDown( 28.21f, landed );
	}

	public void landed()
	{
		print ("THE QUEEN HAS LANDED");
		darkQueenController.floatDownFx.Stop ();

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
		AchievementDisplay.activateDisplayFairy( "Oh no! It's the dark Queen ...", 0.35f, 3.6f );
		fairy.audio.PlayOneShot( fairyVO );
		Invoke ("step2", 5f );
	}

	void step2()
	{
		AchievementDisplay.activateDisplayDarkQueen( "Don't you know that you should never keep your Queen waiting.", 0.35f, 3.6f );
		darkQueenObject.audio.PlayOneShot( darkQueenVO );
		darkQueenController.walk( false );
		Invoke ("step3", 4.2f );
	}

	void step3()
	{
		darkQueenController.CastSpell();
		Invoke ("step4", 4.2f );
	}


	//Make the fairy disappear
	//Player starts running again
	void step4()
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
