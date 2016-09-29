using UnityEngine;
using System.Collections;

public class TrapSpikes : MonoBehaviour {

	public AnimationClip open;
	PlayerController playerController;
	PowerUpManager powerUpManager;

	// Use this for initialization
	void Start ()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		playerController = player.gameObject.GetComponent<PlayerController>();
		GameObject powerUpManagerObject = GameObject.FindGameObjectWithTag("PowerUpManager");
		powerUpManager = powerUpManagerObject.GetComponent<PowerUpManager>();
	}
	
	
	void playAnim( AnimationClip clip )
	{
		if( GetComponent<Animation>() != null )
		{
			GetComponent<Animation>()[clip.name].wrapMode = WrapMode.Once;
			GetComponent<Animation>()[clip.name].speed = 1.4f;
			GetComponent<Animation>().Play(clip.name);
		}
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
		if( other.name == "Hero" )
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
		print ("Player trigger spike trap "  );
		GetComponent<AudioSource>().Play();
		playerController.managePlayerDeath(DeathType.Trap);
		if( GetComponent<Animation>() != null )
		{
			playAnim(open);
		}
		else
		{
			GetComponent<Animator>().SetTrigger("open");
		}
	}
	
	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Paused )
		{
			if( GetComponent<Animation>() != null )
			{
				GetComponent<Animation>().enabled = false;
			}
			else
			{
				GetComponent<Animator>().enabled = false;
			}
			
		}
		else if( newState == GameState.Normal )
		{
			if( GetComponent<Animation>() != null )
			{
				GetComponent<Animation>().enabled = true;
			}
			else
			{
				GetComponent<Animator>().enabled = true;
			}
		}
	}
}
