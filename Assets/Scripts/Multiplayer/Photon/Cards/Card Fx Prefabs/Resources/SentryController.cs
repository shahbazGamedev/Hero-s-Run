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
	Vector3 sentryOffsetToPlayer = new Vector3( 0.8f, 2.3f, 0 );
	
	[Tooltip("Determines how fast does the Sentry match the player's position.")]
	[Range(10,20)]
	[SerializeField] float followSpeed = 15f;

	[Header("Sound Effects")]
	[SerializeField] AudioSource audioSource;
	[SerializeField] List<SentrySoundData> sentrySoundList = new List<SentrySoundData>();

	[Header("Particle Systems")]
	[SerializeField] ParticleSystem onDestroyFx;
	const float DELAY_BEFORE_DESTROY_EFFECTS = 1.3f;

	//Target
	Transform nearestTarget = null;

	//Shooting related
	float aimSpeed = 7.6f;
	float weaponCoolDown = 3.5f;
	float timeOfLastShot;
	float aimRange = 40f;
	float accuracy = 0.002f;
	const float FORWARD_MOVEMENT_ANTICIPATION = 0.15f;

	#region Initialisation
	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
        LockstepManager.LockstepAction lsa = new LockstepManager.LockstepAction( LockstepActionType.ACTIVATE_CARD, gameObject, CardName.Sentry );
		lsa.cardSpawnedObject = this;
		LockstepManager.Instance.addActionToQueue( lsa );
	}

	public override void activateCard()
	{
		findOwner( gameObject.GetPhotonView ().instantiationData );
	}

	void findOwner(object[] data) 
	{
		int viewIdOfOwner = (int) data[0];
		casterTransform = getPlayerByViewID( viewIdOfOwner );
		if( casterTransform != null )
		{
			setCasterName( casterTransform.name );
			float spellDuration = (float) data[1];
			aimRange = (float) data[2];
			accuracy = (float) data[3];
			playSoundEffect( Emotion.Happy );
			StartCoroutine( changeMaterialOnCreate( 2f ) );
			casterTransform.GetComponent<PlayerSpell>().registerSentry( this );
			//The Sentry has a limited lifespan which depends on the level of the Card.
			StartCoroutine( destroySpawnedObject( spellDuration, DELAY_BEFORE_DESTROY_EFFECTS ) );
			//Display the Sentry secondary icon on the minimap
			MiniMap.Instance.displaySecondaryIcon( casterTransform.GetComponent<PhotonView>().viewID, (int) CardName.Sentry, spellDuration );
			Debug.Log("SentryController-The owner of this sentry is: " + casterName );
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
	void FixedUpdate()
	{
		//We don't want the sentry to shoot while paused.
		//Remember that in multiplayer the time scale is not set to 0 while paused.
		if( GameManager.Instance.getGameState() == GameState.Normal && casterTransform != null )
		{
			followPlayer();
			detectNearestTarget();
			lookAtTarget();
		}
	}

	void followPlayer()
	{
		Vector3 desiredPosition = casterTransform.TransformPoint( sentryOffsetToPlayer );
		transform.position = Vector3.Lerp( transform.position, desiredPosition, Time.deltaTime * followSpeed );
	}

	void detectNearestTarget()
	{
		//Only the master client can shoot at targets
		if( PhotonNetwork.isMasterClient )
		{
			if( spawnedObjectState == SpawnedObjectState.Functioning )
			{
				nearestTarget = getNearestTargetWithinRange( aimRange, MaskHandler.getMaskWithPlayersWithCreatures(), false );
			}
		}
	}

	/// <summary>
	/// Gets the desired rotation for aiming and shooting.
	/// The forward adjustment parameter is used to anticipate the player's movement.
	/// Typically you use 0 when aiming and 0.5 when shooting.
	/// For example, with a value of 0.24, the missile will head for a position 0.24 meters in front of the player.
	/// Since the missile has a limited velocity and the player is moving very fast, this gives the missile more accuracy. 
	/// </summary>
	/// <returns>The desired rotation.</returns>
	/// <param name="forwardAdjustment">Forward adjustment.</param>
	Quaternion getDesiredRotation( float forwardAdjustment )
	{
		float heightAdjustment = 0;
		if( nearestTarget.CompareTag("Player") || nearestTarget.CompareTag("Zombie") )
		{
			//The transform position of the player is at his feet. Let's aim at his neck.
			heightAdjustment = 1.2f;
		}
		else
		{
			//For other objects, aim a bit above the center of the object
			heightAdjustment = nearestTarget.localScale.y * 0.01f;
		}
		Vector3 targetCenter = nearestTarget.TransformPoint(0, heightAdjustment, forwardAdjustment );
		Vector3 relativePos = targetCenter - transform.position;
		return Quaternion.LookRotation( relativePos ); 
	}

	void lookAtTarget()
	{
		if( nearestTarget != null )
		{
			//The sentry has a target. Turn towards it at aimSpeed.
			transform.rotation = Quaternion.Lerp( transform.rotation, getDesiredRotation( 0 ), Time.deltaTime * aimSpeed );
			//Verify if we can hit the nearest target
			aim();
		}
		else
		{
			//The sentry does not have a target. Resume looking in the same direction as the player.
			Quaternion desiredRotation = casterTransform.rotation; 
			desiredRotation.x = 0f;
			desiredRotation.z = 0f;
			transform.rotation = Quaternion.Lerp( transform.rotation, casterTransform.rotation, Time.deltaTime * aimSpeed );
		}
	}

	void aim()
	{
		//Verify if we can hit the nearest target
		RaycastHit hit;
		gameObject.layer = MaskHandler.ignoreRaycastLayer;
		if (Physics.Raycast(transform.position, transform.forward, out hit, aimRange ))
		{
			if( hit.collider.transform == nearestTarget )
			{
				shoot();
			}
		}
		gameObject.layer = MaskHandler.destructibleLayer;
	}

	void shoot()
	{
		if( Time.time - timeOfLastShot > weaponCoolDown )
		{
			timeOfLastShot = Time.time;

			transform.rotation = getDesiredRotation( FORWARD_MOVEMENT_ANTICIPATION );

			//The sentry is not perfectly accurate when shooting.
			Vector3 direction = transform.forward;
			direction.x += Random.Range( -accuracy, accuracy );
			direction.y += Random.Range( -accuracy, accuracy );
			direction.z += Random.Range( -accuracy, accuracy );
	
			//Create missile
			object[] data = new object[4];
			data[0] = casterTransform.GetComponent<PhotonView>().viewID;

			data[1] = direction;
			data[2] = GetComponent<PhotonView>().viewID;
			//Self-destruct time for missile
			data[3] = 10f;
			PhotonNetwork.InstantiateSceneObject( "Sentry Missile", transform.position + transform.forward.normalized, Quaternion.Euler( direction ), 0, data );
		}
	}
	#endregion

	public override void destroySpawnedObjectNow()
	{
		StartCoroutine( destroySpawnedObject( 0, DELAY_BEFORE_DESTROY_EFFECTS ) );
	}

	IEnumerator destroySpawnedObject( float delayBeforeSentryExpires, float delayBeforeDestroyEffects )
	{
		GetComponent<SphereCollider>().enabled = false;
		yield return new WaitForSeconds(delayBeforeSentryExpires);
		casterTransform.GetComponent<PlayerSpell>().cardDurationExpired( CardName.Sentry, false );
		setSpawnedObjectState(SpawnedObjectState.BeingDestroyed);
		nearestTarget = null;
		StopCoroutine( "changeMaterialOnCreate" );
		GetComponent<Renderer>().material = onDestroy;
		playSoundEffect( Emotion.Sad, true );

		yield return new WaitForSeconds(delayBeforeDestroyEffects);
		MiniMap.Instance.hideSecondaryIcon( casterGameObject );
		onDestroyFx.transform.SetParent( null );
		onDestroyFx.Play();
		Destroy( gameObject );
		Destroy( onDestroyFx.gameObject, 3f );
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
