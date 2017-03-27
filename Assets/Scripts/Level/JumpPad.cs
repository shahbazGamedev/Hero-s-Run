using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : Device {


	void OnTriggerEnter(Collider other)
	{
		if( other.gameObject.CompareTag("Player") && state == DeviceState.On )
		{
			GetComponent<AudioSource>().Play();
			other.gameObject.GetComponent<PlayerInput>().doubleJump( 15f );
		}
	}
}
