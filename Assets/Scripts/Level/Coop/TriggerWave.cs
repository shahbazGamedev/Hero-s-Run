using System.Collections;
using UnityEngine;

public class TriggerWave : MonoBehaviour {

	
	void OnTriggerEnter(Collider other)
	{
		if( other.CompareTag("Player")  )
		{
			CoopWaveGenerator.Instance.activateNewWave();
 			//This is to avoid OnTriggerEnter being called a second time.
			GetComponent<BoxCollider>().enabled = false;
		}
	}
}
