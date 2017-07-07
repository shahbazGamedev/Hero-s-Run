using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : Device {

	[SerializeField] float doubleJumpSpeed = 17f;

	void OnTriggerEnter(Collider other)
	{
		if( state == DeviceState.On )
		{
			if( other.gameObject.CompareTag("Player")  )
			{
				if( other.GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Flying )
				{				
					GetComponent<AudioSource>().Play();
					other.GetComponent<PlayerInput>().doubleJump( doubleJumpSpeed );
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
