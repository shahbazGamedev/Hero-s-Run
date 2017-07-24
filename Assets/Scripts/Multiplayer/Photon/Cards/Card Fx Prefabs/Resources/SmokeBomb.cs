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
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );
		GetComponentInChildren<ParticleSystem>().Play( true );
		yield return new WaitForSeconds( duration );
		GetComponentInChildren<ParticleSystem>().Stop( true );
		//Give time for the smoke to dissipate before destroying object
		yield return new WaitForSeconds(2f);
		GameObject.Destroy( gameObject );
	}

}