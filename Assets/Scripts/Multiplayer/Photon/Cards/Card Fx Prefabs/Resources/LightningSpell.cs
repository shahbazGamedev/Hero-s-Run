using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightningSpell : Photon.PunBehaviour {
	
	[SerializeField] Sprite  minimapIcon;


	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );
		GameObject.Destroy( gameObject, 3f );
	}

}

