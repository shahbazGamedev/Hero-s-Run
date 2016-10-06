using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileReset : MonoBehaviour {

	public List<TileObjectData> TileObjectList = new List<TileObjectData>();

	void Awake()
	{
		createPrefabs();
	}

	void destroyExistingPrefabs()
	{
		Transform child;
		foreach( TileObjectData tod in TileObjectList )
		{
			string objectName = tod.prefab.name + "(Clone)";
			child = transform.FindChild(objectName);
			if( child != null )
			{
				Destroy( child.gameObject );
			}
		}
	}

	void createPrefabs()
	{
		//Instantiate the prefabs
		foreach( TileObjectData tod in TileObjectList )
		{
			GameObject go = (GameObject)Instantiate(tod.prefab, Vector3.zero, Quaternion.identity );
			go.transform.parent = gameObject.transform;
			go.transform.localPosition = tod.localPosition;
			go.transform.localEulerAngles = tod.localRotation;
			//Debug.Log("resetTile creating prefab named " + go.name ) ;
		}
	}

	//This method is called to reset the tile because it is being recycled.
	//1) Destroy power-ups
	public void resetTile()
	{
		Debug.Log ("TileReset- resetting tile named " + gameObject.name );

		//Reset power-ups and zombies
		Transform child;
		for( int i = 0; i < transform.childCount; i++ )
		{
			child = transform.GetChild(i);
			if( child.CompareTag("PowerUp") )
			{
				//Debug.Log("resetTile destroying power-up named " + " i " + i + " " + child.name ) ;
				Destroy( child.gameObject );
			}
			//Reset zombies if any
			else if( child.CompareTag("Zombie") )
			{
				ZombieController zombieController = child.GetComponent<ZombieController>();
				zombieController.resetCreature();
			}
			else if( child.name == "ZombieTrigger" )
			{
				//Reset any zombie wave configurations
				Debug.Log("resetTile ZombieTrigger is reset." ) ;
				ZombieTrigger zombieTrigger = child.GetComponent<ZombieTrigger>();
				zombieTrigger.reset();

			}
		}

		//Reset dynamic prefabs like barrels, pumpkins, cows and chickens
		destroyExistingPrefabs();
		createPrefabs();

	}

	[System.Serializable]
	public class TileObjectData
	{
		public GameObject prefab;
		public Vector3 localPosition;
		public Vector3 localRotation;
	}

}
