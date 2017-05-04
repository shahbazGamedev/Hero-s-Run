using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInventory {

	const int MAXIMUM_INCREASE_TO_GEM_BALANCE = 14000;
	int currentGems = 23;

	//Delegate used to communicate to other classes when an inventory value changes such as the gem balance
	public delegate void PlayerInventoryChangedNew( PlayerInventoryEvent eventType, int newValue );
	public static event PlayerInventoryChangedNew playerInventoryChangedNew;

	public int getGemBalance()
	{
		return currentGems;
	}

	public void deductGems( int gemAmount )
	{
		if( currentGems >= gemAmount )
		{
			setGemBalance( currentGems - gemAmount );
		}
		else
		{
			Debug.LogWarning("PlayerInventory-the gem amount you want to deduct " + gemAmount + " is bigger than your current balance " + currentGems + "." );
		}
	}

	public void setGemBalance( int value )
	{
		if( value >= 0 && value <= MAXIMUM_INCREASE_TO_GEM_BALANCE )
		{
			currentGems = value;
			if( playerInventoryChangedNew != null ) playerInventoryChangedNew( PlayerInventoryEvent.Gem_Balance_Changed, currentGems );
			Debug.Log("PlayerInventory-setting current gems to: " + value );
		}
		else
		{
			Debug.LogWarning("PlayerInventory-the gem value specified " + value + " is incorrect. It needs to be between 0 and " + MAXIMUM_INCREASE_TO_GEM_BALANCE.ToString() + ".");
		}
	}

	public void serializePlayerprofile()
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setPlayerInventory( json );
		PlayerStatsManager.Instance.savePlayerStats();
	}
}

