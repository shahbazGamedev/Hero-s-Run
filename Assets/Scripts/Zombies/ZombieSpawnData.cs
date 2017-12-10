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

	//Delay before zombie is spawned
	public float spawnDelay = 0;
	public ZombieSpawnType spawnType = ZombieSpawnType.BurrowUp;
	//If a coffin is specified, the coffin open animation will be played at the same time as the zombie is spawned
	public GameObject coffin;
	//If true, the zombie heads for the player (as opposed to staying in its lane).
	public bool followsPlayer = false;
}
