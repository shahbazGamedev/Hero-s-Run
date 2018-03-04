using System.Collections;
using UnityEngine;

/// <summary>
/// UI bounce effect.
/// Scales the UI size up and down very slightly.
/// This gives a bit of a bounce when displaying a popup for example.
/// </summary>
public class UIBounceEffect : MonoBehaviour
{
	[SerializeField] float scaleUpStartDelay = 0;
	[SerializeField] float scale = 1.014f;
	[SerializeField] float scaleUpDuration = 0.18f;
	[SerializeField] float scaleDownDuration = 0.25f;
	[SerializeField] bool scaleUpOnEnable = true;
	Vector3 initialScale;

	public void scaleUp()
	{
		initialScale = gameObject.GetComponent<RectTransform>().localScale;
		Vector3 increasedScale = initialScale * scale;
		CancelInvoke("scaleDown");
		LeanTween.cancel( gameObject );
		LeanTween.scale( gameObject.GetComponent<RectTransform>(), increasedScale, scaleUpDuration ).setOnComplete(scaleDown).setOnCompleteParam(gameObject).setDelay( scaleUpStartDelay ).setIgnoreTimeScale( true );
	}
	
	void scaleDown()
	{
		LeanTween.scale( gameObject.GetComponent<RectTransform>(), initialScale, scaleDownDuration ).setIgnoreTimeScale( true );
	}

	void OnEnable()
	{
		if( scaleUpOnEnable ) scaleUp();
	}
}
