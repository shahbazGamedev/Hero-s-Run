using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum SegmentTheme {
		Forest = 0,
		Fairyland = 1,
		Cemetery = 2,
		Hell = 3,
		Tutorial = 4,
		Volcano = 5,
		Island = 6
}

public enum TileType {
	None = -1,
	Start = 0,
	End = 1,
	Straight = 3,
	Left = 4,
	Right = 5,
	Straight_Log = 6,
	Straight_River = 7,
	Landmark_Windmill = 8,
	Landmark_Defense_Tower = 9,
	T_Junction_Landmark_Cemetery = 10,
	Landmark_Dragon_Lair = 11,
	Landmark_Dragon_Landing = 12,
	Landmark_Clocktower = 13,
	T_Junction = 15,
	Straight_River_Crossing = 16,
	Straight_Slope = 17,
	Straight_Double = 18,
	Straight_Bezier = 19,
	Straight_River_Log_Crossing = 20,
	Landmark_Banquet_Hall = 22,
	Landmark_Drawbridge = 24,
	Checkpoint = 25,
	Opening = 26,
	Landmark_Evil_Tree = 27,
	Opening2 = 28,
	Opening3 = 29,
	Opening4 = 30,
	Opening5 = 31,
	Landmark_Fairy_Message = 32,
	Landmark_Broken_Bridge = 33,
	T_Junction_Landmark_Cemetery_Queen = 34,
	Landmark_Cemetery_Queen = 35,
	Landmark_Magic_Bridge = 36,
	Landmark_Tomb_Start = 37,
	Landmark_Tomb_Double = 38,
	Start_Fairyland = 39

}

public class GenerateLevel  : MonoBehaviour {
	
	private LevelData levelData;
	
	const float TILE_SIZE = 36.4f;
	int tileDepthMult = 1; //A value of one means the tile depth is 1 x TILE_SIZE, a value of two means 2 x TILE_SIZE, etc.
	
	//worldRoadSegments is a List of game object tiles
	List<GameObject> worldRoadSegments = new List<GameObject>(300);
	int playerTileIndex = 0;		//Index of the active tile (the one the player is on).

	//tileCreationIndex is incremented each time a new tile is added.
	//It is also used to control the power up density as we add a power up every 'X' tiles.
	//Lastly it is appended at the name of every tile created to facilitate debugging.
	int tileCreationIndex = 0;

	Vector3 previousTilePos = new Vector3( 0,0,0 );
	Quaternion previousTileRot = Quaternion.identity;
	TileType previousTileType = TileType.None;

	//Generate level supports tiles that have slopes.
	//For a slope tile to work, the slope must use the X axis, not Z.
	//tileEndHeight is the delta between the height at the end of the tile and the height at the center of the tile.
	//So if a tile has a slope of 10%, the center height is 3.209 meters and the tileEndHeight is also 3.209 meters for a total elevation of 6.418 meters.
	//This number is used to make sure that the subsequent tiles are at the proper height.
	float  tileEndHeight = 0;

	//The number of visible tiles at any given time (all other tiles are deactivated)
	const int DEFAULT_NBR_VISIBLE_TILES = 3; 
	int nbrVisibleTiles = DEFAULT_NBR_VISIBLE_TILES;
	
	//For randomizing power ups
	PowerUpManager powerUpManager;
	
	//Path to Resources folder containing the tiles for the theme
	string currentThemePath = "";
	SegmentTheme currentTheme;
	
	//The surrounding plane (like an ocean) is always centered with the current tile
	Transform surroundingPlane;
	
	const int NUMBER_OF_TUTORIALS = 6; //Change Lane, Jump, Slide, etc.
	Dictionary<TutorialEvent,int> tutorialStartTileIndex = new Dictionary<TutorialEvent, int>(NUMBER_OF_TUTORIALS);

	//To improve performance by preloading prefabs and avoiding reloading tile prefabs that have previously been loaded.
	const int NUMBER_OF_THEMES = 6; //Island, Forest, Fairyland, Cemetery, Hell and Volcano. TUTORIAL does not use tilePrefabsPerTheme.
	Dictionary<SegmentTheme,Dictionary<TileType,GameObject>> tilePrefabsPerTheme  = new Dictionary<SegmentTheme,Dictionary<TileType,GameObject>>(NUMBER_OF_THEMES);

	//For the endless running game mode
	List<GameObject> recycledTiles = new List<GameObject>(50);

	//For seamless progression
	Queue<TileData> levelTileList = new Queue<TileData>();
	int seamlessLevelIndex = 0;

	void Awake ()
	{
		Debug.Log ("Initializing generate Level.");
		//The activity indicator may have been started in MainMenuHandler
		Handheld.StopActivityIndicator();

		levelData = LevelManager.Instance.getLevelData();
							
		//For power ups
		GameObject powerUpManagerObject = GameObject.FindGameObjectWithTag("PowerUpManager");
		powerUpManager = powerUpManagerObject.GetComponent<PowerUpManager>();
	}

	void loadTilePrefabs( SegmentTheme theme )
	{
		//Don't bother reloading the prefabs if they have already been loaded
		if( tilePrefabsPerTheme.ContainsKey(theme) ) return;

		GameObject go;

		//Load tile prefabs from the specified theme folder
		Object[] themePrefabs = Resources.LoadAll("Level/Tiles/" + theme.ToString() + "/", typeof(GameObject));
		Dictionary<TileType,GameObject> themePrefabsDict = new Dictionary<TileType,GameObject>(themePrefabs.Length);

		//Copy to dictionary
		for(int i = 0; i < themePrefabs.Length; i++ )
		{
			go = (GameObject)themePrefabs[i];
			SegmentInfo si = go.GetComponent<SegmentInfo>();
			if( si.tileType != TileType.None )
			{
				//Debug.Log ("loadTilePrefabs-adding tile type: " + si.tileType );
				themePrefabsDict.Add( si.tileType, go );
			}
		}
		tilePrefabsPerTheme.Add(theme, themePrefabsDict);

	}

	void Start ()
	{
		CreateLevel ();
	}
	
	void CreateLevel ()
	{
		//Reset values
		worldRoadSegments.Clear();
		tileCreationIndex = 0;
		tutorialStartTileIndex.Clear();
		playerTileIndex = 0;
		seamlessLevelIndex = 0;
						
		//LevelInfo has the parameters for a single level.
		//Get the level info for the current level.
		int levelToLoad = LevelManager.Instance.getNextLevelToComplete();
		seamlessLevelIndex = levelToLoad;

		LevelData.LevelInfo levelInfo = levelData.getLevelInfo( levelToLoad );
		//Also set it in the LevelManager
		LevelManager.Instance.setLevelInfo( levelInfo );

		//Sets the skybox, the directional light intensity and direction for the current level
		levelData.setSunParameters(levelInfo.sunType);

		//Enable the fog and change the fog tint if enabled
		//The fog surrounds and follows the player at all time for the entire level.
		if( levelInfo.hasFog )
		{
			GameObject weatherManagerObject = GameObject.FindGameObjectWithTag("WeatherManager");
			WeatherManager weatherManager = weatherManagerObject.GetComponent<WeatherManager>();
			weatherManager.setFogTint(levelInfo.fogTint, levelInfo.fogFade );
			weatherManager.setFogHeightDelta(levelInfo.fogHeightDelta );
		}

		//Verify if we should include a plane surrounding the tiles (like an ocean)
		if( levelInfo.includeSurroundingPlane )
		{
			GameObject prefab = Resources.Load( "Level/surroundingPlane") as GameObject;
			GameObject go = (GameObject)Instantiate(prefab, new Vector3( 0, -30f, 0 ), Quaternion.identity );
			surroundingPlane = go.transform;
			if( surroundingPlane.renderer.material != null )
			{
				surroundingPlane.renderer.material = levelInfo.surroundingPlaneMaterial;
			}
			else
			{
				Debug.LogWarning("GenerateLevel-CreateLevel: includeSurroundingPlane is set to true but no surroundingPlaneMaterial has been specified.");
			}
		}
		
		if( levelInfo.isTutorial )
		{
			//The tutorial uses addCustomTile and not the regular system.
			createTutorialLevel();
			//The only allowed power-up in the tutorial is Magic Boots
			PlayerStatsManager.Instance.setPowerUpSelected(PowerUpType.MagicBoots);
		}
		else
		{
			//Create all the road segments that compose the road
			List<LevelData.RoadSegment> roadSegmentList = levelInfo.roadSegmentList;

			if( levelInfo.startTile != TileType.None )
			{
				//Instantiate Start tile. There is only one Start tile per level.
				//The Start tile has the same theme as the first road segment.
				setCurrentTheme( roadSegmentList[0].theme );
				addTile( levelInfo.startTile );
			}


			for( int i=0; i < roadSegmentList.Count; i++ )
			{
				addRoadSegment( roadSegmentList[i] );
			}
		}

		levelInfo.lengthInMeters = calculateLevelLength();

		//The player controller needs info about the tile the player is on.
		setFirstTileInfoInPlayer();

		//Make the first few tiles active
		activateInitialTiles(0);

		//Fade-in the level ambience soundtrack
		StartCoroutine( SoundManager.fadeInAmbience( levelInfo.AmbienceSound, 10f ) );

		//Should we display the Tap to Play message right away by changing the game state to GameState.Menu or will the game state be set by another script?
		if( !levelInfo.waitForTapToPlay )
		{
			GameManager.Instance.setGameState(GameState.Menu);
		}

		Debug.Log("GenerateLevel-CreateLevel: Level " + levelInfo.LevelName + " has been created." );
		Debug.Log("GenerateLevel-CreateLevel: The number of coins spawned is : " + CoinManager.realNumberCoinsSpawned );
		Debug.Log("GenerateLevel-CreateLevel: The level length in meters is : " + levelInfo.lengthInMeters );

	}

	float calculateLevelLength()
	{
		float levelLength = 0;
		GameObject tile;
		int numberOfTJunctions = 0;

		for(int i = 0; i < worldRoadSegments.Count; i++ )
		{
			tile = worldRoadSegments[i];
			SegmentInfo si = tile.GetComponent<SegmentInfo>();
			if( si.tileType == TileType.End )
			{
				//The cullis gate is 34.6 meters from the start of the tile.
				//But the checkpoint trigger (which changes the game state) is at 18.2 meters. The trigger depth is 1 meters. So we have, 18.2m - 0.5m - 17.7m.
				//When the game state is not Normal, the run distance stops being calculated.
				levelLength = levelLength + 17.7f;
			}
			else if( si.tileType == TileType.Checkpoint )
			{
				//The checkpoint trigger is 18.2 meters from the start of the tile. The trigger depth is 1 meters. So we have, 18.2m - 0.5m - 17.7m.
				levelLength = levelLength + 17.7f;
			}
			else if( si.tileType == TileType.Start )
			{
				//The distance between the player's start position (0,ground height,0) and the end of the tile, is 54.4 meters.
				levelLength = levelLength + 54.4f;
			}
			else
			{
				levelLength = levelLength + getTileDepth(si.tileType) * TILE_SIZE;
			}
			if( si.tileType == TileType.T_Junction || si.tileType == TileType.T_Junction_Landmark_Cemetery ) numberOfTJunctions++;

		}
		//T-Junctions create 4 tiles on the left path and 2 tiles on the right path. All the tiles have a depthTileMult of 1.
		//For the time being, we are calculating the length of the right path only.
		//So we need to substract the length added by the left path tiles.
		levelLength = levelLength - (numberOfTJunctions * 4 * TILE_SIZE );

		return levelLength;
	}

	//The player controller needs info about the tile the player is on.
	//This is normally set each time the player crosses an Entrance trigger.
	//However:
	//they are no Entrance in a Start tile and
	//when a player starts the game at a checkpoint, the player will be positioned in the center of the tile and therefore not cross the Entrance trigger for that tile.
	//Because of that, we simply use the info from the first tile (the one with index 0) in worldRoadSegments.
	private void setFirstTileInfoInPlayer()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		PlayerController playerController = player.GetComponent<PlayerController>();
		GameObject firstTile = worldRoadSegments[0];
		playerController.currentTile = firstTile;
		playerController.tileRotationY = firstTile.transform.eulerAngles.y;
		playerController.currentTilePos = firstTile.transform.position;
		SegmentInfo si = firstTile.GetComponent<SegmentInfo>();
		playerController.currentTileType = si.tileType;
		si.isFirstTileOfLevel = true;

		//If the player starts off on a Start tile, the camera will be looking at the front of player and do a rotation when the player starts running.
		//However, if the player is not on a Start tile and is starting at a Checkpoint, we want the camera to look at the back of the player (and therefore, there is no need for a rotation when the player starts running).
		//if( si.tileType != TileType.Start )
		if( true )
		{
			SimpleCamera sc = player.GetComponent<SimpleCamera>();
			sc.playCutscene(CutsceneType.Checkpoint);
		}
	}

	private void setCurrentTheme( SegmentTheme newTheme )
	{
		currentThemePath = "Level/Tiles/" + newTheme.ToString() + "/";
		currentTheme = newTheme;
	}

	float calculateTileHeight( float xTileAngle, float tileDepthM )
	{
		if( xTileAngle == 0 )
		{
			return 0;
		}
		else
		{
			return -TILE_SIZE/2f * tileDepthM * Mathf.Tan(xTileAngle * Mathf.Deg2Rad);
		}
	}

	//Important: as previousTileType value, use one of the three basic tile types (Straight, Left or Right). Do
	//not use the precise tile type (such as Straight_double) or else the method ensureTileHasZeroRotation won't work as intended.
	private GameObject addTile ( TileType type )
	{
		GameObject tile = getRecycledTile(type);
		if( tile != null )
		{
			//Debug.LogWarning("RECYCLING " + tile.name + " tile creation index " + tileCreationIndex );
		}
		else
		{
			//Debug.LogWarning("INSTANTIATING " + type + " tile creation index " + tileCreationIndex + " current theme is " + currentTheme );
		}

		return addTile( type, tile );
	}


	//Important: as previousTileType value, use one of the three basic tile types (Straight, Left or Right). Do
	//not use the precise tile type (such as Straight_double) or else the method ensureTileHasZeroRotation won't work as intended.
	private GameObject addTile ( TileType type, GameObject recycledTile )
	{
		GameObject go = null;
		GameObject prefab = null;
		Quaternion tileRot = Quaternion.identity;
		Vector3 tilePos = Vector3.zero;

		float tileHeight = 0;

		if( recycledTile == null )
		{
			//Instantiate the prefab
			if( !tilePrefabsPerTheme.ContainsKey(currentTheme) ) loadTilePrefabs( currentTheme );

			Dictionary<TileType,GameObject> tilePrefabs = tilePrefabsPerTheme[currentTheme];
			if( tilePrefabs == null ) Debug.LogError("addTile: tilePrefabsPerTheme does not contain entries for theme " + currentTheme );
			if( tilePrefabs.ContainsKey(type) )
			{
				prefab = tilePrefabs[type];
			}
			else
			{
				Debug.LogError("addTile: could not find prefab for the tile type: " + type + " in the current theme " + currentTheme + " folder." );
			}

			tileHeight = calculateTileHeight( prefab.transform.eulerAngles.x,1 ); //From a slope perspective, the slope length is one, even though the tile is longer
			tileRot = getTileRotation(prefab.transform.eulerAngles.x);
			tilePos = getTilePosition(tileHeight);
			go = (GameObject)Instantiate(prefab, tilePos, tileRot );
		}
		else
		{
			//Use the tile given to us
			go = recycledTile;
			tileHeight = calculateTileHeight( go.transform.eulerAngles.x,1 ); //From a slope perspective, the slope length is one, even though the tile is longer
			tileRot = getTileRotation(go.transform.eulerAngles.x);
			tilePos = getTilePosition(tileHeight);
			go.transform.rotation = tileRot;
			go.transform.position = tilePos;
		} 
		
		tileDepthMult= getTileDepth( type );

		previousTileType = getTileSubType(type); //for constructing the level, we use the simpler types like straight, left and right
		SegmentInfo si = getSegmentInfo( go );
		if( si.tileEndHeight != 0 )
		{
			tileEndHeight = si.tileEndHeight;
		}
		else
		{
			tileEndHeight = tileHeight;
		}
		si.entranceCrossed = false;
		si.tile = go;
		previousTilePos = tilePos;
		previousTileRot = tileRot;
		powerUpManager.considerAddingPowerUp( go, tileCreationIndex );
		go.SetActive( false );
		worldRoadSegments.Add( go );
		if( recycledTile == null ) go.name = go.name + "-" + tileCreationIndex.ToString();
		tileCreationIndex++;

		return go;
	}

	int getTileDepth( TileType type )
	{
		int depth = 0;

		switch (type)
		{
			case TileType.Straight:
			case TileType.Straight_Log:
			case TileType.Straight_River:
			case TileType.Straight_River_Crossing:
			case TileType.Straight_River_Log_Crossing:
			case TileType.Left:
			case TileType.Opening5:
			case TileType.Right:
			case TileType.Landmark_Windmill:
			case TileType.Landmark_Defense_Tower:
			case TileType.Landmark_Dragon_Landing:
			case TileType.Straight_Slope:
			case TileType.T_Junction:
			case TileType.T_Junction_Landmark_Cemetery:
			case TileType.T_Junction_Landmark_Cemetery_Queen:
			case TileType.Landmark_Cemetery_Queen:
				depth = 1;
				break;
			
			case TileType.Opening:
			case TileType.Opening2:
			case TileType.Opening3:
			case TileType.Opening4:
			case TileType.Start:
			case TileType.Landmark_Fairy_Message:
			case TileType.Landmark_Evil_Tree:
			case TileType.End:
			case TileType.Checkpoint:
			case TileType.Straight_Double:
			case TileType.Straight_Bezier:
			case TileType.Landmark_Dragon_Lair:
			case TileType.Landmark_Clocktower:
			case TileType.Landmark_Drawbridge:
			case TileType.Landmark_Banquet_Hall:
			case TileType.Landmark_Tomb_Double:
				depth = 2;
				break;

			case TileType.Landmark_Tomb_Start:
			case TileType.Landmark_Broken_Bridge:
			case TileType.Start_Fairyland:
				depth = 3;
				break;

			case TileType.Landmark_Magic_Bridge:
				depth = 4;
				break;

			default:
				Debug.LogError("GenerateLevel-getTileDepth: unknown tile type specified: " + type );
				break;
		}
		return depth;
	}


	//Important: as previousTileType value, use one of the three basic tile types (Straight, Left or Right). Do
	//not use the precise tile type (such as Straight_double) or else the method ensureTileHasZeroRotation won't work as intended.
	private GameObject addCustomTile ( string tileName, int tileDepthMult, TileType type )
	{
		GameObject prefab = getTilePrefab(tileName);
		float tileHeight = calculateTileHeight( prefab.transform.eulerAngles.x,tileDepthMult );
		Quaternion tileRot = getTileRotation(prefab.transform.eulerAngles.x);
		Vector3 tilePos = getTilePosition(tileHeight);
		GameObject go = (GameObject)Instantiate(prefab, tilePos, tileRot );
		previousTileType = type;
		this.tileDepthMult = tileDepthMult;
		tileEndHeight = tileHeight;
		SegmentInfo si = getSegmentInfo( go );
		si.entranceCrossed = false;
		si.tile = go;
		previousTilePos = tilePos;
		previousTileRot = tileRot;
		go.SetActive( false );
		worldRoadSegments.Add( go );
		go.name = go.name + tileCreationIndex.ToString();
		tileCreationIndex++;
		
		return go;
	}

	//Look for the prefab in the theme folder. If it is not found, look in the shared folder.
	private GameObject getTilePrefab( string tileName )
	{
		//Step 1 - try to get the tile prefab in the current theme folder
		GameObject prefab = Resources.Load(currentThemePath + tileName) as GameObject;
		//Step 2 - if it is not found, give an error message
		if( prefab == null )
		{
			Debug.LogError("GenerateLevel-getTilePrefab: Unable to locate the tile named " + tileName + " in the theme, " + currentThemePath + ", folder.");
		}
		return prefab;
	}

	//Height is the Y component of the tile prefab.
	private Vector3 getTilePosition( float height )
	{
		Vector3 tilePos = Vector3.zero;
		float previousTileRotY = Mathf.Floor( previousTileRot.eulerAngles.y );
		float tileDepth =  tileDepthMult * TILE_SIZE;
		//Determine the tile's height.
		float tileHeight = height + tileEndHeight + previousTilePos.y;
		switch (previousTileType)
		{
			case TileType.Straight:
			if( previousTileRotY == 0 )
			{
				tilePos.Set ( previousTilePos.x, tileHeight, previousTilePos.z + tileDepth );
			}
			else if( previousTileRotY == 270f || previousTileRotY == -90f )
			{
				tilePos.Set ( previousTilePos.x - tileDepth, tileHeight, previousTilePos.z );				
			}
			else
			{
				tilePos.Set ( previousTilePos.x + tileDepth, tileHeight, previousTilePos.z  );				
			}
			return tilePos;
	
	        case TileType.Left:
			if( previousTileRotY == 0 )
			{
				tilePos.Set ( previousTilePos.x - tileDepth, tileHeight, previousTilePos.z );
			}
			else if( previousTileRotY == 270f || previousTileRotY == -90f )
			{
				tilePos.Set ( previousTilePos.x , tileHeight, previousTilePos.z - tileDepth );				
			}
			else
			{
				tilePos.Set ( previousTilePos.x, tileHeight, previousTilePos.z + tileDepth );				
			}
			return tilePos;
		
	        case TileType.T_Junction:
			case TileType.T_Junction_Landmark_Cemetery:
			tilePos.Set ( previousTilePos.x + tileDepth, tileHeight, previousTilePos.z );
			return tilePos;

			case TileType.Right:
			if( previousTileRotY == 0 )
			{
				tilePos.Set ( previousTilePos.x + tileDepth, tileHeight, previousTilePos.z );
			}
			else if( previousTileRotY == 270f || previousTileRotY == -90f )
			{
				tilePos.Set ( previousTilePos.x , tileHeight, previousTilePos.z + tileDepth );				
			}
			else
			{
				tilePos.Set ( previousTilePos.x, tileHeight, previousTilePos.z - tileDepth );				
			}
			return tilePos;
			
	        default:
			tilePos.Set( 0,0,0 );
            return tilePos;
		}
	}

	//Slope is the X rotation of the tile prefab.
	private Quaternion getTileRotation( float slope )
	{
		float previousTileRotY = Mathf.Floor( previousTileRot.eulerAngles.y );
		Quaternion tileRot = Quaternion.identity;
		TileType subType = getTileSubType( previousTileType );
		switch (subType)
		{
			case TileType.None:
			return Quaternion.identity;
		
			case TileType.Straight:
			tileRot =  Quaternion.Euler( slope, previousTileRot.eulerAngles.y, 0 );
			return tileRot;

			case TileType.Left:
			if( previousTileRotY == 0 )
			{
				tileRot =  Quaternion.Euler(slope, -90f, 0);
			}
			else if( previousTileRotY == 270f || previousTileRotY == -90f )
			{
				tileRot =  Quaternion.Euler( slope, 0, 0 );
			}
			else
			{
				tileRot =  Quaternion.Euler( slope, 0, 0 );
			}
			return tileRot;
		
			case TileType.Right:
			if( previousTileRotY == 0 )
			{
				tileRot =  Quaternion.Euler( slope, 90f, 0 );
			}
			else if( previousTileRotY == 270f || previousTileRotY == -90f )
			{
				tileRot =  Quaternion.Euler( slope, 0, 0 );
			}
			else
			{
				tileRot =  Quaternion.Euler( slope, 0, 0 );
			}
			return tileRot;
			
	        default:
			Debug.LogError ("getTileRotation : unhandled tile type: " + subType );
			tileRot.eulerAngles = Vector3.zero;
			return tileRot;
		}
	}
	
	//nbrTilesInSegment includes the road segment end tile
	private  void addRoadSegment ( LevelData.RoadSegment roadSegment )
	{
		int localTileIndex = 0;
		setCurrentTheme( roadSegment.theme );

		int maxIndex = roadSegment.NumberOfTiles - 1;
		
		while ( localTileIndex < maxIndex )
		{
			addSegmentTile();
			localTileIndex++;
		}

		//Add end tile
        switch (roadSegment.endTile)
		{
		
		case TileType.Landmark_Windmill:
			//We want the Landmark tile to have a 0 degree rotation.
			ensureTileHasZeroRotation();
			addTile( TileType.Landmark_Windmill );
			addTile( TileType.Left );		
			addTile( TileType.Straight );		
            break;

		case TileType.Landmark_Dragon_Lair:
			//We want the Dragon Lair tile to have a 0 degree rotation.
			ensureTileHasZeroRotation();
			addTile( TileType.Landmark_Dragon_Lair );
			break;

		case TileType.T_Junction_Landmark_Cemetery:
			addRandomTJunction( TileType.T_Junction_Landmark_Cemetery );
			break;

		case TileType.T_Junction_Landmark_Cemetery_Queen:
			addRandomTJunction( TileType.T_Junction_Landmark_Cemetery_Queen );
			break;

		case TileType.Landmark_Defense_Tower:
			//We want the Landmark tile to have a 0 degree rotation.
			ensureTileHasZeroRotation();
			addTile( TileType.Landmark_Defense_Tower );
			addTile( TileType.Left );		
			addTile( TileType.Straight );		
			break;

		case TileType.Landmark_Broken_Bridge:
			//We want the Landmark tile to have a 0 degree rotation.
			ensureTileHasZeroRotation();
			addTile( TileType.Landmark_Broken_Bridge );
			break;

		case TileType.Landmark_Tomb_Start:
			//We want the Landmark tile to have a 0 degree rotation.
			ensureTileHasZeroRotation();
			addTile( TileType.Landmark_Tomb_Start );
			break;

		default:
			addTile( roadSegment.endTile );
			break;
		}
	}

	private void addRandomLandmark()
	{
		TileType randomLandmark = TileType.None;
		//Step 1 select a random landmark appropriate for the theme
		float rd = Random.value;
		switch (currentTheme)
		{
			case SegmentTheme.Cemetery:
				if( rd < 0.33f)
				{
					randomLandmark = TileType.Landmark_Clocktower;
				}
				else if( rd < 0.66f)
				{
					randomLandmark = TileType.T_Junction_Landmark_Cemetery;
				}
				else
				{
					randomLandmark = TileType.Landmark_Evil_Tree;
				}
				break;
			case SegmentTheme.Forest:
				if( rd < 0.3f)
				{
					randomLandmark = TileType.Landmark_Clocktower;
				}
				else if( rd < 0.6f)
				{
				randomLandmark = TileType.Landmark_Clocktower;
				}
				else if( rd < 0.9f)
				{
				randomLandmark = TileType.Landmark_Clocktower;
				}
				else
				{
				randomLandmark = TileType.Landmark_Clocktower;
				}
				break;
			case SegmentTheme.Hell:
				if( rd < 0.33f)
				{
					randomLandmark = TileType.Landmark_Banquet_Hall;
				}
				else if( rd < 0.67f)
				{
					randomLandmark = TileType.Landmark_Clocktower;
				}
				else
				{
					randomLandmark = TileType.Landmark_Drawbridge;
				}
				break;
			default:
				return;
		}
		//Step 2 add the landmark
		switch (randomLandmark)
		{
			
			case TileType.Landmark_Windmill:
				//We want the Landmark tile to have a 0 degree rotation.
				ensureTileHasZeroRotation();
				addTile( TileType.Landmark_Windmill );
				addTile( TileType.Left );		
				addTile( TileType.Straight );		
				break;

			case TileType.T_Junction_Landmark_Cemetery:
				addRandomTJunction( TileType.T_Junction_Landmark_Cemetery );
				break;
				
			case TileType.Landmark_Defense_Tower:
				//We want the Landmark tile to have a 0 degree rotation.
				ensureTileHasZeroRotation();
				addTile( TileType.Landmark_Defense_Tower );
				addTile( TileType.Left );		
				addTile( TileType.Straight );		
				break;
				
			default:
				addTile( randomLandmark );
				break;
		}
	}


	private void prepareTileList ( int levelToPrepare )
	{

		TileType beforePrepareListTileType = previousTileType;
		Quaternion beforePrepareListRotation = Quaternion.Euler( previousTileRot.eulerAngles.x, previousTileRot.eulerAngles.y, previousTileRot.eulerAngles.z );
		Vector3 beforePrepareListPosition = new Vector3( previousTilePos.x, previousTilePos.y, previousTilePos.z );

		LevelData.LevelInfo levelInfo = levelData.getLevelInfo( levelToPrepare );

		//Also set it in the LevelManager
		LevelManager.Instance.setLevelInfo( levelInfo );

		//Create all the road segments that compose the road
		List<LevelData.RoadSegment> roadSegmentList = levelInfo.roadSegmentList;

		Debug.LogWarning ("prepareTileList: level name: " + levelInfo.LevelName + ". It has " + roadSegmentList.Count + " road segments." );

		int maxIndex = 0;

		for( int i=0; i < roadSegmentList.Count; i++ )
		{
			maxIndex = roadSegmentList[i].NumberOfTiles - 1; //maxIndex does not include the end tile
			for( int j=0; j < maxIndex; j++ )
			{
				TileType tileCreationType = getSegmentTile (roadSegmentList[i].theme);
				if( tileCreationType != TileType.None )
				{
					addTileData( tileCreationType, roadSegmentList[i].theme );
				}
			}
			addRoadSegmentEndTile( roadSegmentList[i].endTile, roadSegmentList[i].theme );
		}

		previousTileType = beforePrepareListTileType;
		previousTileRot  = Quaternion.Euler( beforePrepareListRotation.eulerAngles.x, beforePrepareListRotation.eulerAngles.y, beforePrepareListRotation.eulerAngles.z );
		previousTilePos  = new Vector3( beforePrepareListPosition.x, beforePrepareListPosition.y, beforePrepareListPosition.z );

	}

	private void addTileData( TileType type, SegmentTheme theme )
	{
		TileData tileData = new TileData();
		tileData.tileTheme = theme;
		tileData.tileType = type;
		levelTileList.Enqueue (tileData);
		previousTileRot = getTileRotation(0);
		previousTileType = type;
		//print ("addTileData: adding tile type " + tileData.tileType + " with theme " + tileData.tileTheme + " prev rot " + previousTileRot.eulerAngles );
	}

	private TileType getTileSubType( TileType type )
	{
		switch (type)
		{
		case TileType.None:
			return TileType.None;
			
		case TileType.Opening:
		case TileType.Landmark_Evil_Tree:
		case TileType.Start:
		case TileType.End:
		case TileType.Landmark_Banquet_Hall:
		case TileType.Landmark_Clocktower:
		case TileType.Landmark_Dragon_Lair:
		case TileType.Landmark_Dragon_Landing:
		case TileType.Landmark_Drawbridge:
		case TileType.Straight_Bezier:
		case TileType.Straight_Double:
		case TileType.Landmark_Fairy_Message:
		case TileType.Landmark_Broken_Bridge:
		case TileType.Straight_Log:
		case TileType.Straight_River:
		case TileType.Straight_River_Crossing:
		case TileType.Straight_River_Log_Crossing:
		case TileType.Straight_Slope:
		case TileType.Checkpoint:
		case TileType.Straight:
		case TileType.Landmark_Cemetery_Queen:
		case TileType.Landmark_Magic_Bridge:
		case TileType.Landmark_Tomb_Start:
		case TileType.Landmark_Tomb_Double:
		case TileType.Start_Fairyland:
			return TileType.Straight;

		case TileType.Left:
			return TileType.Left;
			
		case TileType.T_Junction:
		case TileType.T_Junction_Landmark_Cemetery:
		case TileType.T_Junction_Landmark_Cemetery_Queen:
		case TileType.Landmark_Defense_Tower:
		case TileType.Landmark_Windmill:
		case TileType.Right:
			return TileType.Right;
			
		default:
			Debug.LogWarning ("getTileSubType : unhandled tile type: " + type );
			return TileType.None;
		}
	}

	private void addRoadSegmentEndTile( TileType endTileType, SegmentTheme theme )
	{
		switch (endTileType)
		{
			
		case TileType.Landmark_Windmill:
			//We want the Landmark tile to have a 0 degree rotation.
			ensureTileHasZeroRotation2(theme);
			addTileData(TileType.Landmark_Windmill, theme);
			addTileData(TileType.Left, theme);
			addTileData(TileType.Straight, theme);
			break;
			
		case TileType.Landmark_Dragon_Lair:
			//We want the Dragon Lair tile to have a 0 degree rotation.
			ensureTileHasZeroRotation2(theme);
			addTileData(TileType.Landmark_Dragon_Lair, theme);
			break;
			
		case TileType.T_Junction_Landmark_Cemetery:
			addRandomTJunction2( TileType.T_Junction_Landmark_Cemetery, theme );
			break;
			
		case TileType.T_Junction_Landmark_Cemetery_Queen:
			addRandomTJunction2( TileType.T_Junction_Landmark_Cemetery_Queen, theme );
			break;

		case TileType.Landmark_Defense_Tower:
			//We want the Landmark tile to have a 0 degree rotation.
			ensureTileHasZeroRotation2(theme);
			addTileData(TileType.Landmark_Defense_Tower, theme );
			addTileData(TileType.Left, theme);
			addTileData(TileType.Straight, theme);
			break;
			
		case TileType.Landmark_Broken_Bridge:
			//We want the Landmark tile to have a 0 degree rotation.
			ensureTileHasZeroRotation2(theme);
			addTileData(TileType.Landmark_Broken_Bridge, theme );
			break;

		case TileType.Landmark_Tomb_Start:
			//We want the Landmark tile to have a 0 degree rotation.
			ensureTileHasZeroRotation2(theme);
			addTileData(TileType.Landmark_Tomb_Start, theme );
			break;

		default:
			addTileData(endTileType, theme);
			break;
		}
	}

	//Add tiles such as to garanty that the tile added after these will have a zero Y rotation.
	private void ensureTileHasZeroRotation2( SegmentTheme theme)
	{
		float yRot = Mathf.Floor( previousTileRot.eulerAngles.y );
		TileType subType = getTileSubType( previousTileType );
		switch (subType)
		{
		case TileType.Left:
			if( yRot == 0 )
			{
				addTileData(TileType.Right, theme);
			}
			return;
			
		case TileType.Right:
			if( yRot == 0 )
			{
				addTileData(TileType.Left, theme);
			}
			return;
			
		case TileType.Straight:
			if( yRot == 270f || yRot == -90f )
			{
				addTileData(TileType.Right, theme);
			}
			else if( yRot == -270f || yRot == 90f )
			{
				addTileData(TileType.Left, theme);
			}
			return;
		}
	}

	private void addRandomTJunction2( TileType tJunctionTileType, SegmentTheme theme )
	{
		//We want the T-Junction tile to have a 0 degree rotation.
		ensureTileHasZeroRotation2( theme );
		//Adding T-Junction
		//The additional left and right tiles that are associated with a T-Junction
		//will be added at runtime.
		addTileData(tJunctionTileType, theme);
	}

	public class TileData
	{
		public TileType tileType = TileType.None;
		public SegmentTheme tileTheme;
	}

	//Add tiles such as to garanty that the tile added after these will have a zero Y rotation.
	private void ensureTileHasZeroRotation()
	{
		float yRot = Mathf.Floor( previousTileRot.eulerAngles.y );
		TileType subType = getTileSubType( previousTileType );

		switch (subType)
		{
			case TileType.Left:
			if( yRot == 0 )
			{
				addTile ( TileType.Right );
			}
			return;

			case TileType.Right:
			if( yRot == 0 )
			{
				addTile ( TileType.Left );
			}
			return;

			case TileType.Straight:
			if( yRot == 270f || yRot == -90f )
			{
				addTile ( TileType.Right );
			}
			else if( yRot == -270f || yRot == 90f )
			{
				addTile ( TileType.Left );
			}
			return;
		}
	}

	private void addTileInEndlessMode()
	{
		if( Random.value < 0.05f )
		{
			//Add a random landmark tile
			addRandomLandmark();
		}
		else
		{
			//Add a random normal tile
			addSegmentTile();
		}
	}


	private void addSegmentTile()
	{
		float previousRotY = Mathf.Floor( previousTileRot.eulerAngles.y );

		//Straight section
		if( Random.value < 0.55f )
		{
			float rdLog = Random.value;
			if( rdLog < 0.1f )
			{
				//Add a straight slope
				addTile ( TileType.Straight_Slope );
			}
			else if( rdLog < 0.25f )
			{
				//Add a straight tile with a log obstacle in the middle
				addTile ( TileType.Straight_Log );
			}
			else if( rdLog  < 0.45f )
			{
				//Add a river straight tile, but only if the preceding was a straight tile
				//or else you can see half of the waterfall rock through the trees and it does not look nice.
				//Also, we do not want two back-to-back rivers.
				if ( previousTileType == TileType.Straight || previousTileType == TileType.Straight_Log )
				{
					//We have two types of rivers: stone crossing and broken bridge.
					if( Random.value <= 0.5f )
					{
						addTile ( TileType.Straight_River );
					}
					else
					{
						if( currentThemePath == "Level/Tiles/Forest/" || currentThemePath == "Level/Tiles/Hell/" )
						{
							if( Random.value <= 0.4f )
							{
								addTile ( TileType.Straight_River_Crossing );
							}
							else
							{
								addTile ( TileType.Straight_River_Log_Crossing );
							}
						}
						else
						{
							addTile ( TileType.Straight_River_Crossing );
						}
					}
				}
				else
				{
					//Add a normal straight tile
					addTile ( TileType.Straight );
				}
			}
			else if( rdLog < 0.7f )
			{
				//Add a normal straight tile
				addTile ( TileType.Straight );
			}
			else if( rdLog < 0.9f ) //normal value is 0.9f
			{
				//Add a double length straight tile
				if ( previousTileType == TileType.Straight_Double )
				{
					//Add a normal straight tile. We don't want two of them back to back
					addTile ( TileType.Straight );
				}
				else
				{
					//Add a double length straight tile
					addTile ( TileType.Straight_Double );
				}
			}
			else
			{
				//Add a T-junction
				addRandomTJunction( TileType.T_Junction );
			}
		}
		else
		{
			//Turn section
			if( previousTileType == TileType.Straight )
			{
				if( previousRotY == 0 )
				{
					if( Random.value <= 0.5f )
					{
						addTile ( TileType.Left );
					}
					else
					{
						addTile ( TileType.Right );					
					}
				}
				else if( previousRotY == 270f || previousRotY == -90f )
				{
					addTile ( TileType.Right );
				}
				else
				{
					addTile ( TileType.Left );
				}

			}
			else if( previousTileType == TileType.Left )
			{
				if( previousRotY == 0 )
				{
					addTile ( TileType.Right );					
				}
				else if( previousRotY == 270f || previousRotY == -90f )
				{
					Debug.LogWarning("GenerateLevel-addSegmentTile: invalid angle.");
				}
				else
				{
					if(  Random.value <= 0.5f )
					{
						addTile ( TileType.Left );
					}
					else
					{
						addTile ( TileType.Right );					
					}

				}
			}
			else if( previousTileType == TileType.Right )
			{
				if( previousRotY == 0 )
				{
					addTile ( TileType.Left );					
				}
				else if( previousRotY == 270f || previousRotY == -90f )
				{
					if( Random.value <= 0.5f )
					{
						addTile ( TileType.Left );
					}
					else
					{
						addTile ( TileType.Right );					
					}
				}
				else
				{
					Debug.LogWarning("GenerateLevel-addSegmentTile: invalid angle.");

				}
			}
		}
	}

	private TileType getSegmentTile( SegmentTheme tileCreationTheme )
	{
		float previousRotY = Mathf.Floor( previousTileRot.eulerAngles.y );
		//Debug.Log("GenerateLevel-getSegmentTile: previous angle: " + previousRotY );

		//Straight section
		if( Random.value < 0.6f )
		{
			float rdLog = Random.value;
			if( rdLog < 0.1f )
			{
				//Add a straight slope
				return TileType.Straight_Slope;
			}
			else if( rdLog < 0.25f )
			{
				//Add a straight tile with a log obstacle in the middle
				return TileType.Straight_Log;
			}
			else if( rdLog  < 0.45f )
			{
				//Add a river straight tile, but only if the preceding was a straight tile
				//or else you can see half of the waterfall rock through the trees and it does not look nice.
				//Also, we do not want two back-to-back rivers.
				if ( previousTileType == TileType.Straight || previousTileType == TileType.Straight_Log )
				{
					//We have two types of rivers: stone crossing and broken bridge.
					if( Random.value <= 0.5f )
					{
						return TileType.Straight_River;
					}
					else
					{
						if( tileCreationTheme == SegmentTheme.Forest || tileCreationTheme == SegmentTheme.Hell )
						{
							if( Random.value <= 0.2f )
							{
								return TileType.Straight_River_Crossing;
							}
							else
							{
								return TileType.Straight_River_Log_Crossing;
							}
						}
						else
						{
							return TileType.Straight_River_Crossing;
						}
					}
				}
				else
				{
					//Add a normal straight tile
					return TileType.Straight;
				}
			}
			else if( rdLog < 0.7f )
			{
				//Add a normal straight tile
				return TileType.Straight;
			}
			else if( rdLog < 2 ) //normal value is 0.9f
			{
				//Add a double length straight
				if ( previousTileType == TileType.Straight_Double )
				{
					//Add a normal straight tile. We don't want two of them back to back
					return TileType.Straight;
				}
				else
				{
					if( tileCreationTheme == SegmentTheme.Forest )
					{
						//Add a double length straight tile
						return TileType.Straight_Double;
					}
					else if( tileCreationTheme == SegmentTheme.Cemetery )
					{
						if( Random.value < 0.5f )
						{
							//Add a double length straight tile
							return TileType.Straight_Double;
						}
						else
						{
							//Add a double length evil tree tile
							return TileType.Landmark_Evil_Tree;
						}
					}
					else
					{
						//Add a double length straight tile
						return TileType.Straight_Double;
					}
				}
			}
			else
			{
				//Add a T-junction
				addRandomTJunction2(TileType.T_Junction, tileCreationTheme);
				return TileType.None;
			}
		}
		else
		{
			//Turn section
			if( previousTileType == TileType.Straight )
			{
				if( previousRotY == 0 )
				{
					if( Random.value <= 0.5f )
					{
						return TileType.Left;
					}
					else
					{
						return TileType.Right;					
					}
				}
				else if( previousRotY == 270f || previousRotY == -90f )
				{
					return TileType.Right;
				}
				else
				{
					return TileType.Left;
				}
				
			}
			else if( previousTileType == TileType.Left )
			{
				if( previousRotY == 0 )
				{
					return TileType.Right;					
				}
				else if( previousRotY == 270f || previousRotY == -90f )
				{
					//Debug.LogWarning("GenerateLevel-getSegmentTile: invalid angle: " + previousRotY + " previous tile was " + previousTileType );
				}
				else
				{
					if(  Random.value <= 0.5f )
					{
						return TileType.Left;
					}
					else
					{
						return TileType.Right;					
					}
					
				}
			}
			else if( previousTileType == TileType.Right )
			{
				if( previousRotY == 0 )
				{
					return TileType.Left;					
				}
				else if( previousRotY == 270f || previousRotY == -90f )
				{
					if( Random.value <= 0.5f )
					{
						return TileType.Left;
					}
					else
					{
						return TileType.Right;					
					}
				}
				else
				{
					//Debug.LogWarning("GenerateLevel-getSegmentTile: invalid angle: " + previousRotY + " previous tile was " + previousTileType );
					
				}
			}
		}

		return TileType.Straight;					
	}

	private void addRandomTJunction( TileType tileType )
	{
		//We want the T-Junction tile to have a 0 degree rotation.
		ensureTileHasZeroRotation();
		//Adding T-Junction
		GameObject  tJunction = addTile ( tileType );
		if( tJunction.transform.eulerAngles.y != 0 )
		{
			Debug.LogError("addRandomTJunction: ERROR added non zero rotation T-Junction.");
		}
		//Adding 4 tiles to the left of the T-Junction
		//If the player chooses to go Left, we will put all of the tiles that belong to the Right path in an empty game object called T-Junction
		//and move that group so it aligns with the last left tile we added.
		previousTileType = TileType.Left;
		addTile ( TileType.Straight );
		addTile ( TileType.Right );
		addTile ( TileType.Straight );
		addTile ( TileType.Right );
		//Reset values so that the Right path gets constructed normally
		SegmentInfo si = getSegmentInfo( tJunction );
		float tJunctionTileEndHeight = si.tileEndHeight; //For some T-Junction (notably the one in Fairyland), the end of the tile is lower than the begining of the tile
		previousTilePos = new Vector3( tJunction.transform.position.x, tJunction.transform.position.y + tJunctionTileEndHeight, tJunction.transform.position.z );
		previousTileRot = tJunction.transform.rotation;
		previousTileType = TileType.Right;
		//Also, add two tiles to the right to avoid the possibility
		//of a second random next T-Junction overlaping this one.
		addTile ( TileType.Left );
		addTile ( TileType.Straight );

	}

	void createTutorialLevel()
	{
		setCurrentTheme( SegmentTheme.Island );
		
		//Step 0 - Instantiate START tile
		addCustomTile( "Opening",2,TileType.Straight );
		
		//Step 1 - change lane by swiping tutorial
		tutorialStartTileIndex.Add (TutorialEvent.CHANGE_LANES, tileCreationIndex);
		addCustomTile( "Opening2",2,TileType.Straight );
		addCustomTile( "Opening3",2,TileType.Straight );
		addCustomTile( "Opening3",2,TileType.Straight );
		addCustomTile( "Opening4",2,TileType.Straight );
		addCustomTile( "Opening5",1,TileType.Right );
		//previousTilePos = new Vector3( 0, 32.3f, 473.2f );
		//addCustomTile( "Straight",1,TileType.Straight );
		/*
		//Step 2 - turn corners tutorial
		tutorialStartTileIndex.Add (TutorialEvent.TURN_CORNERS, tileCreationIndex);
		addCustomTile( "Turn corners Announce",1,TileType.Straight );
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Turn corners right Windmill",1,TileType.Right );
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Turn corners left",1,TileType.Left );
		
		//Step 3 - jump tutorial
		tutorialStartTileIndex.Add (TutorialEvent.JUMP, tileCreationIndex);
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Jump Announce",1,TileType.Straight );
		addCustomTile( "Jump over cows",2,TileType.Straight );
		addCustomTile( "Jump over river",1,TileType.Straight );
		
		//Step 4 - slide tutorial
		tutorialStartTileIndex.Add (TutorialEvent.SLIDE, tileCreationIndex);
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Slide Announce",1,TileType.Straight );
		addCustomTile( "Slide under clocktower gate",2,TileType.Straight );
		
		//Step 5 - slide breakable tutorial
		tutorialStartTileIndex.Add (TutorialEvent.SLIDE_BREAKABLE, tileCreationIndex);
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Breakable Announce",1,TileType.Straight );
		addCustomTile( "Breakable break a barrel",1,TileType.Straight );
		
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Right",1,TileType.Right );
		
		//Step 6 - activate powerup with double tap
		//Note that you cannot die while trying to activate a power-up, hence it is not added to the list
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Powerup Announce",1,TileType.Straight );
		addCustomTile( "Powerup capture first star",1,TileType.Straight );
		addCustomTile( "Powerup capture second star",1,TileType.Straight );
		
		//Step 7 - tilt to change lanes and pick-up coins
		tutorialStartTileIndex.Add (TutorialEvent.TILT_CHANGE_LANES, tileCreationIndex);
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Tilt Announce",1,TileType.Straight );
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Tilt left",1,TileType.Straight );
		addCustomTile( "Tilt right",1,TileType.Straight );
		
		//Last step - arrive at castle
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "End",2,TileType.Right ); */
	}


	void createTutorialLevelOld()
	{
		setCurrentTheme( SegmentTheme.Tutorial );

		//Step 0 - Instantiate START tile
		addCustomTile( "Start",2,TileType.Straight );

		//Step 1 - change lane by swiping tutorial
		tutorialStartTileIndex.Add (TutorialEvent.CHANGE_LANES, tileCreationIndex);
		addCustomTile( "Change lane Announce",1,TileType.Straight );
		addCustomTile( "Change lane Go left",2,TileType.Straight );
		addCustomTile( "Change lane Go right",2,TileType.Straight );

		//Step 2 - turn corners tutorial
		tutorialStartTileIndex.Add (TutorialEvent.TURN_CORNERS, tileCreationIndex);
		addCustomTile( "Turn corners Announce",1,TileType.Straight );
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Turn corners right Windmill",1,TileType.Right );
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Turn corners left",1,TileType.Left );

		//Step 3 - jump tutorial
		tutorialStartTileIndex.Add (TutorialEvent.JUMP, tileCreationIndex);
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Jump Announce",1,TileType.Straight );
		addCustomTile( "Jump over cows",2,TileType.Straight );
		addCustomTile( "Jump over river",1,TileType.Straight );
	
		//Step 4 - slide tutorial
		tutorialStartTileIndex.Add (TutorialEvent.SLIDE, tileCreationIndex);
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Slide Announce",1,TileType.Straight );
		addCustomTile( "Slide under clocktower gate",2,TileType.Straight );

		//Step 5 - slide breakable tutorial
		tutorialStartTileIndex.Add (TutorialEvent.SLIDE_BREAKABLE, tileCreationIndex);
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Breakable Announce",1,TileType.Straight );
		addCustomTile( "Breakable break a barrel",1,TileType.Straight );

		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Right",1,TileType.Right );

		//Step 6 - activate powerup with double tap
		//Note that you cannot die while trying to activate a power-up, hence it is not added to the list
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Powerup Announce",1,TileType.Straight );
		addCustomTile( "Powerup capture first star",1,TileType.Straight );
		addCustomTile( "Powerup capture second star",1,TileType.Straight );

		//Step 7 - tilt to change lanes and pick-up coins
		tutorialStartTileIndex.Add (TutorialEvent.TILT_CHANGE_LANES, tileCreationIndex);
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Tilt Announce",1,TileType.Straight );
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "Tilt left",1,TileType.Straight );
		addCustomTile( "Tilt right",1,TileType.Straight );

		//Last step - arrive at castle
		addCustomTile( "Straight",1,TileType.Straight );
		addCustomTile( "End",2,TileType.Right );
	}

	public GameObject getFirstTileOfActiveTutorial()
	{
		//Get tile index
		int tutorialTileIndex;
		if( tutorialStartTileIndex.ContainsKey(TutorialManager.activeTutorial) )
		{
			tutorialTileIndex = tutorialStartTileIndex[TutorialManager.activeTutorial];
			print ("getFirstTileOfActiveTutorial " + tutorialTileIndex );
			GameObject tutorialTile = worldRoadSegments[tutorialTileIndex];
			print ("getFirstTileOfActiveTutorial " + tutorialTile.name );

			//Center the surrounding plane around the current tile
			if( surroundingPlane != null )
			{
				surroundingPlane.position = new Vector3( tutorialTile.transform.position.x, surroundingPlane.position.y, tutorialTile.transform.position.z );
			}
			//Make sure the playerTileIndex points to this tile
			playerTileIndex = tutorialTileIndex;
			//Make sure the next tiles are active
			activateInitialTiles(playerTileIndex);

			//Reset all the tiles starting from that point so that if a barrel was broken for example, it would be replaced
			for( int i = tutorialTileIndex; i < worldRoadSegments.Count; i++ )
			{
				TileReset tr = worldRoadSegments[i].GetComponent<TileReset>();
				if( tr != null ) tr.resetTile();				
			}

			//Deactivate all the tile after the new current tile + nbrVisibleTiles
			for( int i = tutorialTileIndex + nbrVisibleTiles + 1; i < worldRoadSegments.Count; i++ )
			{
				worldRoadSegments[i].SetActive(false);
				getSegmentInfo(worldRoadSegments[i]).entranceCrossed = false;

			}
			return tutorialTile;
		}
		else
		{
			Debug.LogError("GenerateLevel: getFirstTileOfActiveTutorial error: could not find entry for " + TutorialManager.activeTutorial );
			return null;
		}
	}
	
	private SegmentInfo getSegmentInfo( GameObject tile )
	{
		SegmentInfo si = tile.GetComponent<SegmentInfo>();
		if( si != null )
		{
			return si;
		}
		else
		{
			Debug.LogError("GenerateLevel-getSegmentInfo: tile named " + tile.name + " does not have a SegmentInfo component attached to it.");
			return null;
		}
	}

	GameObject getRecycledTile( TileType type )
	{
		if (recycledTiles.Count == 0 ) return null;

		foreach( GameObject tile in recycledTiles )
		{
			if( getTileType( tile ) == type )
			{
				recycledTiles.Remove(tile);
				return tile;
			}
		}
		return null;
	}
	
	public void tileEntranceCrossed( Transform currentTile )
	{
		playerTileIndex++;

		//print ("tileEntranceCrossed: player entered " + currentTile.name + " and the player tile index is: " + playerTileIndex );

		//If in endless runner mode, each time we enter a new tile, add a recycled tile at the end
		if( GameManager.Instance.getGameMode() == GameMode.Endless  )
		{
			//Recycle the tile two behind the player if there is one
			int indexOfTileToRecycle = playerTileIndex - 2;
			if( indexOfTileToRecycle >= 0 )
			{
				GameObject tileToRecycle = worldRoadSegments[indexOfTileToRecycle];
				//Do not recycle Start or End tiles
				TileType tileType = getSegmentInfo( tileToRecycle ).tileType;
				if( tileType != TileType.Start && tileType != TileType.End )
				{
					TileReset tr = tileToRecycle.GetComponent<TileReset>();
					if( tr != null )
					{
						tr.resetTile();
					}
					else
					{
						Debug.LogWarning("tileEntranceCrossed: Tile named " + tileToRecycle.name + " does not have a Reset Tile component.");
					}
					recycledTiles.Add( tileToRecycle );
				}
				//Add a tile at the end
				addTileInEndlessMode();
			}
		}
		else
		{
			//We are not in endless mode.
			//Add a tile from the next level if any
			if( levelTileList.Count > 0 )
			{
				TileData td = levelTileList.Dequeue();
				setCurrentTheme( td.tileTheme );
				//Debug.LogWarning("tileEntranceCrossed: Adding next level tile of type: " + td.tileType + " theme: " + td.tileTheme );
				if( td.tileType == TileType.T_Junction || td.tileType == TileType.T_Junction_Landmark_Cemetery || td.tileType == TileType.T_Junction_Landmark_Cemetery_Queen )
				{
					//Because T-Junction construction changes the previous position and rotation values, we cannot do it during the prepareTileList phase.
					addRandomTJunction( td.tileType );
				}
				else
				{
					addTile( td.tileType );
				}
			}
		}

		//Center the surrounding plane around the current tile
		if( surroundingPlane != null )
		{
			surroundingPlane.position = new Vector3( currentTile.position.x, surroundingPlane.position.y, currentTile.position.z );
		}

		updateActiveTiles();
			
	}

	public void playerTurnedAtTJunction( bool isGoingRight, GameObject tile )
	{
		if( !isGoingRight )
		{
			//Player took the Left path.
			//Player did not take the main path (which is always to the right).
			//Move all the tiles from the right path to the tip of the left path of the T-Junction.
			//We are adding 5 to the index because:
			//The T-Junction tile itself plus the 4 tiles that are created on the left make 5.
			int startValue = playerTileIndex + 5;
			for ( int j=startValue; j < worldRoadSegments.Count; j++ )
		    {
				//Move the tiles on the right path to the left path. 
				worldRoadSegments[j].transform.position = new Vector3( worldRoadSegments[j].transform.position.x - (2 * TILE_SIZE), worldRoadSegments[j].transform.position.y, worldRoadSegments[j].transform.position.z + (2 * TILE_SIZE) );
			}
			//Also change the previousTilePos so that recycled tiles get added at the right place
			previousTilePos = new Vector3( previousTilePos.x - (2 * TILE_SIZE), previousTilePos.y, previousTilePos.z + (2 * TILE_SIZE) );

		}
		else
		{
			//Player took the Right path.
			//We need to update which tiles are active.
			//Remember, the first tile to the right of the T-Junction tile has a higher index (not just +1)
			//because we inserted tiles to the left of the T-Junction tile when it got created.

			//Deactivate 4 left tiles
			if( playerTileIndex + 1 < worldRoadSegments.Count ) worldRoadSegments[playerTileIndex + 1].SetActive(false);
			if( playerTileIndex + 2 < worldRoadSegments.Count ) worldRoadSegments[playerTileIndex + 2].SetActive(false);
			if( playerTileIndex + 3 < worldRoadSegments.Count ) worldRoadSegments[playerTileIndex + 3].SetActive(false);
			if( playerTileIndex + 4 < worldRoadSegments.Count ) worldRoadSegments[playerTileIndex + 4].SetActive(false);


			//Activate nbrVisiblesTiles to the right
			if( playerTileIndex + 5 < worldRoadSegments.Count ) worldRoadSegments[playerTileIndex + 5].SetActive(true);
			if( playerTileIndex + 6 < worldRoadSegments.Count ) worldRoadSegments[playerTileIndex + 6].SetActive(true);
			if( playerTileIndex + 7 < worldRoadSegments.Count ) worldRoadSegments[playerTileIndex + 7].SetActive(true);

			//Adjust player tile index
			playerTileIndex = playerTileIndex + 4;

		}
	}
	
	private void activateInitialTiles( int startIndex )
	{
		//Activate the current plus nbrVisibleTiles prefabs that are next on the player's path
		int endIndex = startIndex + nbrVisibleTiles;
		if( endIndex >= worldRoadSegments.Count ) endIndex = worldRoadSegments.Count - 1;
		for( int i=startIndex; i <= endIndex; i++ )
		{
			getSegmentInfo(worldRoadSegments[i]).entranceCrossed = false;
			worldRoadSegments[i].SetActive( true );
			onTileActivation(i);
		}
	}

	
	//At any given time, there are 5 active tiles:
	//The current tile
	//The preceding tile
	//The nbrVisibleTiles tiles that come after the current tile
	private void updateActiveTiles()
	{
		//Disable tile two behind the player
		int index = playerTileIndex - 2;
		if( index >= 0 ) worldRoadSegments[index].SetActive(false);
			
		//Enable next tile nbrVisibleTiles in front of player
		index = playerTileIndex + nbrVisibleTiles;
		if( index >= worldRoadSegments.Count ) index = worldRoadSegments.Count - 1; //needed in case the player turns right at a T-Junction and the Checkpoint tile is right after the T-Junction
		if( index < worldRoadSegments.Count )
		{
			worldRoadSegments[index].SetActive(true);
			onTileActivation(index);
		}
		else
		{
			Debug.LogWarning("prout index." + index + " " + worldRoadSegments.Count );
		}

		//Activate zombies (if any) of next tile
		index = playerTileIndex + 1;
		if( index < worldRoadSegments.Count )
		{
			//If the tile with this index has a Zombie Trigger
			//also activate the next zombie wave.
			ZombieTrigger zombieTrigger = worldRoadSegments[index].GetComponent<ZombieTrigger>();
			if( zombieTrigger != null )
			{
				zombieTrigger.activateNextWave();
			}
		}
	}

	private void onTileActivation( int index )
	{
		//If the tile we just activated has a T-Junction or Landmark_Cemetery type
		//also enable the first tile on it's right side which has an index of + 5 compared to the T-Junction tile itself.
		SegmentInfo si = getSegmentInfo(worldRoadSegments[index]);
		if( si.tileType == TileType.T_Junction || si.tileType == TileType.T_Junction_Landmark_Cemetery )
		{
			int firstTileToTheRight = index + 5;
			if( firstTileToTheRight < worldRoadSegments.Count ) worldRoadSegments[firstTileToTheRight].SetActive(true);
		}
		if( GameManager.Instance.getGameMode() == GameMode.Story )
		{
			if( si.isCheckpoint )
			{
				seamlessLevelIndex++;
				if( seamlessLevelIndex < LevelManager.Instance.getNumberOfLevels() )
				{
					prepareTileList( seamlessLevelIndex );
				}
				else
				{
					Debug.LogWarning("Player is reaching end of the game.");
				}
			}
		}
	}

	TileType getTileType( GameObject tile )
	{
		return getSegmentInfo(tile).tileType;
	}

	public void setNumberVisibleTiles ( int nbrTiles )
	{
		nbrVisibleTiles = nbrTiles;
	}
	
	public void resetNumberVisibleTiles ()
	{
		nbrVisibleTiles = DEFAULT_NBR_VISIBLE_TILES;
	}

}
