using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Emotion {

	Happy = 1,
	Sad = 2,
	Victory = 3
}

public class SentryController : CardSpawnedObject {

	[Header("Sentry")]

	[Header("Sound Effects")]
	[SerializeField] AudioSource audioSource;
	[SerializeField] List<SentrySoundData> sentrySoundList = new List<SentrySoundData>();

	[Header("Particle Systems")]
	[SerializeField] ParticleSystem onDestroyFx;
	const float DELAY_BEFORE_DESTROY_EFFECTS = 1.3f;

	//My owner (i.e. the player who created this Sentry)
	private GameObject myOwner;
	private Transform myOwnerTransform;
	private PlayerRace myOwnerPlayerRace;

	//Target
	Transform nearestTarget = null;

	//Shooting related
	float aimSpeed = 7.6f;
	float weaponCoolDown = 3f;
	float timeOfLastShot;
	float aimRange = 40f;
	float accuracy = 0.005f;

	#region Initialisation
	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		findOwner( gameObject.GetPhotonView ().instantiationData );
	}

	void findOwner(object[] data) 
	{
		int viewIdOfOwner = (int) data[0];
		GameObject[] playersArray = GameObject.FindGameObjectsWithTag("Player");
		for( int i = 0; i < playersArray.Length; i ++ )
		{
			if( playersArray[i].GetPhotonView().viewID == viewIdOfOwner )
			{
				float spellDuration = (float) data[1];
				aimRange = (float) data[2];
				accuracy = (float) data[3];
				myOwner = playersArray[i];
				casterName = myOwner.name;
				myOwnerTransform = myOwner.transform;
				myOwnerPlayerRace = myOwner.GetComponent<PlayerRace>();
				transform.SetParent( myOwnerTransform );
				playSoundEffect( Emotion.Happy );
				StartCoroutine( changeMaterialOnCreate( 2f ) );
				myOwner.GetComponent<PlayerSpell>().registerSentry( this );
				//The Sentry has a limited lifespan which depends on the level of the Card.
				StartCoroutine( destroySpawnedObject( spellDuration, DELAY_BEFORE_DESTROY_EFFECTS ) );
				//Display the Sentry secondary icon on the minimap
				MiniMap.Instance.displaySecondaryIcon( myOwnerTransform.GetComponent<PhotonView>().viewID, (int) CardName.Sentry, spellDuration );
				break;
			}
		}
		if( myOwner != null )
		{
			Debug.Log("SentryController-The owner of this sentry is: " + myOwner.name );
		}
		else
		{
			Debug.LogError("SentryController error: could not find the sentry owner with the Photon view id of " + viewIdOfOwner );
		}
	}

	IEnumerator changeMaterialOnCreate( float delayBeforeMaterialChange )
	{
		yield return new WaitForSeconds(delayBeforeMaterialChange);
		GetComponent<Renderer>().material = onFunctioning;
		setSpawnedObjectState( SpawnedObjectState.Functioning );
	}
	#endregion

	#region Target detection and shooting
	void LateUpdate()
	{
		//We don't want the sentry to shoot while paused.
		//Remember that in multiplayer the time scale is not set to 0 while paused.
		if( GameManager.Instance.getGameState() == GameState.Normal )
		{
			detectNearestTarget();
			lookAtTarget();
		}
	}

	void detectNearestTarget()
	{
		if( spawnedObjectState == SpawnedObjectState.Functioning )
		{
			nearestTarget = getNearestTargetWithinRange( aimRange );
		}
	}

	Quaternion getDesiredRotation()
	{
		float heightAdjustment = 0;
		if( nearestTarget.CompareTag("Player") )
		{
			//The transform position of the player is at his feet. Let's aim at his torso.
			heightAdjustment = 1f;
		}
		else
		{
			//For other objects, aim a bit above the center of the object
			heightAdjustment = nearestTarget.localScale.y * 0.01f;
		}
		Vector3 targetCenter = new Vector3( nearestTarget.position.x, nearestTarget.position.y + heightAdjustment, nearestTarget.position.z );
		Vector3 relativePos = targetCenter - transform.position;
		return Quaternion.LookRotation( relativePos ); 
	}

	void lookAtTarget()
	{
		if( nearestTarget != null )
		{
			//The sentry has a target. Turn towards it at aimSpeed.
			transform.rotation = Quaternion.Lerp( transform.rotation, getDesiredRotation(), Time.deltaTime * aimSpeed );
			//Verify if we can hit the nearest target
			aim();
		}
		else
		{
			//The sentry does not have a target. Resume looking in the same direction as the player.
			Quaternion desiredRotation = myOwnerTransform.rotation; 
			desiredRotation.x = 0f;
			desiredRotation.z = 0f;
			transform.rotation = Quaternion.Lerp( transform.rotation, myOwnerTransform.rotation, Time.deltaTime * aimSpeed );
		}
	}

	void aim()
	{
		//Verify if we can hit the nearest target
		RaycastHit hit;
		gameObject.layer = ignoreRaycastLayer;
		if (Physics.Raycast(transform.position, transform.forward, out hit, aimRange ))
		{
			if( hit.collider.transform == nearestTarget )
			{
				shoot();
			}
		}
		gameObject.layer = destructibleLayer;
	}

	void shoot()
	{
		if( Time.time - timeOfLastShot > weaponCoolDown )
		{
			timeOfLastShot = Time.time;

			transform.rotation = getDesiredRotation();

			//The sentry is not perfectly accurate when shooting.
			Vector3 direction = transform.forward;
			direction.x += Random.Range( -accuracy, accuracy );
			direction.y += Random.Range( -accuracy, accuracy );
			direction.z += Random.Range( -accuracy, accuracy );
	
			//Create missile
			object[] data = new object[2];
			data[0] = direction;
			data[1] = GetComponent<PhotonView>().viewID;
			PhotonNetwork.InstantiateSceneObject( "Sentry Missile", transform.position + transform.forward.normalized, Quaternion.Euler( direction ), 0, data );
		}
	}
	#endregion

	public void destroySpawnedObjectNow()
	{
		StartCoroutine( destroySpawnedObject( 0, DELAY_BEFORE_DESTROY_EFFECTS ) );
	}

	IEnumerator destroySpawnedObject( float delayBeforeSentryExpires, float delayBeforeDestroyEffects )
	{
		yield return new WaitForSeconds(delayBeforeSentryExpires);

		setSpawnedObjectState(SpawnedObjectState.BeingDestroyed);
		nearestTarget = null;
		StopCoroutine( "changeMaterialOnCreate" );
		GetComponent<Renderer>().material = onDestroy;
		playSoundEffect( Emotion.Sad, true );

		yield return new WaitForSeconds(delayBeforeDestroyEffects);
		MiniMap.Instance.hideSecondaryIcon( myOwner );
		onDestroyFx.transform.SetParent( null );
		onDestroyFx.Play();
		Destroy( gameObject );
		Destroy( onDestroyFx, 3f );
	}

	#region Sound
	public void playSoundEffect( Emotion emotion, bool forcePlay = false )
	{
		//Don't interrupt the current sound effect for another one.
		if( audioSource.isPlaying && !forcePlay ) return;

		//Do we have one or more sound effects that match?
		List<SentrySoundData> availableSoundsList = sentrySoundList.FindAll(soundClip => ( soundClip.emotion == emotion ) );

		if( availableSoundsList.Count > 0 )
		{
			if( availableSoundsList.Count == 1 )
			{
				audioSource.PlayOneShot( availableSoundsList[0].clip );
			}
			else
			{
				//We have multiple entries that match. Let's play a random one.
				int random = Random.Range( 0, availableSoundsList.Count );
				audioSource.PlayOneShot(  availableSoundsList[random].clip );
			}
		}
	}

	/// <summary>
	/// Stops the audio source.
	/// </summary>
	public void stopAudioSource()
	{
		audioSource.Stop();
	}
	#endregion

	/// <summary>
	/// If a projectile fired by the Sentry kills a target, the projectile will call this method.
	/// The Sentry then plays a victory sound.
	/// </summary>
	public void targetHit()
	{
		playSoundEffect( Emotion.Victory );
	}

	[System.Serializable]
	public class SentrySoundData
	{
		public Emotion emotion;
		public AudioClip clip;
	}

}
