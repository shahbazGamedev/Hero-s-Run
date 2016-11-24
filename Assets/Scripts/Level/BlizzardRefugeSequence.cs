using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlizzardRefugeSequence : MonoBehaviour {

	PlayerController playerController;
	FairyController fairyController;
	WorldSoundManager worldSoundManager;
	public float walkDistance = 12f;
	public Vector3 fairyPositionBehindPlayer = new Vector3( 0.5f, 1.1f, -0.12f );
	public Camera cutsceneCamera;
	public GameObject fakeFloor;
	public GameObject snowStorm;

	bool hasBeenTriggered = false;

	// Use this for initialization
	void Awake () {

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		playerController = playerObject.GetComponent<PlayerController>();

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();
	
		GameObject worldSoundManagerObject = GameObject.FindGameObjectWithTag("World Sound Manager");
		worldSoundManager = worldSoundManagerObject.GetComponent<WorldSoundManager>();
	}

	void OnEnable()
	{
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
	}
	
	void OnDisable()
	{
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Blizzard_Take_Refuge && !hasBeenTriggered )
		{
			hasBeenTriggered = true;

			startSequence();
		}
	}

	void startSequence()
	{
		//Slowdown player and remove player control
		playerController.placePlayerInCenterLane();
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(walkDistance, afterPlayerSlowdown ) );
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
		fairyController.speak("VO_FA_BLIZZARD_REFUGE", 3.5f, false );
		Invoke("startFadeIn", 3f);
	}

	void startFadeIn()
	{
		playerController.GetComponent<HUDHandler>().fadeEffect( true, afterFadeIn );
	}

	void afterFadeIn()
	{
		//Use cut-scene camera
		//A fake hero and fairy are in the tower
		Invoke("insideTower", 4f);
	}

	void insideTower()
	{
		cutsceneCamera.gameObject.SetActive( true );
		fakeFloor.SetActive( false );
		playerController.GetComponent<HUDHandler>().fadeEffect( false, insideTowerFairySpeaks );
	}

	void insideTowerFairySpeaks()
	{
		//Play sweet animation - NEEDED
		fairyController.speak("VO_FA_BLIZZARD_SO_COLD", 3.5f, false );
		Invoke("insideTowerWindQuietsDown", 12f);
		worldSoundManager.crossFadeToMainAmbienceQuiet( 10f );
	}

	void insideTowerWindQuietsDown()
	{
		fairyController.speak("VO_FA_BLIZZARD_WIND_QUIET", 3.5f, false );
		playerController.GetComponent<HUDHandler>().fadeEffect( true, fadeOutOutside );
	}
	
	void fadeOutOutside()
	{
		cutsceneCamera.gameObject.SetActive( false );
		Camera.main.GetComponent<StormManager>().deactivateStorm();
		snowStorm.SetActive( false );
		playerController.GetComponent<HUDHandler>().fadeEffect( false, preparePlayerForRunning );

	}

	void preparePlayerForRunning()
	{
		//Give player a few seconds to adjust before starting running again
		Invoke("playerStartsRunningAgain", 3f );
	}

	void playerStartsRunningAgain()
	{
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
