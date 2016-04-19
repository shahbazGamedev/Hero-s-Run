using UnityEngine;
using System.Collections;

public class ChickenController : MonoBehaviour {

	bool wasTriggered = false; 		
	public float timeWasHit = 0; 	//Save the collision time to avoid multiple collision events on impact
	public bool wasHit = false;		//Save the fact that this chicken was hit so the player does not stumble a second time

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" )
		{
			Animation anim = transform.parent.GetComponent<Animation>();
			anim.CrossFade("A_Chicken_Panic");
			transform.parent.GetComponent<AudioSource>().Play();
			wasTriggered = true;
		}
	}

	void OnEnable()
	{
		PlayerController.playerStateChanged += PlayerStateChange;
	}
	
	void OnDisable()
	{
		PlayerController.playerStateChanged -= PlayerStateChange;
	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying && wasTriggered )
		{
			//Only destroy the chicken that was involved
			StartCoroutine( SoundManager.fadeOutClip( transform.parent.GetComponent<AudioSource>(), transform.parent.GetComponent<AudioSource>().clip, SoundManager.STANDARD_FADE_TIME ) );
			DestroyObject(transform.parent.gameObject );
		}
	}
}
