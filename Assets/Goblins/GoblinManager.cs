using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoblinManager : BaseClass {


	Transform player;
	PlayerController playerController;
	public ParticleSystem zNukeEffect;
	public const int NUMBER_STARS_PER_GOBLIN = 20;

	// Use this for initialization
	void Awake ()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = (PlayerController) player.gameObject.GetComponent(typeof(PlayerController));
	}

	public void knockbackGoblins( float impactDiameter )
	{
		int GoblinLayer = 11;
		int GoblinMask = 1 << GoblinLayer;
		//Use a sphere that starts impactDiameter/2 meters in front of the player
		Vector3 relativePos = new Vector3(0f , 0f , impactDiameter/2f );
		Vector3 exactPos = player.TransformPoint(relativePos);

		zNukeEffect.transform.position = exactPos;
		zNukeEffect.GetComponent<AudioSource>().Play ();
		zNukeEffect.Play();

		//Count the number of goblins that are knocked back so we can give the player stars.
		//The more goblins he topples, the more stars he gets.
		int numberOfGoblins = 0;

		Collider[] hitColliders = Physics.OverlapSphere(exactPos, impactDiameter, GoblinMask );
		for( int i =0; i < hitColliders.Length; i++ )
		{
			GoblinController goblinController = hitColliders[i].GetComponent<GoblinController>();
			if( goblinController.getGoblinState() != GoblinController.GoblinState.Dying )
			{
				goblinController.knockbackGoblin();
				numberOfGoblins++;
			}
		}
		if( numberOfGoblins != 0 )
		{
			int totalStars = numberOfGoblins * NUMBER_STARS_PER_GOBLIN;
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
		knockbackGoblins( impactDiameter );
	}

	void ResurrectionBegin()
	{
		GoblinController[] allGoblinControllers = playerController.currentTile.GetComponentsInChildren<GoblinController>();
		for( int i = 0; i < allGoblinControllers.Length; i++ )
		{
			allGoblinControllers[i].resetGoblin();
		}
	}

}
