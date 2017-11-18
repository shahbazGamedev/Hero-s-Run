using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieManager : MonoBehaviour {

	[SerializeField] string zombieBoyPrefabName;
	[SerializeField] string zombieGirlPrefabName;
	[SerializeField] List<Material> zombieGirlMaterials = new List<Material>(4);
	[SerializeField] List<Material> zombieBoyMaterials = new List<Material>(4);

	public List<GameObject> zombieFactory = new List<GameObject>();
	int zombieFactoryIndex = 0;

	Transform player;
	PlayerController playerController;
	public static int numberOfZombieWavesTriggered = 0; //could eventually put that number in the player stats
	public const int NUMBER_STARS_PER_ZOMBIE = 20;
	public ParticleSystem debris; //Particle fx that plays when a zombie burrows up

	void Start ()
	{
		//Reset this static value
		numberOfZombieWavesTriggered = 0;

		//Hides zombies
		for( int i=0; i < zombieFactory.Count; i++ )
		{
			zombieFactory[i].SetActive( false );
		}
	}

	//Called when the player resurrects
	public void resetAllZombies()
	{
		//Reset our zombies
		for( int i=0; i < zombieFactory.Count; i++ )
		{
			ZombieController zombieController = zombieFactory[i].GetComponent<ZombieController>();
			zombieController.resetCreature();
		}
	}

	public Material getRandomZombieMaterial( Sex sex )
	{
		Material zombieMaterial = null;
		if( sex == Sex.FEMALE )
		{
			int rnd = Random.Range( 0, zombieGirlMaterials.Count );
			zombieMaterial = zombieGirlMaterials[rnd];
		}
		else
		{
			int rnd = Random.Range( 0, zombieBoyMaterials.Count );
			zombieMaterial = zombieBoyMaterials[rnd];
		}
		return zombieMaterial;
	}

	GameObject getAvailableZombie()
	{
		GameObject zombie = zombieFactory[zombieFactoryIndex];
		zombieFactoryIndex++;
		if( zombieFactoryIndex == zombieFactory.Count ) zombieFactoryIndex = 0;
		ZombieController zombieController = zombie.GetComponent<ZombieController>();
		//Reset his values before returning him spic and span. 
		zombieController.setCreatureState( CreatureState.Reserved );
		zombie.SetActive( true );
		CapsuleCollider capsuleCollider = zombie.GetComponent<CapsuleCollider>();
		capsuleCollider.enabled = true;
		return zombie;
	}

	//Called by a zombie trigger
	public void triggerZombieWave( List<GameObject> spawnLocations )
	{
		GameObject locationObject;
		ZombieSpawnData zsd;
		for( int i=0; i < spawnLocations.Count; i++ )
		{
			locationObject = spawnLocations[i];
			if( locationObject != null )
			{
				zsd = locationObject.GetComponent<ZombieSpawnData>();
				if( zsd != null )
				{
					//Debug.Log(  " triggerZombieWave " + i + " delay " + zsd.spawnDelay + " pos " + locationObject.transform.position );
					StartCoroutine( spawnZombie( locationObject.transform, zsd ) );
				}
			}
			else
			{
				Debug.LogWarning( "ZombieManager: triggerZombieWave locationObject is null. spawnLocations.Count: " + spawnLocations.Count  );
			}
		}
	}

	IEnumerator spawnZombie( Transform spawnLocation, ZombieSpawnData zsd )
	{
		//Verify where is the ground before the spawn delay
		RaycastHit hit;
		float zombieHeight = 0;
		Vector3 rayCastStart = spawnLocation.position;
		if (Physics.Raycast(rayCastStart, Vector3.down, out hit, 10f ))
		{
			zombieHeight = hit.point.y + 0.09f;
		}
		else
		{
			Debug.LogError("ZombieManager-spawnZombie - solid ground below spawnLocation: " + spawnLocation.localPosition + " was not found. Using a Y value of 0 in tile " + spawnLocation.root.name );
		}

		yield return new WaitForSeconds(zsd.spawnDelay);

		object[] data = new object[2];
		data[0] = (int) zsd.spawnType;
		data[1] = zsd.followsPlayer;
		float rnd = Random.value;
		string prefabName;
		if( rnd < 0.5f )
		{
			prefabName = zombieBoyPrefabName;
		}
		else
		{
			prefabName = zombieGirlPrefabName;
		}
		PhotonNetwork.InstantiateSceneObject( prefabName, new Vector3( spawnLocation.position.x, zombieHeight, spawnLocation.position.z ), spawnLocation.rotation, 0, data );
	
	}

	void spawnZombie()
	{
		//Select an available zombie from the factory
		GameObject zombie = getAvailableZombie();

		if( zombie != null )
		{
			//Place the zombie somewhere in front of the player
			zombie.transform.position = selectRandomLane();

			//Make sure the zombie faces the player
			zombie.transform.eulerAngles = new Vector3 ( 0, player.eulerAngles.y + 180f, 0 );

			//Make the zombie burrow out of the ground
			Animation zombieBoyAnimator = (Animation) zombie.GetComponent("Animation");
			ZombieController zombieController = (ZombieController) zombie.GetComponent("ZombieController");
			zombieController.setCreatureState( CreatureState.BurrowUp );
			zombieBoyAnimator.CrossFade("burrowUp", 0.1f, 0 );
		}
		else
		{
			Debug.LogWarning("ZombieManager-spawnZombie: no zombies available.");
		}
	}

	Vector3 selectRandomLane()
	{
		//-1 left, 0 center, 1 right lane
		int selectedLane = Random.Range(-1,2);
		//The faster the player, the further away we need to place the zombies to give enough time to the player to avoid them.
		float distanceToPlayer = player.GetComponent<PlayerController>().getSpeed() * 5f;
		
		Transform tile = playerController.currentTile.transform;
		
		Vector3 zombiePosition = Vector3.zero;
		
		float tileRotationY = Mathf.Floor( tile.eulerAngles.y );
		if( tileRotationY == 0 )
		{
			if( selectedLane == -1 )
			{
				//Left
				zombiePosition.Set ( tile.position.x - PlayerController.laneLimit, 0.28f, player.position.z + distanceToPlayer );
			}
			else if ( selectedLane == 0 )
			{
				//Center
				zombiePosition.Set ( tile.position.x, 0.28f, player.position.z + distanceToPlayer );
			}
			else
			{
				//Right
				zombiePosition.Set ( tile.position.x + PlayerController.laneLimit, 0.28f, player.position.z + distanceToPlayer );
			}
		}
		else if( tileRotationY == 90f || tileRotationY == -270f )
		{
		}
		else if( tileRotationY == -90f || tileRotationY == 270f )
		{
		}
		
		return zombiePosition;
		
	}

	void OnEnable()
	{
		PlayerController.resurrectionBegin += ResurrectionBegin;
		PlayerController.localPlayerCreated += LocalPlayerCreated;
	}
	
	void OnDisable()
	{
		PlayerController.resurrectionBegin -= ResurrectionBegin;
		PlayerController.localPlayerCreated -= LocalPlayerCreated;
	}

	void ResurrectionBegin()
	{
		resetAllZombies();
	}

	void LocalPlayerCreated( Transform playerTransform, PlayerController playerController )
	{
		player = playerTransform;
		this.playerController = playerController;
	}

}
