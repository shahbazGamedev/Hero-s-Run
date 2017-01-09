using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlizzardStormBringerSequence : MonoBehaviour {

	public float walkDistance = 12f;
	public Vector3 fairyPositionBehindPlayer = new Vector3( 0.5f, 1.1f, -0.12f );
	public TakePictureTrigger takePictureTrigger;

	Transform darkQueen;
	DarkQueenController darkQueenController;
	public ParticleSystem lightningStrike;
	PlayerController playerController;
	FairyController fairyController;
	bool hasBeenTriggered = false;

	// Use this for initialization
	void Awake () {

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();
	
		//Note that the Dark Queen is not in the Level scene. She is only in the tiles that use her.
		darkQueen = GameObject.FindGameObjectWithTag("DarkQueen").transform;
		darkQueenController = darkQueen.GetComponent<DarkQueenController>();
		darkQueen.gameObject.SetActive( false );
	}

	void OnEnable()
	{
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
	}
	
	void OnDisable()
	{
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject trigger )
	{
		if( eventType == GameEvent.Blizzard_Dark_Queen_Calls_Storm && !hasBeenTriggered )
		{
			hasBeenTriggered = true;
			startSequence( trigger.transform );
		}
	}

	//Dark Queen sequence that summons storm
	void startSequence( Transform trigger )
	{
		print ("Start of Dark Queen storm bringer sequence.");
		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		playerController = playerObject.GetComponent<PlayerController>();

		//Slowdown player and remove player control
		playerController.placePlayerInCenterLane();
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(19f, afterPlayerSlowdown, trigger ) );
		arriveAndCastSpell();
	}
	
	void afterPlayerSlowdown()
	{
		playerController.anim.SetTrigger("Idle_Look");
		//Call fairy
		fairyController.setYRotationOffset( -10f );
		fairyController.Appear ( FairyEmotion.Worried );
		fairyController.speak("VO_FA_BLIZZARD_WHAT_DOING_HERE", 2.5f, false );
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
		darkQueenController.speak("VO_DQ_BLIZZARD_LUCKY", 5.5f, false );
		darkQueenController.floatDownFx.Stop ();
		darkQueen.GetComponent<Animation>().Play("DarkQueen_Idle");
		Invoke("castSpell", darkQueen.GetComponent<Animation>()["DarkQueen_Idle"].length + 6f);
	}
	
	void castSpell()
	{
		darkQueenController.speak("VO_DQ_BLIZZARD_STORM", 4.5f, false );
		darkQueen.GetComponent<Animation>().CrossFade("DarkQueen_SpellCast");
		Invoke("playSpellFX", 0.3f);
		Invoke("darkQueenLeaves", darkQueen.GetComponent<Animation>()["DarkQueen_SpellCast"].length );
	}
	
	void playSpellFX()
	{
		darkQueen.GetComponent<AudioSource>().PlayOneShot( darkQueenController.spellSound );
		darkQueenController.spellFx.Play();
		Invoke( "summonStorm", 1f );
	}
	
	void summonStorm()
	{
		if( lightningStrike != null )
		{
			lightningStrike.Play();
			lightningStrike.GetComponent<AudioSource>().Play();
			Invoke("summonStormNow", 12.5f);
			Invoke("takePicture", 1f);
		}
	}

	void takePicture()
	{
		takePictureTrigger.takePicture();
	}

	void summonStormNow()
	{
		print ("Dark Queen summons storm.");
		StormManager stormManager = Camera.main.GetComponent<StormManager>();
		stormManager.initiateStorm();
	}

	void darkQueenLeaves()
	{
		darkQueenController.floatDownFx.Play ();
		darkQueen.GetComponent<Animation>()["DarkQueen_Leave"].speed = 1.2f;
		darkQueen.GetComponent<Animation>().Play("DarkQueen_Leave");
		darkQueenController.brightenLights( darkQueen.GetComponent<Animation>()["DarkQueen_Leave"].length/1.2f );
		Invoke("pauseBeforeRunningAgain", darkQueen.GetComponent<Animation>()["DarkQueen_Leave"].length/1.2f );
	}
	
	void pauseBeforeRunningAgain()
	{
		Invoke("playerStartsRunningAgain", 2f );
	}

	void playerStartsRunningAgain()
	{
		darkQueenController.Disappear();
		//Give player control
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
