using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Grenade.
/// </summary>
public class Grenade : CardSpawnedObject {
	
	[SerializeField] ParticleSystem explosionEffect;

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
		casterTransform = getCaster( casterViewId );
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
		if( numberOfBlastVictims == 1 )
		{
			SkillBonusHandler.Instance.grantComboScoreBonus( ZombieController.SCORE_PER_KNOCKBACK, "COOP_SCORE_BONUS_TOPPLED_ZOMBIE", casterTransform, numberOfBlastVictims );
		}
		else if( numberOfBlastVictims > 1 )
		{
			SkillBonusHandler.Instance.grantComboScoreBonus( ZombieController.SCORE_PER_KNOCKBACK, "COOP_SCORE_BONUS_COMBO_ZOMBIE", casterTransform, numberOfBlastVictims );
		}
		explode();
		GameObject.Destroy( gameObject );
	}

	void explode()
	{
		ParticleSystem effect = GameObject.Instantiate( explosionEffect, transform.position, transform.rotation );
		effect.GetComponent<AudioSource>().Play ();
		effect.Play();
		//Destroy the particle effect after a few seconds
		GameObject.Destroy( effect.gameObject, 3f );
	}

}