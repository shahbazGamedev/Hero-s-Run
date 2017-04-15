using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Glyph : CardSpawnedObject {
	
	void OnTriggerEnter(Collider other)
	{
		if( other.gameObject.CompareTag("Player") && other.gameObject.name != casterName )
		{
			other.GetComponent<PlayerControl>().managePlayerDeath ( DeathType.FallForward );
		}
	}

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = this.gameObject.GetPhotonView ().instantiationData;

		casterName = data[0].ToString();

		float delayBeforeSpellExpires = (float) data[1];
		GameObject.Destroy( gameObject, delayBeforeSpellExpires );

		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position the glyph flush with the ground and try to center it in the middle of the road if possible.
		positionSpawnedObject( 0.05f );
	}

}