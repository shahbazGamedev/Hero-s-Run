using UnityEngine;
using System.Collections;

public class Firewall : CardSpawnedObject {
	
	int flameDamage;
	const float DELAY_BEFORE_DESTROYING = 2.2f;

	[SerializeField] AudioClip onFlameContact;
	[SerializeField] GameObject flames;

	void OnTriggerEnter(Collider other)
	{
		if( GameManager.Instance.isCoopPlayMode() )
		{
			if( other.CompareTag("Zombie") )
			{
				GetComponent<AudioSource>().PlayOneShot(onFlameContact);
				ICreature creatureController = other.GetComponent<Collider>().GetComponent<ICreature>();
				creatureController.knockback( casterTransform, false );
				SkillBonusHandler.Instance.grantScoreBonus( 25, "COOP_SCORE_BONUS_FIREWALL", casterTransform );
			}
		}
 		else
		{
			if( other.CompareTag("Player") )
			{
				if( other.name != casterName )
				{
					GetComponent<AudioSource>().PlayOneShot(onFlameContact);
					other.GetComponent<PlayerHealth>().deductHealth( flameDamage, casterTransform.GetComponent<PlayerControl>() );
					addSkillBonus( 25, "SKILL_BONUS_FIREWALL" );
				}
			}
		}
	}

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		//Note that the Flames game object prefab and the box collider are disabled.
		//They will be enabled when the card gets activated by the lockstep manager.
		LockstepManager.LockstepAction lsa = new LockstepManager.LockstepAction( LockstepActionType.CARD, gameObject, CardName.Firewall );
		lsa.cardSpawnedObject = this;
		LockstepManager.Instance.addActionToQueue( lsa );
	}

	public override void activateCard()
	{
		StartCoroutine( activate() );
	}

	IEnumerator activate()
	{
		object[] data = gameObject.GetPhotonView ().instantiationData;

		casterTransform = getPlayerByViewID( (int) data[0] );
		setCasterName( casterTransform.name );

		float delayBeforeSpellExpires = (float) data[1];

		flameDamage = (int) data[2];

		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position the fire wall flush with the ground and try to center it in the middle of the road if possible.
		positionSpawnedObject();

		GetComponent<BoxCollider>().enabled = true;

		flames.SetActive( true );

		yield return new WaitForSeconds( delayBeforeSpellExpires );

		//Firewall has expired.
		GetComponent<BoxCollider>().enabled = false;
		flames.GetComponentInChildren<ParticleSystem>().Stop( true );
		//The flames plays the looping fire sound.
		//Reduce the volume
		StartCoroutine( dimVolume( flames.GetComponent<AudioSource>(), DELAY_BEFORE_DESTROYING ) );
		//Give time for the flames to dissipate before destroying object
		yield return new WaitForSeconds(DELAY_BEFORE_DESTROYING);
		GameObject.Destroy( gameObject );
	}
}
