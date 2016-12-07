using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SegmentInfo : MonoBehaviour {
	
	public GameObject tile;
	public TileType tileType;
	//Does this tile use straight corridors or bezier curves?
	public bool usesBezierCurve = false;
	//A tile can have multiple Cubic bezier curves.
	public List<BezierData> curveList = new List<BezierData>();
	//For drawing, each curve is divided into small line segments.
	public const int CURVE_DISTANCE_STEPS = 200;
	//This flag is used to avoid tileEntranceCrossed being called multiple time which can happen with onTriggerEnter
	public bool entranceCrossed = false;
	public bool addJumpBoost = false; //If true, give an extra boost when jumping
	public float tileEndHeight = 0; //if not zero, the height of the NEXT tile will be adjusted
	//tileIndex is populated at runtime. The Start tile has an index of 0, the second tile in the level has an index of 1, etc.
	public int tileIndex = -1;
	public int tileDepth = 1;
	public TileSubType tileSubType = TileSubType.Straight;

	[Header("Coin Generator")]
	[Tooltip("The name of the coin prefab to use. It should be located in the Resources/Level/Props/ folder.")]
	public string coin_prefab_name = "Coin_1";
	[Tooltip("Before generating new coins, should the previous ones be destroyed.")]
	public bool destroyExistingCoinsFirst = true;
	[Tooltip("Should you use a raycast to calculate the distance to ground. If true, coins will be placed at the height specified by distanceToGround.")]
	public bool useRaycast = true;
	public float distanceToGround = 0.8f;
	public float distanceBetweenCoins = 2.2f;
	const int LINE_VERTEX_COUNT = 200;
	public bool drawBezierGizmo = true;

	public void addCoins()
	{
		drawBezierGizmo = false;
		//Create the bezier control points if they do not exist
		createBezierData();

		//Consider destroying existing coins before adding new ones
		if( destroyExistingCoinsFirst )
		{
			destroyExistingCoins();
		}

		//Create a container to store the coins, but only if one doesn't already exist
		Transform coins = transform.FindChild("Coins");
		if( coins == null)
		{
			//Nope, it doesn't exist. We need to create it
			GameObject coinsGameObject = new GameObject("Coins");
			coins = coinsGameObject.transform;
			//Make the tile the parent of the Coins object
			coins.parent = transform;
			coins.localPosition = Vector3.zero;
		}

		//Load the coin prefab from the resources folder
		GameObject coinPrefab = (GameObject) Resources.Load("Level/Props/" + coin_prefab_name, typeof(GameObject));
		
		//Add the coins
		SegmentInfo.BezierData bezierData = curveList[0];
		GameObject go;
		int counter = 0;
		for(float i=bezierData.bezierStart.position.z; i < bezierData.bezierEnd.position.z - distanceBetweenCoins; i = i + distanceBetweenCoins )
		{
			Vector3 toPosition = new Vector3( 0, 0, i );
			if( useRaycast )
			{
				//Place the coin constant distance to ground by using a raycast
				toPosition.y = calculateYPosition( toPosition );
			}
			go = (GameObject)Instantiate(coinPrefab, toPosition, Quaternion.identity );
			go.transform.parent = coins;
			counter++;
			go.name = "Coin " + counter.ToString();
		}
		drawBezierGizmo = true;

   }

	void createBezierData()
	{
		if( curveList.Count == 0 )
		{
			SegmentInfo.BezierData bezierData = new BezierData();
			curveList.Add( bezierData );
			float tileLength = GenerateLevel.TILE_SIZE * tileDepth;

			//Create Start point, but only if one doesn't already exist
			Transform bezierStart = transform.FindChild("bezierStart");
			if( bezierStart == null)
			{
				//Nope, it doesn't exist. We need to create it
				GameObject bezierStartObject = new GameObject("bezierStart");
				bezierStart = bezierStartObject.transform;
				//Make the tile the parent of the point
				bezierStart.parent = transform;
				bezierData.bezierStart = bezierStart;
				//Position it at the beginning of the tile
				bezierStart.localPosition = new Vector3( 0, distanceToGround, -GenerateLevel.TILE_SIZE/2f );
			}
			//Create End point, but only if one doesn't already exist
			Transform bezierEnd = transform.FindChild("bezierEnd");
			if( bezierEnd == null)
			{
				//Nope, it doesn't exist. We need to create it
				GameObject bezierEndObject = new GameObject("bezierEnd");
				bezierEnd = bezierEndObject.transform;
				//Make the tile the parent of the point
				bezierEnd.parent = transform;
				bezierData.bezierEnd = bezierEnd;
		        switch (tileSubType)
				{
			        case TileSubType.Straight:
						bezierEnd.localPosition = new Vector3( 0, distanceToGround, -GenerateLevel.TILE_SIZE/2f + tileLength );
		                break;
			                
			        case TileSubType.Right:
			        case TileSubType.T_Junction:
						bezierEnd.localPosition = new Vector3( GenerateLevel.TILE_SIZE/2f, distanceToGround, 0 );
		                break;
			                
			        case TileSubType.Left:
						bezierEnd.localPosition = new Vector3( -GenerateLevel.TILE_SIZE/2f, distanceToGround, 0 );
		                break;
				}
			}
			//Create Control point 1, but only if one doesn't already exist
			Transform bezierControl1 = transform.FindChild("bezierControl1");
			if( bezierControl1 == null)
			{
				//Nope, it doesn't exist. We need to create it
				GameObject bezierControl1Object = new GameObject("bezierControl1");
				bezierControl1 = bezierControl1Object.transform;
				//Make the tile the parent of the point
				bezierControl1.parent = transform;
				bezierData.bezierControl1 = bezierControl1;
		        switch (tileSubType)
				{
			        case TileSubType.Straight:
						bezierControl1.localPosition = new Vector3( 0, distanceToGround, 0 );
		                break;
			                
			        case TileSubType.Right:
			        case TileSubType.T_Junction:
						bezierControl1.localPosition = new Vector3( -1f, distanceToGround, 0 );
		                break;
			                
			        case TileSubType.Left:
						bezierControl1.localPosition = new Vector3( 1f, distanceToGround, 0 );
		                break;
				}
			}
			//Create Control point 2, but only if one doesn't already exist
			Transform bezierControl2 = transform.FindChild("bezierControl2");
			if( bezierControl2 == null)
			{
				//Nope, it doesn't exist. We need to create it
				GameObject bezierControl2Object = new GameObject("bezierControl2");
				bezierControl2 = bezierControl2Object.transform;
				//Make the tile the parent of the point
				bezierControl2.parent = transform;
				bezierData.bezierControl2 = bezierControl2;
		        switch (tileSubType)
				{
			        case TileSubType.Straight:
						bezierControl2.localPosition = new Vector3( 0, distanceToGround, 0 );
		                break;
			                
			        case TileSubType.Right:
			        case TileSubType.T_Junction:
						bezierControl2.localPosition = new Vector3( -2f, distanceToGround, 1f );
		                break;
			                
			        case TileSubType.Left:
						bezierControl2.localPosition = new Vector3( 2f, distanceToGround, 1f );
		                break;
				}
			}
		}
	}

	void destroyExistingCoins()
	{
		//Find the Coins container
		Transform coins = transform.FindChild("Coins");
		if( coins != null)
		{
			GameObject.DestroyImmediate( coins.gameObject );
		}
	}


	float calculateYPosition( Vector3 toPosition )
	{
		RaycastHit hit;
		if (Physics.Raycast(new Vector3(toPosition.x, toPosition.y + 10f, toPosition.z), Vector3.down, out hit, 50f ))
		{
			Debug.Log("SegmentInfo: calculateYPosition - object below is " + hit.collider.name );
			return hit.point.y + distanceToGround;
		}
		else
		{
			Debug.LogWarning("SegmentInfo: calculateYPosition - there is no ground below position " + toPosition );
			return 0;
		}
	}

	//Display the bezier curve(s) if any.
	void OnDrawGizmos ()
	{
		if( curveList.Count > 0 && drawBezierGizmo )
		{
			Gizmos.color = Color.red;
			SegmentInfo.BezierData bezierData;
			float step = 1f/CURVE_DISTANCE_STEPS;
			for( int i=0; i < curveList.Count; i++ )
			{
				bezierData = curveList[i];
				Vector3 fromPosition = bezierData.bezierStart.position;
				for(int j=0; j < CURVE_DISTANCE_STEPS; j++ )
				{
					float t = j * step;
					Vector3 toPosition = Utilities.Bezier3( bezierData.bezierStart.position, bezierData.bezierControl1.position, bezierData.bezierControl2.position, bezierData.bezierEnd.position, t );
					Gizmos.DrawLine ( fromPosition, toPosition );
					fromPosition = toPosition;
		
				}
			}
		}
	}

	[System.Serializable]
	public class BezierData
	{
		public Transform bezierStart;
		public Transform bezierControl1;
		public Transform bezierControl2;
		public Transform bezierEnd;
	}

}
