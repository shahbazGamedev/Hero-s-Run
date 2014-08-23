using UnityEngine;
using System.Collections;

public class FallingTreeSequence : MonoBehaviour {

	Transform player;
	PlayerController playerController;
	Transform fairy;
	FairyController fairyController;

	public string fairyMessageTextId;
	bool hasBeenTriggered = false;

	
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
		print ("start tree seq");
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(16f, afterPlayerSlowdown ) );
	}

	void afterPlayerSlowdown()
	{
		print ("after slow down");
		playerController.anim.SetTrigger("Idle_Look");
		//Call fairy
		fairyController.Appear ();
		Invoke ("step1", 1f );
	}

	//Fairy tells something to player
	void step1()
	{
		AchievementDisplay.activateDisplayFairy( "Oh my god! the bridge, its broken...", 0.35f, 2.75f );
		//Player looks at fairy
		playerController.lookOverShoulder( 0.4f, 2.75f );
		Invoke ("step2", 3.5f );
	}

	void step2()
	{
		makeTreeFall();
		Invoke ("step3", 3.5f );
	}

	//Make the fairy disappear
	void step3()
	{
		fairyController.Disappear ();
		playerController.allowRunSpeedToIncrease = true;
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

	void makeTreeFall()
	{
		print ("makeTreeFall");
		GameObject go = GameObject.FindGameObjectWithTag("FallingTree");
		//Make the tree fall
		Rigidbody body = go.rigidbody;
		body.useGravity = true;
		float pushPower = 800f;
		Vector3 force = go.transform.TransformPoint(new Vector3( 0f, 1f, 8f));
		force.Normalize();
		force = force * pushPower ;
		body.AddForceAtPosition(force, new Vector3 (go.transform.position.x,go.transform.position.y + 6f, go.transform.position.z )  );
	}
}
