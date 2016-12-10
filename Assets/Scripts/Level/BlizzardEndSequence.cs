using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlizzardEndSequence : MonoBehaviour {

	PlayerController playerController;
	FairyController fairyController;
	public float walkToLedgeDistance = 12f;
	public float walkToCullisGateDistance = 12f;
	public Vector3 fairyPositionBehindPlayer = new Vector3( 0.5f, 1.1f, -0.12f );
	public List<AudioClip> thunderSounds = new List<AudioClip>();
	public float maxThunderInterval = 20.0f;
	public float minThunderInterval = 7f;
	public AudioSource thunderAudioSource;
	public ParticleSystem thunderStrike;

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
	
	void PlayerStateChange( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying )
		{
			CancelInvoke();
		}
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject trigger )
	{
		if( eventType == GameEvent.Blizzard_End_Sequence && !hasBeenTriggered )
		{
			hasBeenTriggered = true;
			startSequence( trigger.transform );
		}
	}

	void startSequence( Transform trigger )
	{
		//Slowdown player and remove player control
		playerController.placePlayerInCenterLane();
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(walkToLedgeDistance, afterPlayerSlowdown, trigger ) );
		thunder();
	}
	
	void afterPlayerSlowdown()
	{
		playerController.anim.SetTrigger("Idle_Look");
		//Call fairy
		fairyController.setYRotationOffset( -6f );
		fairyController.Appear ( FairyEmotion.Worried );
		moveFairyBehindPlayer();
	}

	void moveFairyBehindPlayer()
	{
		StartCoroutine( fairyController.goHere( 1.6f, fairyPositionBehindPlayer, fairyTalks ) );
	}

	void fairyTalks()
	{
		fairyController.speak("VO_FA_BLIZZARD_END", 3.5f, false );
		Invoke("walkToCullisGate", 6f );
	}

	void walkToCullisGate()
	{
		StartCoroutine( playerController.walkForDistance( walkToCullisGateDistance, 3.5f, playerController.afterPlayerSlowdown, false ) );
	}

	void thunder()
	{
		thunderStrike.Play();
		Invoke ("thunderSound", 0.15f);
		Invoke ("thunder", Random.Range(minThunderInterval, maxThunderInterval));
	}

	void thunderSound()
	{
		if( thunderSounds.Count > 0 )
		{
			int index = Random.Range( 0, thunderSounds.Count );
			thunderAudioSource.PlayOneShot( thunderSounds[index] );
		}
	}

}
