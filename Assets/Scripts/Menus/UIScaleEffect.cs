using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScaleEffect : MonoBehaviour {

	public RectTransform rectangle;

	public void doScaleEffect()
	{
		scaleUp();
	}

	void scaleUp()
	{
		rectangle.transform.localScale = new Vector3( 0.6f, 0.6f, 0.6f );
		rectangle.gameObject.SetActive( true );
		Vector3 bigSize = new Vector3( 1.07f, 1.07f, 1.07f );
		LeanTween.scale( rectangle, bigSize, 0.36f ).setEase(LeanTweenType.easeOutQuad).setOnComplete(scaleNormal).setOnCompleteParam(gameObject);
	}
		
	void scaleNormal()
	{
		LeanTween.scale( rectangle, Vector3.one, 0.3f ).setEase(LeanTweenType.easeOutQuad).setOnComplete(scaleDown).setOnCompleteParam(gameObject);;
	}
	
	void scaleDown()
	{
		LeanTween.scale( rectangle, new Vector3(0, 0,0 ), 0.36f ).setEase(LeanTweenType.easeOutQuad).setDelay(3f).setOnComplete(hide).setOnCompleteParam(gameObject);
	}

	public void hide()
	{
		rectangle.gameObject.SetActive( false );
	}
	
}
