﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ValidGameMode {
	Story = 1,
	Endless = 2,
	Any = 3
}

public enum TileGroupType {
	Mines_Start = 1,
	Mines_End = 2,
	Mines_Obstacles = 3,
	Mines_Giant_Crystal = 4,
	Mines_Combat_1 = 5,
	Jungle_Obstacles_1 = 100
}

[System.Serializable]
public class TileGroup{

	[Header("Tile Group Parameters")]
	//Other potential parameters: validDifficultyLevel, allowEnemies, requiresZeroRotation, hasZeroRotation, accentCombatMusic
	public SegmentTheme theme = SegmentTheme.Forest;
	public TileGroupType tileGroupType;
	public string description;
	public ValidGameMode validGameMode = ValidGameMode.Any;
	public List<TileType> tileList = new List<TileType>();
	[Range(0,100)]
	public int frequency = 0;
}
