using System.Collections;
using UnityEngine;

public enum LootBoxStatus
{
	NOT_INITIALIZED = 0,
	STARTED_TO_FALL = 1,
	HIT_GROUND = 2,
	DESTROYED = 3
}

public class LootBoxHandler : MonoBehaviour {

	[SerializeField] float fallEventDelay = 0.4f;
	LootBoxStatus state = LootBoxStatus.NOT_INITIALIZED;
	bool hasHitTheGround = false;

	#region Events
	public delegate void LootBoxEvent( LootBoxStatus value, Transform lootBox );
	public static event LootBoxEvent lootBoxEvent;
	#endregion

	void Start ()
	{
		Invoke( "sendFallStartedEvent", fallEventDelay );
	}

	void sendFallStartedEvent()
	{
		if( lootBoxEvent != null ) lootBoxEvent( LootBoxStatus.STARTED_TO_FALL, transform );	
	}

	void OnDisable ()
	{
		if( lootBoxEvent != null ) lootBoxEvent( LootBoxStatus.DESTROYED, transform );	
	}

	void OnCollisionEnter(Collision collision)
	{
		float maximumImpactStrength = 300f;
		float impactStrength = collision.relativeVelocity.sqrMagnitude;
		if( impactStrength > 0 )
		{
			//The collision.relativeVelocity.sqrMagnitude ranges between 0 and 300 or so.
			//The higher the impact strength, the louder the sound.
			impactStrength = Mathf.Clamp( impactStrength, 0, maximumImpactStrength );
			GetComponent<AudioSource>().PlayOneShot( GetComponent<AudioSource>().clip, impactStrength/maximumImpactStrength );
		}

		if( !hasHitTheGround )
		{
			lootBoxEvent( LootBoxStatus.HIT_GROUND, transform );
			//Play an impact particle effect
			ParticleSystem impact = GetComponentInChildren<ParticleSystem>();
			impact.Play();
		}
		hasHitTheGround = true;
	}
}
