using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerUpDisplayData : MonoBehaviour {


	//List of each consumable powerup available.
	public List<ConsumablePowerUpData> powerUpList = new List<ConsumablePowerUpData>(4);
	static Dictionary<PowerUpType,ConsumablePowerUpData> powerUpDictionary = new Dictionary<PowerUpType,ConsumablePowerUpData>(4);
	static Texture2D powerUpAtlas;
	
	void Awake()
	{
		DontDestroyOnLoad( gameObject );
		fillDictionary();
		powerUpAtlas = Resources.Load("PowerUps/3D/Power_Ups") as Texture2D;
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
	
	public static Texture2D getPowerUpImage()
	{
		return powerUpAtlas;
	}

	public static Rect getPowerUpTexCoord( PowerUpType type )
	{
		ConsumablePowerUpData pud = powerUpDictionary[type];
		return pud.textureCoordinates;
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

	[System.Serializable]
	public class ConsumablePowerUpData
	{
		public PowerUpType powerUpType = PowerUpType.None;
		public string textID;
		public Rect textureCoordinates;
	}
}
