using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FairylandBrokenBridgeSequence : MonoBehaviour {

	PlayerController playerController;
	FairyController fairyController;

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
	const int NUMBER_OF_ROWS = 134; //134 is full length. We can make the bridge shorter to force the player to jump.
	float localBridgeHeight = -0.75f;
	public List<HexagonRowData> hexagonsActivePerRow = new List<HexagonRowData>(NUMBER_OF_ROWS);

	// Use this for initialization
	void Awake () {

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		playerController = playerObject.GetComponent<PlayerController>();

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();

		lane1StartLocalPos = lane3StartLocalPos + HEXAGON_SIZE; 	//Leftmost lane
		lane2StartLocalPos = lane3StartLocalPos + HALF_HEXAGON_SIZE; 	
		lane4StartLocalPos = lane3StartLocalPos; 	
		lane5StartLocalPos = lane3StartLocalPos + HALF_HEXAGON_SIZE;
		lane6StartLocalPos = lane3StartLocalPos + HEXAGON_SIZE; 	//Rightmost lane

	}
	
	public void startSequence()
	{
		//Slowdown player and remove player control
		print ("Start of broken bridge sequence");
		playerController.placePlayerInCenterLane();
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(18.1f, afterPlayerSlowdown ) );
	}

	void afterPlayerSlowdown()
	{
		playerController.anim.SetTrigger("Idle_Look");
		//Call fairy
		fairyController.setYRotationOffset( -10f );
		fairyController.Appear ( FairyEmotion.Worried );
		Invoke ("step1", 1f );
	}

	//Fairy tells something to player
	void step1()
	{
		AchievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText("FAIRY_FAIRYLAND_BRIDGE"), 0.35f, 3.6f );
		Invoke ("step2", 3.75f );
	}

	//Fairy cast spell;
	void step2()
	{
		fairyController.CastSpell();
		Invoke ("step3", 4.2f );
	}

	//Spell works and bridge is rebuilt
	void step3()
	{
		//Rebuild bridge magically row by row
		rebuildBridge();
		Invoke ("step4", 4.6f );
	}
	
	//Make the fairy disappear
	//Player starts running again
	void step4()
	{
		fairyController.Disappear ();
		playerController.allowRunSpeedToIncrease = true;
		playerController.startRunning(false);
		fairyController.resetYRotationOffset();
	}
	
	void rebuildBridge()
	{
		print ("rebuildBridge: it has " + hexagonsActivePerRow.Count + " rows." );
		//Create rows
		float delay = 0.1f; //Used to be 0.135 to have jump
		for( int i = 0; i < hexagonsActivePerRow.Count; i++ )
		{
			Invoke ("createRow", lastActivateTime + delay );
			lastActivateTime = lastActivateTime + delay;
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
			if( hexagonsActivePerRow[rowIndex].lane3Active )
			{
				go = (GameObject)Instantiate(hexagon, Vector3.zero, Quaternion.identity );
				go.transform.parent = gameObject.transform;
				go.transform.localPosition = new Vector3( -HALF_HEXAGON_SIZE, localBridgeHeight,lane3StartLocalPos + rowIndex * HEXAGON_SIZE );
			}
			//lane 4
			if( hexagonsActivePerRow[rowIndex].lane4Active )
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
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
	}
	
	void OnDisable()
	{
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Broken_Bridge && !hasBeenTriggered )
		{
			hasBeenTriggered = true;

			startSequence();
		}
	}

	[System.Serializable]
	public class HexagonRowData
	{
		public bool lane1Active = false;
		public bool lane2Active = false;
		public bool lane3Active = false;
		public bool lane4Active = false;
		public bool lane5Active = false;
		public bool lane6Active = false;
	}

}
