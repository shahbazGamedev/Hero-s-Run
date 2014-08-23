using UnityEngine;
using System.Collections;

//Used by the gargoyle that belches flames
//This script should be attached to the collider of the Flame game object (not on the parent gargoyle).
//The collider should be named exactly "Flame".
//Also see PlayerController.
public class TrapFlame : MonoBehaviour {

	//The trap is deactivated temporarily:
	//a) while the player is sliding
	//b) during the grace period after the player died which lasts a few seconds
	public bool isActive = true;
	PlayerController playerController;
	PowerUpManager powerUpManager;

	// Use this for initialization
	void Awake ()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		playerController = player.GetComponent<PlayerController>();
		GameObject powerUpManagerObject = GameObject.FindGameObjectWithTag("PowerUpManager");
		powerUpManager = powerUpManagerObject.GetComponent<PowerUpManager>();

	}
	
	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" && isActive && !CheatManager.Instance.getIsInvincible() )
		{
			//Is the player protected by a Shield Power Up?
			if( PowerUpManager.isThisPowerUpActive( PowerUpType.Shield ) )
			{
				//This Power Up only works one time, so deactivate it
				powerUpManager.deactivatePowerUp( PowerUpType.Shield, false );
			}
			else
			{
				playerController.managePlayerDeath ( DeathType.Flame );

			}
		}
	}

}
