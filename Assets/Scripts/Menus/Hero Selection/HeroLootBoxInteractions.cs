using System.Collections;
using UnityEngine;

public class HeroLootBoxInteractions : MonoBehaviour {

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
}
