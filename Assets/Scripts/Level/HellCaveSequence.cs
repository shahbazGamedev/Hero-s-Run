using UnityEngine;
using System.Collections;

public class HellCaveSequence : MonoBehaviour {

	PlayerController playerController;
	FairyController fairyController;
	[Tooltip("In order to add echo to the voices, instead of getting the fairy to play them, we use a game object with an audio source and an echo filter to play them instead.")]
	public GameObject fairy_vo_with_echo;

	bool hasBeenTriggered = false;

	// Use this for initialization
	void Awake () {

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		playerController = playerObject.GetComponent<PlayerController>();

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();
	
	}
	
	void startSequence( Transform trigger )
	{
		//Slowdown player and remove player control
		print ("Start of Hell Cave sequence");
		playerController.placePlayerInCenterLane();
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(5.9f, afterPlayerSlowdown, trigger ) );
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
		DialogManager.dialogManager.activateDisplayFairy( LocalizationManager.Instance.getText("VO_FA_ECHO"), 2f );
		fairy_vo_with_echo.GetComponent<AudioSource>().PlayOneShot( DialogManager.dialogManager.getVoiceOver("VO_FA_OH_NO") );
		Invoke ("step2", 4f );
	}

	void step2()
	{
		DialogManager.dialogManager.activateDisplayFairy( LocalizationManager.Instance.getText("VO_FA_ANYONE_HERE"), 3f );
		fairy_vo_with_echo.GetComponent<AudioSource>().PlayOneShot( DialogManager.dialogManager.getVoiceOver("VO_FA_NOBODY_LIKES_YOU") );
		Invoke ("step3", 4.4f );
	}

	void step3()
	{
		DialogManager.dialogManager.activateDisplayFairy( LocalizationManager.Instance.getText("VO_FA_IT_IS_DARK"), 2.9f );
		//fairy_vo_with_echo.GetComponent<AudioSource>().PlayOneShot( DialogManager.dialogManager.getVoiceOver("VO_FA_IT_IS_DARK") );
		Invoke ("step4", 1.8f );
	}

	void step4()
	{
		fairyController.setYRotationOffset( -50f );
		fairyController.CastSpell( materializeTorch );
	}

	void materializeTorch()
	{
		playerController.GetComponent<Torch>().enableTorch( true );
		Invoke ("step5", 2.2f );
	}

	//Make the fairy disappear
	//Player starts running again
	void step5()
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
	
	void PlayerStateChange( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying )
		{
			CancelInvoke();
		}
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject trigger )
	{
		if( eventType == GameEvent.Hell_Cave_Sequence && !hasBeenTriggered )
		{
			hasBeenTriggered = true;
			startSequence( trigger.transform );
		}
	}
}
