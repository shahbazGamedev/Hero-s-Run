using UnityEngine;
using System.Collections;

/// <summary>
/// Smoke Bomb.
/// </summary>
public class SmokeBomb : CardSpawnedObject {
	
	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = this.gameObject.GetPhotonView ().instantiationData;
		StartCoroutine( activate( (float) data[0] ) );
	}

	IEnumerator activate( float duration )
	{
		//Separate the smoke from the canister.
		Transform smoke = transform.FindChild("Smoke");
		smoke.SetParent( null, true );
		//Position the smoke in the center of the tile if possible.
		positionSpecifiedObject( smoke, 0 );
		//Register the smoke on the minimap
		MiniMap.Instance.registerRadarObject( smoke.gameObject, minimapIcon );
		//Start the smoke particle system
		smoke.GetComponentInChildren<ParticleSystem>().Play( true );
		yield return new WaitForSeconds( duration );
		smoke.GetComponentInChildren<ParticleSystem>().Stop( true );
		//Give time for the smoke to dissipate before destroying object
		yield return new WaitForSeconds(2.2f);
		GameObject.Destroy( gameObject );
		GameObject.Destroy( smoke.gameObject );
	}

}