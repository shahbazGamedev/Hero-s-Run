using UnityEngine;
using System.Collections;
using DynamicFogAndMist;

public class StormManager : MonoBehaviour {

	public DynamicFog dynamicFog;
	[Range(0f, 1f)]
	public float stormIntensity = 0;
	public bool isStormActive = false;

	// Use this for initialization
	void Start ()
	{
		StartCoroutine( activateStorm( 50f ) );
	}
	
	// Update is called once per frame
	void Update ()
	{
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
		dynamicFog.skyHaze = Mathf.Lerp( 26f, 337f, intensity );

		//Force the materials to update
		dynamicFog.UpdateMaterialProperties();
	}

	public IEnumerator activateStorm( float duration, System.Action onFinish = null )
	{
		float elapsedTime = 0;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			setStormIntensity( elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		
		setStormIntensity( 1f );
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
		dynamicFog.skyHaze = Mathf.Lerp( 337f, 337f, intensity );
		dynamicFog.skySpeed = Mathf.Lerp( 0.3f, 0.49f, intensity );
		dynamicFog.skyNoiseStrength = Mathf.Lerp( 0.6f, 0.72f, intensity );
		dynamicFog.skyAlpha = Mathf.Lerp( 0.93f, 0.97f, intensity );

		//Force the materials to update
		dynamicFog.UpdateMaterialProperties();
	}
}
