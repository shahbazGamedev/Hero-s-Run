using System.Collections;
using UnityEngine;

public class AnimatedLootBox : MonoBehaviour {

	[SerializeField] GameObject lid;
	LootBoxType lootBoxType = LootBoxType.FREE;
	bool isOpen = false;

	//Event management used to notify other classes when a loot box has been opened
	public delegate void LootBoxOpenedEvent( LootBoxType value );
	public static event LootBoxOpenedEvent lootBoxOpenedEvent;

	public void setLootBoxType ( LootBoxType value )
	{
		lootBoxType = value;
	}

	public void Open ()
	{
		if( isOpen ) return;
		isOpen = true;
		LeanTween.rotateX( lid, -60f, 0.4f ).setEaseOutCubic();
		GetComponent<AudioSource>().Play();
		if( lootBoxOpenedEvent != null ) lootBoxOpenedEvent( lootBoxType );
	}
	
}
