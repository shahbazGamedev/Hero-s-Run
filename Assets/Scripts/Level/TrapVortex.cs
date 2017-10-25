using UnityEngine;
using System.Collections;

public class TrapVortex : MonoBehaviour {

	PlayerController playerController;
	PowerUpManager powerUpManager;
	public ParticleSystem waterSplash;
	public static float delayBeforeBeingPulledDown = 0.5f;
	public static float timeRequiredToGoDown = 3.5f;
	public static float distanceTravelledDown = 3.5f;

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
		if( other.CompareTag("Player") )
		{
			//Is the player protected by a Shield Power Up?
			if( PowerUpManager.isThisPowerUpActive( PowerUpType.Shield ) )
			{
				//This Power Up only works one time, so deactivate it
				powerUpManager.deactivatePowerUp( PowerUpType.Shield, false );
			}
			else
			{
				triggerTrap();
			}
		}
	}

	void triggerTrap()
	{
		print ("Player trigger vortex trap "  );
		Invoke("playDeathSpurt", 0.4f); 
		playerController.managePlayerDeath(DeathType.VortexTrap);
	}

	void playDeathSpurt()
	{
		GetComponent<AudioSource>().Play();
		waterSplash.Play();
	}

}
