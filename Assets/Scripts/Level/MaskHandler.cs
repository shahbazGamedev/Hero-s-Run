using System.Collections;
using UnityEngine;

public class MaskHandler : MonoBehaviour {

	public const int ignoreRaycastLayer = 2;
	public const int playerLayer = 8;
	public const int deviceLayer = 16;				//For example a Jump-pad or a teleporter.
	public const int destructibleLayer = 17;		//Not used right now. Was used for the old ice wall that could be destroyed.
	public const int creatureLayer = 20; 			//For example a zombie.
	public const int levelDestructibleLayer = 18; 	//For example a bridge.
	public const int movableLayer = 19; 			//For example a barrel, a crate, or a cone.

	static int maskAll;
	static int maskAllExceptDevices;
	static int maskWithPlayersWithCreatures;
	static int maskAllExceptPlayers;
	static int maskMovableAndLevelDestructible;

	static int maskOnlyPlayer;
	static int maskOnlyCreatures;

	void Awake ()
	{
		initialiseMasks();
	}

	void initialiseMasks()
	{
		maskAll = 1 << playerLayer;
		maskAll |= 1 << creatureLayer;
		maskAll |= 1 << destructibleLayer;
		maskAll |= 1 << deviceLayer;

		maskAllExceptDevices = 1 << playerLayer;
		maskAllExceptDevices |= 1 << creatureLayer;
		maskAllExceptDevices |= 1 << destructibleLayer;

		maskWithPlayersWithCreatures = 1 << playerLayer;
		maskWithPlayersWithCreatures |= 1 << creatureLayer;

		maskAllExceptPlayers = 1 << creatureLayer;
		maskAllExceptPlayers |= 1 << destructibleLayer;
		maskAllExceptPlayers |= 1 << deviceLayer;

		maskMovableAndLevelDestructible = 1 << movableLayer;
		maskMovableAndLevelDestructible |= 1 << levelDestructibleLayer;

		maskOnlyPlayer = 1 << playerLayer;

		maskOnlyCreatures = 1 << creatureLayer;
	}

	#region multiple layers
	public static int getMaskAll()
	{
		return maskAll;
	}

	public static int getMaskAllWithoutDevices()
	{
		return maskAllExceptDevices;
	}

	public static int getMaskWithPlayersWithCreatures()
	{
		return maskWithPlayersWithCreatures;
	}

	public static int getMaskAllExceptPlayers()
	{
		return maskAllExceptPlayers;
	}

	public static int getMaskMovableAndLevelDestructible()
	{
		return maskMovableAndLevelDestructible;
	}
	#endregion

	#region single-layer
	public static int getMaskOnlyPlayer()
	{
		return maskOnlyPlayer;
	}

	public static int getMaskOnlyCreatures()
	{
		return maskOnlyCreatures;
	}
	#endregion

}
