using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DemonManager : BaseClass {


	Transform player;
	PlayerController playerController;
	public const int NUMBER_STARS_PER_DEMON = 20;

	// Use this for initialization
	void Awake ()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = player.gameObject.GetComponent<PlayerController>();
	}

	public void knockbackDemons( float impactDiameter )
	{
		int DemonLayer = 12;
		int DemonMask = 1 << DemonLayer;
		//Use a sphere that starts impactDiameter/2 meters in front of the player
		Vector3 relativePos = new Vector3(0f , 0f , impactDiameter/2f );
		Vector3 exactPos = player.TransformPoint(relativePos);

		//Count the number of demons that are knocked back so we can give the player stars.
		//The more demons he topples, the more stars he gets.
		int numberOfDemons = 0;

		Collider[] hitColliders = Physics.OverlapSphere(exactPos, impactDiameter, DemonMask );
		for( int i =0; i < hitColliders.Length; i++ )
		{
			DemonController demonController = hitColliders[i].GetComponent<DemonController>();
			if( demonController.getDemonState() != DemonController.DemonState.Dying )
			{
				demonController.knockbackDemon();
				numberOfDemons++;
			}
		}
		if( numberOfDemons > 0 )
		{
			int totalStars = numberOfDemons * NUMBER_STARS_PER_DEMON;
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
		knockbackDemons( impactDiameter );
	}

	void ResurrectionBegin()
	{
		DemonController[] allDemonControllers = playerController.currentTile.GetComponentsInChildren<DemonController>();
		for( int i = 0; i < allDemonControllers.Length; i++ )
		{
			allDemonControllers[i].resetDemon();
		}
		//And for good measure, any other that are too close but maybe not on the current tile
		resetDemons( 54f );

	}

	void resetDemons( float resetDiameter )
	{
		int DemonLayer = 11;
		int DemonMask = 1 << DemonLayer;
		//Use a sphere that starts resetDiameter/2 meters in front of the player
		Vector3 relativePos = new Vector3(0f , 0f , resetDiameter/2f );
		Vector3 exactPos = player.TransformPoint(relativePos);

		Collider[] hitColliders = Physics.OverlapSphere(exactPos, resetDiameter, DemonMask );
		for( int i =0; i < hitColliders.Length; i++ )
		{
			DemonController demonController = hitColliders[i].GetComponent<DemonController>();
			if( demonController.getDemonState() != DemonController.DemonState.Dying )
			{
				demonController.resetDemon();
			}
		}
	}

}
