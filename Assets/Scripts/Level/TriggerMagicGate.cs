using UnityEngine;
using System.Collections;

public class TriggerMagicGate : MonoBehaviour {


	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero"  )
		{
			GetComponent<AudioSource>().Play();
			LevelData levelData = LevelManager.Instance.getLevelData();
			Debug.Log("Magic Portal triggered");
			levelData.setSunParameters(SunType.Morning);

		}
	}
}