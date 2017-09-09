using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LootBoxCanvas : MonoBehaviour {

	int currentIndex = 0;
	int lootBoxesOwned = 0;
	LootBoxOwnedData selectedLootBoxData;
	GameObject lootBox;
	[SerializeField] Transform lootBoxSpawnLocation;
	GameObject hero;
	[SerializeField] Transform heroSpawnLocation;
	[SerializeField] TextMeshProUGUI lootBoxOwnedText;
	[SerializeField] TextMeshProUGUI lootBoxTypeText;
	[SerializeField] RadialTimerButton radialTimerButton;
	[SerializeField] List<LootBoxData> lootBoxDataList = new List<LootBoxData>();
	[SerializeField] Transform holder3D;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();
		loadHero();
		if( Debug.isDebugBuild ) addLootBoxesForTesting ();
		lootBoxesOwned = GameManager.Instance.playerInventory.getNumberOfLootBoxesOwned();
		if( lootBoxesOwned > 0 )
		{
			currentIndex = 0;
			configureLootBox();
		}
		else
		{
			radialTimerButton.isActive = false;
			lootBoxTypeText.text = LocalizationManager.Instance.getText( "LOOT_BOX_NAME_NONE_LEFT" );
		}
	}
	
	void loadHero()
	{
		int heroIndex = GameManager.Instance.playerProfile.selectedHeroIndex;
		HeroManager.HeroCharacter heroCharacter = HeroManager.Instance.getHeroCharacter( heroIndex );
		if( heroCharacter != null )
		{
			hero = GameObject.Instantiate( heroCharacter.skinPrefab, heroSpawnLocation.position, heroSpawnLocation.rotation );
			hero.transform.SetParent( holder3D );
			hero.transform.localScale = Vector3.one;
		}
	}

	void addLootBoxesForTesting ()
	{
		GameManager.Instance.playerInventory.removeAllLootBoxesOwned();
		GameManager.Instance.playerInventory.addLootBox( new LootBoxOwnedData( LootBoxType.LEVEL_UP, 1, 1 ) );
		GameManager.Instance.playerInventory.addLootBox( new LootBoxOwnedData( LootBoxType.RACE_WON, 1, 1 ) );
		GameManager.Instance.playerInventory.addLootBox( new LootBoxOwnedData( LootBoxType.FREE, 1, 1 ) );
		GameManager.Instance.playerInventory.addLootBox( new LootBoxOwnedData( LootBoxType.SECTOR_UNLOCKED, 1, 1 ) );
		GameManager.Instance.playerInventory.addLootBox( new LootBoxOwnedData( LootBoxType.LEVEL_UP, 2, 2 ) );
		GameManager.Instance.playerInventory.addLootBox( new LootBoxOwnedData( LootBoxType.RACE_WON, 2, 2 ) );
		GameManager.Instance.playerInventory.addLootBox( new LootBoxOwnedData( LootBoxType.FREE, 2, 2 ) );
	}

	public void OnClickLootBox()
	{
		LevelData.MultiplayerInfo multiplayerInfo = LevelManager.Instance.getLevelData().getRaceTrackByTrophies();
		Debug.Log( GameManager.Instance.playerProfile.getUserName() + " is requesting a client loot box of type " + selectedLootBoxData.type + ". His current sector is " + multiplayerInfo.circuitInfo.sectorNumber );
		LootBoxServerManager.Instance.requestLootBox( selectedLootBoxData.type, multiplayerInfo.circuitInfo.sectorNumber );

		GameManager.Instance.playerInventory.removeLootBoxAt( currentIndex );

		lootBoxesOwned = GameManager.Instance.playerInventory.getNumberOfLootBoxesOwned();
		if( lootBoxesOwned > 0 )
		{
			currentIndex++;
			if( currentIndex >= lootBoxesOwned )
			{
				currentIndex = 0;
			}
			configureLootBox();
		}
		else
		{
			radialTimerButton.isActive = false;
			lootBoxTypeText.text = LocalizationManager.Instance.getText( "LOOT_BOX_NAME_NONE_LEFT" );
		}
	}

	void configureLootBox()
	{
		selectedLootBoxData = GameManager.Instance.playerInventory.getLootBoxAt( currentIndex );
		lootBoxOwnedText.text = lootBoxesOwned.ToString();
		lootBoxTypeText.text = LocalizationManager.Instance.getText( "LOOT_BOX_NAME_" + selectedLootBoxData.type.ToString().ToUpper() );
		LootBoxData lootBoxData = getLootBoxData( selectedLootBoxData.type );
		print("lootBoxData.lootBoxPrefab " + lootBoxData.lootBoxPrefab.name );
		GameObject.DestroyImmediate( lootBox );
		lootBox = GameObject.Instantiate( lootBoxData.lootBoxPrefab, lootBoxSpawnLocation.position, lootBoxSpawnLocation.rotation );
		lootBox.transform.SetParent( holder3D );
		lootBox.transform.localScale = new Vector3( lootBoxData.lootBoxPrefab.transform.localScale.x, lootBoxData.lootBoxPrefab.transform.localScale.y, lootBoxData.lootBoxPrefab.transform.localScale.z );
	}

	public void OnClickPrevious()
	{
		if( lootBoxesOwned == 0 ) return;
		currentIndex--;
		if( currentIndex < 0 ) currentIndex = lootBoxesOwned  - 1;
		configureLootBox();
	}

	public void OnClickNext()
	{
		if( lootBoxesOwned == 0 ) return;
		currentIndex++;
		if( currentIndex >= lootBoxesOwned )
		{
			currentIndex = 0;
		}
		configureLootBox();
	}
	
	#region Loot Box Data
	public LootBoxData getLootBoxData( LootBoxType type )
	{
		LootBoxData lootBoxData = lootBoxDataList.Find( lbd => lbd.type == type);
		if( lootBoxData != null )
		{
			return lootBoxData;
		}
		else
		{
			Debug.LogError("LootBoxCanvas-Could not find LootBoxData for type: " + type + ". Please configure this type in the editor." );
			return null;
		}
	}
	#endregion

}
