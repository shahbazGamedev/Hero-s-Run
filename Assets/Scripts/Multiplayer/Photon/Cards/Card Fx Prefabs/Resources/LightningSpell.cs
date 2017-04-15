using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightningSpell : CardSpawnedObject {
	
	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		GameObject.Destroy( gameObject, 2.5f );
	}

}

