using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerInventoryEvent {
	Key_Changed = 0,
	Life_Changed = 1,
	Coin_Changed = 2,
	Coin_Doubler_Changed = 3,
	Score_Changed = 4,
	Key_Found_In_Episode_Changed = 5,
	Gem_Balance_Changed = 6
}

[System.Serializable]
public class PlayerInventory {

	const int MAXIMUM_INCREASE_TO_COIN_BALANCE = 100000;
	const int MAXIMUM_INCREASE_TO_GEM_BALANCE = 14000;
	[SerializeField] int currentCoins = 0;
	[SerializeField] int currentGems = 25;

	//Delegate used to communicate to other classes when an inventory value changes such as the gem balance
	public delegate void PlayerInventoryChangedNew( PlayerInventoryEvent eventType, int previousValue, int newValue );
	public static event PlayerInventoryChangedNew playerInventoryChangedNew;

	#region Coins
	public int getCoinBalance()
	{
		return currentCoins;
	}

	public void deductCoins( int coinAmount )
	{
		if( currentCoins >= coinAmount )
		{
			setCoinBalance( currentCoins - coinAmount );
		}
		else
		{
			Debug.LogWarning("PlayerInventory-the coin amount you want to deduct " + coinAmount + " is bigger than your current balance " + currentCoins + "." );
		}
	}

	public void addCoins( int coinAmount )
	{
		if( coinAmount >= 0 && coinAmount <= MAXIMUM_INCREASE_TO_COIN_BALANCE )
		{
			setCoinBalance( currentCoins + coinAmount );
		}
		else
		{
			Debug.LogWarning("PlayerInventory-the coin value specified " + coinAmount + " is incorrect. It needs to be between 0 and " + MAXIMUM_INCREASE_TO_COIN_BALANCE.ToString() + ".");
		}
	}

	void setCoinBalance( int value )
	{
		if( value >= 0 )
		{
			if( playerInventoryChangedNew != null ) playerInventoryChangedNew( PlayerInventoryEvent.Coin_Changed, currentCoins, value );
			currentCoins = value;
			//Save
			serializePlayerInventory();
			Debug.Log("PlayerInventory-setting current coins to: " + value );
		}
		else
		{
			Debug.LogWarning("PlayerInventory-the coin value specified " + value + " is incorrect. It needs to be zero or greater.");
		}
	}
	#endregion

	#region Gems
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

	public void addGems( int gemAmount )
	{
		if( gemAmount >= 0 && gemAmount <= MAXIMUM_INCREASE_TO_GEM_BALANCE )
		{
			setGemBalance( currentGems + gemAmount );
		}
		else
		{
			Debug.LogWarning("PlayerInventory-the gem value specified " + gemAmount + " is incorrect. It needs to be between 0 and " + MAXIMUM_INCREASE_TO_GEM_BALANCE.ToString() + ".");
		}
	}

	void setGemBalance( int value )
	{
		if( value >= 0 )
		{
			if( playerInventoryChangedNew != null ) playerInventoryChangedNew( PlayerInventoryEvent.Gem_Balance_Changed, currentGems, value );
			currentGems = value;
			//Save
			serializePlayerInventory();
			Debug.Log("PlayerInventory-setting current gems to: " + value );
		}
		else
		{
			Debug.LogWarning("PlayerInventory-the gem value specified " + value + " is incorrect. It needs to be zero or greater." );
		}
	}
	#endregion

	public void serializePlayerInventory()
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setPlayerInventory( json );
		PlayerStatsManager.Instance.savePlayerStats();
	}
}

