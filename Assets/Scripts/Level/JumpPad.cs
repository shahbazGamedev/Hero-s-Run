using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour {

	void OnTriggerEnter(Collider other)
	{
		if( other.gameObject.CompareTag("Player")  )
		{
			GetComponent<AudioSource>().Play();
			other.gameObject.GetComponent<PlayerInput>().doubleJump( 15f );
		}
	}
}
