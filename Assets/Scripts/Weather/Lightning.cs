using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Lightning : MonoBehaviour {
	
	public float minDecayTime = 0.24f;
	public float maxDecayTime = 0.65f; //used to be 0.75
	public float maxInterval = 20.0f;
	public float minInterval = 7f;
	const float SHADOW_STRENGTH = 0.48f;
	//The light intensity of the lightning flash
	public float flashIntensity = 5f;
	public List<AudioClip> thunderSounds = new List<AudioClip>();
	bool lightningActive = false;
	//The directional light intensity as set in the level
	float originalIntensity;
	Quaternion originalRotation;
	LightShadows originalShadowSetting;
	float originalShadowStrength;

	// Update is called once per frame
	void Update () 
	{
		if( lightningActive )
		{
			// Make shadows rotate slowly to give the illusion that the lightning is occuring in various locations.
			transform.RotateAround(Vector3.zero, Vector3.up, Time.deltaTime);
		}
	}

	void playThunderSound()
	{
		if( thunderSounds.Count > 0 )
		{
			int index = Random.Range( 0, thunderSounds.Count );
			audio.PlayOneShot( thunderSounds[index] );
		}
	}

	void FlashRepeat()
	{
		light.shadowStrength = SHADOW_STRENGTH;
		light.shadows = LightShadows.Hard;
		light.intensity = flashIntensity;
		playThunderSound();
		StartCoroutine("DecayRepeat");
	}


	IEnumerator DecayRepeat()
	{
		// Set random duration of flash
		float decayTime = Random.Range(minDecayTime, maxDecayTime);
		float t = 0;
		while (t < decayTime)
		{
			light.intensity = Mathf.Lerp(flashIntensity, originalIntensity, t/decayTime);
			t += Time.deltaTime;
			yield return null;
		}
		// Set up another flash interval
		Invoke ("FlashRepeat", Random.Range(minInterval, maxInterval));
		resetLight();
	}

	void FlashOnce()
	{
		light.shadowStrength = SHADOW_STRENGTH;
		light.shadows = LightShadows.Hard;

		light.intensity = flashIntensity;
		playThunderSound();
		StartCoroutine("DecayOnce");
	}
	
	IEnumerator DecayOnce()
	{
		// Set random duration of flash
		float decayTime = Random.Range(minDecayTime, maxDecayTime);
		float t = 0;
		while (t < decayTime)
		{
			light.intensity = Mathf.Lerp(flashIntensity, originalIntensity, t/decayTime);
			t += Time.deltaTime;
			yield return null;
		}
		resetLight();
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
	
	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		controlLightning( eventType );
	}

	public void controlLightning( GameEvent eventType )
	{

		if( eventType == GameEvent.Start_Lightning )
		{
			rememberOriginalLightSettings();
			//Ignore if already started
			if( !lightningActive )
			{
				print ("Lightning-controlLightning GameEvent.Start_Lightning");
				Invoke ("FlashRepeat", Random.Range(minInterval, maxInterval));
				lightningActive = true;
			}
		}
		else if( eventType == GameEvent.Stop_Lightning )
		{
			stopLightning();
		}
		else if( eventType == GameEvent.Lightning_Flash )
		{
			print ("Lightning-controlLightning GameEvent.Lightning_Flash");
			rememberOriginalLightSettings();
			FlashOnce();
		}
	}

	//Remember the current light intensity and light rotation since we will be changing it
	void rememberOriginalLightSettings()
	{
		originalIntensity = light.intensity;
		originalRotation = light.transform.rotation;
		originalShadowSetting = light.shadows;
		originalShadowStrength = light.shadowStrength;
	}

	void resetLight()
	{
		light.intensity = originalIntensity;
		light.transform.rotation = originalRotation;
		light.shadows = originalShadowSetting;
		light.shadowStrength = originalShadowStrength;
	}

	void stopLightning()
	{
		if( lightningActive )
		{
			print ("Lightning-stopLightning");
			lightningActive = false;
			CancelInvoke("FlashRepeat");
			StopCoroutine("DecayRepeat");
			StopCoroutine("DecayOnce");
			resetLight();
		}
	}

	void GameStateChange( GameState newState )
	{
		if( newState == GameState.StatsScreen )
		{
			stopLightning();
		}
	}

}
