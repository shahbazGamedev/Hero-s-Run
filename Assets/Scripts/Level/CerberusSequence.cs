using UnityEngine;
using System.Collections;

public class CerberusSequence : MonoBehaviour {

	PlayerController playerController;
	FairyController fairyController;
	CerberusController cerberusController;

	public bool hasBeenTriggered = false;

	// Use this for initialization
	void Awake () {

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		playerController = playerObject.GetComponent<PlayerController>();

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();
	
		GameObject cerberusObject = GameObject.FindGameObjectWithTag("Cerberus");
		cerberusController = cerberusObject.GetComponent<CerberusController>();
	}
	
	void startSequence()
	{
		//Slowdown player and remove player control
		print ("Start of Cerberus sequence");
		playerController.placePlayerInCenterLane();
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(18.1f, afterPlayerSlowdown ) );
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
		AchievementDisplay.achievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText("FAIRY_DRAGON_BRIDGE"), 3.6f );
		Invoke ("step2", 3.75f );
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
		if( eventType == GameEvent.Start_Cerberus_encounter && !hasBeenTriggered )
		{
			hasBeenTriggered = true;

			startSequence();
		}
	}
}
