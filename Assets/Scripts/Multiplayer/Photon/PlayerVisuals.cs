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

	public void playDustPuff( bool activate, bool loop = true )
	{
		ParticleSystem.MainModule main = dustPuff.main;
		main.loop = loop;
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

	//This is where for the master client only, we instantiate the player skin. PhotonNetwork.InstantiateSceneObject will instantiate the skin on the remote players
	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		if( GetComponent<PlayerAI>() == null )
		{
			instantiateHuman( info );
		}
		else
		{
			instantiateBot( info );
		}
	}

	//This is where for the master client only, we instantiate the player skin. PhotonNetwork.InstantiateSceneObject will instantiate the skin on the remote players
	void instantiateHuman( PhotonMessageInfo info )
	{
		gameObject.name = info.sender.NickName;
		HeroManager.HeroCharacter selectedHero = HeroManager.Instance.getHeroCharacter( (int)info.sender.CustomProperties["Hero"] );
		Debug.Log("PlayerVisuals-OnPhotonInstantiate-Hero name: " + selectedHero.name + " Hero Index " + (int)info.sender.CustomProperties["Hero"] + " isMasterClient: " + PhotonNetwork.isMasterClient + " Name: " + info.sender.NickName );
		if ( PhotonNetwork.isMasterClient )
		{
			object[] data = new object[1];
			data[0] = this.photonView.viewID;
		    GameObject heroSkin = (GameObject)PhotonNetwork.InstantiateSceneObject (selectedHero.skinPrefab, Vector3.zero, Quaternion.identity, 0, data);
			Animator anim = gameObject.GetComponent<Animator>();
			heroSkin.transform.SetParent( transform, false );
			heroSkin.transform.localPosition = Vector3.zero;
			heroSkin.transform.localRotation = Quaternion.identity;
			anim.avatar = heroSkin.GetComponent<PlayerSkinInfo>().animatorAvatar;
			anim.Rebind(); //Important
		}
		//Register with the minimap and the turn-ribbon
		if( this.photonView.isMine )
		{
			MiniMap.Instance.registerLocalPlayer( transform );
			PhotonNetwork.player.TagObject = gameObject;
			GameObject.FindGameObjectWithTag("Turn-Ribbon").GetComponent<TurnRibbonHandler>().setPlayerControl( GetComponent<PlayerControl>() );
		}
		else
		{
			MiniMap.Instance.registerRadarObject( gameObject, selectedHero.minimapIcon, GetComponent<PlayerControl>() );
		}		
	}

	//We are a bot.
	void instantiateBot( PhotonMessageInfo info )
	{
		HeroManager.BotHeroCharacter botHero = GetComponent<PlayerAI>().botHero;

		//Name our game object
		gameObject.name = botHero.userName;

		//Create the skin for the bot and set up the animator
		if ( PhotonNetwork.isMasterClient )
		{
			object[] stuff = new object[1];
			stuff[0] = info.sender.NickName;
		    GameObject heroSkin = (GameObject)PhotonNetwork.InstantiateSceneObject (botHero.skinPrefab, Vector3.zero, Quaternion.identity, 0, stuff);
			Animator anim = gameObject.GetComponent<Animator>();
			heroSkin.transform.SetParent( transform, false );
			heroSkin.transform.localPosition = Vector3.zero;
			heroSkin.transform.localRotation = Quaternion.identity;
			anim.avatar = heroSkin.GetComponent<PlayerSkinInfo>().animatorAvatar;
			anim.Rebind(); //Important
		}
		//Register with the minimap
		MiniMap.Instance.registerRadarObject( gameObject, botHero.minimapIcon, GetComponent<PlayerControl>() );
	}

}
