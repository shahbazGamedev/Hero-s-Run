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
	Jumping = 10,
	Glide = 11
}

public sealed class CreatureManager : MonoBehaviour {

	Transform player;
	PlayerController playerController;
	public const int NUMBER_SOFT_CURRENCY_PER_CREATURE = 20;
	const float DEACTIVATE_DIAMETER = 60f;
	int zombieLayer = 9;
	int goblinLayer = 11;
	int demonLayer = 12;
	int cerberusLayer = 13;
	int wraithLayer = 14;
	int skeletonLayer = 15;

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

		//Count the number of creatures that are knocked back so we can give the player coins.
		//The more creatures he topples, the more coins he gets.
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
			int totalCoins = numberOfCreatures * NUMBER_SOFT_CURRENCY_PER_CREATURE;
			//Give coins
			PlayerStatsManager.Instance.modifyCurrentCoins( totalCoins, true, false );
			
			//Display coin total picked up icon
			HUDHandler.hudHandler.displayCoinPickup( totalCoins );

		}
	}
	
	void OnEnable()
	{
		PowerUpManager.zNukeExploded += ZNukeExploded;
		PlayerController.resurrectionBegin += ResurrectionBegin;
		PlayerController.localPlayerCreated += LocalPlayerCreated;
	}
	
	void OnDisable()
	{
		PowerUpManager.zNukeExploded -= ZNukeExploded;
		PlayerController.resurrectionBegin -= ResurrectionBegin;
		PlayerController.localPlayerCreated -= LocalPlayerCreated;
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

	void LocalPlayerCreated( Transform playerTransform, PlayerController playerController )
	{
		this.playerController = playerController;
		this.player = playerTransform;
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
