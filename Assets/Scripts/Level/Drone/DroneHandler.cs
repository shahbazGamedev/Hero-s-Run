using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneHandler : CardSpawnedObject {

	[Header("Drone")]
	[SerializeField] float projectileSpeed = 1000;
    [SerializeField] Transform spawnPositionLeft;
    [SerializeField] Transform spawnPositionRight;
	
	[Header("Sound Effects")]
	[SerializeField] AudioSource audioSource;
	[SerializeField] List<SentrySoundData> sentrySoundList = new List<SentrySoundData>();

	[Header("Particle Systems")]
	[SerializeField] ParticleSystem onDestroyFx;
	const float DELAY_BEFORE_DESTROY_EFFECTS = 1.3f;

	//Target
	public Transform nearestTarget = null;

	//Shooting related
	[SerializeField] float seekSpeed = 3f;
	float weaponCoolDown = 3f;
	float timeOfLastShot;
	float aimRange = 50f;
	float accuracy = 0.0005f;
	const float FORWARD_MOVEMENT_ANTICIPATION = 0.15f;
   	RaycastHit hit;

	Quaternion initialRotation;
	Vector3 initialPosition;

	#region Initialisation
	void Start()
	{
		setSpawnedObjectState( SpawnedObjectState.Functioning );
		initialRotation = transform.rotation;
		initialPosition = transform.position;
	}
	#endregion

	#region Target detection and shooting
	void FixedUpdate()
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
		//Only the master client can shoot at targets
		if( PhotonNetwork.isMasterClient )
		{
			if( spawnedObjectState == SpawnedObjectState.Functioning )
			{
				nearestTarget = getNearestTargetWithinRange( aimRange, MaskHandler.getMaskOnlyPlayer() );
			}
		}
	}

	void lookAtTarget()
	{
		if( nearestTarget != null )
		{
			//The drone has a target. Change lane to face it at seekSpeed.
			float nearestTargetRotationY = Mathf.Floor( nearestTarget.eulerAngles.y );
			Vector3 targetPosition;
			if( nearestTargetRotationY == 0 )
			{
				//Target is running South-North. Use X.
				targetPosition = new Vector3( nearestTarget.position.x, transform.position.y, transform.position.z );
			}
			else
			{
				//Target is running West-East. Use Z.
				targetPosition = new Vector3( transform.position.x, transform.position.y, nearestTarget.position.z );
			}
			transform.position = Vector3.MoveTowards( transform.position, targetPosition, Time.deltaTime * seekSpeed );

			//Now orient the drone so that it aims towards the player
			transform.LookAt(nearestTarget.position);
			//Cap the X angle
			float xAngle = Mathf.Min( transform.eulerAngles.x, 30f );
			transform.rotation = Quaternion.Euler (xAngle, 180f, 0 );

			//Verify if we can hit the nearest target
			shoot();
 		}
		else
		{
			//The drone doesn't have a target. Go back to the initial position and rotation.
			transform.rotation = Quaternion.Lerp( transform.rotation, initialRotation, Time.deltaTime * seekSpeed );
			transform.position = Vector3.MoveTowards( transform.position, initialPosition, Time.deltaTime * seekSpeed );
		}
	}

	void Update () 
	{
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
			shoot();
        }
	}

	void shoot()
	{
		if( nearestTarget == null ) return;
		if( Time.time - timeOfLastShot > weaponCoolDown )
		{
			timeOfLastShot = Time.time;
			//Create projectiles
			object[] data = new object[6];
			data[0] = projectileSpeed;
			data[1] = aimRange;
			data[2] = nearestTarget.TransformPoint(0.25f,0.8f,0);
			data[3] = nearestTarget.TransformPoint(-0.25f,0.8f,0);
			data[4] = spawnPositionLeft.position;
			data[5] = spawnPositionRight.position;
			PhotonNetwork.InstantiateSceneObject( "Drone Projectiles", transform.position, transform.rotation, 0, data );
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
		//casterTransform.GetComponent<PlayerSpell>().cardDurationExpired( CardName.Sentry );
		setSpawnedObjectState(SpawnedObjectState.BeingDestroyed);
		nearestTarget = null;
		StopCoroutine( "changeMaterialOnCreate" );
		GetComponent<Renderer>().material = onDestroy;
		playSoundEffect( Emotion.Sad, true );

		yield return new WaitForSeconds(delayBeforeDestroyEffects);
		//MiniMap.Instance.hideSecondaryIcon( casterGameObject );
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

