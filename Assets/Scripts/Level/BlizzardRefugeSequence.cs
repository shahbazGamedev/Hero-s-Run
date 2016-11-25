using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlizzardRefugeSequence : MonoBehaviour {

	public float walkDistance = 12f;
	public Vector3 fairyPositionBehindPlayer = new Vector3( 0.5f, 1.1f, -0.12f );
	public Camera cutsceneCamera;
	public GameObject fakeFloor;
	public GameObject snowStorm;
	public GameObject deadTreeOnThePath;
	public ParticleSystem holyBlast;
	public GameObject flyingDebris;
	public float flyingDebrisForce = 2600f;
	public float flyingDebrisTorque = 400f;
	public Transform fairyInsideTower;
	public Transform fairyLocationByWindow;
	public ParticleSystem fogByWindow;

	PlayerController playerController;
	FairyController fairyController;
	WorldSoundManager worldSoundManager;
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
		throwDebris();
		moveFairyBehindPlayer();
	}

	void throwDebris()
	{
		flyingDebris.transform.LookAt( playerController.transform );
		flyingDebris.GetComponent<Rigidbody>().AddForce( flyingDebris.transform.forward * flyingDebrisForce + new Vector3( 0, 200f, 0 ) );
		flyingDebris.GetComponent<Rigidbody>().AddTorque( 0, flyingDebrisTorque,0 );
	}

	void moveFairyBehindPlayer()
	{
		StartCoroutine( fairyController.goHere( 1.6f, fairyPositionBehindPlayer, fairyTalks ) );
	}

	void fairyTalks()
	{
		fairyController.speak("VO_FA_BLIZZARD_REFUGE", 3.5f, false );
		Invoke("startFadeIn", 5f);
	}

	void startFadeIn()
	{
		playerController.GetComponent<HUDHandler>().fadeEffect( true, afterFadeIn );
	}

	void afterFadeIn()
	{
		//A fake hero and fairy are in the tower
		Invoke("insideTower", 3f);
	}

	void insideTower()
	{
		//Play the inside tower animation where the hero is trying to keep the fairy warm
		cutsceneCamera.gameObject.SetActive( true );
		fakeFloor.SetActive( false );
		//Add fog by the window
		fogByWindow.Play();
		playerController.GetComponent<HUDHandler>().fadeEffect( false, insideTowerFairySpeaks1 );
	}

	void insideTowerFairySpeaks1()
	{
		fairyController.speak("VO_FA_BLIZZARD_SO_COLD", 3.5f, false );
		Invoke("insideTowerFairySpeaks2", 5f);
	}

	void insideTowerFairySpeaks2()
	{
		fairyController.speak("VO_FA_BLIZZARD_FAIL", 6f, false );
		Invoke("insideTowerFairySpeaks3", 9f);
	}

	void insideTowerFairySpeaks3()
	{
		if( PlayerStatsManager.Instance.isAvatarMale() )
		{
			fairyController.speak("VO_FA_BLIZZARD_MALE_FRIEND", 4f, false );
		}
		else
		{
			fairyController.speak("VO_FA_BLIZZARD_FEMALE_FRIEND", 4f, false );
		}
		Invoke("fadeToWhite", 5f);
	}

	void fadeToWhite()
	{
		playerController.GetComponent<HUDHandler>().fadeEffect( true, waitBeforLoweringWindVolume );
	}

	void waitBeforLoweringWindVolume()
	{
		//Move fairy by the window
		fairyInsideTower.localPosition = fairyLocationByWindow.localPosition;
		fairyInsideTower.localRotation = fairyLocationByWindow.localRotation;
		//Remove fog by the window
		fogByWindow.Stop();
		Invoke("lowerWindVolume", 4f);
	}

	void lowerWindVolume()
	{
		worldSoundManager.crossFadeToMainAmbienceQuiet( 8f );
		Invoke("insideTowerWindQuietsFadeOut", 9f);
	}

	void insideTowerWindQuietsFadeOut()
	{
		playerController.GetComponent<HUDHandler>().fadeEffect( false, insideTowerWindHasDiedDown );
	}

	void insideTowerWindHasDiedDown()
	{
		LeanTween.rotate( fairyInsideTower.gameObject, new Vector3( 18f, 52f, 0 ), 1.7f ).setEase(LeanTweenType.easeOutQuad);
		fairyController.speak("VO_FA_BLIZZARD_WIND_QUIET", 3.5f, false );
		Invoke("fadeOut", 3.75f );
	}
	
	void fadeOut()
	{
		playerController.GetComponent<HUDHandler>().fadeEffect( true, goBackOutside );
	}

	void goBackOutside()
	{
		fakeFloor.SetActive( true );
		cutsceneCamera.gameObject.SetActive( false );
		Camera.main.GetComponent<StormManager>().deactivateStorm();
		snowStorm.SetActive( false );
		playerController.GetComponent<HUDHandler>().fadeEffect( false, mentionTree );

	}

	//Fairy tells something to player
	void mentionTree()
	{
		fairyController.speak("VO_FA_BLIZZARD_VAPORISE", 3.6f, false );
		Invoke ("castSpell", 3.75f );
	}

	//Fairy cast spell
	void castSpell()
	{
		fairyController.CastSpell( liftTree );
	}

	void liftTree()
	{
		LeanTween.moveLocalY( deadTreeOnThePath, deadTreeOnThePath.transform.localPosition.y + 0.5f, 1.8f).setEasePunch().setOnComplete(vaporiseTree).setOnCompleteParam(gameObject);
	}

	void vaporiseTree()
	{
		holyBlast.Play();
		Invoke("preparePlayerForRunning", 0.3f );
	}

	void preparePlayerForRunning()
	{
		deadTreeOnThePath.SetActive( false );
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
