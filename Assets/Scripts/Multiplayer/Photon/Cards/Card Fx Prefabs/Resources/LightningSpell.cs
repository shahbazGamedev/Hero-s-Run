using UnityEngine;
using System.Collections;

public class LightningSpell : CardSpawnedObject {
	
	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		if( GameManager.Instance.isCoopPlayMode() )
		{
			//Read the data
			object[] data = gameObject.GetPhotonView ().instantiationData;
			casterTransform = getPlayerByViewID( (int) data[0] );
			setCasterName( casterTransform.name );
			positionSpawnedObject( 4f );
			//Up to 10 targets with a max. random delay of 0.4f per target plus time for the last target to fall down.
			GameObject.Destroy( gameObject, 5f );
		}
		else
		{
			GameObject.Destroy( gameObject, 2.5f );
		}
	}
}

