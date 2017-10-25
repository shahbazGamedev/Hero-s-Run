using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TrapCullisGateController : MonoBehaviour {

	[Header("General")]
	public ParticleSystem lightEffect;
	public ParticleSystem vortex;
	public string messageTextId = "CULLIS_GATE_XXX";
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
		if( dimSunlightOnActivation )
		{
			GameObject sunlight = GameObject.FindGameObjectWithTag("Sunlight");
			sunlightHandler = sunlight.GetComponent<SunlightHandler>();
		}
	}

	//Step 0 - player arrives in the middle of the cullis gate and activates the trigger
	void OnTriggerEnter(Collider other)
	{
		if( other.CompareTag("Player") && !isActive )
		{
			isActive = true;
			playerEnteredCullisGate();
		}
	}

	//Step 1 - player is wondering what is going on
	public void playerEnteredCullisGate()
	{
		player.GetComponent<PlayerController>().anim.CrossFadeInFixedTime("VictoryLoop", 0.25f);
		Invoke("startLookingAround", 2f );
	}

	//Step 2 - player starts looking around
	void startLookingAround()
	{
		player.GetComponent<PlayerController>().anim.CrossFadeInFixedTime("Idle_Look", 0.25f);
		Invoke("dimLights", 2f );
	}	

	//Step 3 - both the sunlight and the ambient source dim and an audio effect plays
	void dimLights()
	{
		StartCoroutine( sunlightHandler.fadeOutLight( sunlightDimDuration, sunlightIntensityAfterDim, true ) );
		GetComponent<AudioSource>().Play();
		Invoke("playFX", 1.8f );
	}

	//Step 4 - a green smog surrounds the player
	void playFX()
	{
		lightEffect.Play();
		Invoke("fairySpeaks", 2.75f );
	}

	//Step 5 - the fairy speaks and says, something is going terribly wrong!
	void fairySpeaks()
	{
		DialogManager.dialogManager.activateDisplayFairy( LocalizationManager.Instance.getText(messageTextId), 5f );
		Invoke("startVortex", 1.5f );
	}

	//Step 6 - a vortex starts spinning underneath the player
	void startVortex()
	{
		vortex.Play();
		Invoke("pullHeroUnderground", 1.5f );
	}

	//Step 7 - the player is pulled underground
	void pullHeroUnderground()
	{
		//Center player in the exact center of the cullis gate
		player.transform.SetParent( transform );
		player.transform.localPosition = new Vector3 ( 0, 0.54f, 0 );
		player.GetComponent<PlayerCamera>().lockCamera( true );
		player.GetComponent<PlayerController>().anim.speed = 3.8f;
		player.GetComponent<PlayerController>().anim.CrossFadeInFixedTime("Fall", 0.25f);
		LeanTween.moveY( player, transform.position.y - 4f, 5f ).setEase(LeanTweenType.easeOutExpo);
		Invoke("quit", 5.5f );
	}

	//Step 8 - save and return to world map
	void quit()
	{
		player.transform.SetParent( null );
		//Save the player stats before continuing
		PlayerStatsManager.Instance.savePlayerStats();
		bool isGameFinished = LevelManager.Instance.incrementNextEpisodeToComplete();
		if( isGameFinished )
		{
		}
		else
		{
			LevelManager.Instance.setEpisodeCompleted( true );
		}
		FacebookManager.Instance.postHighScore( LevelManager.Instance.getCurrentEpisodeNumber() + 1 );
		GameManager.Instance.setGameState(GameState.PostLevelPopup);
		SceneManager.LoadScene( (int) GameScenes.WorldMap );
	}

}
