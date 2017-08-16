using UnityEngine;
using System.Collections;

public class BulletDestroy : MonoBehaviour
{
	public float destroyTime = 2f;
	void OnEnable()
	{
		Invoke ("Destroy", destroyTime);
	}

	void Destroy ()
	{
		gameObject.SetActive (false);
	}

	void OnDisable()
	{
		CancelInvoke ();
	}
}
