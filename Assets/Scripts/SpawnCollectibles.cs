using UnityEngine;
using System.Collections;

public class SpawnCollectibles : MonoBehaviour {
	/*
	//Make these game objects static as we only want to load them once for all instances
	static GameObject CoinPack_4x1_1x5 = Resources.Load("Coins/CoinPack_4x1_1x5") as GameObject;
	static GameObject CoinPack_5x1 = Resources.Load("Coins/CoinPack_5x1") as GameObject;
	static GameObject CoinPack_7x1_corner_left = Resources.Load("Coins/CoinPack_7x1_corner_left") as GameObject;
	static GameObject CoinPack_7x1_corner_right = Resources.Load("Coins/CoinPack_7x1_corner_right") as GameObject;
	static GameObject expensiveCoin = Resources.Load("Coins/Coin_100") as GameObject;
	bool initialized = false;

	// Use this for initialization
	void OnEnable ()
	{
		return;
		if ( initialized || LevelManager.Instance.isTutorialActive() ) return;
		initialized = true;


		//There is a 20% chance that a coin pack will be spawned
		float rd = Random.Range( 0f, 1f );
		float heightOffset = 0.8f;
		float offset = 0;
		
		//There are sometime small rounding errors In Unity that can make angles change slightly
		//such as 90.0001f instead of 90f. This is why we use Mathf.Floor.
		float corridorY = Mathf.Floor( transform.eulerAngles.y );

		if( rd < 0.2f )
		{
			//Yes, we should spawn a coin pack

			//Are we part of a corner?
			if( gameObject.name.Contains("corner") )
			{
				GameObject goCorner = null;
				float corridorYlocal = Mathf.Floor( transform.localRotation.eulerAngles.y );
				if( corridorYlocal == 0 )
				{
					//We have a right corner
					goCorner = (GameObject)Instantiate(CoinPack_7x1_corner_right, Vector3.zero, Quaternion.identity );
					CoinManager.realNumberCoinsSpawned = CoinManager.realNumberCoinsSpawned + 7;
				}
				else if( corridorYlocal == 90f )
				{
					//We have a left corner
					goCorner = (GameObject)Instantiate(CoinPack_7x1_corner_left, Vector3.zero, Quaternion.identity );
					CoinManager.realNumberCoinsSpawned = CoinManager.realNumberCoinsSpawned + 7;
				}
				goCorner.transform.parent = transform;
				goCorner.transform.localRotation = Quaternion.Euler(0,0,0);
				goCorner.transform.localPosition = new Vector3( 0,heightOffset,0 );
			}
			else
			{
				//Use a straight coin pack
				rd = Random.Range( 0f, 1f );
				Vector3 pos = new Vector3( 0,0,0 );
				//50% chance it will spawn on the right side and 50% chance on the left side
				if( rd <= 0.5f )
				{
					offset = -PlayerController.laneLimit;
				}
				else
				{
					offset = PlayerController.laneLimit;
				}
				//If the Y angle of the corridor is 0 or 180 degrees, the lanes are along the X=axis.
				//If the Y angle of the corridor is 90 or -90 degrees, the lanes are along the Z-axis.

				//Facing right
				if ( corridorY == 90f || corridorY == -270f)
				{
					pos = new Vector3( transform.position.x, transform.position.y + heightOffset, transform.position.z + offset );
				}
				//Facing left
				else if ( corridorY == -90f || corridorY == 270f )
				{
					pos = new Vector3( transform.position.x, transform.position.y + heightOffset, transform.position.z + offset );
				}
				//Straight
				else
				{
					pos = new Vector3( transform.position.x + offset, transform.position.y + heightOffset, transform.position.z );
				}
				//Spawn different types of coin packs
				GameObject go;
				if ( Random.Range( 0f, 1f ) < 0.5f )
				{
					go = (GameObject)Instantiate(CoinPack_5x1, pos, Quaternion.Euler( 0, corridorY, 0 ) );
					CoinManager.realNumberCoinsSpawned = CoinManager.realNumberCoinsSpawned + 5;
				}
				else
				{
					go = (GameObject)Instantiate(CoinPack_4x1_1x5, pos, Quaternion.Euler( 0, corridorY, 0 ) );
					CoinManager.realNumberCoinsSpawned = CoinManager.realNumberCoinsSpawned + 9;
				}
				go.transform.parent = transform;
				//Set the local rotation to 0 as well. This is so coins spawned over a slope will be parallel to the floor.
				go.transform.localRotation = Quaternion.Euler(0,0,0);
			}
		}
		//We might also spawn some special 100 value coins which are located higher.
		//You can only get these coins with the magic boots which allow you to jump higher
		if ( Random.Range( 0f, 1f ) < 0.12f )
		{
			Vector3 pos = new Vector3( transform.position.x, transform.position.y + 5.2f, transform.position.z );
			GameObject go = (GameObject)Instantiate(expensiveCoin, pos, Quaternion.Euler( 0, corridorY, 0 ) );
			CoinManager.realNumberCoinsSpawned = CoinManager.realNumberCoinsSpawned + 100;
			go.transform.parent = transform;

		}
	}*/
}
