using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FadeInCanvasGroup : MonoBehaviour {
	
	[SerializeField] float fadeInDuration = 0.5f;
	[SerializeField] float fadeOutDuration = 0.5f;

	void OnEnable ()
	{
		LeanTween.alphaCanvas( GetComponent<CanvasGroup>(), 1f, fadeInDuration );		
	}

	void OnDisable ()
	{
		GetComponent<CanvasGroup>().alpha = 0;		
	}

	public void fadeOut ()
	{
		LeanTween.alphaCanvas( GetComponent<CanvasGroup>(), 0f, fadeOutDuration );		
	}
	
}
