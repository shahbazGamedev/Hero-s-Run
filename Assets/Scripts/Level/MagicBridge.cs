using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MagicBridge : MonoBehaviour {
	
	bool hasBeenTriggered = false;
	
	float lastActivateTime = 0;

	public GameObject hexagon;
	const float HEXAGON_SIZE = 1f;
	const float HALF_HEXAGON_SIZE = HEXAGON_SIZE/2f;

	float lane1StartLocalPos; 		//Leftmost lane
	float lane2StartLocalPos; 	
	float lane3StartLocalPos = 3.1f; 	//Center lane
	float lane4StartLocalPos; 	
	float lane5StartLocalPos; 		
	float lane6StartLocalPos; 		//Rightmost lane

	int rowIndex = 0;
	const int NUMBER_OF_ROWS = 46;
	float localBridgeHeight = -0.75f;
	public List<HexagonRowData> hexagonsActivePerRow = new List<HexagonRowData>(NUMBER_OF_ROWS);
	public float rowCreationDelay = 0; //How much time before the next row is created


	// Use this for initialization
	void Awake () {

		lane1StartLocalPos = lane3StartLocalPos + HEXAGON_SIZE; 	//Leftmost lane
		lane2StartLocalPos = lane3StartLocalPos + HALF_HEXAGON_SIZE; 	
		lane4StartLocalPos = lane3StartLocalPos; 	
		lane5StartLocalPos = lane3StartLocalPos + HALF_HEXAGON_SIZE;
		lane6StartLocalPos = lane3StartLocalPos + HEXAGON_SIZE; 	//Rightmost lane

	}

	void buildMagicBridge()
	{
		//Create rows
		for( int i = 0; i < hexagonsActivePerRow.Count; i++ )
		{
			if( rowCreationDelay != 0 )
			{
				Invoke ("createRow", lastActivateTime + rowCreationDelay );
				lastActivateTime = lastActivateTime + rowCreationDelay;
			}
			else
			{
				createRow();
			}
		}
	}
	
	void createRow()
	{
		GameObject go;

			//lane 1
			if( hexagonsActivePerRow[rowIndex].lane1Active )
			{
				go = (GameObject)Instantiate(hexagon, Vector3.zero, Quaternion.identity );
				go.transform.parent = gameObject.transform;
				go.transform.localPosition = new Vector3( -2.5f * HEXAGON_SIZE, localBridgeHeight,lane1StartLocalPos + rowIndex * HEXAGON_SIZE );
			}
			//lane 2
			if( hexagonsActivePerRow[rowIndex].lane2Active )
			{
				go = (GameObject)Instantiate(hexagon, Vector3.zero, Quaternion.identity );
				go.transform.parent = gameObject.transform;
				go.transform.localPosition = new Vector3( -1.5f * HEXAGON_SIZE, localBridgeHeight,lane2StartLocalPos + rowIndex * HEXAGON_SIZE );
			}
			//lane 3
			if( hexagonsActivePerRow[rowIndex].lane3Active || true )
			{
				go = (GameObject)Instantiate(hexagon, Vector3.zero, Quaternion.identity );
				go.transform.parent = gameObject.transform;
				go.transform.localPosition = new Vector3( -HALF_HEXAGON_SIZE, localBridgeHeight,lane3StartLocalPos + rowIndex * HEXAGON_SIZE );
			}
			//lane 4
			if( hexagonsActivePerRow[rowIndex].lane4Active || true )
			{
				go = (GameObject)Instantiate(hexagon, Vector3.zero, Quaternion.identity );
				go.transform.parent = gameObject.transform;
				go.transform.localPosition = new Vector3( HALF_HEXAGON_SIZE, localBridgeHeight,lane4StartLocalPos + rowIndex * HEXAGON_SIZE );
			}
			//lane 5
			if( hexagonsActivePerRow[rowIndex].lane5Active )
			{
				go = (GameObject)Instantiate(hexagon, Vector3.zero, Quaternion.identity );
				go.transform.parent = gameObject.transform;
				go.transform.localPosition = new Vector3( 1.5f * HEXAGON_SIZE, localBridgeHeight,lane5StartLocalPos + rowIndex * HEXAGON_SIZE );
			}
			//lane 6
			if( hexagonsActivePerRow[rowIndex].lane6Active )
			{
				go = (GameObject)Instantiate(hexagon, Vector3.zero, Quaternion.identity );
				go.transform.parent = gameObject.transform;
				go.transform.localPosition = new Vector3( 2.5f * HEXAGON_SIZE, localBridgeHeight,lane6StartLocalPos + rowIndex * HEXAGON_SIZE );
			}
			rowIndex++;
	}
	
	void OnEnable()
	{
		PlayerController.playerStateChanged += PlayerStateChange;
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
	}
	
	void OnDisable()
	{
		PlayerController.playerStateChanged -= PlayerStateChange;
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
	}
	
	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
		{
			CancelInvoke();
		}
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Build_Magic_Bridge && !hasBeenTriggered )
		{
			hasBeenTriggered = true;
			print ("Build_Magic_Bridge: it has " + hexagonsActivePerRow.Count + " rows." );
			buildMagicBridge();
		}
	}

	[System.Serializable]
	public class HexagonRowData
	{
		public bool lane1Active = false;
		public bool lane2Active = false;
		public bool lane3Active = true;
		public bool lane4Active = true;
		public bool lane5Active = false;
		public bool lane6Active = false;
	}

}
