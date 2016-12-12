using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CreatureState {
	Reserved = -1,
	Available = 0,
	Idle = 1,
	Walking = 2,
	Running = 3,
	Crawling = 4,
	BurrowUp = 5,
	StandUpFromBack = 6,
	Attacking = 7,
	Dying = 8,
	Victory = 9,
	Jumping = 10
}

public sealed class CreatureManager : BaseClass {

	Transform player;
	PlayerController playerController;
	public const int NUMBER_STARS_PER_CREATURE = 20;
	const float DEACTIVATE_DIAMETER = 60f;
	int zombieLayer = 9;
	int goblinLayer = 11;
	int demonLayer = 12;
	int cerberusLayer = 13;
	int wraithLayer = 14;
	int skeletonLayer = 15;

	// Use this for initialization
	void Awake ()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = player.GetComponent<PlayerController>();
	}

	int getKnockbackCreatureMask()
	{
		int mask = 1 << zombieLayer;
		mask |= 1 << goblinLayer;
 		mask |= 1 << demonLayer;
		mask |= 1 << cerberusLayer;
		mask |= 1 << wraithLayer;
		mask |= 1 << skeletonLayer;
		return mask;
	}

	public void knockbackCreatures( float impactDiameter )
	{
		//Use a sphere that starts impactDiameter/2 meters in front of the player
		Vector3 relativePos = new Vector3(0f , 0f , impactDiameter/2f );
		Vector3 exactPos = player.TransformPoint(relativePos);

		//Count the number of creatures that are knocked back so we can give the player stars.
		//The more creatures he topples, the more stars he gets.
		int numberOfCreatures = 0;
		Collider[] hitColliders = Physics.OverlapSphere(exactPos, impactDiameter, getKnockbackCreatureMask() );
		for( int i =0; i < hitColliders.Length; i++ )
		{
			ICreature creatureController = hitColliders[i].GetComponent<ICreature>();
			if( creatureController.getCreatureState() != CreatureState.Dying )
			{
				creatureController.knockback();
				numberOfCreatures++;
			}
		}
		if( numberOfCreatures != 0 )
		{
			int totalStars = numberOfCreatures * NUMBER_STARS_PER_CREATURE;
			//Give stars
			PlayerStatsManager.Instance.modifyCurrentCoins( totalStars, true, false );
			
			//Display star total picked up icon
			HUDHandler.hudHandler.displayStarPickup( totalStars, Color.magenta );

		}
	}
	
	void OnEnable()
	{
		PowerUpManager.zNukeExploded += ZNukeExploded;
		PlayerController.resurrectionBegin += ResurrectionBegin;
	}
	
	void OnDisable()
	{
		PowerUpManager.zNukeExploded -= ZNukeExploded;
		PlayerController.resurrectionBegin -= ResurrectionBegin;
	}

	void ZNukeExploded( float impactDiameter )
	{
		knockbackCreatures( impactDiameter );
	}

	void ResurrectionBegin()
	{
		ICreature[] allCreatureControllers = playerController.currentTile.GetComponentsInChildren<ICreature>();
		for( int i = 0; i < allCreatureControllers.Length; i++ )
		{
			allCreatureControllers[i].deactivate();
		}
		//And for good measure, any other that are too close but maybe not on the current tile
		deactivateCreatures( DEACTIVATE_DIAMETER );

	}

	//Note that the ZombieManager class, handles reseting zombies.
	int getResetCreatureMask()
	{
		int mask = 1 << goblinLayer;
 		mask |= 1 << demonLayer;
		mask |= 1 << cerberusLayer;
		mask |= 1 << wraithLayer;
		mask |= 1 << skeletonLayer;
		return mask;
	}

	void deactivateCreatures( float resetDiameter )
	{
		//Use a sphere that starts resetDiameter/2 meters in front of the player
		Vector3 relativePos = new Vector3(0f , 0f , resetDiameter/2f );
		Vector3 exactPos = player.TransformPoint(relativePos);

		Collider[] hitColliders = Physics.OverlapSphere(exactPos, resetDiameter, getResetCreatureMask() );
		for( int i =0; i < hitColliders.Length; i++ )
		{
			ICreature creatureController = hitColliders[i].GetComponent<ICreature>();
			if( creatureController.getCreatureState() != CreatureState.Dying )
			{
				creatureController.deactivate();
			}
		}
	}

}
