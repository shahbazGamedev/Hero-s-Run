using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	public Light fireLight;
	public ParticleSystem fireParticleSystem;
	PlayerController playerController;

	void Start()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		playerController = player.GetComponent<PlayerController>();
	}

	public void OnCollisionEnter(Collision collision)
	{
	    Debug.Log("Projectile-OnCollisionEnter with " + collision.gameObject.name);
	    GetComponent<Rigidbody>().velocity = Vector3.zero;
	    GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
	    GetComponent<Rigidbody>().Sleep();
		//Play collision sound
		GetComponent<AudioSource>().Play();
		if( fireLight != null ) fireLight.enabled = false;
		if( fireParticleSystem != null ) fireParticleSystem.gameObject.SetActive(false);
		if( collision.gameObject.name == "Hero" )
		{
			playerController.managePlayerDeath(DeathType.Obstacle);
		}
  	}
}
