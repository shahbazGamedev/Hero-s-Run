using UnityEngine;
using System.Collections;

public class TrapFireStreamsTrigger : MonoBehaviour {

	PlayerController playerController;
	PowerUpManager powerUpManager;
	
	// Use this for initialization
	void Start ()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		playerController = player.GetComponent<PlayerController>();
		GameObject powerUpManagerObject = GameObject.FindGameObjectWithTag("PowerUpManager");
		powerUpManager = powerUpManagerObject.GetComponent<PowerUpManager>();
		
	}
	
	void OnTriggerEnter(Collider other)
	{
		Debug.Log( "OnTriggerEnter TrapFireStreamsTrigger " );
		if( other.name == "Hero" && !CheatManager.Instance.getIsInvincible() )
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
