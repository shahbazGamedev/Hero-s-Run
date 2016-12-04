using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SegmentTheme {
		Forest = 0,
		Fairyland = 1,
		Cemetery = 2,
		Dark_Tower = 6,
		Jungle = 7,
		Blizzard = 10,
		Caves = 11,
		Hell = 12
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
	Forest_Statue = 9,
	Landmark_Clocktower = 13,
	T_Junction_2 = 14,
	T_Junction = 15,
	Straight_River_Crossing = 16,
	Straight_Slope = 17,
	Straight_Double = 18,
	Straight_Bezier = 19,
	Straight_River_Log_Crossing = 20,
	Checkpoint = 25,
	Opening = 26,
	Landmark_Evil_Tree = 27,
	Opening2 = 28,
	Opening3 = 29,
	Opening4 = 30,
	Opening5 = 31,
	Landmark_Fairy_Message = 32,
	Landmark_Broken_Bridge = 33,
	Landmark_Cemetery_Queen = 35,
	Landmark_Magic_Bridge = 36,
	Landmark_Tomb_Start = 37,
	Landmark_Tomb_Double = 38,
	Landmark_Tomb_Start_2 = 40,
	Landmark_Tomb_End = 41,
	Landmark_Cemetery_Coach = 42,
	Landmark_Graveyard = 43,
	Landmark_Graveyard_Start = 44,
	Landmark_Graveyard_End = 45,
	Landmark_Graveyard_Ghost = 46,
	Landmark_Graveyard_1 = 47,
	Landmark_Graveyard_2 = 48,
	Landmark_Graveyard_3 = 49,
	Landmark_Graveyard_4 = 50,
	Landmark_Graveyard_5 = 51,
	Landmark_Graveyard_6 = 52,
	Landmark_Graveyard_7 = 53,
	Landmark_Bog_Valley = 55,
	Landmark_Treasure_Key = 57,
	Landmark_Collapsing_Bridge = 58,
	Landmark_Goblin_Loot = 59,
	Landmark_Zipline = 61,
	Fairyland_Mushroom_Jump = 62,
	Fairyland_Goblin_Jump = 63,
	Fairyland_Pushed_Barrels = 64,
	Fairyland_Roasting_Pig = 65,
	Jungle_Pyramid = 66,
	Jungle_Valley_Start = 67,
	Jungle_Valley_1 = 68,
	Jungle_Valley_2 = 69,
	Jungle_Valley_End = 72,
	Jungle_Vortex = 73,
	Jungle_Enemy = 74,
	Mines_Torch = 75,
	Mines_Giant_Crystal = 76,
	Blizzard_Snow_Balls = 77,
	Blizzard_Avalanche = 78,
	Blizzard_Goblin_Jump = 79,
	Blizzard_Goblins_Valley = 80,
	Hell_Cerberus = 81,
	Hell_Cavern_Gibbets = 82,
	Hell_Bridge = 83,
	Hell_Banquet_Hall = 84,
	Hell_Drawbridge = 85,
	Hell_Pendulums = 86,
	Hell_Flame_Columns = 87,
	Hell_Demons = 88,
	Hell_Floor_Traps = 89,
	Blizzard_Before_End = 90,
	Blizzard_Refuge = 91,
	Blizzard_Dark_Queen_Calls_Storm = 92,
	Mines_Giant_Cave = 93,
	Forest_Burning_Bridge = 94,
	Forest_Magic_Mirror = 95,
	Cemetery_Endless_Graveyard_Start = 96

}

public enum TileSubType {
	None = -1,
	Straight = 3,
	Left = 4,
	Right = 5,
	T_Junction = 15,
}


public sealed class GenerateLevel  : MonoBehaviour {
	
	private LevelData levelData;
	
	public const float TILE_SIZE = 36.4f;
	const float UNDERNEATH_TILE_BY = 30f;
	int tileDepthMult = 1; //A value of one means the tile depth is 1 x TILE_SIZE, a value of two means 2 x TILE_SIZE, etc.
	
	//worldRoadSegments is a List of game object tiles
	List<GameObject> worldRoadSegments = new List<GameObject>(300);
	public int playerTileIndex = 0;		//Index of the active tile (the one the player is on).

	//tileCreationIndex is incremented each time a new tile is added.
	//It is also used to control the power up density as we add a power up every 'X' tiles.
	//Lastly it is appended at the name of every tile created to facilitate debugging.
	int tileCreationIndex = 0;

	Vector3 previousTilePos = new Vector3( 0,0,0 );
	Quaternion previousTileRot = Quaternion.identity;
	TileSubType previousTileType = TileSubType.None;

	//This number is used to make sure that the subsequent tiles are at the proper height.
	float  tileEndHeight = 0;

	//The number of visible tiles at any given time (all other tiles are deactivated).
	int nbrVisibleTiles = 3;
	
	//For adding power-ups in tiles
	public PowerUpManager powerUpManager;
	public PlayerController playerController;
	public TileGroupManager tileGroupManager;
	
	//For configuring the dynamic fog using the level data
	public Camera mainCamera;

	SegmentTheme currentTheme;
	
	//The surrounding plane (like an ocean) is always centered with the current tile
	public Transform surroundingPlane;
	
	//To improve performance by preloading prefabs and avoiding reloading tile prefabs that have previously been loaded.
	const int NUMBER_OF_THEMES = 8; //Dark Tower, Forest, Fairyland, Cemetery, etc.
	Dictionary<SegmentTheme,Dictionary<TileType,GameObject>> tilePrefabsPerTheme  = new Dictionary<SegmentTheme,Dictionary<TileType,GameObject>>(NUMBER_OF_THEMES);

	//For the endless running game mode
	Queue<TileType> endlessTileList = new Queue<TileType>();
	TileGroupType previousRandomTileGroupType = TileGroupType.None;

	//For the Episode Progress Indicator - to know where to display the checkpoint indicators
	List<int> indexOfCheckpointTiles = new List<int>();
	
	void Awake ()
	{
		//The activity indicator may have been started
		Handheld.StopActivityIndicator();

		levelData = LevelManager.Instance.getLevelData();

		createLevel ();
	}

	void Start()
	{
		//If the number of checkpoints passed is greater than 0, it means the player will restart at the center of a Checkpoint tile and not on the Start tile.
		//If the episode requires a tapToPlay event (usually coming from a script attached to the Start tile), it will never come.
		//Therefore, we need to send the event instead.
		LevelData.EpisodeInfo currentEpisode = LevelManager.Instance.getCurrentEpisodeInfo();
		if( currentEpisode.waitForTapToPlay && LevelManager.Instance.getNumberOfCheckpointsPassed() > 0 )
		{
			GameManager.Instance.setGameState( GameState.Menu );
		}
	}

	private void loadTilePrefabs( SegmentTheme theme )
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
			SegmentInfo si = getSegmentInfo( go );
			if( si.tileType != TileType.None )
			{
				//Debug.Log ("loadTilePrefabs-adding tile type: " + si.tileType );
				themePrefabsDict.Add( si.tileType, go );
			}
		}
		tilePrefabsPerTheme.Add(theme, themePrefabsDict);

	}

	private void createLevel ()
	{
		//Reset values
		worldRoadSegments.Clear();
		tileCreationIndex = 0;
		playerTileIndex = 0;
						
		LevelData.EpisodeInfo currentEpisode = LevelManager.Instance.getCurrentEpisodeInfo();

		//Sets the skybox, the directional light intensity and direction for the current episode
		levelData.initialise();
		levelData.setSunParameters(currentEpisode.sunType);

		//Verify if we should include a plane surrounding the tiles (like an ocean)
		if( currentEpisode.includeSurroundingPlane )
		{
			GameObject go = (GameObject)Instantiate(surroundingPlane.gameObject, new Vector3( 0, -UNDERNEATH_TILE_BY, 0 ), Quaternion.identity );
			surroundingPlane = go.transform;
			if( surroundingPlane.GetComponent<Renderer>().material != null )
			{
				surroundingPlane.GetComponent<Renderer>().material = currentEpisode.surroundingPlaneMaterial;
				surroundingPlane.gameObject.SetActive( true );
			}
			else
			{
				Debug.LogWarning("GenerateLevel-CreateLevel: includeSurroundingPlane is set to true but no surroundingPlaneMaterial has been specified.");
			}
		}
		else
		{
			surroundingPlane.gameObject.SetActive( false );
		}

		List<TileGroupType> tileGroupList = currentEpisode.tileGroupList;

		if( GameManager.Instance.getGameMode() == GameMode.Story )
		{
			generateStoryLevel( tileGroupList );
		}
		else
		{
			generateEndlessLevel( tileGroupList );
		}

		//The player controller needs info about the tile the player is on.
		setFirstTileInfoInPlayer();

		//Make the first few tiles active
		activateInitialTiles(0);

		//Configure fog, if any
		mainCamera.GetComponent<DynamicFogAndMist.DynamicFog>().enabled = currentEpisode.isFogEnabled;
		mainCamera.GetComponent<DynamicFogAndMist.DynamicFog>().UpdateMaterialProperties();

		Debug.Log("GenerateLevel-CreateLevel: Level " + currentEpisode.episodeName + " has been created." );
		Debug.Log("GenerateLevel-CreateLevel: The number of coins spawned is : " + CoinManager.coinManager.realNumberCoinsSpawned );

	}

	private void generateStoryLevel( List<TileGroupType> tileGroupList )
	{
		int numbersOfCheckpointsPassed = LevelManager.Instance.getNumberOfCheckpointsPassed();
		int episodeCheckpointIndex = 0;
		for( int i=0; i < tileGroupList.Count; i++ )
		{
			TileGroup tg = tileGroupManager.getTileGroup(tileGroupList[i]);
			if( isTileGroupACheckpoint( tg ) )
			{
				episodeCheckpointIndex++;
			}
			if( numbersOfCheckpointsPassed > episodeCheckpointIndex ) continue; //we want to restart at the last passed checkpoint, not from the Start tile
			if( LevelManager.Instance.getOnlyUseUniqueTiles() && tg.frequency != TileGroup.FrequencyType.Unique ) continue;
			if( tg.frequency != TileGroup.FrequencyType.Never && (tg.validGameMode == ValidGameMode.Any || tg.validGameMode == ValidGameMode.Story) )
			{
				setCurrentTheme(tg.theme );
				List<TileType> individualTiles = tg.tileList;
				for( int j=0; j < individualTiles.Count; j++ )
				{
					if( individualTiles[j] == TileType.Checkpoint )
					{
						indexOfCheckpointTiles.Add( tileCreationIndex );
					}
					addTileNew( individualTiles[j] );
				}
			}
		}
		worldRoadSegments.TrimExcess();		
	}

	private bool isTileGroupACheckpoint( TileGroup tg )
	{
		if( tg.tileGroupType == TileGroupType.Blizzard_Checkpoint || tg.tileGroupType == TileGroupType.Cemetery_Checkpoint
			|| tg.tileGroupType == TileGroupType.Fairyland_Checkpoint || tg.tileGroupType == TileGroupType.Jungle_Checkpoint
			|| tg.tileGroupType == TileGroupType.Mines_Checkpoint || tg.tileGroupType == TileGroupType.Tanglewood_Checkpoint
			|| tg.tileGroupType == TileGroupType.Hell_Checkpoint )
		{
			return true;
		}
		{
			return false;
		}
	}

	private void generateEndlessLevel( List<TileGroupType> tileGroupList )
	{
		//the first tile group needs to have the Start tile and 3 others
		TileGroup tg = tileGroupManager.getTileGroup(tileGroupList[0]);
		if( tg.validGameMode == ValidGameMode.Any || tg.validGameMode == ValidGameMode.Endless )
		{
			setCurrentTheme(tg.theme );

			List<TileType> individualTiles = tg.tileList;
			for( int j=0; j < individualTiles.Count; j++ )
			{
				addTileNew( individualTiles[j] );
			}
		}
		//Now add a first random tile group to the endless tiles Queue
		addRandomTileGroupToEndlessTilesQueue();
	}

	private void addRandomTileGroupToEndlessTilesQueue()
	{
		TileGroup rtg = tileGroupManager.getRandomTileGroup( currentTheme );
		//Try to avoid having two identical tile groups back to back if possible.
		//We will make one attempt to change it if it is identical.
		if( rtg.tileGroupType == previousRandomTileGroupType ) rtg = tileGroupManager.getRandomTileGroup( currentTheme );
		previousRandomTileGroupType = rtg.tileGroupType;
		List <TileType> tiles = rtg.tileList;
		for( int j=0; j < tiles.Count; j++ )
		{
			endlessTileList.Enqueue(tiles[j]);
		}
	}

	//The player controller needs info about the tile the player is on.
	//This is normally set each time the player crosses an Entrance trigger.
	//However:
	//they are no Entrance in a Start tile and
	//when a player starts the game at a checkpoint, the player will be positioned in the center of the tile and therefore not cross the Entrance trigger for that tile.
	//Because of that, we simply use the info from the first tile (the one with index 0) in worldRoadSegments.
	private void setFirstTileInfoInPlayer()
	{
		GameObject firstTile = worldRoadSegments[0];
		playerController.currentTile = firstTile;
		playerController.tileRotationY = firstTile.transform.eulerAngles.y;
		playerController.currentTilePos = firstTile.transform.position;
		SegmentInfo si = getSegmentInfo( firstTile );
		playerController.currentTileType = si.tileType;

		//If the player starts off on a Start tile, the camera will be looking at the front of player and do a rotation when the player starts running.
		//However, if the player is not on a Start tile and is starting at a Checkpoint, we want the camera to look at the back of the player (and therefore, there is no need for a rotation when the player starts running).
		//if( si.tileType != TileType.Start )
		if( true )
		{
			SimpleCamera sc = playerController.gameObject.GetComponent<SimpleCamera>();
			sc.playCutscene(CutsceneType.Checkpoint);
		}
	}

	private void setCurrentTheme( SegmentTheme newTheme )
	{
		currentTheme = newTheme;
	}

	//Important: as previousTileType value, use one of the three basic tile types (Straight, Left or Right). Do
	//not use the precise tile type (such as Straight_double) or else the method ensureTileHasZeroRotation won't work as intended.
	private GameObject addTile ( TileType type )
	{
		GameObject go = null;
		GameObject prefab = null;
		Quaternion tileRot = Quaternion.identity;
		Vector3 tilePos = Vector3.zero;

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

		tileRot = getTileRotation();
		tilePos = getTilePosition();
		go = (GameObject)Instantiate(prefab, tilePos, tileRot );
		go.name = type.ToString() + " " + tileCreationIndex.ToString();

		SegmentInfo si = getSegmentInfo( go );
		tileDepthMult= si.tileDepth;
		previousTileType = si.tileSubType; //for constructing the level, we use the simpler types like straight, left and right
		tileEndHeight = si.tileEndHeight;
		si.entranceCrossed = false;
		si.tile = go;
		previousTilePos = tilePos;
		previousTileRot = tileRot;
		powerUpManager.considerAddingPowerUp( go, tileCreationIndex );
		go.SetActive( false );
		worldRoadSegments.Add( go );
		si.tileIndex = tileCreationIndex;
		tileCreationIndex++;

		return go;
	}

	private Vector3 getTilePosition()
	{
		Vector3 tilePos = Vector3.zero;
		float previousTileRotY = Mathf.Floor( previousTileRot.eulerAngles.y );
		float tileDepth =  tileDepthMult * TILE_SIZE;
		//Determine the tile's height.
		float tileHeight = tileEndHeight + previousTilePos.y;
		switch (previousTileType)
		{
			case TileSubType.Straight:
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
	
	        case TileSubType.Left:
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
		
	        case TileSubType.T_Junction:
			tilePos.Set ( previousTilePos.x + tileDepth, tileHeight, previousTilePos.z );
			return tilePos;

			case TileSubType.Right:
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

	private Quaternion getTileRotation()
	{
		float previousTileRotY = Mathf.Floor( previousTileRot.eulerAngles.y );
		Quaternion tileRot = Quaternion.identity;
		switch (previousTileType)
		{
			case TileSubType.None:
			return Quaternion.identity;
		
			case TileSubType.Straight:
			tileRot =  Quaternion.Euler( 0, previousTileRot.eulerAngles.y, 0 );
			return tileRot;

			case TileSubType.Left:
			if( previousTileRotY == 0 )
			{
				tileRot =  Quaternion.Euler(0, -90f, 0);
			}
			else if( previousTileRotY == 270f || previousTileRotY == -90f )
			{
				tileRot =  Quaternion.Euler( 0, 0, 0 );
			}
			else
			{
				tileRot =  Quaternion.Euler( 0, 0, 0 );
			}
			return tileRot;
		
			case TileSubType.Right:
			if( previousTileRotY == 0 )
			{
				tileRot =  Quaternion.Euler( 0, 90f, 0 );
			}
			else if( previousTileRotY == 270f || previousTileRotY == -90f )
			{
				tileRot =  Quaternion.Euler( 0, 0, 0 );
			}
			else
			{
				tileRot =  Quaternion.Euler( 0, 0, 0 );
			}
			return tileRot;
			
	        default:
			Debug.LogError ("getTileRotation : unhandled tile type: " + previousTileType );
			tileRot.eulerAngles = Vector3.zero;
			return tileRot;
		}
	}
	
	private void addTileNew ( TileType tileType )
	{
        switch (tileType)
		{
			case TileType.T_Junction:
			case TileType.T_Junction_2:
				addRandomTJunction(tileType);
				break;
			default:
				addTile( tileType );
				break;
		}
	}

	//Add tiles such as to garanty that the tile added after these will have a zero Y rotation.
	private void ensureTileHasZeroRotation()
	{
		float yRot = Mathf.Floor( previousTileRot.eulerAngles.y );

		switch (previousTileType)
		{
			case TileSubType.Left:
			if( yRot == 0 )
			{
				addTile ( TileType.Right );
			}
			return;

			case TileSubType.Right:
			if( yRot == 0 )
			{
				addTile ( TileType.Left );
			}
			return;

			case TileSubType.Straight:
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
		previousTileType = TileSubType.Left;
		addTile ( TileType.Straight );
		addTile ( TileType.Right );
		addTile ( TileType.Straight );
		addTile ( TileType.Right );
		//Reset values so that the Right path gets constructed normally
		SegmentInfo si = getSegmentInfo( tJunction );
		float tJunctionTileEndHeight = si.tileEndHeight; //For some T-Junction (notably the one in Fairyland), the end of the tile is lower than the begining of the tile
		previousTilePos = new Vector3( tJunction.transform.position.x, tJunction.transform.position.y + tJunctionTileEndHeight, tJunction.transform.position.z );
		previousTileRot = tJunction.transform.rotation;
		previousTileType = TileSubType.Right;
		//Also, add two tiles to the right to avoid the possibility
		//of a second random next T-Junction overlaping this one.
		addTile ( TileType.Left );
		addTile ( TileType.Straight );

	}

	public void enableSurroundingPlane( bool enable )
	{
		if( surroundingPlane != null ) surroundingPlane.gameObject.SetActive( enable );
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

	void addTileInEndlessMode()
	{
		if( endlessTileList.Count > 0 )
		{
			//Yes, we still have tiles in the queue
			addTileNew( endlessTileList.Dequeue() );
		}
		else
		{
			//We have run out of tiles. Add a random tile group.
			addRandomTileGroupToEndlessTilesQueue();
			//Now that we have added additional tiles, we can get one
			addTileNew( endlessTileList.Dequeue() );
		}
	}

	public void tileEntranceCrossed( Transform currentTile )
	{
		playerTileIndex++;

		//print ("tileEntranceCrossed: player entered " + currentTile.name + " and the player tile index is: " + playerTileIndex );

		//If in endless runner mode, each time we enter a new tile, add a new tile at the end
		if( GameManager.Instance.getGameMode() == GameMode.Endless  )
		{
			//Add a tile at the end
			addTileInEndlessMode();
		}

		//Center the surrounding plane around the current tile
		if( surroundingPlane != null )
		{
			surroundingPlane.position = new Vector3( currentTile.position.x, currentTile.position.y -UNDERNEATH_TILE_BY, currentTile.position.z );
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
			//Also change the previousTilePos so that new tiles get added at the right place
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
			for( int i = ( playerTileIndex + 5 ); i < ( playerTileIndex + 5 + nbrVisibleTiles ); i++ )
			{
				if( i < worldRoadSegments.Count ) worldRoadSegments[i].SetActive(true);
			}

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
		//If the tile we just activated is a T-Junction
		//also enable the first two tiles on its right side which have an index of + 5 and +6 respectively compared to the T-Junction tile itself.
		SegmentInfo si = getSegmentInfo(worldRoadSegments[index]);
		if( si.tileSubType == TileSubType.T_Junction )
		{
			int firstTileToTheRight = index + 5;
			int secondTileToTheRight = index + 6;
			if( firstTileToTheRight < worldRoadSegments.Count ) worldRoadSegments[firstTileToTheRight].SetActive(true);
			if( secondTileToTheRight < worldRoadSegments.Count ) worldRoadSegments[secondTileToTheRight].SetActive(true);
		}
	}

	TileType getTileType( GameObject tile )
	{
		return getSegmentInfo(tile).tileType;
	}

	public float getEpisodeProgress()
	{
		return (float)playerTileIndex/(tileCreationIndex - 1);
	}

	public List<int> getIndexOfCheckpointTiles()
	{
		return indexOfCheckpointTiles;
	}

	public int getNumberOfTiles()
	{
		return tileCreationIndex - 1;
	}

}
