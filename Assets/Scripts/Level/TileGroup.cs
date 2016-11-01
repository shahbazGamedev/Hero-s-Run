using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ValidGameMode {
	Story = 1,
	Endless = 2,
	Any = 3
}

public enum TileGroupType {

	Dark_Tower_Start = 100,
	Dark_Tower_End = 101,
	Dark_Tower_Obstacle_1 = 102,

	Forest_Start = 200,
	Forest_End = 201,
	Forest_Obstacles_1 = 202,

	Fairyland_Start = 300,
	Fairyland_End = 301,
	Fairyland_Obstacles_1 = 302,

	Cemetery_Start = 400,
	Cemetery_End = 401,
	Cemetery_Obstacles_1 = 402,

	Jungle_Start = 500,
	Jungle_End = 501,
	Jungle_Obstacles_1 = 502,

	Mines_Start = 600,
	Mines_End = 601,
	Mines_Obstacles_1 = 602,
	Mines_Giant_Crystal = 603,
	Mines_Combat_1 = 604,
	Mines_T_Junction = 605,
	Mines_Treasure_Key = 607,
	Mines_Turns = 608,

	Hell_Start = 700,
	Hell_End = 701,
	Hell_Obstacle_1 = 702,

	Blizzard_Start = 800,
	Blizzard_End = 801,
	Blizzard_Obstacles_1 = 802

}

[System.Serializable]
public class TileGroup{

	public enum FrequencyType {
		Never = 0,
		Very_Rare = 1,
		Rare = 2,
		Common = 4,
		Very_Common = 8,
	}

	[Header("Tile Group Parameters")]
	//Other potential parameters: validDifficultyLevel, allowEnemies, requiresZeroRotation, hasZeroRotation, accentCombatMusic
	public SegmentTheme theme = SegmentTheme.Forest;
	public TileGroupType tileGroupType;
	public string description;
	public ValidGameMode validGameMode = ValidGameMode.Any;
	public FrequencyType frequency = FrequencyType.Common;
	public List<TileType> tileList = new List<TileType>();
}
