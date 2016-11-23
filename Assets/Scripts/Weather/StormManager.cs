using UnityEngine;
using System.Collections;
using DynamicFogAndMist;

public class StormManager : MonoBehaviour {


	[Header("StormManager")]
	public DynamicFog dynamicFog;
	public WorldSoundManager worldSoundManager;
	[Tooltip("The material used to animate the foliage to simulate wind blowing in the leaves. This material needs to have _Amplitude and _Windspeed parameters.")]
	public Material foliage;

	[Header("Parameters used for testing")]
	[Range(0f, 1f)]
	public bool isFogAndHazeActive = false;
	public float fogAndHazeIntensity = 0;
	[Range(0f, 1f)]
	public bool isStormActive = false;
	public float stormIntensity = 0;


	// Use this for initialization
	void Start ()
	{
		StartCoroutine( activateFogAndHaze( 40f, startStorm ) );
	}

	void startStorm ()
	{
		StartCoroutine( activateStorm( 40f ) );
	}
	
	// The update code is for testing
	void Update ()
	{
		if( isFogAndHazeActive) setFogAndHazeIntensity( fogAndHazeIntensity );
		if( isStormActive) setStormIntensity( stormIntensity );
	}

	public IEnumerator activateFogAndHaze( float duration, System.Action onFinish = null )
	{
		float elapsedTime = 0;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			setFogAndHazeIntensity( elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		
		setFogAndHazeIntensity( 1f );
		if( onFinish != null ) onFinish.Invoke();
	}

	void setFogAndHazeIntensity( float intensity )
	{
		//Fog Properties
		dynamicFog.alpha =  Mathf.Lerp( 0.6f, 0.96f, intensity );

		//Sky Properties
		dynamicFog.skyHaze = Mathf.Lerp( 26f, 350f, intensity );

		//Force the materials to update
		dynamicFog.UpdateMaterialProperties();
	}

	//Activating the storm does a few things:
	//1) It adds a second ambience track with louder winds
	//2) It increases the amplitude and speed of the foliage movement
	//3) It increases the fog and and sky haze further
	public IEnumerator activateStorm( float duration, System.Action onFinish = null )
	{
		worldSoundManager.crossFadeToSecondaryAmbience( duration );
		float elapsedTime = 0;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			setStormIntensity( elapsedTime/duration );
			setFoliageMovementIntensity( elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		
		setStormIntensity( 1f );
		setFoliageMovementIntensity( 1f );
		if( onFinish != null ) onFinish.Invoke();		
	}

	void setStormIntensity( float intensity )
	{
		//Fog Properties
		dynamicFog.alpha =  Mathf.Lerp( 0.96f, 1f, intensity );
		dynamicFog.noiseStrength = Mathf.Lerp( 0.4f, 0.45f, intensity );
		dynamicFog.distance = Mathf.Lerp( 0.01f, 0f, intensity );
		dynamicFog.distanceFallOff = Mathf.Lerp( 0.04f, 0.028f, intensity );
		dynamicFog.maxDistance = Mathf.Lerp( 0.999f, 0.999f, intensity );
		dynamicFog.maxDistanceFallOff = Mathf.Lerp(0f, 0f, intensity );
		dynamicFog.height = Mathf.Lerp( 20f, 83f, intensity );
		dynamicFog.heightFallOff = Mathf.Lerp( 1f, 0f, intensity );
		dynamicFog.baselineHeight = Mathf.Lerp(0f, 0f, intensity );
		dynamicFog.turbulence = Mathf.Lerp( 0.4f, 15f, intensity );
		dynamicFog.speed = Mathf.Lerp( 0.005f, 0.2f, intensity );

		//Sky Properties
		dynamicFog.skyHaze = Mathf.Lerp( 350f, 337f, intensity );
		dynamicFog.skySpeed = Mathf.Lerp( 0.3f, 0.49f, intensity );
		dynamicFog.skyNoiseStrength = Mathf.Lerp( 0.6f, 0.72f, intensity );
		dynamicFog.skyAlpha = Mathf.Lerp( 0.93f, 0.97f, intensity );

		//Force the materials to update
		dynamicFog.UpdateMaterialProperties();

	}

	void setFoliageMovementIntensity( float intensity )
	{
		//Fog Properties
		foliage.SetFloat( "_Amplitude", Mathf.Lerp( 0.3f, 0.5f, intensity ) );
		foliage.SetFloat( "_Windspeed", Mathf.Lerp( 0.3f, 0.5f, intensity ) );
	}
	
}
