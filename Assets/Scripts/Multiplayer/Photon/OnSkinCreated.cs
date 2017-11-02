using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrazyMinnow.SALSA;

public class OnSkinCreated : MonoBehaviour
{
	[Tooltip("When playing in a nightime level, the character can be a bit dark so we add emission to brighten the character. Make sure the value is the same in PlayerVisuals and OnSkinCreated.")]
	[Range(0,1)]
	[SerializeField] float nightEmission = 0.33f;

	//When the player skin is instantiated, attach it to the player whose name is stored in the data object array.
	//This is for non-MasterClient players. See PlayerVisuals for the MasterClient players.
	void OnPhotonInstantiate(PhotonMessageInfo info) 
	{
		if ( !PhotonNetwork.isMasterClient )
		{
			object[] data = gameObject.GetPhotonView ().instantiationData;
			GameObject myOwner = null;
			int viewIdOfOwner = (int) data[0];
			GameObject[] playersArray = GameObject.FindGameObjectsWithTag("Player");
			for( int i = 0; i < playersArray.Length; i ++ )
			{
				if( playersArray[i].GetPhotonView().viewID == viewIdOfOwner )
				{
					myOwner = playersArray[i];
					break;
				}
			}
			if( myOwner != null )
			{
				gameObject.name = "Hero Skin";
				Animator anim = myOwner.GetComponent<Animator>();
				transform.SetParent( myOwner.transform, false );
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				anim.avatar = GetComponent<PlayerSkinInfo>().animatorAvatar;
				if( myOwner.GetComponent<Ragdoll>() != null ) myOwner.GetComponent<Ragdoll>().initializeRagdoll ( anim, GetComponent<PlayerSkinInfo>().ragdollRigidBodyParent, myOwner.GetComponent<CapsuleCollider>() );
				anim.runtimeAnimatorController = GetComponent<PlayerSkinInfo>().runtimeAnimatorController;
	
				//For lip-sync
				myOwner.GetComponent<PlayerVoiceOvers>().setLipSyncComponent( GetComponent<PlayerSkinInfo>().headSalsa3D );
				//We want to use the PlayerVoiceOvers's AudioSource and not the AudioSource component that gets created when you attach a Salsa3D component to the head.
				//If we use the Salsa3D AudioSource component, its world position remains at 0,0,0 and therefore the head-to-AudioListener distance soons becomes greater than the sound range and we don't hear
				//the voice over anymore.
				if( GetComponent<PlayerSkinInfo>().headSalsa3D != null ) GetComponent<PlayerSkinInfo>().headSalsa3D.audioSrc = myOwner.GetComponent<PlayerVoiceOvers>().voiceOverAudioSource;

				anim.Rebind(); //Important
				enableNightEmission( gameObject );
				if( Debug.isDebugBuild && GameManager.Instance.playerDebugConfiguration.getAutoPilot() && transform.root.GetComponent<PhotonView>().isMine ) transform.root.gameObject.AddComponent<HyperFocus>();
			}
			else
			{
				Debug.LogError("OnSkinCreated error: could not find player with Photon view Id of " + viewIdOfOwner );
			}
		}
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
