using UnityEngine;
using System.Collections;

public class TriggerMagicGate : MonoBehaviour {


	void OnTriggerEnter(Collider other)
	{
		if( other.gameObject.CompareTag("Player") )
		{
			GetComponent<AudioSource>().Play();
			LevelData levelData = LevelManager.Instance.getLevelData();
			Debug.Log("Magic Portal triggered");
			levelData.setSunParameters(SunType.Morning);

		}
	}
}