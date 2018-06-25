using UnityEngine;
using System.Collections;

/// <summary>
/// Homing missile.
/// The Homing Missile prefab should have an AudioSource component with Play on Awake and Loop set to true. It should have the in-flight audio clip.
/// The Impact particle system should have an AudioSource component with Play on Awake set to true and Loop set to false. It should have the impact audio clip.
/// Put the Rigidbody collision detection to Continuous Dynamic or else the missile will fly right through the player frequently.
/// </summary>
public class HomingMissile : CardSpawnedObject {

	[SerializeField] ParticleSystem inFlightParticleSystem;
	[SerializeField] ParticleSystem impactParticleSystem;
	float missileVelocity = 42f;
	float maxDegreesDelta = 15f;
	Rigidbody homingMissile;
	Transform target;
	float heightAdjustment = 0;
	PlayerControl targetPlayerControl;
	const float LIFESPAN = 30f;

	#region Initialisation
	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		LockstepManager.LockstepAction lsa = new LockstepManager.LockstepAction( LockstepActionType.ACTIVATE_CARD, gameObject, CardName.Homing_Missile );
		lsa.cardSpawnedObject = this;
		LockstepManager.Instance.addActionToQueue( lsa );
	}

	public override void activateCard()
	{
		object[] data = gameObject.GetPhotonView ().instantiationData;
		casterTransform = getPlayerByViewID( (int) data[1] );
		setCasterName( casterTransform.name );

		if( GameManager.Instance.isCoopPlayMode() )
		{
			target = findAffectedCreature( (int) data[0] );
		}
		else
		{
			target = findAffectedPlayer( (int) data[0] );
			if( target != null ) targetPlayerControl = target.GetComponent<PlayerControl>();
		}
		if( target != null )
		{
			heightAdjustment = getHeightAdjusment( target.gameObject );
		 	launchMissile();
		}

		//We want the homing missile to be destroyed if it has not collided with anything after LIFESPAN seconds.
		GameObject.Destroy( gameObject, LIFESPAN );
	}
	#endregion

	#region Creature
	Transform findAffectedCreature( int viewIdOfAffectedCreature ) 
	{
		ZombieController[] zombies = GameObject.FindObjectsOfType<ZombieController>();
		for( int i = 0; i < zombies.Length; i++ )
		{
			if( zombies[i].GetComponent<PhotonView>().viewID == viewIdOfAffectedCreature )
			{
				//If in the short time between the card being cast and the card being activated
				//the creature has died, simply ignore.
				if( zombies[i].getCreatureState() == CreatureState.Dying  )
				{
					return null;
				}
				else
				{
					return zombies[i].transform;
				}
			}
		}
		return null;
	}
	#endregion

	#region Player
	Transform findAffectedPlayer( int viewIdOfAffectedPlayer ) 
	{
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == viewIdOfAffectedPlayer )
			{
				PlayerControl affectedPlayerControl = PlayerRace.players[i].GetComponent<PlayerControl>();

				//If in the short time between the card being cast and the card being activated
				//the player has died, simply ignore.
				if( affectedPlayerControl.getCharacterState() == PlayerCharacterState.Dying || affectedPlayerControl.getCharacterState() == PlayerCharacterState.Idle )
				{
					return null;
				}
				else
				{
					return affectedPlayerControl.transform;
				}
			}
		}
		return null;
	}
	#endregion

	void launchMissile()
	{
		homingMissile = GetComponent<Rigidbody>();
		//if( target != null ) print("Homing Missile target is " + target.name );
		homingMissile.isKinematic = false;
		if( inFlightParticleSystem != null ) inFlightParticleSystem.gameObject.SetActive(true);

		//Add an icon on the minimap
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );
	}

	void LateUpdate()
	{
		if( homingMissile == null ) return;

		//If the player died or is now in the Idle state (because of Stasis for example) or the player now has the Cloak card active, nullify the target
		if( targetPlayerControl != null && ( targetPlayerControl.getCharacterState() == PlayerCharacterState.Dying || targetPlayerControl.getCharacterState() == PlayerCharacterState.Idle || targetPlayerControl.GetComponent<PlayerSpell>().isCardActive(CardName.Cloak) ) )
		{
			target = null;
			targetPlayerControl = null;
		}

		if( target == null )
		{
			//If the target couldn't be found or is no longer present (maybe a player disconnected), just continue straight until you either self-destruct or hit an obstacle.
			homingMissile.velocity = transform.forward * missileVelocity;
		}
		else
		{
			homingMissile.velocity = transform.forward * missileVelocity;
			//Aim for the torso, not the feet
			Quaternion targetRotation = Quaternion.LookRotation( new Vector3( target.position.x, target.position.y + heightAdjustment, target.position.z ) - transform.position ); 
			homingMissile.MoveRotation( Quaternion.RotateTowards( transform.rotation, targetRotation, maxDegreesDelta ) );
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		if( homingMissile == null ) return;
		homingMissile.velocity = Vector3.zero;
		homingMissile = null;

		if( impactParticleSystem != null )
		{
			impactParticleSystem.transform.SetParent( null );
			impactParticleSystem.gameObject.SetActive(true);
		}

		int numberOfBlastVictims = destroyAllTargetsWithinBlastRadius( 10f, MaskHandler.getMaskAllWithoutDevices(), casterTransform );
		SkillBonusHandler.Instance.grantComboScoreBonus( ZombieController.SCORE_PER_KNOCKBACK, "COOP_SCORE_BONUS_COMBO_ZOMBIE", casterTransform, numberOfBlastVictims );

		GameObject.Destroy( gameObject );
		
  	}

}
