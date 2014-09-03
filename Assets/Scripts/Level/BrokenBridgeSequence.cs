using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BrokenBridgeSequence : MonoBehaviour {

	Transform player;
	PlayerController playerController;
	Transform fairy;
	FairyController fairyController;

	bool hasBeenTriggered = false;

	public ParticleSystem fairySpell;

	float lastActivateTime = 0;
	int bridgePartIndex = 0;


	public GameObject hexagon;
	const float HEXAGON_SIZE = 0.9f;
	const float HALF_HEXAGON_SIZE = HEXAGON_SIZE/2f;

	float lane3StartLocalPos = 7f; 	//Center lane
	float lane1StartLocalPos; 	//Leftmost lane
	float lane2StartLocalPos; 	
	float lane4StartLocalPos; 	
	float lane5StartLocalPos; 	//Rightmost lane

	int rowIndex = 0;
	public int numberOfRows = 46;
	float localBridgeHeight = 1f;
	public Material semiMaterialized;
	public List<HexagonRowData> hexagonsActivePerRow = new List<HexagonRowData>(46);

	// Use this for initialization
	void Awake () {

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		player = playerObject.transform;
		playerController = playerObject.GetComponent<PlayerController>();

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairy = fairyObject.transform;
		fairyController = fairyObject.GetComponent<FairyController>();

		lane1StartLocalPos = lane3StartLocalPos + HEXAGON_SIZE; 	//Leftmost lane
		lane2StartLocalPos = lane3StartLocalPos + HALF_HEXAGON_SIZE; 	
		lane4StartLocalPos = lane3StartLocalPos + HALF_HEXAGON_SIZE; 	
		lane5StartLocalPos = lane3StartLocalPos + HEXAGON_SIZE; 	//Rightmost lane

	}
	

	public void startSequence()
	{
		//Slowdown player and remove player control
		print ("Start of broken bridge sequence");
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(23.4f, afterPlayerSlowdown ) );
	}

	void afterPlayerSlowdown()
	{
		playerController.anim.SetTrigger("Idle_Look");
		//Call fairy
		fairyController.setYRotationOffset( -10f );
		fairyController.Appear ();
		Invoke ("step1", 1f );
	}

	//Fairy tells something to player
	void step1()
	{
		AchievementDisplay.activateDisplayFairy( "By the White Tree! the bridge, its broken...", 0.35f, 2.5f );
		//Player looks at fairy
		playerController.lookOverShoulder( 0.4f, 2.75f );
		Invoke ("step2", 3.5f );
	}

	void step2()
	{
		//Fairy cast spell;
		fairyController.CastSpell();
		fairySpell.Play();
		audio.Play();
		Invoke ("step3", 1.5f );
	}

	void step3()
	{
		//Rebuild bridge magically
		testBridge();
		AchievementDisplay.activateDisplayFairy( "Quickly now! I can hear the troll.", 0.35f, 2.75f );
		Invoke ("step4", 5f );
	}

	void testBridge()
	{

		//Create subsequent rows
		for( int i = 0; i < numberOfRows; i++ )
		{
			createRow( i );
		}
	}
	
	void createRow( int rowIndex )
	{
		GameObject go;

			//lane 1
			go = (GameObject)Instantiate(hexagon, Vector3.zero, Quaternion.identity );
			go.transform.parent = gameObject.transform;
			go.transform.localPosition = new Vector3( -2f * HEXAGON_SIZE, localBridgeHeight,lane1StartLocalPos + rowIndex * HEXAGON_SIZE );
			if( !hexagonsActivePerRow[rowIndex].lane1Active )
			{
		   		go.renderer.material = semiMaterialized;
				go.collider.enabled = false;
			}
			//lane 2
			go = (GameObject)Instantiate(hexagon, Vector3.zero, Quaternion.identity );
			go.transform.parent = gameObject.transform;
			go.transform.localPosition = new Vector3( -HEXAGON_SIZE, localBridgeHeight,lane2StartLocalPos + rowIndex * HEXAGON_SIZE );
			if( !hexagonsActivePerRow[rowIndex].lane2Active )
			{
				go.renderer.material = semiMaterialized;
				go.collider.enabled = false;
			}
			//lane 3
			go = (GameObject)Instantiate(hexagon, Vector3.zero, Quaternion.identity );
			go.transform.parent = gameObject.transform;
			go.transform.localPosition = new Vector3( 0, localBridgeHeight,lane3StartLocalPos + rowIndex * HEXAGON_SIZE );
			if( !hexagonsActivePerRow[rowIndex].lane3Active )
			{
				go.renderer.material = semiMaterialized;
				go.collider.enabled = false;
			}
			//lane 4
			go = (GameObject)Instantiate(hexagon, Vector3.zero, Quaternion.identity );
			go.transform.parent = gameObject.transform;
			go.transform.localPosition = new Vector3( HEXAGON_SIZE, localBridgeHeight,lane4StartLocalPos + rowIndex * HEXAGON_SIZE );
			if( !hexagonsActivePerRow[rowIndex].lane4Active )
			{
				go.renderer.material = semiMaterialized;
				go.collider.enabled = false;
			}
			//lane 5
			go = (GameObject)Instantiate(hexagon, Vector3.zero, Quaternion.identity );
			go.transform.parent = gameObject.transform;
			go.transform.localPosition = new Vector3( 2f * HEXAGON_SIZE, localBridgeHeight,lane5StartLocalPos + rowIndex * HEXAGON_SIZE );
			if( !hexagonsActivePerRow[rowIndex].lane5Active )
			{
				go.renderer.material = semiMaterialized;
				go.collider.enabled = false;
			}
		print (" BRIDGE " + rowIndex + " center " + hexagonsActivePerRow[rowIndex].lane3Active );

	}

	void rebuildBridge()
	{
		//foreach( GameObject go in bridgeParts )
		//{
			//float delay = Random.Range( 0.05f, 0.15f );
			//Invoke ("makeBridgePartActive", lastActivateTime + delay );
			//lastActivateTime = lastActivateTime + delay;

		//}
	}

	void makeBridgePartActive()
	{
		//bridgeParts[bridgePartIndex].SetActive( true );
		//bridgePartIndex++;
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
	}

}
