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
			object[] data = this.gameObject.GetPhotonView ().instantiationData;
			print( "OnPhotonInstantiate " + info.sender.NickName + " " + data[0]);
			GameObject myOwner = GameObject.Find(data[0].ToString());
			Animator anim = myOwner.GetComponent<Animator>();
			transform.SetParent( myOwner.transform, false );
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			anim.avatar = GetComponent<PlayerSkinInfo>().animatorAvatar;
			anim.Rebind(); //Important
		}
	}


}
