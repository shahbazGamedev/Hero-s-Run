using System.Collections;
using UnityEngine;

public class MaskHandler : MonoBehaviour {

	public const int ignoreRaycastLayer = 2;
	public const int playerLayer = 8;
	public const int deviceLayer = 16;
	public const int destructibleLayer = 17;
	public const int levelDestructibleLayer = 18;
	public const int movableLayer = 19;

	static int maskWithPlayerWithLevelDestructible;
	static int maskWithPlayerWithoutLevelDestructible;
	static int maskWithoutPlayerWithLevelDestructible;
	static int maskOnlyPlayer;
	static int maskOnlyMovable;

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

		maskWithPlayerWithoutLevelDestructible = 1 << playerLayer;
		maskWithPlayerWithoutLevelDestructible |= 1 << deviceLayer;
		maskWithPlayerWithoutLevelDestructible |= 1 << destructibleLayer;

		maskWithoutPlayerWithLevelDestructible = 1 << deviceLayer;
		maskWithoutPlayerWithLevelDestructible |= 1 << destructibleLayer;
		maskWithoutPlayerWithLevelDestructible |= 1 << levelDestructibleLayer;

		maskOnlyPlayer = 1 << playerLayer;

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

	public static int getMaskOnlyMovable()
	{
		return maskOnlyMovable;
	}

}
