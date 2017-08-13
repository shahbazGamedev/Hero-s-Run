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
					//Only the Master Client can trigger the jump.
					//The Master Client stops moving until the RPC action gets processed.			
					if( PhotonNetwork.isMasterClient )
					{				
						GetComponent<AudioSource>().Play();
						other.GetComponent<PlayerControl>().enablePlayerMovement( false );

						other.GetComponent<PhotonView>().RPC("jumpPadRPC", PhotonTargets.All, transform.position, transform.eulerAngles.y, PhotonNetwork.time, doubleJumpSpeed );
					}
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
