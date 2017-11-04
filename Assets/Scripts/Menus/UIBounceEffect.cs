using System.Collections;
using UnityEngine;

/// <summary>
/// UI bounce effect.
/// Scales the UI size up and down very slightly.
/// This gives a bit of a bounce when displaying a popup for example.
/// </summary>
public class UIBounceEffect : MonoBehaviour
{
	[SerializeField] float scale = 1.014f;
	[SerializeField] float scaleUpDuration = 0.18f;
	[SerializeField] float scaleDownDuration = 0.25f;

	public void scaleUp()
	{
		CancelInvoke("scaleDown");
		LeanTween.cancel( gameObject );
		LeanTween.scale( gameObject.GetComponent<RectTransform>(), new Vector3( scale, scale, scale ), scaleUpDuration ).setOnComplete(scaleDown).setOnCompleteParam(gameObject);
	}
	
	void scaleDown()
	{
		LeanTween.scale( gameObject.GetComponent<RectTransform>(), Vector3.one, scaleDownDuration );
	}

	void OnEnable()
	{
		scaleUp();
	}
}
