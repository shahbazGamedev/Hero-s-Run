using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TileGroupManager : MonoBehaviour {

	[Header("Tile Group Manager")]
	[Tooltip("List of pre-configured tile groups. Edit these in the Level scene. Tile groups are serializable and will be saved with the scene.")]
	public List<TileGroup> tileGroupList = new List<TileGroup>();
	//This dictionary is only used at runtime.
	Dictionary<TileGroupType, TileGroup> tileGroupDictionary = new Dictionary<TileGroupType, TileGroup>();
	
	// Use this for initialization
	void Awake ()
	{
		fillDictionary();
	}

	public void sortTileGroups()
	{
		Debug.Log("sortTileGroups");
		tileGroupList.Sort((x, y) => x.tileGroupType.CompareTo(y.tileGroupType));
	}

	void fillDictionary()
	{
		//Copy the tile group data into a dictionary for convenient access
		tileGroupDictionary.Clear();
		for( int i = 0; i < tileGroupList.Count; i++ )
		{
			tileGroupDictionary.Add(tileGroupList[i].tileGroupType, tileGroupList[i] );
		}
		//We no longer need tileGroupList
		tileGroupList.Clear();
		tileGroupList = null;
	}

	//Returns the specified tile group.
	public TileGroup getTileGroup( TileGroupType tileGroupType )
	{
		if( tileGroupDictionary.ContainsKey( tileGroupType ) )
		{
			return tileGroupDictionary[tileGroupType];
		}
		else
		{
			Debug.LogError("TileGroupManager-getTileGroup error: unable to find TileGroupType " + tileGroupType + " in dictionary." );
			return null;
		}
	}

	//Returns a random tile group that belongs to the specified theme.
	//Only used in Endless mode.
	//Only tile groups with a non-zero frequency will be returned.
	//In addition, only tiles groups with an Endless or Any ValidGameMode will be returned.
	//The frequency of a tile group being selected is based on how big the frequency for that tile group is compared to that of the other tile groups.
	public TileGroup getRandomTileGroup( SegmentTheme theme )
	{
		//Calculate the point total
		int totalFrequencyPoints = 0;
		TileGroup tg;
		List<TileGroup> tileGroupListForTheme = new List<TileGroup>();
		foreach(KeyValuePair<TileGroupType, TileGroup> pair in tileGroupDictionary) 
		{
			tg = pair.Value;
			if( tg.tileGroupType != TileGroupType.Mines_Start && tg.theme == theme && tg.frequency != TileGroup.FrequencyType.Never && ( tg.validGameMode == ValidGameMode.Endless || tg.validGameMode == ValidGameMode.Any ) )
			{
				totalFrequencyPoints = totalFrequencyPoints + (int)tg.frequency;
				tileGroupListForTheme.Add( tg );
			}
		}
		//Generate a a random number
		int rand = Random.Range( 0, totalFrequencyPoints );
		//Use the number to chose a valid, random tile group
		totalFrequencyPoints = 0;
		for( int i=0; i < tileGroupListForTheme.Count; i++ )
		{
			tg = tileGroupListForTheme[i];
			totalFrequencyPoints = totalFrequencyPoints + (int)tg.frequency;
			if( rand < totalFrequencyPoints )
			{
				return tg;
			}
		}
		Debug.LogError("TileGroupManager-getRandomTileGroup: unable to find a random tile group for : " + theme );
		return null;
	}
}
