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
	Gem_Balance_Changed = 6,
	Crown_Balance_Changed = 7
}

[System.Serializable]
public class PlayerInventory {

	const int MAXIMUM_INCREASE_TO_SOFT_CURRENCY_BALANCE = 100000;
	const int MAXIMUM_INCREASE_TO_HARD_CURRENCY_BALANCE = 14000;
	[SerializeField] int currentCoins = 0;
	[SerializeField] int currentGems = 25;

	//lastDisplayedLootBoxesOwned is used in the Main Menu to determine if the number of loot boxes owned has changed.
	[SerializeField] int lastDisplayedLootBoxesOwned = 0;
	[SerializeField] List<LootBoxOwnedData> lootBoxesOwned = new List<LootBoxOwnedData>();

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
		if( coinAmount >= 0 && coinAmount <= MAXIMUM_INCREASE_TO_SOFT_CURRENCY_BALANCE )
		{
			setCoinBalance( currentCoins + coinAmount );
		}
		else
		{
			Debug.LogWarning("PlayerInventory-the coin value specified " + coinAmount + " is incorrect. It needs to be between 0 and " + MAXIMUM_INCREASE_TO_SOFT_CURRENCY_BALANCE.ToString() + ".");
		}
	}

	void setCoinBalance( int value )
	{
		if( value >= 0 )
		{
			if( playerInventoryChangedNew != null ) playerInventoryChangedNew( PlayerInventoryEvent.Coin_Changed, currentCoins, value );
			currentCoins = value;
			//Save
			serializePlayerInventory( true );
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
		if( gemAmount >= 0 && gemAmount <= MAXIMUM_INCREASE_TO_HARD_CURRENCY_BALANCE )
		{
			setGemBalance( currentGems + gemAmount );
		}
		else
		{
			Debug.LogWarning("PlayerInventory-the gem value specified " + gemAmount + " is incorrect. It needs to be between 0 and " + MAXIMUM_INCREASE_TO_HARD_CURRENCY_BALANCE.ToString() + ".");
		}
	}

	void setGemBalance( int value )
	{
		if( value >= 0 )
		{
			if( playerInventoryChangedNew != null ) playerInventoryChangedNew( PlayerInventoryEvent.Gem_Balance_Changed, currentGems, value );
			currentGems = value;
			//Save
			serializePlayerInventory( true );
			Debug.Log("PlayerInventory-setting current gems to: " + value );
		}
		else
		{
			Debug.LogWarning("PlayerInventory-the gem value specified " + value + " is incorrect. It needs to be zero or greater." );
		}
	}
	#endregion

	#region Loot Boxes
	public List<LootBoxOwnedData> getAllLootBoxesOwned()
	{
		return lootBoxesOwned;
	}

	public void addLootBox( LootBoxOwnedData lootBoxOwnedData )
	{
		lootBoxesOwned.Add( lootBoxOwnedData );
		Debug.Log("PlayerInventory - addLootBox: " + lootBoxOwnedData.ToString() );
	}

	public void removeLootBox( LootBoxOwnedData lootBoxOwnedData )
	{
		Debug.Log("PlayerInventory - removeLootBox: " + lootBoxOwnedData.ToString() );
		lootBoxesOwned.Remove( lootBoxOwnedData );
	}

	public LootBoxOwnedData getLootBoxAt( int index )
	{
		if( index < 0 || index >= lootBoxesOwned.Count ) return null;
		return lootBoxesOwned[index];
	}

	public void removeLootBoxAt( int index )
	{
		if( index < 0 || index >= lootBoxesOwned.Count ) return;
		lootBoxesOwned.RemoveAt( index );
	}

	public void removeAllLootBoxesOwned()
	{
		lootBoxesOwned.Clear();
	}

	public int getNumberOfLootBoxesOwned()
	{
		return lootBoxesOwned.Count;
	}

	public int getLastDisplayedLootBoxesOwned()
	{
		return lastDisplayedLootBoxesOwned;
	}

	public void saveLastDisplayedLootBoxesOwned( int value )
	{
		lastDisplayedLootBoxesOwned = value;
		serializePlayerInventory( true );
	}
	#endregion

	public void serializePlayerInventory( bool saveImmediately )
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setPlayerInventory( json );
		if( saveImmediately ) PlayerStatsManager.Instance.savePlayerStats();
	}
}

