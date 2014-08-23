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
			Gizmos.color = Color.green;
			Gizmos.DrawSphere( locationObject.transform.position, 0.05f);
			Gizmos.color = Color.red;
			Gizmos.DrawLine ( locationObject.transform.position, exactPos );
		}
	}
}
