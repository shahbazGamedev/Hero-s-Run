using System.Collections;
using UnityEngine;

public class HeroLootBoxInteractions : MonoBehaviour {

	[SerializeField] AudioClip footStep;

	void OnEnable()
	{
		LootBoxHandler.lootBoxEvent += LootBoxEvent;
	}
	
	void OnDisable()
	{
		LootBoxHandler.lootBoxEvent -= LootBoxEvent;
	}

	void LootBoxEvent( LootBoxStatus state, Transform target )
	{
		switch( state )
		{
			case LootBoxStatus.HIT_GROUND:
				GetComponent<Animator>().SetTrigger("LootBoxLanded");
				break;
		}
	}

	public void Victory_win_start ( AnimationEvent eve )
	{
		//Not used.
	}

	public void Win_footstep_left ( AnimationEvent eve )
	{
		GetComponent<AudioSource>().PlayOneShot( footStep, 0.2f  );
	}

	public void Win_footstep_right ( AnimationEvent eve )
	{
		GetComponent<AudioSource>().PlayOneShot( footStep, 0.2f  );
	}
}
