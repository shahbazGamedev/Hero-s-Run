using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FadeInCanvasGroup : MonoBehaviour {
	
	public float fadeInDuration = 0.5f;

	void OnEnable ()
	{
		LeanTween.alphaCanvas( GetComponent<CanvasGroup>(), 1f, fadeInDuration );		
	}

	void OnDisable ()
	{
		GetComponent<CanvasGroup>().alpha = 0;		
	}
	
}
