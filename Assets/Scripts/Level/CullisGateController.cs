using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CullisGateController : MonoBehaviour {

	[Header("General")]
	public ParticleSystem lightEffect;
	SimpleCamera simpleCamera;
	public bool playCameraCutscene = false;
	public string messageTextId = "CULLIS_GATE_XXX";
	//How long to wait before displaying either the stats screen or loading the net level
	const float WAIT_DURATION = 8f;

	// Use this for initialization
	void Start ()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		simpleCamera = player.GetComponent<SimpleCamera>();
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
		//Save the player stats before continuing
		PlayerStatsManager.Instance.savePlayerStats();
		bool isGameFinished = LevelManager.Instance.incrementNextEpisodeToComplete();
		if( isGameFinished )
		{
			DialogManager.dialogManager.activateDisplayFairy( LocalizationManager.Instance.getText(messageTextId), 5.5f );
		}
		else
		{
			DialogManager.dialogManager.activateDisplayFairy( LocalizationManager.Instance.getText(messageTextId), 5.5f );
			LevelManager.Instance.setEpisodeCompleted( true );
		}
		FacebookManager.Instance.postHighScore( LevelManager.Instance.getCurrentEpisodeNumber() + 1 );
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
