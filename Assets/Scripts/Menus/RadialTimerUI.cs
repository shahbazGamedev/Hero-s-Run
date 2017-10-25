using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;

public class RadialTimerUI : MonoBehaviour {

	[SerializeField] Image radialImage;
	[SerializeField] Image radialImageMask;
	[SerializeField] Text nameText;
	[SerializeField] Text timeRemainingText;

	public void startAnimation ( string name, float duration, Color color )
	{
		gameObject.SetActive( true );
		nameText.text = name;
		StartCoroutine( animate( duration ) );
		radialImage.color = color;
	}
	
	IEnumerator animate( float duration  )
	{
		float startTime = Time.time;
		float elapsedTime = 0;

		float fromValue = 0;
	
		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;

			radialImageMask.fillAmount =  Mathf.Lerp( fromValue, 1f, elapsedTime/duration );
			timeRemainingText.text = ( Math.Ceiling( (1f - radialImageMask.fillAmount) * duration ) ).ToString("N0") + "s";
			yield return new WaitForEndOfFrame();  
	    }
		Destroy( gameObject );
	}
}
