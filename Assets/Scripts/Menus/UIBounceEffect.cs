using System.Collections;
using UnityEngine;

/// <summary>
/// UI bounce effect.
/// Scales the UI size up and down very slightly.
/// This gives a bit of a bounce when displaying a popup for example.
/// </summary>
public class UIBounceEffect : MonoBehaviour
{
	public void scaleUp()
	{
		CancelInvoke("scaleDown");
		LeanTween.cancel( gameObject );
		LeanTween.scale( gameObject.GetComponent<RectTransform>(), new Vector3( 1.014f, 1.014f, 1.014f ), 0.18f ).setOnComplete(scaleDown).setOnCompleteParam(gameObject);
	}
	
	void scaleDown()
	{
		LeanTween.scale( gameObject.GetComponent<RectTransform>(), Vector3.one, 0.25f );
	}

	void OnEnable()
	{
		scaleUp();
	}
}
