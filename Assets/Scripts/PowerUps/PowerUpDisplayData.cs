using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerUpDisplayData : MonoBehaviour {


	//List of each consumable powerup available.
	public List<ConsumablePowerUpData> powerUpList = new List<ConsumablePowerUpData>(4);
	static Dictionary<PowerUpType,ConsumablePowerUpData> powerUpDictionary = new Dictionary<PowerUpType,ConsumablePowerUpData>(4);
	
	void Awake()
	{
		DontDestroyOnLoad( gameObject );
		fillDictionary();
	}

	void fillDictionary()
	{
		//Copy all of the power up data into a dictionary for convenient access
		powerUpDictionary.Clear();
		foreach(ConsumablePowerUpData consumablePowerUpData in powerUpList) 
		{
			powerUpDictionary.Add(consumablePowerUpData.powerUpType, consumablePowerUpData );
		}
		//We no longer need powerUpList
		powerUpList.Clear();
		powerUpList = null;
	}
	
	public static string getPowerUpName( PowerUpType type )
	{
		ConsumablePowerUpData pud = powerUpDictionary[type];
		return LocalizationManager.Instance.getText( pud.textID );
	}

	public static Sprite getPowerUpSprite( PowerUpType type )
	{
		ConsumablePowerUpData pud = powerUpDictionary[type];
		return pud.powerupSprite;
	}

	public static PowerUpType getNextPowerUp()
	{
		int selectedPowerUpIndex = (int)PlayerStatsManager.Instance.getPowerUpSelected();
		//we want the next one
		selectedPowerUpIndex++;
		//Make sure we stay within the range of the consumable power-ups: ZNuke = 3, MagicBoots = 4, SlowTime = 5
		if( selectedPowerUpIndex > 5 ) selectedPowerUpIndex = 3;
		return (PowerUpType)selectedPowerUpIndex;

	}

	public static PowerUpType getPreviousPowerUp()
	{
		int selectedPowerUpIndex = (int)PlayerStatsManager.Instance.getPowerUpSelected();
		//we want the previous one
		selectedPowerUpIndex--;
		//Make sure we stay within the range of the consumable power-ups: ZNuke = 3, MagicBoots = 4, SlowTime = 5
		if( selectedPowerUpIndex < 3 ) selectedPowerUpIndex = 5;
		return (PowerUpType)selectedPowerUpIndex;
		
	}

	public static float getUpgradeBoostValue( PowerUpType type )
	{
		ConsumablePowerUpData pud = powerUpDictionary[type];
		return pud.upgradeBoost;		
	}

	[System.Serializable]
	public class ConsumablePowerUpData
	{
		public PowerUpType powerUpType = PowerUpType.None;
		public string textID;
		public Sprite powerupSprite;
		[Tooltip("How much time in seconds or meters (in the case of the ZNuke) is added for each upgrade level.")]
		//Duration is extended by the upgrade level * upgradeBoost or
		//Impact diameter is extended by the upgrade level * upgradeBoost
		public float upgradeBoost;
	}
}
