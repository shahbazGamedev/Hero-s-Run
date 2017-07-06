using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : Device {


	void OnTriggerEnter(Collider other)
	{
		if( state == DeviceState.On )
		{
			if( other.gameObject.CompareTag("Player")  )
			{
				if( other.GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Flying )
				{				
					GetComponent<AudioSource>().Play();
					other.GetComponent<PlayerInput>().doubleJump( 16f );
				}
			}
			else if( other.attachedRigidbody != null )
			{
				GetComponent<AudioSource>().Play();
				other.attachedRigidbody.AddForce( 0, 250f, 0, ForceMode.Force );
			}
		}
	}
}
