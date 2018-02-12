using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum XPAwardType
 {
	//Competition
	FINISHED_RACE = 1,
	WON = 2,
	FIRST_WIN_OF_THE_DAY = 3,
	CONSECUTIVE_RACE = 4,
	SKILL_BONUS = 5,
	//Coop
	SCORE_BONUS = 100,
	WAVE_BONUS = 101
}

public class ProgressionManager : MonoBehaviour {

	[Header("General")]
	public static ProgressionManager Instance;
	public const int MAX_NUMBER_OF_TRACKS = 10;
	public const int MIN_LEVEL = 1;
	public const int MAX_LEVEL = 100;
	public const int LEVEL_BANDS = 10;
	public const int MAX_XP_IN_ONE_RACE = 2450; //The maximum amount of XP a player can earn in a single race. Used for security checks.
	[SerializeField] List<int> xpNeededPerLevel = new List<int>(MAX_LEVEL);
	[SerializeField] List<XPData> xpPerLevel = new List<XPData>(MAX_LEVEL);
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
			copyXPData();
			//testMethods();
		}
	}

	private void copyXPData()
	{
		int xpSum = 0;
		XPData xpData;
		for( int i = 0; i < xpNeededPerLevel.Count; i++ )
		{
			xpSum = xpSum + xpNeededPerLevel[i];
			xpData = new XPData( xpNeededPerLevel[i], xpSum );
			xpPerLevel.Add( xpData );
		}
	}

	private void testMethods()
	{
		Debug.Log("ProgressionManager-getLevel XP:\t0 Answer: 1\tResults: " + getLevel(    0 ) );
		Debug.Log("ProgressionManager-getLevel XP:\t1000 Answer: 1\tResults: " + getLevel( 1000 ) );
		Debug.Log("ProgressionManager-getLevel XP:\t1500 Answer: 2\tResults: " + getLevel( 1500 ) );
		Debug.Log("ProgressionManager-getLevel XP:\t1501 Answer: 2\tResults: " + getLevel( 1501 ) );
		Debug.Log("ProgressionManager-getLevel XP:\t2000 Answer: 2\tResults: " + getLevel( 2000 ) );
		Debug.Log("ProgressionManager-getLevel XP:\t3001 Answer: 2\tResults: " + getLevel( 3001 ) );
		Debug.Log("ProgressionManager-getLevel XP:\t3500 Answer: 2\tResults: " + getLevel( 3500 ) );
		Debug.Log("ProgressionManager-getLevel XP:\t4500 Answer: 3\tResults: " + getLevel( 4500 ) );
		Debug.Log("ProgressionManager-getLevel XP:\t4501 Answer: 3\tResults: " + getLevel( 4501 ) );
		Debug.Log("ProgressionManager-getLevel XP:\t2000000 Answer: 100\tResults: " + getLevel( 2000000 ) );
		Debug.Log("\n" );
		Debug.Log("ProgressionManager-getTotalXPRequired LVL:\t0 Answer: 0\tResults: " + getTotalXPRequired( 0 ) );
		Debug.Log("ProgressionManager-getTotalXPRequired LVL:\t1 Answer: 0\tResults: " + getTotalXPRequired( MIN_LEVEL ) );
		Debug.Log("ProgressionManager-getTotalXPRequired LVL:\t2 Answer: 1500\tResults: " + getTotalXPRequired( 2 ) );
		Debug.Log("ProgressionManager-getTotalXPRequired LVL:\t3 Answer: 4500\tResults: " + getTotalXPRequired( 3 ) );
		Debug.Log("ProgressionManager-getTotalXPRequired LVL:\t100 Answer: 1986500\tResults: " + getTotalXPRequired( MAX_LEVEL ) );
	}

	/// <summary>
	/// Returns the level based on the numper of XPs.
	/// The level returned is between MIN_LEVEL and MAX_LEVEL.
	/// A new player with 0 XP has a level of MIN_LEVEL.
	/// </summary>
	/// <returns>The level.</returns>
	/// <param name="xp">Xp.</param>
	public int getLevel( int xp )
	{
		if( xp == 0 ) return MIN_LEVEL;

		if( xp >= xpPerLevel[MAX_LEVEL].totalXPRequired ) return MAX_LEVEL;

		return xpPerLevel.FindLastIndex( xpd => xpd.totalXPRequired <= xp );
	}

	/// <summary>
	/// Returns the XP needed to reach the specicied level.
	/// </summary>
	/// <returns>The total XP required.</returns>
	/// <param name="level">Level.</param>
	public int getTotalXPRequired( int level )
	{
		if( level >= MIN_LEVEL && level <= ProgressionManager.MAX_LEVEL )
		{
		return xpPerLevel[level].totalXPRequired;
		}
		else
		{
			Debug.LogWarning("ProgressionManager-getTotalXPRequired: the level specified," + level + " is incorrect. It needs to be between " + MIN_LEVEL.ToString() + " and " + MAX_LEVEL.ToString() );
			return 0;
		}
	}

	//Level parameter is between MIN_LEVEL and MAX_LEVEL
	//There are 10 level bands each with LEVEL_BANDS levels.
	public Color getFrameColor( int level )
	{
		if( level >= MIN_LEVEL && level <= ProgressionManager.MAX_LEVEL )
		{
			level--; //frameColorList is zero indexed
			int frameIndex = (int)Mathf.Floor(level/(float)LEVEL_BANDS);
			return frameColorList[frameIndex];
		}
		else
		{
			Debug.LogWarning("ProgressionManager-getFrameColor: the level specified," + level + " is incorrect. It needs to be between " + MIN_LEVEL.ToString() + " and " + MAX_LEVEL.ToString() );
			return Color.clear;
		}
	}

	public XPAward getXPAward( XPAwardType awardType )
	{
		XPAward xpAward = xpAwardList.Find(award => award.awardType == awardType);
		if( xpAward.awardType == XPAwardType.SKILL_BONUS )
		{
			xpAward.xpAmount = GameManager.Instance.playerProfile.getSkillBonus();
		}
		else if( xpAward.awardType == XPAwardType.SCORE_BONUS )
		{
			PlayerMatchData pmd = LevelManager.Instance.getPlayerMatchDataByName( GameManager.Instance.playerProfile.getUserName() );
			xpAward.xpAmount = pmd.score;
		}
		else if( xpAward.awardType == XPAwardType.WAVE_BONUS )
		{
			int waves = CoopWaveGenerator.numberOfWavesTriggered - 1;
			if( waves < 0 ) waves = 0;
			xpAward.xpAmount = CoopWaveGenerator.XP_EARNED_PER_WAVE * waves;
		}
		return xpAward;
	}

	[System.Serializable]
	public class XPAward
	{
		public XPAwardType awardType;
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

	/// <summary>
	/// Returns a random player icon unique identifier.
	/// Default icons are excluded.
	/// </summary>
	/// <returns>A random player icon unique identifier.</returns>
	public int getRandomPlayerIconUniqueId()
	{
		List<IconData> iconListExcludingDefaults = iconList.FindAll( icon => icon.isDefaultIcon == false );
		int randomNumber = Random.Range(0, iconListExcludingDefaults.Count );
		return iconListExcludingDefaults[randomNumber].uniqueId;
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

	[System.Serializable]
	public class XPData
	{
		public int XPNeededToChangeLevel = 0; 
		public int totalXPRequired = 0;

		public XPData( int XPNeededToChangeLevel, int totalXPRequired )
		{
			this.XPNeededToChangeLevel = XPNeededToChangeLevel;
			this.totalXPRequired = totalXPRequired;
		}
	}
	#endregion
	
}
