using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisuals : Photon.PunBehaviour {

	//Particles
	public ParticleSystem dustPuff;
	public ParticleSystem waterSplashWhileSliding; //Plays when player slides in water.It loops.
	//Casts a circular shadow at the feet of the player
	Projector shadowProjector;

	// Use this for initialization
	void Start () {
		Transform blobShadowProjectorObject = transform.FindChild("Blob Shadow Projector");
		if( blobShadowProjectorObject == null )
		{
			Debug.LogError("PlayerVisuals-error: Unable to find, Blob Shadow Projector." );
		}
		shadowProjector = blobShadowProjectorObject.GetComponent<Projector>();
		
	}

	public void playDustPuff( bool activate )
	{
		if( activate )
		{
			dustPuff.Play();
		}
		else
		{
			dustPuff.Stop();
		}
	}

	public void playWaterSplashWhileSliding( bool activate )
	{
		if( activate )
		{
			waterSplashWhileSliding.Play();
		}
		else
		{
			waterSplashWhileSliding.Stop();
		}
	}

	public void enablePlayerShadow( bool activate )
	{
		shadowProjector.enabled = activate;
	}

	//This is where for the master client only, we instantiate the player skin
	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		gameObject.name = info.sender.NickName;
		Debug.Log("PlayerVisuals-OnPhotonInstantiate-Skin: " + info.sender.CustomProperties["Skin"] + " isMasterClient: " + PhotonNetwork.isMasterClient + " Name: " + info.sender.NickName );
		if ( PhotonNetwork.isMasterClient )
		{
			object[] stuff = new object[1];
			stuff[0] = info.sender.NickName;
		    GameObject heroSkin = (GameObject)PhotonNetwork.InstantiateSceneObject (info.sender.CustomProperties["Skin"].ToString(), Vector3.zero, Quaternion.identity, 0, stuff);
			Animator anim = gameObject.GetComponent<Animator>();
			heroSkin.transform.SetParent( transform, false );
			heroSkin.transform.localPosition = Vector3.zero;
			heroSkin.transform.localRotation = Quaternion.identity;
			anim.avatar = heroSkin.GetComponent<PlayerSkinInfo>().animatorAvatar;
			anim.Rebind(); //Important
		}
		//Register with the minimap
		if( this.photonView.isMine )
		{
			MiniMap.Instance.registerLocalPlayer( transform );
			PhotonNetwork.player.TagObject = gameObject;
		}
		else
		{
			MiniMap.Instance.registerRadarObject( gameObject );
		}		
	}

}
