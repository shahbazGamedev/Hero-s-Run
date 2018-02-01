using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Grenade.
/// </summary>
public class Grenade : CardSpawnedObject {
	
	[SerializeField] ParticleSystem explosionEffect;
	[SerializeField] GameObject burntGroundDecal;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		//Note that the Grenade prefab has its MeshRenderer disabled.
		//We will enable it when the card gets activated by the lockstep manager.
		LockstepManager.LockstepAction lsa = new LockstepManager.LockstepAction( LockstepActionType.CARD, gameObject, CardName.Grenade );
		lsa.cardSpawnedObject = this;
		LockstepManager.Instance.addActionToQueue( lsa );
	}

	public override void activateCard()
	{
		object[] data = this.gameObject.GetPhotonView ().instantiationData;

		int casterViewId = (int) data[0];
		casterTransform = getPlayerByViewID( casterViewId );
		casterName = casterTransform.name;

		//We can now make the Grenade visible
		GetComponent<MeshRenderer>().enabled = true;

		StartCoroutine( startDetonationCountdown( (float) data[1] ) );
	}

	IEnumerator startDetonationCountdown( float blastRadius )
	{
		GetComponent<AudioSource>().Play();
		//the bomb beeps lasts 0.94 seconds
		yield return new WaitForSeconds(0.94f);

		int numberOfBlastVictims = destroyAllTargetsWithinBlastRadius( blastRadius, MaskHandler.getMaskAll(), casterTransform );
		SkillBonusHandler.Instance.grantComboScoreBonus( ZombieController.SCORE_PER_KNOCKBACK, "COOP_SCORE_BONUS_COMBO_ZOMBIE", casterTransform, numberOfBlastVictims );
		explode();
		GameObject.Destroy( gameObject );
	}

	void explode()
	{
		//Place a burnt decal on the ground underneath the grenade.
		Vector3 decalPosition = transform.TransformPoint( new Vector3(0,1f,0) );
		spawnDecalOnTheGround( burntGroundDecal, decalPosition, burntGroundDecal.transform.rotation, 8f );

		ParticleSystem effect = GameObject.Instantiate( explosionEffect, transform.position, transform.rotation );
		effect.GetComponent<AudioSource>().Play ();
		effect.Play();
		//Destroy the particle effect after a few seconds
		GameObject.Destroy( effect.gameObject, 3f );
	}

}