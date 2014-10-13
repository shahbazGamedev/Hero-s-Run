using UnityEngine;
using System.Collections;

public class TriggerSound : MonoBehaviour {

	public bool loop = true;
	public GameObject objectWithAudioSource;
	public float percentageChanceEventTriggered = 1f;

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" && Random.value <= percentageChanceEventTriggered )
		{
			if( objectWithAudioSource != null )
			{
				Debug.Log ("TriggerSound " + objectWithAudioSource.audio.clip.name );
				objectWithAudioSource.audio.Play();
			}
			else
			{
				Debug.Log ("TriggerSound " + audio.clip.name );
				audio.loop = loop;
				audio.Play();
			}
		}
	}
}