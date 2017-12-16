using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieManager : MonoBehaviour {

	[SerializeField] string zombieBoyPrefabName;
	[SerializeField] string zombieGirlPrefabName;
	[SerializeField] string coffinPrefabName;
	[SerializeField] List<Material> zombieGirlMaterials = new List<Material>(4);
	[SerializeField] List<Material> zombieBoyMaterials = new List<Material>(4);

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

	public void triggerZombieWave( List<GameObject> spawnLocations )
	{
		ZombieSpawnData zsd;
		for( int i=0; i < spawnLocations.Count; i++ )
		{
			if( spawnLocations[i] != null )
			{
				zsd = spawnLocations[i].GetComponent<ZombieSpawnData>();
				if( zsd != null )
				{
					StartCoroutine( spawnZombie( spawnLocations[i].transform.position, spawnLocations[i].transform.rotation, zsd ) );
				}
			}
			else
			{
				Debug.LogWarning( "ZombieManager: triggerZombieWave spawnLocations is null. spawnLocations.Count: " + spawnLocations.Count  );
			}
		}
	}

	IEnumerator spawnZombie( Vector3 spawnPosition, Quaternion spawnRotation, ZombieSpawnData zsd )
	{
		//Verify where is the ground before the spawn delay
		RaycastHit hit;
		float zombieHeight = 0;
		Vector3 rayCastStart = new Vector3( spawnPosition.x, spawnPosition.y + 10f, spawnPosition.z );
		if (Physics.Raycast(rayCastStart, Vector3.down, out hit, 20f ))
		{
			zombieHeight = hit.point.y + 0.09f;
		}
		else
		{
			Debug.LogWarning("ZombieManager-spawnZombie - No solid ground below spawnLocation: " + spawnPosition + ". Not spawning." );
			yield return null;
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
		Vector3 zombieSpawnPosition = new Vector3( spawnPosition.x, zombieHeight, spawnPosition.z );
		PhotonNetwork.InstantiateSceneObject( prefabName, zombieSpawnPosition, spawnRotation, 0, data );
		if( zsd.addCoffin ) PhotonNetwork.InstantiateSceneObject( coffinPrefabName, zombieSpawnPosition, spawnRotation, 0, null );
	}

}
