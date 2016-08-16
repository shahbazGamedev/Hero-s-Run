using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Projectile : MonoBehaviour {

	public Light fireLight;
	public ParticleSystem fireParticleSystem;
	public ParticleSystem impactParticleSystem;
	public AudioClip inFlightSound;
	public AudioClip collisionSound;
	PlayerController playerController;
	PowerUpManager powerUpManager;

	void Start()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		playerController = player.GetComponent<PlayerController>();
		powerUpManager = GameObject.FindGameObjectWithTag("PowerUpManager").GetComponent<PowerUpManager>();
	}

	public void launchProjectile()
	{
		GetComponent<AudioSource>().clip = inFlightSound;
		GetComponent<AudioSource>().Play();
		if( fireLight != null ) fireLight.enabled = true;
		if( fireParticleSystem != null ) fireParticleSystem.gameObject.SetActive(true);
	}

	public void OnCollisionEnter(Collision collision)
	{
	    GetComponent<Rigidbody>().velocity = Vector3.zero;
	    GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
	    GetComponent<Rigidbody>().Sleep();
		//Play collision sound
		GetComponent<AudioSource>().clip = collisionSound;
		GetComponent<AudioSource>().Play();
		if( fireLight != null ) fireLight.enabled = false;
		if( fireParticleSystem != null ) fireParticleSystem.gameObject.SetActive(false);
		if( impactParticleSystem != null )
		{
			impactParticleSystem.transform.SetParent( null );
			impactParticleSystem.gameObject.SetActive(true);
		}
		gameObject.SetActive( false );
		if( collision.gameObject.name == "Hero" )
		{
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
