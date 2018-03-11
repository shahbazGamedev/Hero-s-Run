using UnityEngine;
using System.Collections;

public enum ZombieSpawnType
{
	BurrowUp = 1,
	StandUpFromBack = 2,
	Walk = 3,
	Crawl = 4,
	Jump = 5,
	Run = 6
}

public class ZombieSpawnData : MonoBehaviour {

	[Tooltip("Delay before the zombie is spawned. This is usefull so that zombies don't start their animation at the same time.")]
	public float spawnDelay = 0;
	public ZombieSpawnType spawnType = ZombieSpawnType.BurrowUp;
	[Tooltip("If a coffin is specified, the coffin open animation will be played at the same time as the zombie is spawned.")]
	public bool addCoffin;
	[Tooltip("If true, the zombie heads for the player (as opposed to staying in its lane).")]
	public bool followsPlayer = false;
}
