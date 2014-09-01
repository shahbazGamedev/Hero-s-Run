using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BrokenBridgeSequence : MonoBehaviour {

	Transform player;
	PlayerController playerController;
	Transform fairy;
	FairyController fairyController;

	bool hasBeenTriggered = false;

	public ParticleSystem fairySpell;

	public List<GameObject> bridgeParts = new List<GameObject>();
	float lastActivateTime = 0;
	int bridgePartIndex = 0;

	
	// Use this for initialization
	void Awake () {

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		player = playerObject.transform;
		playerController = playerObject.GetComponent<PlayerController>();

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairy = fairyObject.transform;
		fairyController = fairyObject.GetComponent<FairyController>();

	}
	

	public void startSequence()
	{
		//Slowdown player and remove player control
		print ("Start of broken bridge sequence");
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(20.25f, afterPlayerSlowdown ) );
	}

	void afterPlayerSlowdown()
	{
		playerController.anim.SetTrigger("Idle_Look");
		//Call fairy
		fairyController.setYRotationOffset( -10f );
		fairyController.Appear ();
		Invoke ("step1", 1f );
	}

	//Fairy tells something to player
	void step1()
	{
		AchievementDisplay.activateDisplayFairy( "By the White Tree! the bridge, its broken...", 0.35f, 2.5f );
		//Player looks at fairy
		playerController.lookOverShoulder( 0.4f, 2.75f );
		Invoke ("step2", 3.5f );
	}

	void step2()
	{
		//Fairy cast spell;
		fairySpell.Play();
		audio.Play();
		Invoke ("step3", 1.5f );
	}

	void step3()
	{
		//Rebuild bridge magically
		rebuildBridge();
		AchievementDisplay.activateDisplayFairy( "Quickly now! I can hear the troll.", 0.35f, 2.75f );
		Invoke ("step4", 4f );
	}

	void rebuildBridge()
	{
		foreach( GameObject go in bridgeParts )
		{
			float delay = Random.Range( 0.05f, 0.15f );
			Invoke ("makeBridgePartActive", lastActivateTime + delay );
			lastActivateTime = lastActivateTime + delay;

		}
	}

	void makeBridgePartActive()
	{
		bridgeParts[bridgePartIndex].SetActive( true );
		bridgePartIndex++;
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
		if( eventType == GameEvent.Falling_Tree && !hasBeenTriggered )
		{
			hasBeenTriggered = true;

			startSequence();
		}
	}

}
