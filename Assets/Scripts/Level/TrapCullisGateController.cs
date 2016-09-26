using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TrapCullisGateController : MonoBehaviour {

	[Header("General")]
	public ParticleSystem lightEffect;
	public ParticleSystem vortex;
	SimpleCamera simpleCamera;
	public bool playCameraCutscene = false;
	public string messageTextId = "CULLIS_GATE_TUTORIAL";
	//How long to wait before displaying either the stats screen or loading the net level
	const float WAIT_DURATION = 12f;
	GameObject player;
	[Header("Light Dimming")]
	[Tooltip("If true, the sun light will dim to the intensity specified by the sunlightIntensityAfterDim parameter in the time specified by the sunlightDimDuration parameter. In addition, the ambient source color will gradually become black.")]
	public bool dimSunlightOnActivation = false;
	public float sunlightDimDuration = 2f;
	public float sunlightIntensityAfterDim = 0.2f;
	SunlightHandler sunlightHandler;
	bool isActive = false;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.FindGameObjectWithTag("Player");
		simpleCamera = player.GetComponent<SimpleCamera>();
		if( dimSunlightOnActivation )
		{
			GameObject sunlight = GameObject.FindGameObjectWithTag("Sunlight");
			sunlightHandler = sunlight.GetComponent<SunlightHandler>();
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" && !isActive )
		{
			isActive = true;
			playerEnteredCullisGate();
		}
	}

	//Step 1 - player enters cullis gate. This is called by the player controller
	public void playerEnteredCullisGate()
	{
		Debug.Log("Cullis Gate-playerEnteredCullisGate.");
		player.GetComponent<PlayerController>().anim.CrossFadeInFixedTime("VictoryLoop", 0.2f);
		Invoke("startLookingAround", 2f );
	}

	//Step 2 - start looking around
	void startLookingAround()
	{
		player.GetComponent<PlayerController>().anim.CrossFadeInFixedTime("Idle_Look", 0.2f);
		Invoke("dimLights", 2f );
	}	

	//Step 3 - dim lights and start creepy music
	void dimLights()
	{
		StartCoroutine( sunlightHandler.fadeOutLight( sunlightDimDuration, sunlightIntensityAfterDim, true ) );
		GetComponent<AudioSource>().loop = false;
		GetComponent<AudioSource>().Play();
		Invoke("playFX", 1.8f );
	}

	//Step 4 - green effects
	void playFX()
	{
		lightEffect.Play();
		Invoke("fairySpeaks", 2.75f );
	}

	//Step 5 - fairy speaks
	void fairySpeaks()
	{
		DialogManager.dialogManager.activateDisplayFairy( LocalizationManager.Instance.getText(messageTextId), 5f );
		Invoke("startVortex", 1.5f );
	}

	//Step 6 - hero anim becomes teleport leave
	void startVortex()
	{
		vortex.Play();
		Invoke("pullHeroUnderground", 1.5f );
	}

	//Step 7 - pull hero underground
	void pullHeroUnderground()
	{
		//Center player in the exact center of the cullis gate
		player.transform.SetParent( transform );
		player.transform.localPosition = new Vector3 ( 0, 0.54f, 0 );
		player.GetComponent<SimpleCamera>().lockCamera( true );
		player.GetComponent<PlayerController>().anim.speed = 3.8f;
		player.GetComponent<PlayerController>().anim.CrossFadeInFixedTime("Fall", 0.25f);
		LeanTween.moveLocalY( player, transform.position.y - 4f, 5f ).setEase(LeanTweenType.easeOutExpo);

		Invoke("quit", 5.5f );
	}

	//Step 8 - save and return to world map
	void quit()
	{
		player.transform.SetParent( null );
		//Save the player stats before continuing
		LevelManager.Instance.setEpisodeCompleted( true );
		bool isGameFinished = LevelManager.Instance.incrementNextLevelToComplete();
		PlayerStatsManager.Instance.savePlayerStats();
		Debug.Log("Cullis Gate-Returning to world map.");
		SoundManager.soundManager.stopMusic();
		SoundManager.soundManager.stopAmbience();
		GameManager.Instance.setGameState(GameState.PostLevelPopup);
		SceneManager.LoadScene( (int) GameScenes.WorldMap );
	}

}
