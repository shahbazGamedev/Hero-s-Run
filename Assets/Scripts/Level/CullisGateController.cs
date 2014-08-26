using UnityEngine;
using System.Collections;

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
		//if( isGameFinished )
		if( true )
		{
			//AchievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText("LEVEL_GAME_COMPLETED"), 0.3f, 5.5f );
			AchievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText("CULLIS_GATE_DEMO_END"), 0.3f, 5.5f );
			Invoke("displayStatsScreen", WAIT_DURATION );
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

	void displayStatsScreen()
	{
		GameManager.Instance.setGameState( GameState.StatsScreen );
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
		Application.LoadLevel( 4 );
		
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Activate_Cullis_Gate )
		{
			animation.Play();
			audio.loop = false;
			audio.Play();
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
		else if( newState == GameState.StatsScreen )
		{
			fadeOutAllAudio( SoundManager.STANDARD_FADE_TIME );
		}
	}

	void fadeOutAllAudio( float duration )
	{
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
