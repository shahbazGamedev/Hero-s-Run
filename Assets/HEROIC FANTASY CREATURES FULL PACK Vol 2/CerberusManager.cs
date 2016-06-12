using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CerberusManager : BaseClass {


	Transform player;
	PlayerController playerController;
	public const int NUMBER_STARS_PER_CERBERUS = 20;

	// Use this for initialization
	void Awake ()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = player.gameObject.GetComponent<PlayerController>();
	}

	public void knockbackCerberuss( float impactDiameter )
	{
		int CerberusLayer = 12;
		int CerberusMask = 1 << CerberusLayer;
		//Use a sphere that starts impactDiameter/2 meters in front of the player
		Vector3 relativePos = new Vector3(0f , 0f , impactDiameter/2f );
		Vector3 exactPos = player.TransformPoint(relativePos);

		//Count the number of cerberuss that are knocked back so we can give the player stars.
		//The more cerberuss he topples, the more stars he gets.
		int numberOfCerberus = 0;

		Collider[] hitColliders = Physics.OverlapSphere(exactPos, impactDiameter, CerberusMask );
		for( int i =0; i < hitColliders.Length; i++ )
		{
			CerberusController cerberusController = hitColliders[i].GetComponent<CerberusController>();
			if( cerberusController.getCerberusState() != CerberusController.CerberusState.Dying )
			{
				cerberusController.knockbackCerberus();
				numberOfCerberus++;
			}
		}
		if( numberOfCerberus > 0 )
		{
			int totalStars = numberOfCerberus * NUMBER_STARS_PER_CERBERUS;
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
		knockbackCerberuss( impactDiameter );
	}

	void ResurrectionBegin()
	{
		CerberusController[] allCerberusControllers = playerController.currentTile.GetComponentsInChildren<CerberusController>();
		for( int i = 0; i < allCerberusControllers.Length; i++ )
		{
			allCerberusControllers[i].resetCerberus();
		}
		//And for good measure, any other that are too close but maybe not on the current tile
		resetCerberuss( 54f );

	}

	void resetCerberuss( float resetDiameter )
	{
		int CerberusLayer = 11;
		int CerberusMask = 1 << CerberusLayer;
		//Use a sphere that starts resetDiameter/2 meters in front of the player
		Vector3 relativePos = new Vector3(0f , 0f , resetDiameter/2f );
		Vector3 exactPos = player.TransformPoint(relativePos);

		Collider[] hitColliders = Physics.OverlapSphere(exactPos, resetDiameter, CerberusMask );
		for( int i =0; i < hitColliders.Length; i++ )
		{
			CerberusController cerberusController = hitColliders[i].GetComponent<CerberusController>();
			if( cerberusController.getCerberusState() != CerberusController.CerberusState.Dying )
			{
				cerberusController.resetCerberus();
			}
		}
	}

}
