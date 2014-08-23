using UnityEngine;
using System.Collections;

public class TriggerSound : MonoBehaviour {

	public bool loop = true;

	void OnTriggerEnter(Collider other)
	{
		Debug.Log ("TriggerSound " + audio.clip.name );
		audio.loop = loop;
		audio.Play();
	}
}