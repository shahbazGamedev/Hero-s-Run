using UnityEngine;
using System.Collections;

public class TriggerSound : MonoBehaviour {

	public bool loop = true;
	[Tooltip("objectWithAudioSource is optional. If not specified, the audio source attached to the trigger will play its clip. If specified, the audio source of the object specified will play.")]
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