using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SummonSkeletonsSequence : MonoBehaviour {

	public ParticleSystem  poisonMist;

	PlayerController playerController;
	FairyController fairyController;
	Transform darkQueen;
	DarkQueenController darkQueenController;
	public ParticleSystem lightningStrike;
	public List<SkeletonController> summonedSkeletons = new List<SkeletonController>();

	bool hasBeenTriggered = false;

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
	
	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
		{
			CancelInvoke();
		}
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Summon_Skeletons_Sequence && !hasBeenTriggered )
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
		StartCoroutine( playerController.slowDownPlayer(19f, afterPlayerSlowdown ) );
		arriveAndCastSpell();
	}
	
	void afterPlayerSlowdown()
	{
		playerController.anim.SetTrigger("Idle_Look");
		//Call fairy
		fairyController.setYRotationOffset( -10f );
		fairyController.Appear ( FairyEmotion.Worried );
		fairyController.speak("VO_FA_NOT_HER_AGAIN", 2f, true );
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
		darkQueenController.speak("VO_DQ_STARTING_TO_ANNOY", 3f, true );
		darkQueenController.floatDownFx.Stop ();
		darkQueen.GetComponent<Animation>().Play("DarkQueen_Idle");
		Invoke("castSpell", darkQueen.GetComponent<Animation>()["DarkQueen_Idle"].length + 2.25f);
	}
	
	void castSpell()
	{
		darkQueenController.speak("VO_DQ_BRING_BACK_BOOK", 3.8f, true );
		darkQueen.GetComponent<Animation>().CrossFade("DarkQueen_SpellCast");
		Invoke("playSpellFX", 0.3f);
		Invoke("darkQueenLeaves", darkQueen.GetComponent<Animation>()["DarkQueen_SpellCast"].length );
	}
	
	void playSpellFX()
	{
		darkQueen.GetComponent<AudioSource>().PlayOneShot( darkQueenController.spellSound );
		darkQueenController.spellFx.Play();
		poisonMist.Play();
		Invoke( "summonSkeletons", 1f );
	}
	
	void summonSkeletons()
	{
		if( lightningStrike != null )
		{
			lightningStrike.Play();
			lightningStrike.GetComponent<AudioSource>().Play();
			lightningStrike.GetComponent<Light>().enabled = true;
			Invoke("closeLight", 1f);
			for( int i = 0; i < summonedSkeletons.Count; i++ )
			{
				StartCoroutine( summonedSkeletons[i].wakeUp() );
			}
		}
	}

	void closeLight()
	{
		lightningStrike.GetComponent<Light>().enabled = false;
	}

	void darkQueenLeaves()
	{
		darkQueenController.floatDownFx.Play ();
		darkQueen.GetComponent<Animation>()["DarkQueen_Leave"].speed = 1.2f;
		darkQueen.GetComponent<Animation>().Play("DarkQueen_Leave");
		darkQueenController.brightenLights( darkQueen.GetComponent<Animation>()["DarkQueen_Leave"].length/1.2f );
		Invoke("playerStartsRunningAgain", darkQueen.GetComponent<Animation>()["DarkQueen_Leave"].length/1.2f );
	}
	
	void playerStartsRunningAgain()
	{
		darkQueenController.Disappear();
		fairyController.Disappear ();
		playerController.allowRunSpeedToIncrease = true;
		//Give player control
		playerController.allowPlayerMovement(true );
		playerController.startRunning(false);
		fairyController.resetYRotationOffset();
	}

}
