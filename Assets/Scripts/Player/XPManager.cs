﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPManager : MonoBehaviour {

	[Header("General")]
	public static XPManager Instance;
	public const int MAX_LEVEL = 100;
	[SerializeField] List<int> xpNeededPerLevel = new List<int>(MAX_LEVEL);

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
	
}
