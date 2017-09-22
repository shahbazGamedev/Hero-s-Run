using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimateRadialImage : MonoBehaviour {

	Image radialImage;
	Coroutine fillAmountAnimateCoroutine = null;

	void Start ()
	{
		radialImage = GetComponent<Image>();
	}
	
	public void animateFillAmount ( float toValue, float duration, RectTransform optionalMarker = null, System.Action onFinish = null )
	{
		if( fillAmountAnimateCoroutine != null ) StopCoroutine( fillAmountAnimateCoroutine );
		fillAmountAnimateCoroutine = StartCoroutine( fillAmountAnimate( toValue, duration, optionalMarker, onFinish ) );
	}

	IEnumerator fillAmountAnimate( float toValue, float duration, RectTransform optionalMarker = null, System.Action onFinish = null  )
	{
		float radialImageHorizontalLength = radialImage.GetComponent<RectTransform>().sizeDelta.x;
		float startTime = Time.time;
		float elapsedTime = 0;

		float fromValue = radialImage.fillAmount;
		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;

			radialImage.fillAmount =  Mathf.Lerp( fromValue, toValue, elapsedTime/duration );
			if( optionalMarker != null ) optionalMarker.anchoredPosition = new Vector2( radialImage.fillAmount * radialImageHorizontalLength, optionalMarker.anchoredPosition.y );
			yield return new WaitForEndOfFrame();  
	    }
		if( onFinish != null ) onFinish.Invoke();
	}
}
