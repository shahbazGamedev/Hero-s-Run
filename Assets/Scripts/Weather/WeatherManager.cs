using UnityEngine;
using System.Collections;

public class WeatherManager : BaseClass {

	public ParticleSystem rain;
	public ParticleSystem snow;
	ParticleSystem activeParticleSystem; //either rain or snow
	Transform player;
	bool isParticleSystemActive = false;
	public GameObject fog; //Needs to use a cloud from cloud system
	public AudioClip rainSound;
	public AudioClip snowSound;

	Transform fogTarget;
	float fogHeight = 30f;

	Transform weatherTarget; //snow or rain
	float weatherHeight = 8f;

	// Use this for initialization
	void Awake () {
		player = GameObject.FindGameObjectWithTag("Player").transform;
		fogTarget = player;
		weatherTarget = player;
	}
	
	// Update is called once per frame
	void Update () {
		if( PlayerController.deathType != DeathType.Water )
		{
			if( isParticleSystemActive )
			{
				activeParticleSystem.transform.position = new Vector3( weatherTarget.position.x, weatherTarget.position.y + weatherHeight, weatherTarget.position.z );
			}
			if( fog.activeSelf )
			{
				fog.transform.position = new Vector3( fogTarget.position.x, fogTarget.position.y + fogHeight, fogTarget.position.z );
			}
		}
	}

	//The fog follows the target's position. By default, the target is the player, but you can change it
	//with this function. You could set the target to the cutscene camera for example.
	public void setFogTarget( Transform target, float height )
	{
		fogTarget = target;
		fogHeight = height;
		print ("setFogTarget: target is now: " + fogTarget.name );
	}

	//The weather effect (rain or snow) follows the target's position. By default, the target is the player, but you can change it
	//with this function. You could set the target to the cutscene camera for example.
	public void setWeatherTarget( Transform target, float height )
	{
		weatherTarget = target;
		weatherHeight = height;
		print ("setWeatherTarget: target is now: " + weatherTarget.name );
	}

	public void setFogTargetToPlayer()
	{
		fogTarget = player;
		fogHeight = 30f;
	}

	public void setWeatherTargetToPlayer()
	{
		weatherTarget = player;
		weatherHeight = 8f;
	}

	void OnEnable()
	{
		PlayerController.playerStateChanged += PlayerStateChange;
		GameManager.gameStateEvent += GameStateChange;
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
	}
	
	void OnDisable()
	{
		PlayerController.playerStateChanged -= PlayerStateChange;
		GameManager.gameStateEvent -= GameStateChange;
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Start_Raining )
		{
			activateRain( true );
		}
		else if( eventType == GameEvent.Stop_Raining )
		{
			activateRain( false );
		}
		else if( eventType == GameEvent.Start_Snowing )
		{
			print ("WeatherManager-player entered trigger GameEvent.Start_Snowing");
			StartCoroutine( SoundManager.fadeInClip( audio, snowSound, 4f ) );
			activeParticleSystem = snow;
			activeParticleSystem.Play();
			isParticleSystemActive = true;
		}
		else if( eventType == GameEvent.Stop_Snowing )
		{
			print ("WeatherManager-player entered trigger GameEvent.Stop_Snowing");
			StartCoroutine( SoundManager.fadeOutClip( audio, snowSound, 2f ) );
			activeParticleSystem.Stop();
			isParticleSystemActive = false;
		}
		else if( eventType == GameEvent.Start_Fog )
		{
			activateFog( true );
		}
		else if( eventType == GameEvent.Stop_Fog )
		{
			activateFog( false );
		}
	}

	public void activateRain( bool enable )
	{
		if( enable )
		{
			print ("WeatherManager-activateRain GameEvent.Start_Raining");
			StartCoroutine( SoundManager.fadeInClip( audio, rainSound, 4f ) );
			activeParticleSystem = rain;
			activeParticleSystem.Play();
			isParticleSystemActive = true;
		}
		else
		{
			print ("WeatherManager-activateRain GameEvent.Stop_Raining");
			StartCoroutine( SoundManager.fadeOutClip( audio, rainSound, 2f ) );
			if( activeParticleSystem !=null ) activeParticleSystem.Stop();
			isParticleSystemActive = false;
		}
	}
	
	public void activateFog( bool enable )
	{
		if( fog != null )
		{
			if( enable )
			{
				print ("WeatherManager-activateFog GameEvent.Start_Fog");
				//Make the fog active
				fog.SetActive( true );
				//Change the tint of the cloud to the value specified in the level data
				CS_Cloud cs = fog.GetComponent<CS_Cloud>();
				if( cs != null )
				{
					StartCoroutine( fadeInFog(cs,0.5f,2f));
				}
			}
			else
			{
				print ("WeatherManager-activateFog GameEvent.Stop_Fog");
				CS_Cloud cs = fog.GetComponent<CS_Cloud>();
				if( cs != null )
				{
					StartCoroutine( fadeOutFog(cs,0, 4f));
				}
			}
		}
	}


	public void setFogTint( Color tint, float fade )
	{
		if( fog != null )
		{
			//Make the fog active
			fog.SetActive( true );
			//Change the tint of the cloud to the value specified in the level data
			CS_Cloud cs = fog.GetComponent<CS_Cloud>();
			if( cs != null )
			{
				cs.Tint = tint;
				cs.Fading = fade;
			}
		}
	}

	IEnumerator fadeInFog( CS_Cloud cs, float fadeTo, float duration )
	{
		Debug.LogWarning ("fadeInFog is not implemented yet.");
		yield return _sync();
	}

	IEnumerator fadeOutFog( CS_Cloud cs, float fadeTo, float duration )
	{
		float startTime = Time.time;
		float elapsedTime = 0;
		float originalFade = cs.Fading;

		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;
			cs.Fading = Mathf.Lerp( originalFade, 0, elapsedTime/duration);
			yield return _sync();
		}
		fog.SetActive( false );
	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
		{
		}
	}

	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Paused )
		{

		}
		else if( newState == GameState.Normal )
		{
		}
		else if( newState == GameState.StatsScreen )
		{
			if( activeParticleSystem != null ) activeParticleSystem.Stop();
			isParticleSystemActive = false;
		}
	}
}
