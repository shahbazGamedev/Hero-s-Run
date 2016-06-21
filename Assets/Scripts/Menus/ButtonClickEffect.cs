using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ButtonClickEffect : MonoBehaviour {

	public float scaleWhenClicked = 1.06f;
	public float scaleSpeed = 0.15f;
	Vector3 originalScale;
	Vector3 finalScale;

	// Use this for initialization
	void Start ()
	{
		GetComponent<Button>().onClick.RemoveAllListeners();
		GetComponent<Button>().onClick.AddListener( () => scaleUp() );
		originalScale = GetComponent<RectTransform>().localScale;
		finalScale = Vector3.one * scaleWhenClicked;
	}
	
	void scaleUp ()
	{
		GetComponent<RectTransform>().localScale = originalScale;
		LeanTween.scale ( GetComponent<RectTransform>(), finalScale, scaleSpeed ).setEase(LeanTweenType.easeOutQuad).setOnComplete(scaleDown).setOnCompleteParam(gameObject);
	}

	void scaleDown()
	{
		LeanTween.scale ( GetComponent<RectTransform>(), originalScale, scaleSpeed ).setEase(LeanTweenType.easeOutQuad);
	}
}
