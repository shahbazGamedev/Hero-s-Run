using UnityEngine;
using System.Collections;

public class ForceField : CardSpawnedObject {
	
	[SerializeField] ParticleSystem activeForceFieldFx;
	[SerializeField] AudioClip collisionSound;
	const float FORCE_FIELD_WIDTH = 8f;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		//Note that the Force Field prefab has its MeshRenderer and BoxCollider disabled.
		//We will enable them when the card gets activated by the lockstep manager.
		LockstepManager.LockstepAction lsa = new LockstepManager.LockstepAction( LockstepActionType.CARD, gameObject, CardName.Force_Field );
		lsa.cardSpawnedObject = this;
		LockstepManager.Instance.addActionToQueue( lsa );
	}

	public override void activateCard()
	{
		//Read the data
		object[] data = gameObject.GetPhotonView ().instantiationData;
		
		casterTransform = getPlayerByViewID( (int) data[0] );
		if( casterTransform == null )
		{
			Debug.LogError("ForceField-activateCard error casterTransform is null for " + name );
		}
		else
		{
			Debug.Log("ForceField-activateCard casterTransform name is " + casterTransform + " for " + name );
		}
		setCasterName( casterTransform.name );

		//Destroy the force field when it expires
		float duration = (float) data[1];
		Invoke( "scaleDownForceField", duration );

		//Adjust the height
		float height = (float) data[2];
		transform.localScale = new Vector3( 0.1f, height, 1f );

		//Display the force field icon on the minimap
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position it flush with the ground and try to center it in the middle of the road if possible.
		positionSpawnedObject( height * 0.5f );

		//We can now make the force field visible and collidable
		GetComponent<MeshRenderer>().enabled = true;
		GetComponent<BoxCollider>().enabled = true;
		LeanTween.scaleX( gameObject, FORCE_FIELD_WIDTH, 0.33f ).setEaseOutQuart();

		setSpawnedObjectState(SpawnedObjectState.Functioning);

		//Play looping particle effect
		activeForceFieldFx.Play();

		//Play energy sound loop
		GetComponent<AudioSource>().Play();
	}

	void scaleDownForceField()
	{
		//Stop looping particle effect
		activeForceFieldFx.Stop();

		//Stop the energy sound loop
		GetComponent<AudioSource>().Stop();

		//Scale down the force field
		LeanTween.scaleX( gameObject, 0, 0.33f ).setEaseOutQuart().setOnComplete( destroyForceField ).setOnCompleteParam( gameObject );
	}

	void destroyForceField()
	{
		GameObject.Destroy( gameObject );
	}

	void OnTriggerEnter(Collider other)
	{
		//Play collision sound
		GetComponent<AudioSource>().PlayOneShot( collisionSound );

		if( GameManager.Instance.isCoopPlayMode() )
		{
	 		if( other.CompareTag("Zombie") )
			{
				SkillBonusHandler.Instance.grantScoreBonus( 25, "COOP_SCORE_BONUS_FORCE_FIELD", casterTransform );
				ICreature creatureController = other.GetComponent<ICreature>();
				creatureController.knockback( casterTransform, false );
			}
		}
 		else
		{
			if( other.CompareTag("Player") )
			{
				if( other.name != casterName )
				{
					if( casterTransform == null )
					{
						Debug.LogError("ForceField-OnTriggerEnter error casterTransform is null for " + name );
					}
					else
					{
						Debug.Log("ForceField-OnTriggerEnter casterTransform name is " + casterTransform + " for " + name );
					}
					addSkillBonus( 25, "SKILL_BONUS_FORCE_FIELD" );
					//Player ran into force field. He falls backwards.
					other.GetComponent<PlayerControl>().killPlayer( DeathType.Obstacle );
				}
			}
		}
 	}
}