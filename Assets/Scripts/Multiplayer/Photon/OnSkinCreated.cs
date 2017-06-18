using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSkinCreated : MonoBehaviour
{

	//When the player skin is instantiated, attach it to the player whose name is stored in the data object array.
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
				anim.Rebind(); //Important
			}
			else
			{
				Debug.LogError("OnSkinCreated error: could not find player with Photon view Id of " + viewIdOfOwner );
			}
		}
	}

}
