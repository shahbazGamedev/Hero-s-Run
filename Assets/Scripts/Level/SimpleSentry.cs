using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple sentry. When a player crosses the finish line, the sentry will play a sound and look at the player that just arrived for a few seconds.
/// </summary>
[RequireComponent (typeof (AudioSource))]
public class SimpleSentry : MonoBehaviour {

	[Header("Simple Sentry")]
	[SerializeField] List<SentryController.SentrySoundData> sentrySoundList = new List<SentryController.SentrySoundData>();
	AudioSource audioSource;
	Quaternion originalRotation;
	Transform nearestTarget = null;
	float targetAvailableTurnSpeed = 5f;
	float targetNotAvailableTurnSpeed = 3f;

	void Start()
	{
		audioSource = GetComponent<AudioSource>();
		originalRotation = transform.rotation;
	}

	void FixedUpdate()
	{
		//We don't want the sentry to move while paused.
		if( GameManager.Instance.getGameState() == GameState.Normal )
		{
			lookAtTarget();
		}
	}

	void lookAtTarget()
	{
		if( nearestTarget != null )
		{
			//The sentry has a target. Turn towards it.
			transform.rotation = Quaternion.Lerp( transform.rotation, getDesiredRotation(), Time.deltaTime * targetAvailableTurnSpeed );
		}
		else
		{
			//The sentry does not have a target. Resume looking in the original direction.
			transform.rotation = Quaternion.Lerp( transform.rotation, originalRotation, Time.deltaTime * targetNotAvailableTurnSpeed );
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


	void playSoundEffect( Emotion emotion, bool forcePlay = false )
	{
		//Don't interrupt the current sound effect for another one.
		if( audioSource.isPlaying && !forcePlay ) return;

		//Do we have one or more sound effects that match?
		List<SentryController.SentrySoundData> availableSoundsList = sentrySoundList.FindAll(soundClip => ( soundClip.emotion == emotion ) );

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

	void OnEnable()
	{
		PlayerRace.crossedFinishLine += CrossedFinishLine;
	}

	void OnDisable()
	{
		PlayerRace.crossedFinishLine -= CrossedFinishLine;
	}

	void CrossedFinishLine( Transform player, int officialRacePosition )
	{
		print("SimpleSentry-CrossedFinishLine " + player.name + " officialRacePosition " +  officialRacePosition );

		//Don't interrupt the current look-at
		if( nearestTarget != null ) return;

		nearestTarget = player;

		if( officialRacePosition == 0 )
		{
			playSoundEffect( Emotion.Happy );
		}
		else
		{
			playSoundEffect( Emotion.Sad );
		}
		Invoke("stopLooking", 3f );
	}

	void stopLooking()
	{
		nearestTarget = null;
	}

}

