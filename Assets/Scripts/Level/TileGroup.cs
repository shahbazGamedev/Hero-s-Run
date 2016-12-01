using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ValidGameMode {
	Story = 1,
	Endless = 2,
	Any = 3
}

public enum TileGroupType {

	None = 0,
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
	Tanglewood_Magic_Mirror = 215,

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
	Cemetery_Checkpoint = 404,
	Cemetery_Fairy_Message = 405,
	Cemetery_T_Junction = 406,
	Cemetery_Turns_1 = 407,
	Cemetery_Turns_2 = 408,
	Cemetery_Small_Stairs = 409,
	Cemetery_River = 410,
	Cemetery_River_Crossing = 411,
	Cemetery_Zombie_Next_To_Coach = 412,
	Cemetery_Zombie_Driving_Coach = 413,
	Cemetery_Clocktower = 414,
	Cemetery_Evil_Tree = 415,
	Cemetery_Gargoyle_Shooting_Flames = 416,	//straight_log
	Cemetery_Ghost = 418,
	Cemetery_Thorns = 419,						//Landmark_Graveyard_7
	Cemetery_Enemies_1 = 420,					//straight_double
	Cemetery_Nice_And_Easy = 421,				
	Cemetery_Graveyard_Sequence = 422,
	Cemetery_Endless_Graveyard_Sequence = 423,

	Jungle_Start = 500,
	Jungle_End = 501,
	Jungle_Obstacles_1 = 502,				//straight_log, straight_river
	Jungle_Obstacles_2 = 503,				//straight_slope, left, right, straight_river_crossing
	Jungle_Obstacles_3 = 504,				//straight_slope
	Jungle_Obstacles_4 = 505,				//straight_river
	Jungle_Checkpoint = 506,
	Jungle_Fairy_Message = 507,
	Jungle_T_Junction = 508,
	Jungle_Turns_1 = 509,
	Jungle_Turns_2 = 510,
	Jungle_Nice_And_Easy = 511,
	Jungle_Zipline = 512,					//zipline, left, straight_slope, right
	Jungle_Treasure_Key = 513,
	Jungle_Enemies_1 = 514,					//Jungle_Enemy, straight_double
	Jungle_Enemies_2 = 515,					//Jungle_Enemy	
	Jungle_Pyramid_Sequence = 516,			//Jungle_Pyramid, straight, Jungle_Vortex, Jungle_Enemy
	Jungle_Valley = 517,					//Jungle_Valley_Start, Jungle_Valley_1, Jungle_Valley_2, Jungle_Valley_End

	Mines_Start = 600,
	Mines_End = 601,
	Mines_Obstacles_1 = 602,
	Mines_Obstacles_2 = 603,
	Mines_Enemies_1 = 604,					//straight_double
	Mines_T_Junction = 606,
	Mines_Treasure_Key = 607,
	Mines_Turns_1 = 608,
	Mines_Turns_2 = 609,
	Mines_Checkpoint = 610,
	Mines_Fairy_Message = 611,
	Mines_Giant_Crystal_Sequence = 612,
	Mines_Torch_Sequence = 613,
	Mines_Slope = 614,
	Mines_River = 615,
	Mines_River_Crossing = 616,
	Mines_Giant_Cave = 617,			//Mines_Giant_Cave

	Hell_Start = 700,				//Start, Hell_Cerberus, Hell_Cavern_Gibbets, Landmark_Collapsing_Bridge, Hell_Drawbridge
	Hell_End = 701,					//End
	Hell_Banquet_Hall = 702,		//Left, Hell_Banquet_Hall, Right, Straight_Double
	Hell_Fairy_Message = 703,		//Landmark_Fairy_Message
	Hell_Turns_1 = 704,				//Right, straight, Left
	Hell_Turns_2 = 705,				//Left, Right
	Hell_Pendulums_Series = 706,	//Hell_Pendulums, Hell_Pendulums, Hell_Pendulums
	Hell_Flame_Columns_Series = 707,//Hell_Flame_Columns, Hell_Flame_Columns, Hell_Flame_Columns
	Hell_Enemies_1 = 708,			//Hell_Demons
	Hell_Enemies_2 = 709,			//Left, Straight_Double, Right, Straight
	Hell_Obstacles_1 = 710,			//Hell_Floor_Traps, straight, Hell_Pendulums
	Hell_Obstacles_2 = 711,			//Hell_Flame_Columns, Hell_Floor_Traps, Hell_Pendulums, Straight
	Hell_Obstacles_3 = 712,			//Hell_Floor_Traps, Hell_Floor_Traps, left, Hell_Flame_Columns, Hell_Flame_Columns, Hell_Flame_Columns, right, Hell_pendulums, Hell_pendulums, Hell_pendulums
	Hell_Nice_And_Easy = 713,		//Left, Straight, Right, Straight_River, Hell_Bridge
	Hell_Treasure_Key = 714,		//Landmark_Treasure_Key
	Hell_Checkpoint = 715,			//Checkpoint
	Hell_T_Junction = 716,			//T_Junction

	Blizzard_Start = 800,			//Start, Blizzard_Goblin_Jump, Straight_River_Log_Crossing, Left, Right
	Blizzard_End = 801,				//End
	Blizzard_Obstacles_1 = 802,		//straight_slope, straight_log, Left, straight_River_Crossing, Right
	Blizzard_Broken_Bridge = 803,	//Landmark_Broken_Bridge
	Blizzard_Enemies_1 = 804,		//Left, Blizzard_Goblin_Jump, Right, Straight, Straight_River
	Blizzard_Enemies_2 = 805,		//Blizzard_Goblin_Jump, Straight_River_Log_Crossing
	Blizzard_Enemies_3 = 806,		//Straight_double, right, straight, left
	Blizzard_Enemies_4 = 807,		//Blizzard_Goblins_Valley, Straight_River_Crossing
	Blizzard_Nice_And_Easy = 808,	//Left, Blizzard_Goblin_Jump, Right
	Blizzard_T_Junction = 809,		//T_Junction
	Blizzard_Treasure_Key = 810,	//Landmark_Treasure_Key
	Blizzard_Turns_1 = 811,			//Left, straight, Right, straight_slope
	Blizzard_Turns_2 = 812,			//Right, Left
	Blizzard_Checkpoint = 813,		//Checkpoint
	Blizzard_Fairy_Message = 814,	//Fairy_Message
	Blizzard_Avalanche = 815,		//Blizzard_Avalanche, straight_double
	Blizzard_Snow_Balls = 816,		//Blizzard_Snow_balls
	Blizzard_Refuge = 817,			//Straight_Slope, Blizzard_Refuge
	Blizzard_Dark_Queen_Calls_Storm = 818	//Blizzard_Dark_Queen_Calls_Storm

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
