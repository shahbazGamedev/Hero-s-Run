using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrazyMinnow.SALSA;

public class OnSkinCreated : MonoBehaviour
{

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
				myOwner.GetComponent<Ragdoll>().initializeRagdoll ( anim, GetComponent<PlayerSkinInfo>().ragdollRigidBodyParent, myOwner.GetComponent<CapsuleCollider>() );
				anim.runtimeAnimatorController = GetComponent<PlayerSkinInfo>().runtimeAnimatorController;
				//For lip-sync
				myOwner.GetComponent<PlayerVoiceOvers>().setLipSyncComponent( GetComponent<PlayerSkinInfo>().headSalsa3D );
				anim.Rebind(); //Important
				if( Debug.isDebugBuild && GameManager.Instance.playerDebugConfiguration.getAutoPilot() && transform.root.GetComponent<PhotonView>().isMine ) transform.root.gameObject.AddComponent<HyperFocus>();
			}
			else
			{
				Debug.LogError("OnSkinCreated error: could not find player with Photon view Id of " + viewIdOfOwner );
			}
		}
	}

}
