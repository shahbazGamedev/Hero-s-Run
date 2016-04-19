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
				Debug.Log ("TriggerSound " + objectWithAudioSource.GetComponent<AudioSource>().clip.name );
				objectWithAudioSource.GetComponent<AudioSource>().Play();
			}
			else
			{
				Debug.Log ("TriggerSound " + GetComponent<AudioSource>().clip.name );
				GetComponent<AudioSource>().loop = loop;
				GetComponent<AudioSource>().Play();
			}
		}
	}
}