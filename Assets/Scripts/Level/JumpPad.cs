using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : Device {

	[Range(10,19)]
	[SerializeField] float doubleJumpSpeed = 19f;

	//We change the color of these items to red when the jump pad is broken.
	[SerializeField] Light pointLight;
	[SerializeField] ParticleSystem swirlyAura;
	[SerializeField] ParticleSystem glow;
		
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

	new public void changeDeviceState( DeviceState newState )
	{
		base.changeDeviceState( newState );
		switch ( state )
		{
			case DeviceState.Broken:
				pointLight.color = Color.red;

				ParticleSystem.MainModule swirlyAuraMain = swirlyAura.main;
				swirlyAuraMain.startColor = Color.red;

				ParticleSystem.MainModule glowMain = glow.main;
				glowMain.startColor = new Color( 255f, 0, 0, 47f/255f );

			break;
		}
	}

}
