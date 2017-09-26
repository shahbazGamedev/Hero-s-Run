using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FadeInCanvasGroup : MonoBehaviour {
	
	[SerializeField] bool fadeInOnEnable = true;
	[SerializeField] float fadeInDuration = 0.5f;
	[SerializeField] float fadeOutDuration = 0.5f;

	void OnEnable ()
	{
		if( fadeInOnEnable ) LeanTween.alphaCanvas( GetComponent<CanvasGroup>(), 1f, fadeInDuration );		
	}

	void OnDisable ()
	{
		if( fadeInOnEnable ) GetComponent<CanvasGroup>().alpha = 0;		
	}

	public void fadeIn ()
	{
		LeanTween.alphaCanvas( GetComponent<CanvasGroup>(), 1f, fadeInDuration );		
	}

	public void fadeOut ()
	{
		LeanTween.alphaCanvas( GetComponent<CanvasGroup>(), 0f, fadeOutDuration );		
	}
	
}
