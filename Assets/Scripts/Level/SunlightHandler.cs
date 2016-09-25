using UnityEngine;
using System.Collections;

public class SunlightHandler : MonoBehaviour {

	float originalIntensity;

	public IEnumerator fadeOutLight( float duration, float endIntensity, bool fadeOutAmbientSkyColor = false )
	{
		Color startColor = RenderSettings.ambientSkyColor;
		Color endColor = Color.black;

		float elapsedTime = 0;
		originalIntensity = GetComponent<Light>().intensity;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			GetComponent<Light>().intensity =  Mathf.Lerp( originalIntensity, endIntensity, elapsedTime/duration );
			if( fadeOutAmbientSkyColor ) RenderSettings.ambientSkyColor = Color.Lerp( startColor, endColor, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		
		GetComponent<Light>().intensity = endIntensity;
	}

	public IEnumerator fadeInLight( float duration )
	{
		float elapsedTime = 0;
		
		float startIntensity = GetComponent<Light>().intensity;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			GetComponent<Light>().intensity =  Mathf.Lerp( startIntensity, originalIntensity, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		GetComponent<Light>().intensity = originalIntensity;
	}

	void OnEnable()
	{
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
	}
	
	void OnDisable()
	{
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
	}
	

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Dim_light )
		{
			StartCoroutine( fadeOutLight( 1.5f, 0f ) );
		}
		else if( eventType == GameEvent.Brighten_light )
		{
			StartCoroutine( fadeInLight( 1.5f ) );
		}
	}

}
