using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MultiplayerProjectile : MonoBehaviour {

	public Light fireLight;
	public ParticleSystem fireParticleSystem;
	public ParticleSystem impactParticleSystem;
	public AudioClip inFlightSound;
	public AudioClip collisionSound;
	public float bolt_force = 1000f;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = gameObject.GetPhotonView ().instantiationData;
		launchProjectile((Vector3) data[0]);
	}

	void launchProjectile( Vector3 direction )
	{
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<Rigidbody>().AddForce( direction.normalized * bolt_force );
		GameObject.Destroy( gameObject, 10f );
		GetComponent<AudioSource>().clip = inFlightSound;
		GetComponent<AudioSource>().Play();
		if( fireLight != null ) fireLight.enabled = true;
		if( fireParticleSystem != null ) fireParticleSystem.gameObject.SetActive(true);
	}

	void OnCollisionEnter(Collision collision)
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
			GameObject.Destroy( impactParticleSystem, 5f );
		}
		if( GetComponent<MeshRenderer>() != null ) GetComponent<MeshRenderer>().enabled = false;
		if( collision.gameObject.CompareTag("Player") )
		{
			Debug.Log("Sentry Projectile hit target: " + collision.gameObject.name );
			//The explosion knocked down a player. Send him an RPC.
			if( getDotProduct( collision.transform, transform.position ) )
			{
				//Explosion is in front of player. He falls backward.
				collision.gameObject.GetComponent<PhotonView>().RPC("playerDied", PhotonTargets.All, DeathType.Obstacle );
			}
			else
			{
				//Explosion is behind player. He falls forward.
				collision.gameObject.GetComponent<PhotonView>().RPC("playerDied", PhotonTargets.All, DeathType.FallForward );
			}
		}
  	}

	/// <summary>
	/// Gets the dot product.
	/// </summary>
	/// <returns><c>true</c>, if the explosion is in front of the player, <c>false</c> otherwise.</returns>
	/// <param name="player">Player.</param>
	/// <param name="explosionLocation">Explosion location.</param>
	bool getDotProduct( Transform player, Vector3 explosionLocation )
	{
		Vector3 forward = player.TransformDirection(Vector3.forward);
		Vector3 toOther = explosionLocation - player.position;
		if (Vector3.Dot(forward, toOther) < 0)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

}
