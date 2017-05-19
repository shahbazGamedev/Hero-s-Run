using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HomingMissile : CardSpawnedObject {

	[SerializeField] Light fireLight;
	[SerializeField] ParticleSystem fireParticleSystem;
	[SerializeField] ParticleSystem impactParticleSystem;
	[SerializeField] AudioClip inFlightSound;
	[SerializeField] AudioClip collisionSound;
	float missileVelocity = 40f;
	float turn = 20f;
	Rigidbody homingMissile;
	Transform target;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = gameObject.GetPhotonView ().instantiationData;

		casterName = (string) data[0];

		//We want the homing missile to self-destruct after a while
		GameObject.Destroy( gameObject, (float) data[1] );

		StartCoroutine( launchMissile() );
	}

	IEnumerator launchMissile()
	{
		yield return new WaitForSeconds(0.5f);

		homingMissile = GetComponent<Rigidbody>();
		target = getNearestTargetWithinRange( Mathf.Infinity, MaskHandler.getMaskOnlyPlayer() );
		if( target != null ) print("Homing Missile target is: " + target.name );
		homingMissile.isKinematic = false;
		GetComponent<AudioSource>().clip = inFlightSound;
		GetComponent<AudioSource>().Play();
		if( fireLight != null ) fireLight.enabled = true;
		if( fireParticleSystem != null ) fireParticleSystem.gameObject.SetActive(true);
	}

	void LateUpdate()
	{
		if( target == null || homingMissile == null ) return;

		homingMissile.velocity = transform.forward * missileVelocity;
		//Aim for the head, not the feet
		Quaternion targetRotation = Quaternion.LookRotation( new Vector3( target.position.x, target.position.y + 2f, target.position.z ) - transform.position ); 
		homingMissile.MoveRotation( Quaternion.RotateTowards( transform.rotation, targetRotation, turn ) );
	}

	void OnCollisionEnter(Collision collision)
	{
		homingMissile.velocity = Vector3.zero;
		homingMissile = null;

		//Play collision sound at point of impact
		AudioSource.PlayClipAtPoint( collisionSound, collision.contacts[0].point );

		if( impactParticleSystem != null )
		{
			impactParticleSystem.transform.SetParent( null );
			impactParticleSystem.gameObject.SetActive(true);
			GameObject.Destroy( impactParticleSystem, 5f );
		}

		destroyAllTargetsWithinBlastRadius( 15f, true );

		GameObject.Destroy( gameObject );
		
  	}

}
