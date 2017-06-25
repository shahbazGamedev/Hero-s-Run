using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Frisbee : CardSpawnedObject {

	[SerializeField] Light fireLight;
	[SerializeField] ParticleSystem fireParticleSystem;
	[SerializeField] ParticleSystem impactParticleSystem;
	[SerializeField] AudioClip inFlightSound;
	[SerializeField] AudioClip collisionSound;
	float missileVelocity = 21f;
	//Important: if the turn value is too small, you may see the missile spin around the target without ever hitting it because the turn radius is too big.
	//A turn value of 24 for a missile velocity of 40 works well.
	float turn = 24f;
	Rigidbody homingMissile;
	Transform target;
	const float MAX_TIME_BEFORE_THROWING_FRISBEE = 5f;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = gameObject.GetPhotonView ().instantiationData;

		findOwner( data );
		StartCoroutine( destroyFrisbeeOnExpiry( (float) data[1] ) );
		StartCoroutine( destroyFrisbee() );
	}

	Transform findOwner(object[] data) 
	{
		int viewIdOfOwner = (int) data[0];
		GameObject[] playersArray = GameObject.FindGameObjectsWithTag("Player");
		GameObject owner = null;

		for( int i = 0; i < playersArray.Length; i ++ )
		{
			if( playersArray[i].GetPhotonView().viewID == viewIdOfOwner )
			{
				owner = playersArray[i];
				transform.SetParent( owner.transform );
				setCasterName( owner.name );
				break;
			}
		}
		if( owner != null )
		{
			Debug.Log("Frisbee-The owner of this Frisbee is: " + getCasterName() );
			return owner.transform;
		}
		else
		{
			Debug.LogError("Frisbee error: could not find the Frisbee owner with the Photon view id of " + viewIdOfOwner );
			return null;
		}
	}

	IEnumerator destroyFrisbeeOnExpiry( float spellDuration )
	{
		yield return new WaitForSeconds(spellDuration);
		GameObject.Destroy( gameObject );
	}

	IEnumerator destroyFrisbee()
	{
		yield return new WaitForSeconds(MAX_TIME_BEFORE_THROWING_FRISBEE);
		//The player has MAX_TIME_BEFORE_THROWING_FRISBEE seconds to launch the frisbee or else it disappears
		if( transform.parent != null ) GameObject.Destroy( gameObject );
	}


	IEnumerator launchMissile()
	{
		StopCoroutine( destroyFrisbee() );
		GetComponent<FloatObject>().enabled = false;
		yield return new WaitForSeconds(0.05f);
		transform.SetParent( null );

		homingMissile = GetComponent<Rigidbody>();
//target = getNearestTargetWithinRange( Mathf.Infinity, MaskHandler.getMaskOnlyPlayer() );
		if( target != null ) print("Homing Missile target is " + target.name );
		homingMissile.isKinematic = false;
		GetComponent<AudioSource>().clip = inFlightSound;
		GetComponent<AudioSource>().Play();
		if( fireLight != null ) fireLight.enabled = true;
		if( fireParticleSystem != null ) fireParticleSystem.gameObject.SetActive(true);

		//Add an icon on the minimap
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );
	}

	void LateUpdate()
	{
		if( homingMissile == null ) return;

		if( target == null )
		{
			//If the target couldn't be found or is no longer present (maybe a player disconnected), just continue straight until you either self-destruct or hit an obstacle.
			homingMissile.velocity = transform.forward * missileVelocity;
		}
		else
		{
			homingMissile.velocity = transform.forward * missileVelocity;
			//Aim for the torso, not the feet
			Quaternion targetRotation = Quaternion.LookRotation( new Vector3( target.position.x, target.position.y, target.position.z ) - transform.position ); 
			homingMissile.MoveRotation( Quaternion.RotateTowards( transform.rotation, targetRotation, turn ) );
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		StopAllCoroutines();
		if( homingMissile == null ) return;
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

	// Update is called once per frame
	void Update () {

		#if UNITY_EDITOR
		// User pressed the left mouse up
		if (Input.GetMouseButtonUp(0))
		{
			MouseButtonUp(0);
		}
		#else
		detectTaps();
		#endif
	}

	void MouseButtonUp(int Button)
	{
		GameObject gameObject = getTarget(Input.mousePosition);
		if( gameObject != null )
		{
			print("Frisbee-tapped on: " + gameObject.name );
		}
	}

	void detectTaps()
	{
		if ( Input.touchCount > 0 )
		{
			Touch touch = Input.GetTouch(0);
			if( touch.tapCount == 1 )
			{
				if( touch.phase == TouchPhase.Ended  )
				{
					GameObject gameObject = getTarget(Input.GetTouch(0).position);
					if( gameObject != null )
					{
						print("Frisbee-tapped on: " + gameObject.name );
					}
				}
			}
		}
	}

	GameObject getTarget( Vector2 touchPosition )
	{
		// We need to actually hit an object
		RaycastHit hit;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(touchPosition), out hit, 500))
		{
			if (hit.collider && hit.collider.name != getCasterName() )
			{
				target = hit.collider.transform;
				StartCoroutine( launchMissile() );
				return hit.collider.gameObject;

			}
		}
		return null;
	}
}