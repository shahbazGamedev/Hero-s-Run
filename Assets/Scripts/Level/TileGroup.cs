﻿using UnityEngine;
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

	Tanglewood_Start = 200,
	Tanglewood_End = 201,
	Tanglewood_Obstacles_1 = 202,
	Tanglewood_Obstacles_2 = 203,
	Tanglewood_Checkpoint = 204,
	Tanglewood_Fairy_Message = 205,
	Tanglewood_T_Junction = 206,
	Tanglewood_Turns = 207,
	Tanglewood_Slope = 208,
	Tanglewood_River = 209,
	Tanglewood_River_Crossing = 210,
	Tanglewood_River_Log_Crossing = 211,
	Tanglewood_Windmill = 212,
	Tanglewood_Defense_Tower = 213,
	Tanglewood_Burning_Bridge = 214,

	Fairyland_Start = 300,
	Fairyland_End = 301,
	Fairyland_Obstacles_1 = 302, 		//straight_log, straight_river, straight, straight_river_crossing
	Fairyland_Obstacles_2 = 303,		//straight_river_crossing, straight_slope, straight, Fairyland_Mushroom_Jump, straight_log
	Fairyland_Checkpoint = 304,
	Fairyland_Fairy_Message = 305,
	Fairyland_T_Junction = 306,
	Fairyland_Turns = 307,
	Fairyland_Nice_And_Easy = 308,		//straight, left, straight, right
	Fairyland_Roasting_Pig = 312,
	Fairyland_T_Junction_2 = 315,		//T_Junction with stained glass floor
	Fairyland_Pushed_Barrels = 316,
	Fairyland_Goblin_Loot = 317,		//contains a treasure key
	Fairyland_Magic_Bridge = 318,
	Fairyland_Treasure_Key = 319,
	Fairyland_Enemies_1 = 320,			//Fairyland_Goblin_Jump, straight_double
	Fairyland_Enemies_2 = 321,			//straight_double, left, right
	Fairyland_Tomb_Sequence = 322,		//Landmark_Tomb_Start, Landmark_Tomb_Start_2, 7x Tomb_Double, Tomb_End

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
		Never = -1,
		Unique = 0,
		Very_Rare = 1,
		Rare = 2,
		Common = 4,
		Very_Common = 8,
	}

	public SegmentTheme theme = SegmentTheme.Forest;
	public TileGroupType tileGroupType;
	public ValidGameMode validGameMode = ValidGameMode.Any;
	public FrequencyType frequency = FrequencyType.Common;
	public List<TileType> tileList = new List<TileType>();
}
