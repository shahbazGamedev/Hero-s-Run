using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : Device {

	[SerializeField] float doubleJumpSpeed = 17f;

	void OnTriggerEnter(Collider other)
	{
		if( state == DeviceState.On )
		{
			if( other.CompareTag("Player")  )
			{
				//The client stops moving until the RPC action gets processed.			
				if( other.GetComponent<PhotonView>().isMine )
				{				
					GetComponent<AudioSource>().Play();
					other.GetComponent<PlayerControl>().enablePlayerMovement( false );

					other.GetComponent<PhotonView>().RPC("jumpPadRPC", PhotonTargets.All, transform.position, transform.eulerAngles.y, PhotonNetwork.time, doubleJumpSpeed );
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
