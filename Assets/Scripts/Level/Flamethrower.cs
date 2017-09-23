using UnityEngine;
using System.Collections;

//The collider should have the "Obstacle_M" tag.
public class Flamethrower : MonoBehaviour {

	[SerializeField] AudioClip onFlameContact;
	[Range(0,PlayerHealth.DEFAULT_HEALTH)]
	[SerializeField] int flameDamage = 50;

	void OnTriggerEnter(Collider other)
	{
		if( other.CompareTag("Player") )
		{
			GetComponent<AudioSource>().PlayOneShot(onFlameContact);
			other.GetComponent<PlayerHealth>().deductHealth( flameDamage );
		}
	}

}
