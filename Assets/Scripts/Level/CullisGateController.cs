using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CullisGateController : MonoBehaviour {

	[Header("General")]
	public ParticleSystem lightEffect;
	SimpleCamera simpleCamera;
	public bool playCameraCutscene = false;
	public string messageTextId = "CULLIS_GATE_TUTORIAL";
	//How long to wait before displaying either the stats screen or loading the net level
	const float WAIT_DURATION = 8f;
	[Header("Light Dimming")]
	[Tooltip("If true, the sun light will dim to the intensity specified by the sunlightIntensityAfterDim parameter in the time specified by the sunlightDimDuration parameter. In addition, the ambient source color will gradually become black.")]
	public bool dimSunlightOnActivation = false;
	public float sunlightDimDuration = 2f;
	public float sunlightIntensityAfterDim = 0.2f;
	SunlightHandler sunlightHandler;

	// Use this for initialization
	void Start ()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		simpleCamera = player.GetComponent<SimpleCamera>();
		if( dimSunlightOnActivation )
		{
			GameObject sunlight = GameObject.FindGameObjectWithTag("Sunlight");
			sunlightHandler = sunlight.GetComponent<SunlightHandler>();
		}
	}

	void OnEnable()
	{
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
	}
	
	void OnDisable()
	{
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
	}

	public void Activation_complete( AnimationEvent eve )
	{
		print ("Cullis gate activation complete" );
		lightEffect.Play();
		if( playCameraCutscene ) Invoke("playCutscene", 2.2f);
		bool isGameFinished = LevelManager.Instance.incrementNextLevelToComplete();
		//Save the player stats before continuing
		PlayerStatsManager.Instance.savePlayerStats();
		if( isGameFinished )
		{
			DialogManager.dialogManager.activateDisplayFairy( LocalizationManager.Instance.getText(messageTextId), 5.5f );
		}
		else
		{
			DialogManager.dialogManager.activateDisplayFairy( LocalizationManager.Instance.getText(messageTextId), 5.5f );
			LevelManager.Instance.setEpisodeCompleted( true );
		}
		resetAllZombies();
		SoundManager.soundManager.fadeOutAllAudio( SoundManager.STANDARD_FADE_TIME );
		Invoke("quit", WAIT_DURATION );
	}

	void quit()
	{
		Debug.Log("Cullis Gate-Returning to world map.");
		SoundManager.soundManager.stopMusic();
		SoundManager.soundManager.stopAmbience();
		GameManager.Instance.setGameState(GameState.PostLevelPopup);
		SceneManager.LoadScene( (int) GameScenes.WorldMap );
	}

	void playCutscene()
	{
		simpleCamera.playCutscene( CutsceneType.CullisGate );
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Activate_Cullis_Gate )
		{
			GetComponent<Animator>().Play("Activate");
			if( dimSunlightOnActivation ) StartCoroutine( sunlightHandler.fadeOutLight( sunlightDimDuration, sunlightIntensityAfterDim, true ) );
			GetComponent<AudioSource>().loop = false;
			GetComponent<AudioSource>().Play();
		}
	}

	void resetAllZombies()
	{
		//We might have zombies nearby.
		//Zombies play a groan sound every few seconds.
		//We need to cancel the Invoke call in the zombie controller and might as well reset all zombies while we're at it.
		GameObject zombieManagerObject = GameObject.FindGameObjectWithTag("CreatureManager");
		ZombieManager zombieManager = zombieManagerObject.GetComponent<ZombieManager>();
		zombieManager.resetAllZombies();
	}

}
