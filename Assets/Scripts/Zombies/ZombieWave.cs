using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ZombieWave : MonoBehaviour {
	
	public List<GameObject> spawnLocations = new List<GameObject>();
	[SerializeField] Mesh zombieMesh;
	void OnDrawGizmos ()
	{
		GameObject locationObject;
		for( int i=0; i < spawnLocations.Count; i++ )
		{
			locationObject = spawnLocations[i];
			if( locationObject != null )
			{
				Vector3 relativePos = new Vector3( 0 , 0 , 1f );
				Vector3 exactPos = locationObject.transform.TransformPoint(relativePos);
	
				ZombieSpawnData zsd = locationObject.GetComponent<ZombieSpawnData>();
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
				float groundHeight = getGroundHeight( locationObject.transform.position );
				Vector3 groundPosition = new Vector3( locationObject.transform.position.x, groundHeight, locationObject.transform.position.z );
				if( zombieMesh != null ) Gizmos.DrawMesh( zombieMesh, groundPosition, Quaternion.Euler( -90f, locationObject.transform.eulerAngles.y, 0 )  );
	
			}
			else
			{
				Debug.LogWarning("ZombieWave-one of the spawn locations used by " + this.transform.parent.name + " is null." );
			}
		}
	}

	float getGroundHeight( Vector3 startPosition )
	{
		//Calculate the ground height
		RaycastHit hit;
		if (Physics.Raycast( startPosition , Vector3.down, out hit, 10f ))
		{
			return  hit.point.y;
		}
		else
		{
			return startPosition.y;
		}
	}

}
