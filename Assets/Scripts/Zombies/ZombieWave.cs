using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ZombieWave : MonoBehaviour {
	
	public List<GameObject> spawnLocations = new List<GameObject>();
	[SerializeField] Mesh zombieMesh;

	//Important: make sure you display the zombies with the gizmo in the same way that the ZombieManager spawns zombies during the game.
	//We want the results in the Editor to be the same as in the game. see spawnZombies method in ZombieManager.
	void OnDrawGizmos ()
	{
		Transform spawnLocation;
		for( int i=0; i < spawnLocations.Count; i++ )
		{
			spawnLocation = spawnLocations[i].transform;
			if( spawnLocation != null )
			{	
				ZombieSpawnData zsd = spawnLocation.GetComponent<ZombieSpawnData>();
				switch (zsd.spawnType)
				{
				case ZombieSpawnType.BurrowUp:
					Gizmos.color = Color.blue;
					break;
				case ZombieSpawnType.StandUpFromBack:
					Gizmos.color = Color.yellow;
					break;
				case ZombieSpawnType.Walk:
					Gizmos.color = Color.green;
					break;
				case ZombieSpawnType.Crawl:
					Gizmos.color = Color.red;
					break;
				case ZombieSpawnType.Jump:
					Gizmos.color = Color.gray;
					break;
				case ZombieSpawnType.Run:
					Gizmos.color = Color.magenta;
					break;
				}
				float groundHeight = getGroundHeight( spawnLocation.position );
				Vector3 groundPosition = new Vector3( spawnLocation.position.x, groundHeight, spawnLocation.position.z );
				if( zombieMesh != null ) Gizmos.DrawMesh( zombieMesh, groundPosition, Quaternion.Euler( -90f, spawnLocation.eulerAngles.y, 0 )  );
	
			}
			else
			{
				Debug.LogWarning("ZombieWave-the spawn location at index " + i + " is null." );
			}
		}
	}

	float getGroundHeight( Vector3 spawnPosition )
	{
		//Determine the ground height
		RaycastHit hit;
		float groundHeight = 0;
		Vector3 rayCastStart = new Vector3( spawnPosition.x, spawnPosition.y + 10f, spawnPosition.z );
		if (Physics.Raycast(rayCastStart, Vector3.down, out hit, 20f ))
		{
			groundHeight = hit.point.y + 0.05f;
		}
		else
		{
			//Debug.LogWarning("ZombieWave-getGroundHeight - No ground below spawnPosition: " + spawnPosition + ". Keeping original Y value." );
			groundHeight = spawnPosition.y;
		}
		return groundHeight;
	}

}
