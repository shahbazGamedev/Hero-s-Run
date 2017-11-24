using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieManager : MonoBehaviour {

	[SerializeField] string zombieBoyPrefabName;
	[SerializeField] string zombieGirlPrefabName;
	[SerializeField] List<Material> zombieGirlMaterials = new List<Material>(4);
	[SerializeField] List<Material> zombieBoyMaterials = new List<Material>(4);

	public static int numberOfZombieWavesTriggered = 0; //could eventually put that number in the player stats

	void Start ()
	{
		//Reset this static value
		numberOfZombieWavesTriggered = 0;
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

}
