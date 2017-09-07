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

	const int MAXIMUM_INCREASE_TO_COIN_BALANCE = 100000;
	const int MAXIMUM_INCREASE_TO_GEM_BALANCE = 14000;
	[SerializeField] int currentCoins = 0;
	[SerializeField] int currentGems = 25;
	[SerializeField] int currentCrowns = 0;
	//lastDisplayedCrownBalance is used in the Main Menu to determine if the number of crowns has changed.
	[SerializeField] int lastDisplayedCrownBalance = 0;

	[SerializeField] List<LootBoxData> lootBoxesOwned = new List<LootBoxData>();

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
			serializePlayerInventory( true );
			Debug.Log("PlayerInventory-setting current gems to: " + value );
		}
		else
		{
			Debug.LogWarning("PlayerInventory-the gem value specified " + value + " is incorrect. It needs to be zero or greater." );
		}
	}
	#endregion

	#region Crowns
	public int getLastDisplayedCrownBalance()
	{
		return lastDisplayedCrownBalance;
	}

	public void saveLastDisplayedCrownBalance( int value )
	{
		lastDisplayedCrownBalance = value;
		serializePlayerInventory( true );
	}

	public int getCrownBalance()
	{
		return currentCrowns;
	}

	/// <summary>
	/// Sets the crown balance to zero. Does not save the value.
	/// </summary>
	public void resetCrowns()
	{
		setCrownBalance(0);
	}

	public void addCrowns( int crownAmount )
	{
		if( crownAmount >= 0 && crownAmount <= CrownLootBoxHandler.CROWNS_NEEDED_TO_OPEN )
		{
			//You cannot store more than CROWNS_NEEDED_TO_OPEN crowns.
			//If the you have reached the maximum number of crowns, you must open the crown loot box to reset the count to 0.
			int newCrownAmount = currentCrowns + crownAmount;
			if( newCrownAmount > CrownLootBoxHandler.CROWNS_NEEDED_TO_OPEN ) newCrownAmount = CrownLootBoxHandler.CROWNS_NEEDED_TO_OPEN;
	
			if( currentCrowns != newCrownAmount ) setCrownBalance( newCrownAmount );
		}
		else
		{
			Debug.LogWarning("PlayerInventory-the crown value specified " + crownAmount + " is incorrect. It needs to be between 0 and " + CrownLootBoxHandler.CROWNS_NEEDED_TO_OPEN.ToString() + ".");
		}
	}

	void setCrownBalance( int value )
	{
		if( value >= 0 )
		{
			if( playerInventoryChangedNew != null ) playerInventoryChangedNew( PlayerInventoryEvent.Crown_Balance_Changed, currentCrowns, value );
			currentCrowns = value;
			//Save
			serializePlayerInventory( true );
			Debug.Log("PlayerInventory-setting current crowns to: " + value );
		}
		else
		{
			Debug.LogWarning("PlayerInventory-the crown value specified " + value + " is incorrect. It needs to be zero or greater." );
		}
	}
	#endregion

	#region Loot Boxes
	public List<LootBoxData> getAllLootBoxesOwned()
	{
		return lootBoxesOwned;
	}

	public void addLootBox( LootBoxData lootBoxData )
	{
		lootBoxesOwned.Add( lootBoxData );
		Debug.Log("PlayerInventory - addLootBox: " + lootBoxData.ToString() );
	}

	public void removeLootBox( LootBoxData lootBoxData )
	{
		Debug.Log("PlayerInventory - removeLootBox: " + lootBoxData.ToString() );
		lootBoxesOwned.Remove( lootBoxData );
	}

	public int getNumberOfLootBoxesOwned()
	{
		return lootBoxesOwned.Count;
	}
	#endregion

	public void serializePlayerInventory( bool saveImmediately )
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setPlayerInventory( json );
		if( saveImmediately ) PlayerStatsManager.Instance.savePlayerStats();
	}
}

