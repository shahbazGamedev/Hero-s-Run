using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SummonSkeletonsSequence : MonoBehaviour {

	PlayerController playerController;
	FairyController fairyController;
	Transform darkQueen;
	DarkQueenController darkQueenController;
	public ParticleSystem lightningStrike;
	public float walkDistance = 7.55f;
	public Vector3 fairyPositionBehindPlayer = new Vector3( 0, 1f, -0.3f );

	bool hasBeenTriggered = false;
	//Event management used to notify SkeletonControllers when skeletons have been summoned.
	public delegate void SkeletonsSummoned( Transform summoner );
	public static event SkeletonsSummoned skeletonsSummoned;

	// Use this for initialization
	void Awake () {

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		playerController = playerObject.GetComponent<PlayerController>();

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();
	
		//Note that the Dark Queen is not in the Level scene. She is only in the tiles that use her.
		darkQueen = GameObject.FindGameObjectWithTag("DarkQueen").transform;
		darkQueenController = darkQueen.GetComponent<DarkQueenController>();
		darkQueen.gameObject.SetActive( false );
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
		if( eventType == GameEvent.Summon_Skeletons_Sequence && !hasBeenTriggered )
		{
			hasBeenTriggered = true;
			startSequence(trigger.transform);
		}
	}

	//Dark Queen sequence that summon skeletons
	void startSequence( Transform trigger )
	{
		print ("Start of Dark Queen summon skeletons sequence.");

		//Slowdown player and remove player control
		playerController.placePlayerInCenterLane();
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(18.6f, afterPlayerSlowdown, trigger ) );
		arriveAndCastSpell();
	}
	
	void afterPlayerSlowdown()
	{
		playerController.anim.SetTrigger("Idle_Look");
		//Call fairy
		fairyController.setYRotationOffset( -10f );
		fairyController.Appear ( FairyEmotion.Worried );
		fairyController.speak("VO_FA_CANT_BE_GOOD", 2f, false );
	}

	void arriveAndCastSpell()
	{
		darkQueen.gameObject.SetActive( true );
		darkQueenController.floatDownFx.Play ();
		float arriveSpeed = 0.3f;
		darkQueen.GetComponent<Animation>()["DarkQueen_Arrive"].speed = arriveSpeed;
		darkQueen.GetComponent<Animation>().Play("DarkQueen_Arrive");
		Invoke("playLandAnimation", darkQueen.GetComponent<Animation>()["DarkQueen_Arrive"].length/arriveSpeed );
		darkQueenController.dimLights( darkQueen.GetComponent<Animation>()["DarkQueen_Arrive"].length/arriveSpeed, 0.1f );
	}
	
	void playLandAnimation()
	{
		darkQueen.GetComponent<Animation>().CrossFade("DarkQueen_Land", 0.1f);
		Invoke("playIdleAnimation", darkQueen.GetComponent<Animation>()["DarkQueen_Land"].length);
	}
	
	void playIdleAnimation()
	{
		darkQueenController.speak("VO_DQ_WONT_ESCAPE_ME", 3f, false );
		darkQueenController.floatDownFx.Stop ();
		darkQueen.GetComponent<Animation>().Play("DarkQueen_Idle");
		Invoke("castSpell", darkQueen.GetComponent<Animation>()["DarkQueen_Idle"].length + 2.25f);
	}
	
	void castSpell()
	{
		darkQueenController.speak("VO_DQ_JUNGLE_DOOM", 3f, false );
		darkQueen.GetComponent<Animation>().CrossFade("DarkQueen_SpellCast");
		Invoke("playSpellFX", 0.3f);
		Invoke("darkQueenLeaves", darkQueen.GetComponent<Animation>()["DarkQueen_SpellCast"].length );
	}
	
	void playSpellFX()
	{
		darkQueen.GetComponent<AudioSource>().PlayOneShot( darkQueenController.spellSound );
		darkQueenController.spellFx.Play();
		Invoke( "summonSkeletons", 1f );
	}
	
	void summonSkeletons()
	{
		if( lightningStrike != null )
		{
			lightningStrike.Play();
			lightningStrike.GetComponent<AudioSource>().Play();
			Invoke("summonSkeletonsNow", 12.5f);
		}
	}

	void summonSkeletonsNow()
	{
		if(skeletonsSummoned != null) skeletonsSummoned( darkQueen );
	}

	void darkQueenLeaves()
	{
		darkQueenController.floatDownFx.Play ();
		darkQueen.GetComponent<Animation>()["DarkQueen_Leave"].speed = 1.2f;
		darkQueen.GetComponent<Animation>().Play("DarkQueen_Leave");
		darkQueenController.brightenLights( darkQueen.GetComponent<Animation>()["DarkQueen_Leave"].length/1.2f );
		Invoke("lookOverEdge", darkQueen.GetComponent<Animation>()["DarkQueen_Leave"].length/1.2f );
	}
	
	void lookOverEdge()
	{
		StartCoroutine( playerController.walkForDistance( walkDistance, 3.5f, edgeReached, true ) );
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
		darkQueenController.Disappear();
		//Give player control
		playerController.sc.reactivateMaincamera();
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
