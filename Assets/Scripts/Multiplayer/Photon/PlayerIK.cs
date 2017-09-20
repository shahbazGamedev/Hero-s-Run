using System.Collections;
using UnityEngine;

//PlayerIK is used to have the player loot at a specific target for a short duration.
//For now, there are two trigger conditions:
//a) Another player died within active distance
//b) A player is passing you
//For the IK to work, there are 2 conditions:
//a) The rig must be Humanoid
//b) In the Animator windows, under Layers, under Settings, you must have the IK Pass toggled on.
public class PlayerIK : MonoBehaviour {

	[Header("Look At IK")]
	[SerializeField] float lookAtWeight = 0.8f;
	[SerializeField] float bodyWeight = 0.7f;
	[SerializeField] float headWeight = 0.9f;
	[SerializeField] float eyesWeight = 0.9f;
	[SerializeField] float clampWeight = 1f;
	[SerializeField] float activeDistanceSquared = 50f * 50f;
	[SerializeField] float dotProductIK = 0.58f;

	Transform lookAtTarget;
	Animator anim;
	Vector3 playerOffset = new Vector3( 0, 1.4f, 0 ); //Look at the player's eyes, not his feet.
	//NOTE: Your reference to a coroutine will NOT become null once the coroutine has finished running or even if you called StopCoroutine on it.
	Coroutine fadeInLookAtCoroutine;
	Coroutine fadeOutLookAtCoroutine;
	//We want the IK to be disabled at the beginning of the race. It looks weird when the opponents stare at the player right at the get go.
	bool isIKEnabled = false;
	const float DELAY_BEFORE_ACTIVATING_IK = 12f;

	void Awake ()
	{
		anim = GetComponent<Animator>();
	}

	void Start ()
	{
		Invoke( "activateIKAfterDelay", DELAY_BEFORE_ACTIVATING_IK );
	}

	void activateIKAfterDelay()
	{
		isIKEnabled = true;
	}

	/*
		returns:
		-1 if creature is behind player
		+1 if creature is in front
		0 if creature is on the side
		0.5 if creature is facing player and within 60 degrees (i.e. between 30 degrees to the left and 30 degrees to the right)
	*/
	float getDotProduct()
	{
		Vector3 heading = lookAtTarget.position - transform.position;
		return Vector3.Dot( heading.normalized, transform.forward );
	}

	void OnAnimatorIK()
	{
		//Is there an active look-at fade in or fade out?
		if( fadeInLookAtCoroutine != null || fadeOutLookAtCoroutine != null )
		{
			//If we have a target, check that it is still within range.
			if( lookAtTarget != null )
			{
				float distance = Vector3.SqrMagnitude(lookAtTarget.position - transform.position);
				if( distance < activeDistanceSquared && getDotProduct() > dotProductIK )			
				{
					//The target is in range.
					anim.SetLookAtPosition( lookAtTarget.TransformPoint( playerOffset ) );
				}
				else
				{
					//The target is no longer in range.
					//Stop the fade in look-at.
					if( fadeInLookAtCoroutine != null ) StopCoroutine( fadeInLookAtCoroutine );
					lookAtTarget = null;
					//Start the fade out look-at
					if( fadeOutLookAtCoroutine == null )
					{
						fadeOutLookAtCoroutine = StartCoroutine( fadeOutLookAt( 0, 0.9f ) );
					}								
				}
			}
			//In all cases, update the look-at weight
			anim.SetLookAtWeight( lookAtWeight, bodyWeight, headWeight, eyesWeight, clampWeight );
		}
	}

	void setLookAtTarget( float activationDelay, Transform lookAtTarget )
	{
		//Ignore if IK is not enabled.
		if( !isIKEnabled ) return;

		//Ignore if the target is null.
		if( lookAtTarget == null  ) return;

		//Ignore yourself.
		if( transform == lookAtTarget )
		{
 			Debug.LogError("PlayerIK error: you should not set the look-at target to be yourself: " + name );
			return;
		}

		//Ignore if you are in IDLE.
		if( GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Idle ) return;

		//Don't interrupt the current look-at if one is active
		if( fadeInLookAtCoroutine != null ) return;

		//Don't do anything if the proposed target is too far
		float distance = Vector3.SqrMagnitude(lookAtTarget.position - transform.position);
		if( distance > activeDistanceSquared ) return;		

		this.lookAtTarget = lookAtTarget;

		fadeInLookAtCoroutine = StartCoroutine( fadeInLookAt( activationDelay, 0.8f, 0.6f, 1.2f, 0, 0.8f ) );
	}

	IEnumerator fadeInLookAt( float activationDelay, float fadeInFinalWeight, float fadeInDuration, float stayDuration, float fadeOutFinalWeight, float fadeOutDuration )
	{
		Debug.Log( name + " has a look-at target of " + lookAtTarget.name );

		//Wait before activating
		yield return new WaitForSeconds(activationDelay);

		//Fade in
		float elapsedTime = 0;
		float initialWeight = 0;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			lookAtWeight = Mathf.Lerp( initialWeight, fadeInFinalWeight, elapsedTime/fadeInDuration );
			yield return new WaitForEndOfFrame();  
			
		} while ( elapsedTime < fadeInDuration );
		
		lookAtWeight = fadeInFinalWeight;

		//Stay
		yield return new WaitForSeconds(stayDuration);

		//Fade out
		elapsedTime = 0;		
		initialWeight = lookAtWeight;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			lookAtWeight = Mathf.Lerp( initialWeight, fadeOutFinalWeight, elapsedTime/fadeOutDuration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < fadeOutDuration );
		
		lookAtWeight = fadeOutFinalWeight;
		lookAtTarget = null;
		fadeInLookAtCoroutine = null;
	
	}

	IEnumerator fadeOutLookAt( float fadeOutFinalWeight, float fadeOutDuration )
	{ 
		//Fade out
		float elapsedTime = 0;
		float initialWeight = lookAtWeight;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			lookAtWeight = Mathf.Lerp( initialWeight, fadeOutFinalWeight, elapsedTime/fadeOutDuration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < fadeOutDuration );
		
		lookAtWeight = fadeOutFinalWeight;
		fadeOutLookAtCoroutine = null;
	}

	public void isOvertaking( int racePosition )
	{
		//Race position has a value of -1 until updated for the first time
		if( racePosition > 0 )
		{
			//Find out who is the player overtaking us
			PlayerRace playerOvertaking = PlayerRace.players.Find(p => p.racePosition == racePosition - 1 );
			if( playerOvertaking != null )
			{
				//Now look-at the player after a short delay.
				//We don't want the player to start looking at a right angle. We want the opponent to be
				//about 0.05 * 20 (average run speed) = 1 meter in front before looking.
				setLookAtTarget( 0.05f, playerOvertaking.transform );
			}
			else
			{
				Debug.LogError("PlayerIK: could not find overtaking player in race position: " + (racePosition - 1) );
			}
		}
	}

	public void playerDied()
	{
		if( fadeInLookAtCoroutine != null ) StopCoroutine(fadeInLookAtCoroutine);
		lookAtTarget = null;
		if( fadeOutLookAtCoroutine != null ) StopCoroutine(fadeOutLookAtCoroutine);

		//If the look-at weight is not 0, we should return to 0.
		if( lookAtWeight > 0 )
		{
 			fadeOutLookAtCoroutine = StartCoroutine( fadeOutLookAt( 0, 0.8f ) );
		}
	}

}
