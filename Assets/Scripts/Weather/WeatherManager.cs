using UnityEngine;
using System.Collections;

public class WeatherManager : MonoBehaviour {

	public ParticleSystem rain;
	ParticleSystem activeParticleSystem; //either rain or snow
	Transform player;
	SimpleCamera simpleCamera;
	bool isParticleSystemActive = false;

	Transform weatherTarget; //snow or rain
	float weatherHeight = 8f;

	// Update is called once per frame
	void Update ()
	{
		if( simpleCamera != null && !simpleCamera.isCameraLocked )
		{
			if( isParticleSystemActive )
			{
				activeParticleSystem.transform.position = new Vector3( weatherTarget.position.x, weatherTarget.position.y + weatherHeight, weatherTarget.position.z );
			}
		}
	}

	//The weather effect (rain or snow) follows the target's position. By default, the target is the player, but you can change it
	//with this function. You could set the target to the cutscene camera for example.
	public void setWeatherTarget( Transform target, float height )
	{
		weatherTarget = target;
		weatherHeight = height;
		print ("setWeatherTarget: target is now: " + weatherTarget.name );
	}

	public void setWeatherTargetToPlayer()
	{
		weatherTarget = player;
		weatherHeight = 8f;
	}

	void OnEnable()
	{
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
		PlayerController.localPlayerCreated += LocalPlayerCreated;
	}
	
	void OnDisable()
	{
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
		PlayerController.localPlayerCreated -= LocalPlayerCreated;
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
	}

	void LocalPlayerCreated( Transform playerTransform, PlayerController playerController )
	{
		player = playerTransform;
		simpleCamera = player.GetComponent<SimpleCamera>();
		weatherTarget = player;
	}

	public void activateRain( bool enable )
	{
		if( enable )
		{
			print ("WeatherManager-activateRain GameEvent.Start_Raining");
			activeParticleSystem = rain;
			activeParticleSystem.Play();
			isParticleSystemActive = true;
		}
		else
		{
			print ("WeatherManager-activateRain GameEvent.Stop_Raining");
			if( activeParticleSystem !=null ) activeParticleSystem.Stop();
			isParticleSystemActive = false;
		}
	}
}
