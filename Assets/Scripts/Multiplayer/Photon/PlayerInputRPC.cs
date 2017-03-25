using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class PlayerInputRPC : PunBehaviour {

	PlayerControl playerControl;

	// Use this for initialization
	void Awake ()
	{
		//If we are the owner of this component, disable it. We only need it for remote clients.
		if( this.photonView.isMine ) this.enabled = false;
		playerControl = GetComponent<PlayerControl>();
	}

	[PunRPC]
	void sideSwipeRPC( bool direction, Vector3 syncPosition, float syncRotationY, double timeRPCSent, float syncSpeed )
	{
		syncMovement( syncPosition, syncRotationY, timeRPCSent, syncSpeed );
		playerControl.sideSwipe( direction );
	}

	[PunRPC]
	void startSlideRPC( Vector3 syncPosition, float syncRotationY, double timeRPCSent, float syncSpeed )
	{
		//Debug.Log("startSlide RPC received " + gameObject.name );
		syncMovement( syncPosition, syncRotationY, timeRPCSent, syncSpeed );
		playerControl.startSlide();
	}
	
	[PunRPC]
	void attachToZiplineRPC( Vector3 syncPosition, float syncRotationY, double timeRPCSent, float syncSpeed )
	{
		syncMovement( syncPosition, syncRotationY, timeRPCSent, syncSpeed );
		playerControl.attachToZipline();
	}

	[PunRPC]
	void detachToZiplineRPC( Vector3 syncPosition, float syncRotationY, double timeRPCSent, float syncSpeed )
	{
		syncMovement( syncPosition, syncRotationY, timeRPCSent, syncSpeed );
		playerControl.detachFromZipline();
	}

	[PunRPC]
	void jumpRPC( Vector3 syncPosition, float syncRotationY, double timeRPCSent, float syncSpeed )
	{
		syncMovement( syncPosition, syncRotationY, timeRPCSent, syncSpeed );
		playerControl.jump();
	}

	[PunRPC]
	void doubleJumpRPC( Vector3 syncPosition, float syncRotationY, double timeRPCSent, float syncSpeed, float doubleJumpSpeed )
	{
		syncMovement( syncPosition, syncRotationY, timeRPCSent, syncSpeed );
		playerControl.doubleJump( doubleJumpSpeed );
	}

	[PunRPC]
	void teleportRPC( Vector3 destinationPosition, float destinationRotationY )
	{
		//Use the values we received from the master
		transform.eulerAngles = new Vector3( transform.eulerAngles.x, destinationRotationY, transform.eulerAngles.z );
		transform.position = destinationPosition;
		//We may have switched lanes because of the position change. Make sure the lane values are accurate.
		playerControl.recalculateCurrentLane();
	}

	void syncMovement( Vector3 syncPosition, float syncRotationY, double timeRPCSent, float syncSpeed )
	{
		//Use the values we received from the master
		transform.eulerAngles = new Vector3( transform.eulerAngles.x, syncRotationY, transform.eulerAngles.z );
		transform.position = syncPosition;
		playerControl.runSpeed = syncSpeed;
		//We may have switched lanes because of the position change. Make sure the lane values are accurate.
		playerControl.recalculateCurrentLane();
		//There was a delay between the master sending us the command and the remote receiving it.
		//Predict where the player should be and move him there before executing the command.
		float syncTimeDelta = (float)(PhotonNetwork.time - timeRPCSent);
		//1) Get the direction of the player
		Vector3 forward = transform.TransformDirection(Vector3.forward);			
		//2) Scale vector based on run speed
		forward = forward * syncTimeDelta * syncSpeed;
		//3) Add Y component for gravity. Both the x and y components are stored in moveDirection.
		forward.Set( forward.x, playerControl.moveDirection.y * syncTimeDelta, forward.z );
		Debug.Log("syncMovement received N " + gameObject.name + "TD " + syncTimeDelta + " FL " + forward.magnitude );
		playerControl.controller.Move( forward );
		//The master player and the remote player should now be synchronised.
	}
}
