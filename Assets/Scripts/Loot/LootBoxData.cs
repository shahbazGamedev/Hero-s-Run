using System.Collections;
using UnityEngine;

[System.Serializable]
public class LootBoxData {

	public LootBoxType type; 
	public GameObject lootBoxPrefab;
	public int timeToUnlockInHours;
	public Sprite lootBoxSprite;
	public int unlockHardCurrencyCost = 0;
	public Vector2 containsThisRangeOfSoftCurrency = new Vector2 (0, 0);
	public int containsThisAmountOfCards = 0;
	public int containsAtLeastThisAmountOfRareCards = 0;
	public int containsAtLeastThisAmountOfEpicCards = 0;
	public int containsAtLeastThisAmountOfLegendaryCards = 0;

}
