using System.Collections;
using UnityEngine;

public class Shockwave : CardSpawnedObject {

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = this.gameObject.GetPhotonView ().instantiationData;
		int casterViewId = (int) data[0];
		Transform caster = getCaster( casterViewId );
		casterName = caster.name;

		//Do a ripple effect on the minimap
		//To do
		//MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position flush with the ground and try to center it in the middle of the road if possible.
		positionSpawnedObject();

		if( caster != null ) shockwave( caster, (float) data[1] );

	}

	void shockwave( Transform caster, float radius )
	{
		//Shake the camera
		caster.GetComponent<PlayerCamera>().Shake();

		destroyAllTargetsWithinBlastRadius( radius, true );
	}

}
