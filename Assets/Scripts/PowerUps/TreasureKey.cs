using UnityEngine;
using System.Collections;


public class TreasureKey : MonoBehaviour {
	
	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" )
		{
			Debug.Log("Player picked up treasure key.");
			PlayerStatsManager.Instance.incrementNumberKeysFoundInEpisode();
			Destroy( gameObject );
		}
    }
}
