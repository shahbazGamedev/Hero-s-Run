using UnityEngine;
using System.Collections;

public class ChickenController : MonoBehaviour {

	bool wasTriggered = false; 		
	public float timeWasHit = 0; 	//Save the collision time to avoid multiple collision events on impact
	public bool wasHit = false;		//Save the fact that this chicken was hit so the player does not stumble a second time

	void OnTriggerEnter(Collider other)
	{
		if( other.CompareTag("Player") )
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

	void PlayerStateChange( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying && wasTriggered )
		{
			//Only destroy the chicken that was involved
			DestroyObject(transform.parent.gameObject );
		}
	}
}
