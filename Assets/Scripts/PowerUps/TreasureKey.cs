using UnityEngine;
using System.Collections;


public class TreasureKey : MonoBehaviour {
	
	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" )
		{
			//Because we will be destroying the key game object, we will ask the parent (which does not get destroyed) to play the sound.
			transform.parent.GetComponent<TreasureKeyHandler>().keyPickedUp();
			PlayerStatsManager.Instance.incrementNumberKeysFoundInEpisode();
			Destroy( gameObject );
		}
    }
}
