using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectSound : MonoBehaviour {

	[SerializeField] AudioClip onCollisionClip;

	void OnCollisionEnter(Collision collision)
	{
		if( !GetComponent<AudioSource>().isPlaying ) GetComponent<AudioSource>().PlayOneShot( onCollisionClip );
	}
}
