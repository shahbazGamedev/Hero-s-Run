using UnityEngine;
using UnityEngine.UI;
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

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = gameObject.GetPhotonView ().instantiationData;

		casterTransform = getPlayerByViewID( (int) data[0] );
		setCasterName( casterTransform.name );

		//We want the homing missile to self-destruct after a while
		GameObject.Destroy( gameObject, (float) data[1] );

		StartCoroutine( launchMissile() );
	}

	IEnumerator launchMissile()
	{
		yield return new WaitForSeconds(0.35f);

		homingMissile = GetComponent<Rigidbody>();
		target = getNearestTargetWithinRange( Mathf.Infinity, MaskHandler.getMaskWithPlayersWithCreatures(), true );
		if( target != null )
		{
			targetPlayerControl = target.GetComponent<PlayerControl>();
			heightAdjustment = getHeightAdjusment( target.gameObject );
		}
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
		if( numberOfBlastVictims == 1 )
		{
			SkillBonusHandler.Instance.grantComboScoreBonus( ZombieController.SCORE_PER_KNOCKBACK, "COOP_SCORE_BONUS_TOPPLED_ZOMBIE", casterTransform, numberOfBlastVictims );
		}
		else if( numberOfBlastVictims > 1 )
		{
			SkillBonusHandler.Instance.grantComboScoreBonus( ZombieController.SCORE_PER_KNOCKBACK, "COOP_SCORE_BONUS_COMBO_ZOMBIE", casterTransform, numberOfBlastVictims );
		}

		GameObject.Destroy( gameObject );
		
  	}

}
