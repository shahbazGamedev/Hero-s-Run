using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;

public class RadialTimerUI : MonoBehaviour {

	[SerializeField] Image radialImage;
	[SerializeField] Text nameText;
	[SerializeField] Text timeRemainingText;

	public void startAnimation ( string name, float duration )
	{
		gameObject.SetActive( true );
		nameText.text = name;
		StartCoroutine( animate( duration ) );
	}
	
	IEnumerator animate( float duration  )
	{
		float startTime = Time.time;
		float elapsedTime = 0;

		float fromValue = 0;
	
		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;

			radialImage.fillAmount =  Mathf.Lerp( fromValue, 1f, elapsedTime/duration );
			timeRemainingText.text = ( Math.Ceiling( (1f - radialImage.fillAmount) * duration ) ).ToString("N0") + "s";
			yield return new WaitForEndOfFrame();  
	    }
		gameObject.SetActive( false );
	}
}
