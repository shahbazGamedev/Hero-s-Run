﻿using System.Collections;
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
	void sideSwipeRPC( bool direction, Vector3 syncPosition, float syncRotationY, double timeRPCSent )
	{
		playerControl.sideSwipe( direction );
	}

	[PunRPC]
	void startSlideRPC( Vector3 syncPosition, float syncRotationY, double timeRPCSent )
	{
		//Debug.Log("startSlide RPC received " + gameObject.name );
		playerControl.startSlide();
	}
	
	[PunRPC]
	void attachToZiplineRPC( Vector3 syncPosition, float syncRotationY, double timeRPCSent )
	{
		//force all players to be at the beginning of the zipline
		transform.position = syncPosition;
		transform.eulerAngles = new Vector3( transform.eulerAngles.x, syncRotationY, transform.eulerAngles.z );
		LockstepManager.Instance.addActionToQueue( new LockstepManager.LockstepAction( LockstepActionType.ATTACH_TO_ZIPLINE, gameObject ) );
	}

	[PunRPC]
	void detachFromZiplineRPC( Vector3 syncPosition, float syncRotationY, double timeRPCSent )
	{
		//force all players to be at the end of the zipline
		transform.position = syncPosition;
		transform.eulerAngles = new Vector3( transform.eulerAngles.x, syncRotationY, transform.eulerAngles.z );
		LockstepManager.Instance.addActionToQueue( new LockstepManager.LockstepAction( LockstepActionType.DETACH_FROM_ZIPLINE, gameObject ) );
	}

	[PunRPC]
	void jumpRPC( Vector3 syncPosition, float syncRotationY, double timeRPCSent )
	{
		playerControl.jump( false );
	}

	[PunRPC]
	void doubleJumpRPC( Vector3 syncPosition, float syncRotationY, double timeRPCSent, float doubleJumpSpeed )
	{
		playerControl.jump( true, doubleJumpSpeed );
	}

	[PunRPC]
	//Sent by the Master client only.
	void jumpPadRPC( Vector3 syncPosition, float syncRotationY, double timeRPCSent, float doubleJumpSpeed )
	{
		//Force all clients to be on the jump pad, but preserve the Y value.
		transform.position = new Vector3( syncPosition.x, transform.position.y, syncPosition.z );
		transform.eulerAngles = new Vector3( transform.eulerAngles.x, syncRotationY, transform.eulerAngles.z );
		//We may have switched lanes because of the position change. Make sure the lane values are accurate.
		playerControl.recalculateCurrentLane();

		LockstepManager.Instance.addActionToQueue( new LockstepManager.LockstepAction( LockstepActionType.JUMP_PAD, gameObject, CardName.None, doubleJumpSpeed ) );
	}

	[PunRPC]
	//Sent by the Master client only.
	void teleportRPC( Vector3 syncPosition, float syncRotationY, Vector3 destinationPosition, float destinationRotationY, int numberOfTilesSkippedBecauseOfTeleportation )
	{
		//Force all clients to be on the teleporter
		transform.position = syncPosition;
		transform.eulerAngles = new Vector3( transform.eulerAngles.x, syncRotationY, transform.eulerAngles.z );
		//We may have switched lanes because of the position change. Make sure the lane values are accurate.
		playerControl.recalculateCurrentLane();
		//Important: assumes all tiles in between the TX and RX teleporters have a depth of one.
		playerControl.tileDistanceTraveled = playerControl.tileDistanceTraveled + numberOfTilesSkippedBecauseOfTeleportation * GenerateLevel.tileSize;
		LockstepManager.LockstepAction lsa = new LockstepManager.LockstepAction( LockstepActionType.TELEPORTER, gameObject );
		lsa.param1 = destinationRotationY;
		lsa.param2 = numberOfTilesSkippedBecauseOfTeleportation;
		lsa.param3 = destinationPosition;
		LockstepManager.Instance.addActionToQueue( lsa );
	}

}
