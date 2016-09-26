using UnityEngine;
using System.Collections;

public class SoundOnCollision : MonoBehaviour {

	public void OnCollisionEnter(Collision collision)
	{
		//Play collision sound
		if( !GetComponent<AudioSource>().isPlaying ) GetComponent<AudioSource>().Play();
	}
}
