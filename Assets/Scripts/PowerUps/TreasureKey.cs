using UnityEngine;
using System.Collections;


public class TreasureKey : MonoBehaviour {
	
	public AudioClip pickUpSound;

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" )
		{
			Debug.Log("Player picked up treasure key.");
			//Because we will be destroying the key game object, we will ask the parent (which does not get destroyed) to play the sound.
			AudioSource treasureKeyHandlerAudioSource = transform.parent.GetComponent<AudioSource>();
			treasureKeyHandlerAudioSource.clip = pickUpSound;
			treasureKeyHandlerAudioSource.Play();
			PlayerStatsManager.Instance.incrementNumberKeysFoundInEpisode();
			Destroy( gameObject );
		}
    }
}
