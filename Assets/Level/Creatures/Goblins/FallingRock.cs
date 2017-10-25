using UnityEngine;
using System.Collections;

public class FallingRock : MonoBehaviour {

	PlayerController playerController;
	PowerUpManager powerUpManager;

	void Start()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		playerController = player.GetComponent<PlayerController>();
		//For power ups
		powerUpManager = GameObject.FindGameObjectWithTag("PowerUpManager").GetComponent<PowerUpManager>();
	}


	public void OnCollisionEnter(Collision collision)
	{
	    //Debug.Log("FallingRock-OnCollisionEnter with " + collision.gameObject.name + " " + collision.contacts[0].normal );
		//Play collision sound
		//GetComponent<AudioSource>().Play();
		if( collision.gameObject.CompareTag("Player") && collision.contacts[0].normal.y <= 0f )
		{
	   		Debug.LogWarning("FallingRock-OnCollisionEnter with " + collision.gameObject.name + " " + collision.contacts[0].normal );
			//Is the player protected by a Shield Power Up?
			if( PowerUpManager.isThisPowerUpActive( PowerUpType.Shield ) )
			{
				//This Power Up only works one time, so deactivate it
				powerUpManager.deactivatePowerUp( PowerUpType.Shield, false );
			}
			else
			{
				playerController.managePlayerDeath ( DeathType.Obstacle );
			}
		}
  	}
}
