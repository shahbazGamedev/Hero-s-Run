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

public class XPManager : MonoBehaviour {

	[Header("General")]
	public static XPManager Instance;
	public const int MAX_LEVEL = 100;
	[SerializeField] List<int> xpNeededPerLevel = new List<int>(MAX_LEVEL);
	[SerializeField] List<XPAward> xpAwardList = new List<XPAward>();

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
