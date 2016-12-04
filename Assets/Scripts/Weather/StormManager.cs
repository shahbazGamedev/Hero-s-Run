using UnityEngine;
using System.Collections;
using DynamicFogAndMist;

public class StormManager : MonoBehaviour {


	[Header("StormManager")]
	public DynamicFog dynamicFog;
	public WorldSoundManager worldSoundManager;
	public Light sun;
	float originalShadowStrength = 0;
	[Tooltip("Currently NOT working. If I use the original foliage snow shader (with rim level), it does not work on lower end devices and looks black. If I use the country foliage shader, it also makes the foliage black. I am currently using a Standard shader but it does not support the required Windspeed or Amplitude options. The material used to animate the foliage to simulate wind blowing in the leaves. This material needs to have _Amplitude and _Windspeed parameters.")]
	public Material foliage;
	float originalFoliageMovement = 0.2f;

	[Header("Parameters used for testing")]
	public bool isFogAndHazeActive = false;
	[Range(0f, 1f)]
	public float fogAndHazeIntensity = 0;
	public bool isStormActive = false;
	[Range(0f, 1f)]
	public float stormIntensity = 0;


	// Use this for initialization
	void Start ()
	{
		originalShadowStrength = sun.shadowStrength;
		//setFoliageMovementIntensity( 0 ); //currently not working
	}

	public void initiateStorm ()
	{
		StartCoroutine( activateFogAndHaze( 8f, startStorm ) );
	}

	void startStorm ()
	{
		StartCoroutine( activateStorm( 25f ) );
	}
	
	// The update code is for testing
	void Update ()
	{
		if( isFogAndHazeActive) setFogAndHazeIntensity( fogAndHazeIntensity );
		if( isStormActive) setStormIntensity( stormIntensity );
	}

	IEnumerator activateFogAndHaze( float duration, System.Action onFinish = null )
	{
		float elapsedTime = 0;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			setFogAndHazeIntensity( elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		
		setFogAndHazeIntensity( 1f );
		//Since the sky is hazy now, we can remove the shadow casting
		sun.shadows = LightShadows.None;
		if( onFinish != null ) onFinish.Invoke();
	}

	void setFogAndHazeIntensity( float intensity )
	{
		//Fog Properties
		dynamicFog.alpha =  Mathf.Lerp( 0.6f, 0.96f, intensity );

		//Sky Properties
		dynamicFog.skyAlpha = Mathf.Lerp( 0f, 0.93f, intensity );

		//Force the materials to update
		dynamicFog.UpdateMaterialProperties();

		//As the sky is getting hazier, reduce the shadow strength
		setShadowIntensity( intensity );
	}

	//Activating the storm does a few things:
	//1) It adds a second ambience track with louder winds
	//2) It increases the amplitude and speed of the foliage movement
	//3) It increases the fog and and sky haze further
	IEnumerator activateStorm( float duration, System.Action onFinish = null )
	{
		worldSoundManager.crossFadeToSecondaryAmbience( duration );
		float elapsedTime = 0;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			setStormIntensity( elapsedTime/duration );
			//setFoliageMovementIntensity( elapsedTime/duration ); //currently not working
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		
		setStormIntensity( 1f );
		//setFoliageMovementIntensity( 1f ); 
		if( onFinish != null ) onFinish.Invoke();		
	}

	void setStormIntensity( float intensity )
	{
		//Fog Properties
		dynamicFog.alpha =  Mathf.Lerp( 0.96f, 1f, intensity );
		dynamicFog.noiseStrength = Mathf.Lerp( 0.4f, 0.45f, intensity );
		dynamicFog.distance = Mathf.Lerp( 0.01f, 0f, intensity );
		dynamicFog.distanceFallOff = Mathf.Lerp( 0.04f, 0.028f, intensity );
		dynamicFog.height = Mathf.Lerp( 20f, 83f, intensity );
		dynamicFog.heightFallOff = Mathf.Lerp( 1f, 0f, intensity );
		dynamicFog.turbulence = Mathf.Lerp( 0.4f, 15f, intensity );
		dynamicFog.speed = Mathf.Lerp( 0.005f, 0.2f, intensity );

		//Sky Properties
		dynamicFog.skySpeed = Mathf.Lerp( 0.3f, 0.49f, intensity );
		dynamicFog.skyNoiseStrength = Mathf.Lerp( 0.6f, 0.72f, intensity );
		dynamicFog.skyAlpha = Mathf.Lerp( 0.93f, 0.97f, intensity );

		//Force the materials to update
		dynamicFog.UpdateMaterialProperties();
	}

	void setFoliageMovementIntensity( float intensity )
	{
		//Fog Properties
		foliage.SetFloat( "_Amplitude", Mathf.Lerp( originalFoliageMovement, 0.5f, intensity ) );
		foliage.SetFloat( "_Windspeed", Mathf.Lerp( originalFoliageMovement, 0.5f, intensity ) );
	}
	
	void setShadowIntensity( float intensity )
	{
		sun.shadowStrength = Mathf.Lerp( originalShadowStrength, 0, intensity );
	}

	public void deactivateStorm()
	{
		dynamicFog.preset = FOG_PRESET.Mist;
		//Force the materials to update
		dynamicFog.UpdateMaterialProperties();
		//setFoliageMovementIntensity( 0 );
		//Since the sky is clear now, we can add back the shadow casting
		sun.shadowStrength = originalShadowStrength;
		sun.shadows = LightShadows.Soft;
	}

}
