using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Emotion {

	Happy = 1,
	Sad = 2,
	Surprised = 3,
	Disapointed = 4
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
	private GameObject myOwner;
	private Transform myOwnerTransform;
	private PlayerRace myOwnerPlayerRace;
	private SentryState sentryState = SentryState.Initialising;
	private LineRenderer lineRenderer;
	[Header("Target")]
	float aimSpeed = 7.6f;
	Transform nearestTarget = null;
	[Header("Card Parameters")]
	float spellRange = 40f;

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
				myOwner = playersArray[i];
				myOwnerTransform = myOwner.transform;
				myOwnerPlayerRace = myOwner.GetComponent<PlayerRace>();
				transform.SetParent( myOwnerTransform );
				playSoundEffect( Emotion.Happy );
				StartCoroutine( changeMaterialOnCreate( 2f ) );
				myOwner.GetComponent<PlayerSpell>().registerSentry( this );
				lineRenderer = GetComponent<LineRenderer>();
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

	void detectNearestTarget()
	{
		if( sentryState == SentryState.Functioning )
		{
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
	
				//Is this player within spell range?
				if( distanceToTarget > spellRange ) continue;
	
				//Is the player dead or Idle? If so, ignore.
				if( PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Dying || PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Idle ) continue;
	
				//Is it the closest player?
				if( distanceToTarget < nearestDistance )
				{
					nearestTarget = PlayerRace.players[i].transform;
					print("Sentry-The nearest target is " + nearestTarget.name );
					nearestDistance = distanceToTarget;
					lineRenderer.enabled = true;
				}
			}
		}
	}

	void useLaser()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position, transform.forward, out hit, spellRange ))
		{
			float length = Mathf.Min( hit.distance, spellRange );
			lineRenderer.SetPosition(1, new Vector3( 0, 0, length ) );
			if( hit.collider.transform == nearestTarget )
			{
 				nearestTarget.GetComponent<PhotonView>().RPC("playerDied", PhotonTargets.All, DeathType.Obstacle );
				print("Kill " + hit.collider.name + " " + nearestTarget.name );
				nearestTarget = null;
				lineRenderer.enabled = false;
			}
		}
	}

	void setSentryState( SentryState newState )
	{
		sentryState = newState;
	}

	void LateUpdate()
	{
		detectNearestTarget();
		lookAtTarget();
	}

	void lookAtTarget()
	{
		if( nearestTarget != null )
		{
			Vector3 relativePos = nearestTarget.position - transform.position;
			Quaternion desiredRotation = Quaternion.LookRotation( relativePos ); 
			desiredRotation.z = 0f;
			transform.rotation = Quaternion.Lerp( transform.rotation, desiredRotation, Time.deltaTime * aimSpeed );
			useLaser();
		}
		else
		{
			Quaternion desiredRotation = myOwnerTransform.rotation; 
			desiredRotation.z = 0f;
			transform.rotation = Quaternion.Lerp( transform.rotation, myOwnerTransform.rotation, Time.deltaTime * aimSpeed );
		}
	}

	IEnumerator changeMaterialOnCreate( float delayBeforeMaterialChange )
	{
		yield return new WaitForSeconds(delayBeforeMaterialChange);
		GetComponent<Renderer>().material = onFunctioning;
		setSentryState(SentryState.Functioning);
	}

	public IEnumerator destroySentry( float delayBeforeEffects )
	{
		setSentryState(SentryState.BeingDestroyed);
		nearestTarget = null;
		StopCoroutine( "changeMaterialOnCreate" );
		GetComponent<Renderer>().material = onDestroy;
		playSoundEffect( Emotion.Sad, true );
		yield return new WaitForSeconds(delayBeforeEffects);
		onDestroyFx.transform.SetParent( null );
		onDestroyFx.Play();
		Destroy( gameObject );
	}

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

	[System.Serializable]
	public class SentrySoundData
	{
		public Emotion emotion;
		public AudioClip clip;
	}

}
