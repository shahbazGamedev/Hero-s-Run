using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TripMine : CardSpawnedObject {
	
	void OnTriggerEnter(Collider other)
	{
		if( other.gameObject.CompareTag("Player") && other.gameObject.name != casterName )
		{
			destroyAllTargetsWithinBlastRadius( 10f, true );
		}
	}

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = this.gameObject.GetPhotonView ().instantiationData;

		casterName = data[0].ToString();

		float delayBeforeSpellExpires = (float) data[1];
		GameObject.Destroy( gameObject, delayBeforeSpellExpires );

		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position the trip mine flush with the ground and try to center it in the middle of the road if possible.
		positionSpawnedObject( 0 );
	}

}