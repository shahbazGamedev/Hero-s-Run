using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum XPAwardType
 {
	FINISHED_RACE = 1,
	WON = 2,
	FIRST_WIN_OF_THE_DAY = 3,
	CONSECUTIVE_RACE = 4
	
}

public class ProgressionManager : MonoBehaviour {

	[Header("General")]
	public static ProgressionManager Instance;
	public const int MAX_LEVEL = 100;
	public const int LEVEL_BANDS = 10;
	public const int MAX_XP_IN_ONE_RACE = 2450; //The maximum amount of XP a player can earn in a single race. Used for security checks.
	[SerializeField] List<int> xpNeededPerLevel = new List<int>(MAX_LEVEL);
	[SerializeField] List<XPAward> xpAwardList = new List<XPAward>();
	[SerializeField] List<Color> frameColorList = new List<Color>(LEVEL_BANDS);

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
	
}
