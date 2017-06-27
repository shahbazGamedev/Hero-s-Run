using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum XPAwardType
 {
	FINISHED_RACE = 1,
	WON = 2,
	FIRST_WIN_OF_THE_DAY = 3,
	CONSECUTIVE_RACE = 4
}

public enum EloRating
 {
	BEGINNER = 1,
	INTERMEDIATE = 2,
	ADVANCED = 3,
	COMPETITIVE = 4
}

public class ProgressionManager : MonoBehaviour {

	[Header("General")]
	public static ProgressionManager Instance;
	public const int MAX_NUMBER_OF_TRACKS = 10;
	public const int MAX_LEVEL = 100;
	public const int LEVEL_BANDS = 10;
	public const int MAX_XP_IN_ONE_RACE = 2450; //The maximum amount of XP a player can earn in a single race. Used for security checks.
	const int ELO_MAX_LEVEL_BEGINNER = 15;
	[SerializeField] List<int> xpNeededPerLevel = new List<int>(MAX_LEVEL);
	[SerializeField] List<XPAward> xpAwardList = new List<XPAward>();
	[SerializeField] List<Color> frameColorList = new List<Color>(LEVEL_BANDS);
	[SerializeField] List<IconData> iconList = new List<IconData>();

	// Use this for initialization
	void Awake ()
	{
		if(Instance)
		{
			DestroyImmediate(gameObject);
		}
		else
		{
			DontDestroyOnLoad(gameObject);
			Instance = this;
		}
	}

	//Returns the level between 1 and 100
	public int getLevel( int xp )
	{
		int xpSum = 0;
		for( int i = 0; i < xpNeededPerLevel.Count; i++ )
		{
			xpSum = xpSum + xpNeededPerLevel[i];
			if( xpSum >= xp ) return (i + 1 ); //levels start at 1 not 0
		}
		return MAX_LEVEL;
	}

	//Used to match players of similar skill.
	//Only two player pools are used for now: BEGINNER (Elo rating = 1) and ADVANCED (Elo rating = 3).
	//Level parameter is between 1 and MAX_LEVEL.
	public int getEloRating( int level )
	{
		if( level > 0 && level <= ProgressionManager.MAX_LEVEL )
		{
			if( level >= 1 && level <= ELO_MAX_LEVEL_BEGINNER )
			{
				return (int)EloRating.BEGINNER;
			}
			else
			{
				return (int)EloRating.ADVANCED;
			}
		}
		else
		{
			Debug.LogWarning("ProgressionManager-getEloRating: the level specified," + level + " is incorrect. It needs to be between 1 and 100.");
			return (int)EloRating.BEGINNER;
		}
	}

	public int getXPRequired( int level )
	{
		level--;
		return xpNeededPerLevel[level];
	}

	public int getTotalXPRequired( int level )
	{
		int xpSum = 0;
		for( int i = 0; i < level; i++ )
		{
			xpSum = xpSum + xpNeededPerLevel[i];
		}
		return xpSum;
	}

	//Level parameter is between 1 and MAX_LEVEL
	//There are 10 level bands each with LEVEL_BANDS levels.
	public Color getFrameColor( int level )
	{
		if( level > 0 && level <= ProgressionManager.MAX_LEVEL )
		{
			level--; //frameColorList is zero indexed
			int frameIndex = (int)Mathf.Floor(level/(float)LEVEL_BANDS);
			return frameColorList[frameIndex];
		}
		else
		{
			Debug.LogWarning("ProgressionManager-getFrameColor: the level specified," + level + " is incorrect. It needs to be between 1 and 100.");
			return Color.clear;
		}
	}

	public XPAward getXPAward( XPAwardType awardType )
	{
		return xpAwardList.Find(award => award.awardType == awardType);
	}

	[System.Serializable]
	public class XPAward
	{
		public XPAwardType awardType;
		public string awardTextID = string.Empty;
		public int xpAmount = 0;
	}
	
	#region Icon Data
	public bool doesPlayerIconExist( int uniqueId )
	{
		return iconList.Exists(icon => icon.uniqueId == uniqueId);
	}

	public IconData getPlayerIconSpriteByUniqueId( int uniqueId )
	{
		if( iconList.Exists(icon => icon.uniqueId == uniqueId) )
		{
			return iconList.Find(icon => icon.uniqueId == uniqueId);
		}
		else
		{
			Debug.LogWarning("ProgressionManager-the player icon with id " + uniqueId + " could not be found." );
			return null;
		}
	}

	public List<IconData> getPlayerIcons()
	{
		return iconList;
	}

	public int getRandomPlayerIconUniqueId()
	{
		int randomNumber = Random.Range(0, iconList.Count );
		return iconList[randomNumber].uniqueId;
	}

	public int getNumberOfPlayerIcons()
	{
		return iconList.Count;
	}

	[System.Serializable]
	public class IconData
	{
		//Unique ID to identify the player icon.
		public int uniqueId = 0; 
		public Sprite icon;
		//New players have a few icons that are immediately available to them.
		//All other icons need to be found, for example in loot boxes.
		public bool isDefaultIcon;
	}
	#endregion
	
}
