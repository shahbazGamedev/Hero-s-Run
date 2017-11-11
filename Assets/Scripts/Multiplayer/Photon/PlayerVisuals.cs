using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrazyMinnow.SALSA;

public class PlayerVisuals : Photon.PunBehaviour {

	[Tooltip("When playing in a nightime level, the character can be a bit dark so we add emission to brighten the character. Make sure the value is the same in PlayerVisuals and OnSkinCreated.")]
	[Range(0,1)]
	[SerializeField] float nightEmission = 0.33f;
	//Particles
	[SerializeField]  ParticleSystem runVFX;
	[SerializeField]  ParticleSystem dustPuff;
	[SerializeField]  ParticleSystem waterSplashWhileSliding; //Plays when player slides in water.It loops.
	[Tooltip("The impact VFX plays when the player hits an obstacle and falls backward. It should include an AudioSource with the associated impact sound. The AudioSource should have PlayOnAwake set to true.")]
	[SerializeField]  ParticleSystem impactVFXPrefab;
	[SerializeField]  Vector3 impactVFXoffset = new Vector3( 0, 1.8f, 0.3f );
	//Casts a circular shadow at the feet of the player
	Projector shadowProjector;

	// Use this for initialization
	void Start () {
		Transform blobShadowProjectorObject = transform.Find("Blob Shadow Projector");
		if( blobShadowProjectorObject == null )
		{
			Debug.LogError("PlayerVisuals-error: Unable to find, Blob Shadow Projector." );
		}
		shadowProjector = blobShadowProjectorObject.GetComponent<Projector>();
		
	}

	public void pausePlayer( bool isPaused )
	{
		if( isPaused )
		{
			if( runVFX != null && runVFX.isPlaying ) runVFX.Pause();
			if( dustPuff.isPlaying ) dustPuff.Pause();
		}
		else
		{
			if( runVFX != null && runVFX.isPaused ) runVFX.Play();
			if( dustPuff.isPaused ) dustPuff.Play();
		}
	}

	public void handlePlayerStateChange( PlayerCharacterState newState )
	{
		if( runVFX == null ) return;

	    switch (newState)
		{
	        case PlayerCharacterState.Dying:
				runVFX.Stop();
				break;
	                
	        case PlayerCharacterState.StartRunning:
				runVFX.Play();
				break;
	                
			case PlayerCharacterState.DoubleJumping:
			case PlayerCharacterState.Jumping:
				runVFX.Stop();
				break;
		
			case PlayerCharacterState.Turning:
				break;
	                
	        case PlayerCharacterState.Sliding:
			case PlayerCharacterState.Turning_and_sliding:
				runVFX.Stop();
				break;

	        case PlayerCharacterState.Stumbling:
				break;

	        case PlayerCharacterState.Falling:
				runVFX.Stop();
				break;

	        case PlayerCharacterState.Idle:
	        case PlayerCharacterState.Ziplining:
				runVFX.Stop();
				break;

	        case PlayerCharacterState.Running:
				if( !runVFX.isPlaying ) runVFX.Play();
				break;
		}
	}

	public void playImpactVFX()
	{
		if( impactVFXPrefab != null )
		{
			ParticleSystem impactVFX = GameObject.Instantiate( impactVFXPrefab ) as ParticleSystem;
			impactVFX.transform.position = transform.TransformPoint( impactVFXoffset );
			impactVFX.Play();
			GameObject.Destroy( impactVFX, 2f );
		}
		else
		{
			Debug.LogWarning("PlayerVisuals-error: playImpactVFX called but impactVFXPrefab is null. Ignoring." );
		}
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
		//temporary - clean-up after
		if( selectedHero.skinPrefabName == "Hero_prefab" || selectedHero.skinPrefabName == "Heroine_prefab" ) GetComponent<PlayerControl>().hasOmniToolAnimation = false;

		//Load the voice overs for that hero
		GetComponent<PlayerVoiceOvers>().initializeVoiceOvers( selectedHero.name );
		Debug.Log("PlayerVisuals-OnPhotonInstantiate-Hero name: " + selectedHero.name + " Hero Index " + (int)info.sender.CustomProperties["Hero"] + " isMasterClient: " + PhotonNetwork.isMasterClient + " Name: " + info.sender.NickName );
		if ( PhotonNetwork.isMasterClient )
		{
			object[] data = new object[1];
			data[0] = this.photonView.viewID;
		    GameObject heroSkin = (GameObject)PhotonNetwork.InstantiateSceneObject (selectedHero.skinPrefabName, Vector3.zero, Quaternion.identity, 0, data);
			heroSkin.name = "Hero Skin";
			Animator anim = gameObject.GetComponent<Animator>();
			heroSkin.transform.SetParent( transform, false );
			heroSkin.transform.localPosition = Vector3.zero;
			heroSkin.transform.localRotation = Quaternion.identity;
			anim.avatar = heroSkin.GetComponent<PlayerSkinInfo>().animatorAvatar;
			if( GetComponent<Ragdoll>() != null ) GetComponent<Ragdoll>().initializeRagdoll ( anim, heroSkin.GetComponent<PlayerSkinInfo>().ragdollRigidBodyParent, GetComponent<CapsuleCollider>() );
			anim.runtimeAnimatorController = heroSkin.GetComponent<PlayerSkinInfo>().runtimeAnimatorController;

			//For lip-sync
			GetComponent<PlayerVoiceOvers>().setLipSyncComponent( heroSkin.GetComponent<PlayerSkinInfo>().headSalsa3D );
			//We want to use the PlayerVoiceOvers's AudioSource and not the AudioSource component that gets created when you attach a Salsa3D component to the head.
			//If we use the Salsa3D AudioSource component, its world position remains at 0,0,0 and therefore the head-to-AudioListener distance soons becomes greater than the sound range and we don't hear
			//the voice over anymore.
			if( heroSkin.GetComponent<PlayerSkinInfo>().headSalsa3D != null ) heroSkin.GetComponent<PlayerSkinInfo>().headSalsa3D.audioSrc = GetComponent<PlayerVoiceOvers>().voiceOverAudioSource;

			anim.Rebind(); //Important
			enableNightEmission( heroSkin );
			//For debugging only
			if( Debug.isDebugBuild && GameManager.Instance.playerDebugConfiguration.getAutoPilot() && photonView.isMine ) gameObject.AddComponent<HyperFocus>();
		}
		//Register with the minimap, voiceOverManager and the turn-ribbon
		if( photonView.isMine )
		{
			MiniMap.Instance.registerLocalPlayer( transform );
			MiniMap.Instance.registerRadarObject( gameObject, selectedHero.minimapIcon, GetComponent<PlayerControl>() );
			VoiceOverManager.Instance.registerLocalPlayer( transform );
			HUDMultiplayer.hudMultiplayer.registerLocalPlayer( transform );
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
		//temporary - clean-up after
		if( botHero.skinPrefabName == "Hero_prefab" || botHero.skinPrefabName == "Heroine_prefab" ) GetComponent<PlayerControl>().hasOmniToolAnimation = false;

		//Load the voice overs for that hero
		GetComponent<PlayerVoiceOvers>().initializeVoiceOvers( botHero.name );

		//Name our game object
		gameObject.name = botHero.userName;

		//Create the skin for the bot and set up the animator
		if ( PhotonNetwork.isMasterClient )
		{
			object[] stuff = new object[1];
			stuff[0] = info.sender.NickName;
		    GameObject heroSkin = (GameObject)PhotonNetwork.InstantiateSceneObject (botHero.skinPrefabName, Vector3.zero, Quaternion.identity, 0, stuff);
			heroSkin.name = "Hero Skin";
			Animator anim = gameObject.GetComponent<Animator>();
			heroSkin.transform.SetParent( transform, false );
			heroSkin.transform.localPosition = Vector3.zero;
			heroSkin.transform.localRotation = Quaternion.identity;
			anim.avatar = heroSkin.GetComponent<PlayerSkinInfo>().animatorAvatar;
			if( GetComponent<Ragdoll>() != null ) GetComponent<Ragdoll>().initializeRagdoll ( anim, heroSkin.GetComponent<PlayerSkinInfo>().ragdollRigidBodyParent, GetComponent<CapsuleCollider>() );
			anim.runtimeAnimatorController = heroSkin.GetComponent<PlayerSkinInfo>().runtimeAnimatorController;

			//For lip-sync
			GetComponent<PlayerVoiceOvers>().setLipSyncComponent( heroSkin.GetComponent<PlayerSkinInfo>().headSalsa3D );
			//We want to use the PlayerVoiceOvers's AudioSource and not the AudioSource component that gets created when you attach a Salsa3D component to the head.
			//If we use the Salsa3D AudioSource component, its world position remains at 0,0,0 and therefore the head-to-AudioListener distance soons becomes greater than the sound range and we don't hear
			//the voice over anymore.
			if( heroSkin.GetComponent<PlayerSkinInfo>().headSalsa3D != null ) heroSkin.GetComponent<PlayerSkinInfo>().headSalsa3D.audioSrc = GetComponent<PlayerVoiceOvers>().voiceOverAudioSource;

			anim.Rebind(); //Important
			enableNightEmission( heroSkin );
		}
		//Register with the minimap
		MiniMap.Instance.registerRadarObject( gameObject, botHero.minimapIcon, GetComponent<PlayerControl>() );
	}

	//When playing in a nightime level, the character can be a bit dark so we add emission to brighten the character.
	void enableNightEmission( GameObject heroSkin )
	{
		if( LevelManager.Instance.getSelectedCircuit().sunType == SunType.Sky_city_night )
		{
			SkinnedMeshRenderer[] skinnedMeshRenderers = heroSkin.GetComponentsInChildren<SkinnedMeshRenderer>();
			for( int i = 0; i < skinnedMeshRenderers.Length; i++ )
			{
				Material[] materials = skinnedMeshRenderers[i].materials;
				for( int j = 0; j < materials.Length; j++ )
				{
					if( !materials[j].name.Contains("omnitool") ) materials[j].SetColor( "_EmissionColor", new Color( nightEmission, nightEmission, nightEmission ) );
				}
			}
		}
	}

}
