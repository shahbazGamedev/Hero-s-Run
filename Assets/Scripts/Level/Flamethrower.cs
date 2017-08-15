using UnityEngine;
using System.Collections;

//The collider should have the "Obstacle_M" tag.
public class Flamethrower : MonoBehaviour {

	[SerializeField] AudioClip onFlameContact;

	void OnTriggerEnter(Collider other)
	{
		if( other.CompareTag("Player") )
		{
			other.GetComponent<AudioSource>().PlayOneShot(onFlameContact);
			other.GetComponent<PlayerControl>().killPlayer ( DeathType.Flame );
		}
	}

}
