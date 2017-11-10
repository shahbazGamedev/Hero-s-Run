using System.Collections;
using UnityEngine;

public class MaskHandler : MonoBehaviour {

	public const int ignoreRaycastLayer = 2;
	public const int playerLayer = 8;
	public const int deviceLayer = 16;
	public const int destructibleLayer = 17;
	public const int levelDestructibleLayer = 18;
	public const int movableLayer = 19;
	public const int creatureLayer = 20; //For example, a zombie.

	static int maskWithPlayerWithLevelDestructible;
	static int maskWithPlayerWithoutLevelDestructible;
	static int maskWithoutPlayerWithLevelDestructible;
	static int maskOnlyPlayer;
	static int maskWithPlayersWithCreatures;
	static int maskOnlyMovable;
	static int maskWithPlayerWithoutDevices;

	void Awake ()
	{
		initialiseMasks();
	}

	void initialiseMasks()
	{
		maskWithPlayerWithLevelDestructible = 1 << playerLayer;
		maskWithPlayerWithLevelDestructible |= 1 << deviceLayer;
		maskWithPlayerWithLevelDestructible |= 1 << destructibleLayer;
		maskWithPlayerWithLevelDestructible |= 1 << levelDestructibleLayer;
		maskWithPlayerWithLevelDestructible |= 1 << creatureLayer;

		maskWithPlayerWithoutDevices = 1 << playerLayer;
		maskWithPlayerWithoutDevices |= 1 << destructibleLayer;
		maskWithPlayerWithoutDevices |= 1 << levelDestructibleLayer;
		maskWithPlayerWithoutDevices |= 1 << creatureLayer;

		maskWithPlayerWithoutLevelDestructible = 1 << playerLayer;
		maskWithPlayerWithoutLevelDestructible |= 1 << deviceLayer;
		maskWithPlayerWithoutLevelDestructible |= 1 << destructibleLayer;
		maskWithPlayerWithoutLevelDestructible |= 1 << creatureLayer;

		maskWithoutPlayerWithLevelDestructible = 1 << deviceLayer;
		maskWithoutPlayerWithLevelDestructible |= 1 << destructibleLayer;
		maskWithoutPlayerWithLevelDestructible |= 1 << levelDestructibleLayer;
		maskWithoutPlayerWithLevelDestructible |= 1 << creatureLayer;

		maskOnlyPlayer = 1 << playerLayer;

		maskWithPlayersWithCreatures = 1 << playerLayer;
		maskWithPlayersWithCreatures |= 1 << creatureLayer;

		maskOnlyMovable = 1 << movableLayer;
	}

	public static int getMaskWithPlayerWithLevelDestructible()
	{
		return maskWithPlayerWithLevelDestructible;
	}

	public static int getMaskWithPlayerWithoutLevelDestructible()
	{
		return maskWithPlayerWithoutLevelDestructible;
	}

	public static int getMaskWithoutPlayerWithLevelDestructible()
	{
		return maskWithoutPlayerWithLevelDestructible;
	}

	public static int getMaskOnlyPlayer()
	{
		return maskOnlyPlayer;
	}

	public static int getMaskWithPlayersWithCreatures()
	{
		return maskWithPlayersWithCreatures;
	}

	public static int getMaskOnlyMovable()
	{
		return maskOnlyMovable;
	}

	public static int getMaskWithPlayerWithoutDevices()
	{
		return maskWithPlayerWithoutDevices;
	}

}
