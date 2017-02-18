using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class PlayerInputRPC : PunBehaviour {

	PlayerControl playerControl;
	PowerUpManager powerUpManager;

	// Use this for initialization
	void Awake ()
	{
		//If we are the owner of this component, disable it. We only need it for remote clients.
		if( this.photonView.isMine ) this.enabled = false;
		playerControl = GetComponent<PlayerControl>();

		//For power ups
		GameObject powerUpManagerObject = GameObject.FindGameObjectWithTag("PowerUpManager");
		powerUpManager = (PowerUpManager) powerUpManagerObject.GetComponent("PowerUpManager");
	}

	[PunRPC]
	void sideSwipeRPC( bool direction )
	{
		playerControl.sideSwipe( direction );
	}

	[PunRPC]
	void startSlideRPC()
	{
		Debug.Log("startSlide RPC received " + gameObject.name );
		playerControl.startSlide();
	}
	
	[PunRPC]
	void attachToZiplineRPC()
	{
		playerControl.attachToZipline();
	}

	[PunRPC]
	void jumpRPC()
	{
		playerControl.jump();
	}

	[PunRPC]
	void handlePowerUpRPC()
	{
		playerControl.handlePowerUp();
	}

}
