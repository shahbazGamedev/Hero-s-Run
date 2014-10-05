using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ZombieWave : MonoBehaviour {
	
	public List<GameObject> spawnLocations = new List<GameObject>();

	void OnDrawGizmos ()
	{
		GameObject locationObject;
		for( int i=0; i < spawnLocations.Count; i++ )
		{
			locationObject = spawnLocations[i];
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
			}
			Gizmos.DrawSphere( locationObject.transform.position, 0.06f);

			Gizmos.DrawLine ( locationObject.transform.position, exactPos );
		}
	}
}
