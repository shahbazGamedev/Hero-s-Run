using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class FadeIn : MonoBehaviour {

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
			Debug.Log("Fader-fadeIn: fading in image " +  gameObject.name + " " + fadeDuration );
			Color startColor = new Color( imageToFade.color.r, imageToFade.color.g, imageToFade.color.b, 0 );
			Color endColor = new Color( imageToFade.color.r, imageToFade.color.g, imageToFade.color.b, 1f );

			float time = 0;

			while ( time <= fadeDuration )
			{
				time += Time.deltaTime;
				imageToFade.color = Color.Lerp( startColor, endColor, time / fadeDuration );
				yield return new WaitForFixedUpdate(); 
			}
		}
	}
	
}
