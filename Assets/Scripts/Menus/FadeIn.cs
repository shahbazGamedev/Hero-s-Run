using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class FadeIn : BaseClass {

	public float fadeDuration = 2f;
	Image imageToFade;

	void Awake()
	{
		imageToFade = gameObject.GetComponent<Image>();
	}

	// Fade-in image on enable
	void OnEnable ()
	{
		StartCoroutine( "fadeIn" );
	}

	IEnumerator fadeIn()
	{
		if( imageToFade != null )
		{
			Debug.Log("Fader-fadeIn: fading in image " +  gameObject.name );
			Color startColor = new Color( imageToFade.color.r, imageToFade.color.g, imageToFade.color.b, 0 );
			Color endColor = new Color( imageToFade.color.r, imageToFade.color.g, imageToFade.color.b, 1f );

			//Time in seconds to fade
			float time = fadeDuration;
			float originalTime = time;

			while ( time > 0.0f )
			{
				time -= Time.deltaTime;
				imageToFade.color = Color.Lerp( endColor, startColor, time / originalTime );

				yield return _sync();
			}
		}
	}
	
}
