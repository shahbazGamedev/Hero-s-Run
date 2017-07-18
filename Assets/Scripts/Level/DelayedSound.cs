using System.Collections;
using UnityEngine;

public class DelayedSound : MonoBehaviour {

	[SerializeField] float delayBeforeSoundPlays = 3f;

	void Start ()
	{
		GetComponent<AudioSource>().PlayDelayed( delayBeforeSoundPlays );
	}
	
}
