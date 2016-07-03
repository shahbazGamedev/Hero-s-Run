using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CreatureState {
	Idle = 1,
	Walking = 2,
	Running = 3,
	Attacking = 4,
	Dying = 5,
	Victory = 6,
	Jumping = 7
}

public class CreatureManager : BaseClass {

	Transform player;
	PlayerController playerController;
	public const int NUMBER_STARS_PER_CREATURE = 20;

	// Use this for initialization
	void Awake ()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = (PlayerController) player.gameObject.GetComponent(typeof(PlayerController));
	}

	int getCreatureMask()
	{
		int zombieLayer = 9;
		int goblinLayer = 11;
		int demonLayer = 12;
		int cerberusLayer = 13;
		int mask = 1 << zombieLayer;
		mask |= 1 << goblinLayer;
 		mask |= 1 << demonLayer;
		mask |= 1 << cerberusLayer;
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
		Collider[] hitColliders = Physics.OverlapSphere(exactPos, impactDiameter, getCreatureMask() );
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
			HUDHandler.displayCoinTotal( totalStars, Color.magenta, false );

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
		Debug.LogWarning("ZNukeExploded: impactDiameter: " + impactDiameter );
		knockbackCreatures( impactDiameter );
	}

	void ResurrectionBegin()
	{
		ICreature[] allCreatureControllers = playerController.currentTile.GetComponentsInChildren<ICreature>();
		for( int i = 0; i < allCreatureControllers.Length; i++ )
		{
			allCreatureControllers[i].resetCreature();
		}
		//And for good measure, any other that are too close but maybe not on the current tile
		resetCreatures( 54f );

	}

	void resetCreatures( float resetDiameter )
	{
		//Use a sphere that starts resetDiameter/2 meters in front of the player
		Vector3 relativePos = new Vector3(0f , 0f , resetDiameter/2f );
		Vector3 exactPos = player.TransformPoint(relativePos);

		Collider[] hitColliders = Physics.OverlapSphere(exactPos, resetDiameter, getCreatureMask() );
		for( int i =0; i < hitColliders.Length; i++ )
		{
			ICreature creatureController = hitColliders[i].GetComponent<ICreature>();
			if( creatureController.getCreatureState() != CreatureState.Dying )
			{
				creatureController.resetCreature();
			}
		}
	}

}
