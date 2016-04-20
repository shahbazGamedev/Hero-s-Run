using UnityEngine;
using System.Collections;

public class TrapVortex : MonoBehaviour {

	PlayerController playerController;
	PowerUpManager powerUpManager;
	public ParticleSystem playerDeathSpurt;
	public GameObject zombieHand;

	// Use this for initialization
	void Start ()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		playerController = player.gameObject.GetComponent<PlayerController>();
		GameObject powerUpManagerObject = GameObject.FindGameObjectWithTag("PowerUpManager");
		powerUpManager = powerUpManagerObject.GetComponent<PowerUpManager>();

	}
	
	
	
	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
	}
	
	void OnTriggerEnter(Collider other)
	{
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
				triggerTrap();
			}
		}
	}

	void triggerTrap()
	{
		print ("Player trigger vortex trap "  );
		Invoke("playDeathSpurt", 1.25f); 
		Invoke("attachZombieHand", 0.15f); 
		playerController.managePlayerDeath(DeathType.VortexTrap);
	}

	void attachZombieHand()
	{
		zombieHand.SetActive(true);
		zombieHand.transform.parent = playerController.transform;
		zombieHand.transform.localPosition = new Vector3( 0f, 0.384f, -0.445f);
	}

	void playDeathSpurt()
	{
		GetComponent<AudioSource>().Play();
		playerDeathSpurt.Play();
	}

	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Paused )
		{
		}
		else if( newState == GameState.Normal )
		{
		}
	}
}
