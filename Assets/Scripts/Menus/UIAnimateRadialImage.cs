using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimateRadialImage : MonoBehaviour {

	Image radialImage;
	Coroutine fillAmountAnimateCoroutine;

	void Start ()
	{
		radialImage = GetComponent<Image>();
	}
	
	public void animateFillAmount ( float toValue, float duration, System.Action onFinish = null )
	{
		if( fillAmountAnimateCoroutine != null ) StopCoroutine( fillAmountAnimateCoroutine );
		fillAmountAnimateCoroutine = StartCoroutine( fillAmountAnimate( toValue, duration, onFinish ) );
	}

	IEnumerator fillAmountAnimate( float toValue, float duration, System.Action onFinish = null  )
	{
		float startTime = Time.time;
		float elapsedTime = 0;

		float fromValue = radialImage.fillAmount;
		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;

			radialImage.fillAmount =  Mathf.Lerp( fromValue, toValue, elapsedTime/duration );
			yield return new WaitForEndOfFrame();  
	    }
		if( onFinish != null ) onFinish.Invoke();
	}
}
