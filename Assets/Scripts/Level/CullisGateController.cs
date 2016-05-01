using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CullisGateController : MonoBehaviour {

	Animation animation;
	public ParticleSystem lightEffect;
	SimpleCamera simpleCamera;
	public bool playCameraCutscene = false;
	public GameObject cullisGate;
	public string messageTextId = "CULLIS_GATE_TUTORIAL";
	//How long to wait before displaying either the stats screen or loading the net level
	const float WAIT_DURATION = 8f;

	// Use this for initialization
	void Start ()
	{
		animation = cullisGate.GetComponent<Animation>();
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		simpleCamera = player.GetComponent<SimpleCamera>();
	}

	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
	}

	public void Activation_complete()
	{
		print ("Cullis gate activation complete" );
		lightEffect.Play();
		if( playCameraCutscene ) Invoke("playCutscene", 2.2f);
		bool isGameFinished = LevelManager.Instance.incrementNextLevelToComplete();
		//Save the player stats before continuing
		PlayerStatsManager.Instance.savePlayerStats();
		if( isGameFinished )
		{
			AchievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText(messageTextId), 0.3f, 5.5f );
			fadeOutAllAudio( SoundManager.STANDARD_FADE_TIME );
			Invoke("quit", WAIT_DURATION );
		}
		else
		{
			if( LevelManager.Instance.isTutorialActive() )
			{
				AchievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText("CULLIS_GATE_TUTORIAL"), 0.3f, 5.5f );
			}
			else
			{
				AchievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText(messageTextId), 0.3f, 5.5f );
			}
			Invoke("loadLevelNow", WAIT_DURATION );
		}
	}

	void quit()
	{
		Debug.Log("Cullis Gate-Game is finished. Returning to world map.");
		SoundManager.stopMusic();
		SoundManager.stopAmbience();
		GameManager.Instance.setGameState(GameState.PostLevelPopup);
		SceneManager.LoadScene( (int) GameScenes.WorldMap );
	}

	void playCutscene()
	{
		simpleCamera.playCutscene( CutsceneType.CullisGate );
	}

	void loadLevelNow()
	{
		StartCoroutine( loadLevel () );
	}

	IEnumerator loadLevel()
	{
		Handheld.StartActivityIndicator();
		yield return new WaitForSeconds(0);
		//Load level scene
		SceneManager.LoadScene( (int)GameScenes.Level );
		
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Activate_Cullis_Gate )
		{
			animation.Play();
			GetComponent<AudioSource>().loop = false;
			GetComponent<AudioSource>().Play();
			Invoke("Activation_complete", 1.667f);
		}
	}

	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Paused )
		{
			animation.enabled = false;
		}
		else if( newState == GameState.Normal )
		{
			animation.enabled = true;
		}
	}

	void fadeOutAllAudio( float duration )
	{
		//We might have zombies nearby.
		//Zombies play a groan sound every few seconds.
		//We need to cancel the Invoke call in the zombie controller and might as well reset all zombies while we're at it.
		GameObject zombieManagerObject = GameObject.FindGameObjectWithTag("ZombieManager");
		ZombieManager zombieManager = zombieManagerObject.GetComponent<ZombieManager>();
		zombieManager.resetAllZombies();

		AudioSource[] allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
		foreach(AudioSource audioSource in allAudioSources )
		{
			//Don't fade out GUI sounds
			if( !audioSource.ignoreListenerPause )
			{
				if( audioSource.clip != null && audioSource.isPlaying )
				{
					StartCoroutine( SoundManager.fadeOutClip( audioSource, audioSource.clip, duration ) );
				}
			}
		}
	}
}
