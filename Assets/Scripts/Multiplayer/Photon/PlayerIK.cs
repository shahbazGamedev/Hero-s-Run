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
	public Transform lookAtTarget;
	Animator anim;
	[SerializeField] float lookAtWeight = 0.8f;
	[SerializeField] float bodyWeight = 0.7f;
	[SerializeField] float headWeight = 1f;
	[SerializeField] float eyesWeight = 1f;
	[SerializeField] float clampWeight = 1f;
	[SerializeField] float activeDistanceSquared = 50f * 50f;
	[SerializeField] float dotProductIK = 0.55f;
	public bool lookAtActive = false;
	public bool enableIK = true; //this could be used to disable IK in case the rig does not support it or the performance on the device suffers

	void Awake ()
	{
		anim = GetComponent<Animator>();
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
		if( lookAtTarget != null && enableIK && getDotProduct() > dotProductIK )
		{
			float distance = Vector3.SqrMagnitude(lookAtTarget.position - transform.position);
			if( distance < activeDistanceSquared )			
			{
				anim.SetLookAtPosition( lookAtTarget.position );
				anim.SetLookAtWeight( lookAtWeight, bodyWeight, headWeight, eyesWeight, clampWeight );
			}
			else
			{
				//Look-at target is no longer in range
				StartCoroutine( fadeOutLookAtPosition( 0.2f, 0.9f ) );									
			}
		}
	}

	IEnumerator setLookAtTarget( float activationDelay, Transform lookAtTarget )
	{
		//Ignore null targets
		if( lookAtTarget == null  ) yield break;

		print("setLookAtTarget " + lookAtTarget.name );

		//Don't interrupt the current look-at if one is active
		if( lookAtActive ) yield break;

		//Don't do anything if the proposed target is too far
		float distance = Vector3.SqrMagnitude(lookAtTarget.position - transform.position);
		if( distance > activeDistanceSquared ) yield break;		

		//Wait before activating
		yield return new WaitForSeconds(activationDelay);

		this.lookAtTarget = lookAtTarget;
		StartCoroutine( fadeInLookAtPosition( 0.8f, 0.6f, 3f ) );
	}

	IEnumerator fadeInLookAtPosition( float finalWeight, float fadeDuration, float stayDuration  )
	{
		lookAtActive = true;
		float elapsedTime = 0;

		//Fade in
		elapsedTime = 0;
		
		float initialWeight = 0;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			lookAtWeight = Mathf.Lerp( initialWeight, finalWeight, elapsedTime/fadeDuration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < fadeDuration );
		
		lookAtWeight = finalWeight;

		//Stay
		yield return new WaitForSeconds(stayDuration);

		//Fade out
		StartCoroutine( fadeOutLookAtPosition( 0.2f, 0.5f ) );
	
	}

	IEnumerator fadeOutLookAtPosition( float finalWeight, float fadeDuration )
	{
		float elapsedTime = 0;
		
		//Fade out
		elapsedTime = 0;
		
		float initialWeight = lookAtWeight;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			lookAtWeight = Mathf.Lerp( initialWeight, finalWeight, elapsedTime/fadeDuration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < fadeDuration );
		
		lookAtWeight = finalWeight;
		lookAtTarget = null;
		lookAtActive = false;
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
				string heroName;
				if( playerOvertaking.GetComponent<PlayerAI>() == null )
				{
					//A the player
					heroName = HeroManager.Instance.getHeroCharacter( GameManager.Instance.playerProfile.selectedHeroIndex ).name;
				}
				else
				{
					//We're a bot
					heroName = 	playerOvertaking.GetComponent<PlayerAI>().botHero.userName;
				}
				//Now look-at the player after a short delay.
				//We don't want the player to start looking at a right angle. We want the opponent to be
				//about 0.05 * 20 (average run speed) = 1 meter in front before looking.
				StartCoroutine( setLookAtTarget( 0.05f, playerOvertaking.transform ) );
			}
			else
			{
				Debug.LogError("PlayerIK: could not find overtaking player in race position: " + (racePosition - 1) );
			}
		}
	}

	public void playerDied()
	{
		StopCoroutine("setLookAtTarget");

		if( lookAtTarget != null && enableIK ) StartCoroutine( fadeOutLookAtPosition( 0.2f, 0.9f ) );

		//Advise other players of death so that they can do a look-at
		for( int i = 0; i < PlayerRace.players.Count; i ++ )
		{
			if( PlayerRace.players[i] != this )
			{
				StartCoroutine( PlayerRace.players[i].GetComponent<PlayerIK>().setLookAtTarget( 0, transform ) );
			}
		}
	}

}
