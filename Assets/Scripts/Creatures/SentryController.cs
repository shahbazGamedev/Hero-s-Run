using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Emotion {

	Happy = 1,
	Sad = 2,
	Victory = 3
}

public class SentryController : MonoBehaviour {

	enum SentryState {
	
		Initialising = 1,
		Functioning = 2,
		BeingDestroyed = 3
	}

	[Header("Sentry")]
	[Header("Sound Effects")]
	[SerializeField] AudioSource audioSource;
	[SerializeField] List<SentrySoundData> sentrySoundList = new List<SentrySoundData>();
	[Header("Materials")]
	[SerializeField] Material onCreate;
	[SerializeField] Material onDestroy;
	[SerializeField] Material onFunctioning;
	[Header("Particle Systems")]
	[SerializeField] ParticleSystem onDestroyFx;
	const float DELAY_BEFORE_DESTROY_EFFECTS = 1.3f;
	private GameObject myOwner;
	private Transform myOwnerTransform;
	private PlayerRace myOwnerPlayerRace;
	private SentryState sentryState = SentryState.Initialising;
	private LineRenderer lineRenderer; //The line renderer is used for debuging
	[Header("Target")]
	float aimSpeed = 7.6f;
	Transform nearestTarget = null;
	float weaponCoolDown = 3f;
	float timeOfLastShot;
	[Header("Card Parameters")]
	float aimRange = 40f;
	float accuracy = 0.01f;

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
				myOwnerTransform = myOwner.transform;
				myOwnerPlayerRace = myOwner.GetComponent<PlayerRace>();
				transform.SetParent( myOwnerTransform );
				playSoundEffect( Emotion.Happy );
				StartCoroutine( changeMaterialOnCreate( 2f ) );
				myOwner.GetComponent<PlayerSpell>().registerSentry( this );
				lineRenderer = GetComponent<LineRenderer>();
				//The Sentry has a limited lifespan which depends on the level of the Card.
				StartCoroutine( destroySentry( spellDuration, DELAY_BEFORE_DESTROY_EFFECTS ) );
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
		setSentryState(SentryState.Functioning);
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
		if( sentryState == SentryState.Functioning )
		{
			//If the nearest target is no longer within spell range, reset it.
			if( nearestTarget != null && Vector3.Distance( transform.position, nearestTarget.position ) > aimRange ) nearestTarget = null;

			//If the nearest target is dead or idle, reset it.
			if( nearestTarget != null && ( nearestTarget.GetComponent<PlayerControl>().deathType != DeathType.Alive || nearestTarget.GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Idle ) ) nearestTarget = null;

			//Ignore if we already have a target
			if( nearestTarget != null ) return;
	
			float nearestDistance = 100000;
			//Keep nearest target only
			for( int i =0; i < PlayerRace.players.Count; i++ )
			{
				//Ignore the sentry owner
				if( PlayerRace.players[i] == myOwnerPlayerRace ) continue;
	
				//Calculate the distance to the other player
				float distanceToTarget = Vector3.Distance( transform.position, PlayerRace.players[i].transform.position );
	
				//Is this player within aiming range?
				if( distanceToTarget > aimRange ) continue;
	
				//Is the player dead or Idle? If so, ignore.
				if( PlayerRace.players[i].GetComponent<PlayerControl>().deathType != DeathType.Alive || PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Idle ) continue;
	
				//Is it the closest player?
				if( distanceToTarget < nearestDistance )
				{
					nearestTarget = PlayerRace.players[i].transform;
					print("Sentry-The nearest target is " + nearestTarget.name );
					nearestDistance = distanceToTarget;
					lineRenderer.enabled = false;
				}
			}
		}
	}

	void lookAtTarget()
	{
		if( nearestTarget != null )
		{
			//The sentry has a target. Turn towards it at aimSpeed.
			//Note that the transform position of the player is at his feet. Let's aim at his torso.
			Vector3 playerTorso = new Vector3( nearestTarget.position.x, nearestTarget.position.y + 1f, nearestTarget.position.z );
			Vector3 relativePos = playerTorso - transform.position;
			Quaternion desiredRotation = Quaternion.LookRotation( relativePos ); 
			desiredRotation.z = 0f;
			transform.rotation = Quaternion.Lerp( transform.rotation, desiredRotation, Time.deltaTime * aimSpeed );
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
		if (Physics.Raycast(transform.position, transform.forward, out hit, aimRange ))
		{
			lineRenderer.SetPosition(1, new Vector3( 0, 0, aimRange ) );
			if( hit.collider.transform == nearestTarget )
			{
				shoot();
			}
		}
	}

	void shoot()
	{
		if( Time.time - timeOfLastShot > weaponCoolDown )
		{
			timeOfLastShot = Time.time;

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

	void setSentryState( SentryState newState )
	{
		sentryState = newState;
	}

	public void destroySentryNow()
	{
		StartCoroutine( destroySentry( 0, DELAY_BEFORE_DESTROY_EFFECTS ) );
	}

	IEnumerator destroySentry( float delayBeforeSentryExpires, float delayBeforeDestroyEffects )
	{
		yield return new WaitForSeconds(delayBeforeSentryExpires);

		setSentryState(SentryState.BeingDestroyed);
		nearestTarget = null;
		StopCoroutine( "changeMaterialOnCreate" );
		GetComponent<Renderer>().material = onDestroy;
		playSoundEffect( Emotion.Sad, true );

		yield return new WaitForSeconds(delayBeforeDestroyEffects);
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
