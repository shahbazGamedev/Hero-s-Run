﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimateSlider : MonoBehaviour {

	Slider slider;

	void Start ()
	{
		slider = GetComponent<Slider>();
	}
	
	public void animateSlider ( float toValue, float duration, System.Action onFinish = null )
	{
		StartCoroutine( sliderAnimate( toValue, duration, onFinish ) );
	}

	IEnumerator sliderAnimate( float toValue, float duration, System.Action onFinish = null  )
	{
		float startTime = Time.time;
		float elapsedTime = 0;

		float fromValue = slider.value;
	
		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;

			slider.value =  Mathf.Lerp( fromValue, toValue, elapsedTime/duration );
			yield return new WaitForEndOfFrame();  
	    }
		if( onFinish != null ) onFinish.Invoke();
	}
}
