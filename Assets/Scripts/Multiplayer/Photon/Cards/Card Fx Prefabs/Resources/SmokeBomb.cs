using UnityEngine;
using System.Collections;

/// <summary>
/// Smoke Bomb.
/// </summary>
public class SmokeBomb : CardSpawnedObject {
	
	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		//Note that the Smoke child object is disabled.
		//We will enable it when the card gets activated by the lockstep manager.
		//This will cause both the particle system and audio to play.
		LockstepManager.LockstepAction lsa = new LockstepManager.LockstepAction( LockstepActionType.CARD, gameObject, CardName.Smoke_Bomb );
		lsa.cardSpawnedObject = this;
		LockstepManager.Instance.addActionToQueue( lsa );
	}

	public override void activateCard()
	{
		StartCoroutine( activate() );
	}

	IEnumerator activate()
	{
		//Separate the smoke from the canister.
		Transform smoke = transform.Find("Smoke");
		smoke.SetParent( null, true );

		//Activate the smoke and hiss sound by activating the object
		smoke.gameObject.SetActive( true );

		//Position the smoke in the center of the tile if possible.
		positionSpecifiedObject( smoke, 0 );

		//Register the smoke on the minimap
		MiniMap.Instance.registerRadarObject( smoke.gameObject, minimapIcon );

		object[] data = this.gameObject.GetPhotonView ().instantiationData;
		float duration = (float) data[0];
		yield return new WaitForSeconds( duration );

		//Smoke bomb has expired.
		smoke.GetComponentInChildren<ParticleSystem>().Stop( true );
		//Give time for the smoke to dissipate before destroying object
		yield return new WaitForSeconds(2.2f);
		GameObject.Destroy( gameObject );
		GameObject.Destroy( smoke.gameObject );
	}

}