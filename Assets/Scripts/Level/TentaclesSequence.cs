using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TentaclesSequence : MonoBehaviour {

	PlayerController playerController;
	FairyController fairyController;



	public static bool hasBeenTriggered = false;
	
	float lastActivateTime = 0;

	public GameObject tentaclePrefab;
	public GameObject debrisPrefab;

	float tentacleHalfHeight = 2f;
	Transform player;

	int rowIndex = 0;
	const int NUMBER_OF_ROWS = 46;

	// Use this for initialization
	void Awake () {

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		player = playerObject.transform;
		playerController = playerObject.GetComponent<PlayerController>();

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();



	}
	
	void Start()
	{
	}

	public void startSequence()
	{
		//Slowdown player and remove player control
		print ("Start of tentacles sequence");
		InvokeRepeating( "pierceUp", 0.2f, 2f );
	}

	void pierceUp()
	{
		print ("Shooting up tentacle");
		GameObject go = (GameObject)Instantiate(tentaclePrefab, Vector3.zero, Quaternion.identity );
		float attackDistance = 0.81f * PlayerController.getPlayerSpeed();
		//Pick random X location
		float xPos = Random.Range(-1.3f, 1.3f);
		Vector3 exactPos = player.TransformPoint(new Vector3( xPos,-0.9f-tentacleHalfHeight,attackDistance));
		go.transform.position = exactPos;
		go.transform.rotation = player.rotation;
		go.name = "Fence";
		LeanTween.moveLocalY(go, go.transform.position.y + tentacleHalfHeight, 1.15f ).setEase(LeanTweenType.easeOutExpo);
		go.audio.Play ( (ulong)1.15 );
		//Golden Shower
		GameObject flyingDebris = (GameObject)Instantiate(debrisPrefab, Vector3.zero, Quaternion.identity );
		flyingDebris.transform.position = new Vector3( exactPos.x, exactPos.y + 4f, exactPos.z );
		BreakableObject bo = flyingDebris.GetComponent<BreakableObject>();
		bo.triggerBreak( player.collider );
	}

	//Fairy tells something to player
	void step1()
	{
		AchievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText("FAIRY_DRAGON_BRIDGE"), 0.35f, 3.6f );
		Invoke ("step2", 3.75f );
	}

	//Fairy cast spell;
	void step2()
	{
		fairyController.CastSpell();
		Invoke ("step3", 4.2f );
	}

	//Spell works and bridge is rebuilt
	//Dragon takes-off
	void step3()
	{
		Invoke ("step4", 2.5f );
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
			print ("Player died cancelling invoke repeating");
			CancelInvoke("pierceUp");
		}
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Tentacles && !hasBeenTriggered )
		{
			hasBeenTriggered = true;

			startSequence();
		}
	}
	
}
