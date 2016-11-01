using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum SegmentTheme {
		Forest = 0,
		Fairyland = 1,
		Cemetery = 2,
		Hell_Arrival = 3,
		Tutorial = 4,
		Volcano = 5,
		Dark_Tower = 6,
		Jungle = 7,
		Hell_Caverns = 8,
		Hell_Fortress = 9,
		Blizzard = 10,
		Caves = 11

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
	Landmark_Cemetery_Queen = 35,
	Landmark_Magic_Bridge = 36,
	Landmark_Tomb_Start = 37,
	Landmark_Tomb_Double = 38,
	Start_Fairyland = 39,
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
	Jungle_Start = 60,
	Landmark_Zipline = 61

}

public enum TileSubType {
	None = -1,
	Straight = 3,
	Left = 4,
	Right = 5,
	T_Junction = 15,
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
	TileSubType previousTileType = TileSubType.None;

	//Generate level supports tiles that have slopes.
	//For a slope tile to work, the slope must use the X axis, not Z.
	//tileEndHeight is the delta between the height at the end of the tile and the height at the center of the tile.
	//So if a tile has a slope of 10%, the center height is 3.209 meters and the tileEndHeight is also 3.209 meters for a total elevation of 6.418 meters.
	//This number is used to make sure that the subsequent tiles are at the proper height.
	float  tileEndHeight = 0;

	//The number of visible tiles at any given time (all other tiles are deactivated).
	int nbrVisibleTiles = 3;
	
	//For randomizing power ups
	PowerUpManager powerUpManager;
	
	//Path to Resources folder containing the tiles for the theme
	string currentThemePath = "";
	SegmentTheme currentTheme;
	
	//The surrounding plane (like an ocean) is always centered with the current tile
	public Transform surroundingPlane;
	
	//To improve performance by preloading prefabs and avoiding reloading tile prefabs that have previously been loaded.
	const int NUMBER_OF_THEMES = 12; //Dark Tower, Forest, Fairyland, Cemetery, etc.
	Dictionary<SegmentTheme,Dictionary<TileType,GameObject>> tilePrefabsPerTheme  = new Dictionary<SegmentTheme,Dictionary<TileType,GameObject>>(NUMBER_OF_THEMES);

	//For the endless running game mode
	List<GameObject> recycledTiles = new List<GameObject>(50);

	//For seamless progression
	//Queue<TileData> levelTileList = new Queue<TileData>();
	int seamlessLevelIndex = 0;

	//NEW FOR TILE GROUP
	public TileGroupManager tileGroupManager;
	Queue<TileType> endlessTileList = new Queue<TileType>();
	public bool useOldSystem = true;

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
		if( useOldSystem ) 
		{
			//CreateLevel ();
		}
		else
		{
			NewCreateLevel ();
		}
	}
	
	/*void CreateLevel ()
	{
		//Reset values
		worldRoadSegments.Clear();
		recycledTiles.Clear();
		//levelTileList.Clear();
		tileCreationIndex = 0;
		playerTileIndex = 0;
		seamlessLevelIndex = 0;
						
		//LevelInfo has the parameters for a single level.
		//Get the level info for the current level.
		int levelToLoad = LevelManager.Instance.getNextLevelToComplete();
		LevelManager.Instance.setLevelNumberOfLastCheckpoint( levelToLoad );

		seamlessLevelIndex = levelToLoad;

		LevelData.LevelInfo levelInfo = levelData.getLevelInfo( levelToLoad );
		//Also set it in the LevelManager
		LevelManager.Instance.setLevelInfo( levelInfo );

		//Sets the skybox, the directional light intensity and direction for the current level
		levelData.initialise();
		levelData.setSunParameters(levelInfo.sunType);

		//Verify if we should include a plane surrounding the tiles (like an ocean)
		if( levelInfo.includeSurroundingPlane )
		{
			GameObject go = (GameObject)Instantiate(surroundingPlane.gameObject, new Vector3( 0, -30f, 0 ), Quaternion.identity );
			surroundingPlane = go.transform;
			if( surroundingPlane.GetComponent<Renderer>().material != null )
			{
				surroundingPlane.GetComponent<Renderer>().material = levelInfo.surroundingPlaneMaterial;
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

		nbrVisibleTiles = levelInfo.nbrVisibleTiles;

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
			//addRoadSegment( roadSegmentList[i] );
		}

		//The player controller needs info about the tile the player is on.
		setFirstTileInfoInPlayer();

		//Make the first few tiles active
		activateInitialTiles(0);

		//Fade-in the level ambience soundtrack
		StartCoroutine( SoundManager.soundManager.fadeInAmbience( levelInfo.AmbienceSound, 10f ) );

		//Set the music track to play if a value is set
		SoundManager.soundManager.setMusicTrack( levelInfo.MusicTrack );

		Debug.Log("GenerateLevel-CreateLevel: Level " + levelInfo.LevelName + " has been created." );
		Debug.Log("GenerateLevel-CreateLevel: The number of coins spawned is : " + CoinManager.coinManager.realNumberCoinsSpawned );

	}*/

	void NewCreateLevel ()
	{
		//Reset values
		worldRoadSegments.Clear();
		recycledTiles.Clear();
		//levelTileList.Clear();
		tileCreationIndex = 0;
		playerTileIndex = 0;
		seamlessLevelIndex = 0;
						
		//LevelInfo has the parameters for a single level.
		//Get the level info for the current level.
		//int levelToLoad = LevelManager.Instance.getNextLevelToComplete();
		//LevelManager.Instance.setLevelNumberOfLastCheckpoint( levelToLoad );

		//seamlessLevelIndex = levelToLoad;

		//LevelData.LevelInfo levelInfo = levelData.getLevelInfo( levelToLoad );
		//Also set it in the LevelManager
		//LevelManager.Instance.setLevelInfo( levelInfo );
		LevelData.EpisodeInfo currentEpisode = LevelManager.Instance.getCurrentEpisodeInfo();

		//Sets the skybox, the directional light intensity and direction for the current level
		levelData.initialise();
		levelData.setSunParameters(currentEpisode.sunType);

		//Verify if we should include a plane surrounding the tiles (like an ocean)
		if( currentEpisode.includeSurroundingPlane )
		{
			GameObject go = (GameObject)Instantiate(surroundingPlane.gameObject, new Vector3( 0, -30f, 0 ), Quaternion.identity );
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

		//Create all the road segments that compose the road
		//List<LevelData.RoadSegment> roadSegmentList = levelInfo.roadSegmentList;
		List<TileGroupType> tileGroupList = currentEpisode.tileGroupList;

		//if( levelInfo.startTile != TileType.None )
		//{
			//Instantiate Start tile. There is only one Start tile per level.
			//The Start tile has the same theme as the first road segment.
			//setCurrentTheme( roadSegmentList[0].theme );
			//addTile( levelInfo.startTile );
		//}


		//for( int i=0; i < roadSegmentList.Count; i++ )
		//{
			//addRoadSegment( roadSegmentList[i] );
		//}

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

		//Fade-in the level ambience soundtrack
		StartCoroutine( SoundManager.soundManager.fadeInAmbience( currentEpisode.AmbienceSound, 10f ) );

		//Set the music track to play if a value is set
		SoundManager.soundManager.setMusicTrack( currentEpisode.MusicTrack );

		Debug.Log("GenerateLevel-CreateLevel: Level " + currentEpisode.episodeName + " has been created." );
		Debug.Log("GenerateLevel-CreateLevel: The number of coins spawned is : " + CoinManager.coinManager.realNumberCoinsSpawned );

	}

	void generateStoryLevel( List<TileGroupType> tileGroupList )
	{
		for( int i=0; i < tileGroupList.Count; i++ )
		{
			TileGroup tg = tileGroupManager.getTileGroup(tileGroupList[i]);
			if( tg.validGameMode == ValidGameMode.Any || tg.validGameMode == ValidGameMode.Story )
			{
				setCurrentTheme(tg.theme );
	
				Debug.LogWarning("TILE GROUP " +  tg.tileGroupType.ToString() );
				List<TileType> individualTiles = tg.tileList;
				for( int j=0; j < individualTiles.Count; j++ )
				{
					Debug.Log("TILE  " + individualTiles[j].ToString() );
					addTileNew( individualTiles[j] );
				}
			}
		}

	}

	void generateEndlessLevel( List<TileGroupType> tileGroupList )
	{
		for( int i=0; i < 1; i++ )
		{
			TileGroup tg = tileGroupManager.getTileGroup(tileGroupList[i]);
			if( tg.validGameMode == ValidGameMode.Any || tg.validGameMode == ValidGameMode.Endless )
			{
				setCurrentTheme(tg.theme );
	
				Debug.LogWarning("TILE GROUP " +  tg.tileGroupType.ToString() );
				List<TileType> individualTiles = tg.tileList;
				for( int j=0; j < individualTiles.Count; j++ )
				{
					Debug.Log("TILE  " + individualTiles[j].ToString() );
					addTileNew( individualTiles[j] );
				}
			}
		}

		//Now add a first random tile group to the endless tiles Queue
		TileGroup rtg = tileGroupManager.getRandomTileGroup( currentTheme );
		List <TileType> tiles = rtg.tileList;
		for( int j=0; j < tiles.Count; j++ )
		{
			Debug.Log("RANDOM TILE  " + tiles[j].ToString() );
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
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		PlayerController playerController = player.GetComponent<PlayerController>();
		GameObject firstTile = worldRoadSegments[0];
		playerController.currentTile = firstTile;
		playerController.tileRotationY = firstTile.transform.eulerAngles.y;
		playerController.currentTilePos = firstTile.transform.position;
		SegmentInfo si = firstTile.GetComponent<SegmentInfo>();
		playerController.currentTileType = si.tileType;

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

		SegmentInfo si = getSegmentInfo( go );
		tileDepthMult= si.tileDepth;
		previousTileType = si.tileSubType; //for constructing the level, we use the simpler types like straight, left and right
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
		si.tileIndex = tileCreationIndex;
		tileCreationIndex++;

		return go;
	}

	/*int getTileDepth( TileType type )
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
			case TileType.Right:
			case TileType.Landmark_Windmill:
			case TileType.Landmark_Defense_Tower:
			case TileType.Landmark_Dragon_Landing:
			case TileType.Straight_Slope:
			case TileType.T_Junction:
			case TileType.Landmark_Cemetery_Queen:
			case TileType.Landmark_Graveyard_Ghost:
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
			case TileType.Landmark_Tomb_Start_2:
			case TileType.Landmark_Broken_Bridge:
			case TileType.Landmark_Cemetery_Coach:
			case TileType.Landmark_Graveyard:
			case TileType.Landmark_Graveyard_1:
			case TileType.Landmark_Graveyard_2:
			case TileType.Landmark_Graveyard_3:
			case TileType.Landmark_Graveyard_4:
			case TileType.Landmark_Graveyard_5:
			case TileType.Landmark_Graveyard_6:
			case TileType.Landmark_Graveyard_7:
			case TileType.Landmark_Graveyard_Start:
			case TileType.Landmark_Graveyard_End:
			case TileType.Landmark_Collapsing_Bridge:
			case TileType.Landmark_Goblin_Loot:
			case TileType.Landmark_Zipline:
				depth = 2;
				break;

			case TileType.Landmark_Tomb_Start:
			case TileType.Landmark_Tomb_End:
			case TileType.Start_Fairyland:
				case TileType.Landmark_Treasure_Key:
				depth = 3;
				break;

			case TileType.Landmark_Magic_Bridge:
			case TileType.Landmark_Bog_Valley:
			case TileType.Opening5:
				depth = 4;
				break;

			case TileType.Jungle_Start:
				depth = 5;
				break;

			default:
				Debug.LogError("GenerateLevel-getTileDepth: unknown tile type specified: " + type );
				break;
		}
		return depth;
	}*/

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

	//Slope is the X rotation of the tile prefab.
	private Quaternion getTileRotation( float slope )
	{
		float previousTileRotY = Mathf.Floor( previousTileRot.eulerAngles.y );
		Quaternion tileRot = Quaternion.identity;
		switch (previousTileType)
		{
			case TileSubType.None:
			return Quaternion.identity;
		
			case TileSubType.Straight:
			tileRot =  Quaternion.Euler( slope, previousTileRot.eulerAngles.y, 0 );
			return tileRot;

			case TileSubType.Left:
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
		
			case TileSubType.Right:
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
			Debug.LogError ("getTileRotation : unhandled tile type: " + previousTileType );
			tileRot.eulerAngles = Vector3.zero;
			return tileRot;
		}
	}
	
	private  void addTileNew ( TileType tileType )
	{
        switch (tileType)
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

		case TileType.Landmark_Zipline:
			//We want the Landmark tile to have a 0 degree rotation.
			ensureTileHasZeroRotation();
			addTile( TileType.Landmark_Zipline );
			addTile( TileType.Left );		
			break;

		case TileType.Landmark_Cemetery_Coach:
			//We want the Landmark tile to have a 0 degree rotation.
			ensureTileHasZeroRotation();
			addTile( TileType.Landmark_Cemetery_Coach );
			break;

		case TileType.T_Junction:
			addRandomTJunction(tileType);
			break;

		default:
			addTile( tileType );
			break;
		}
	}

	//nbrTilesInSegment includes the road segment end tile
	/*private  void addRoadSegment ( LevelData.RoadSegment roadSegment )
	{
		int localTileIndex = 0;
		setCurrentTheme( roadSegment.theme );

		int maxIndex = roadSegment.NumberOfTiles - 1;
		
		while ( localTileIndex < maxIndex )
		{
//addSegmentTile();
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

		case TileType.Landmark_Zipline:
			//We want the Landmark tile to have a 0 degree rotation.
			ensureTileHasZeroRotation();
			addTile( TileType.Landmark_Zipline );
			addTile( TileType.Left );		
			break;

		case TileType.Landmark_Cemetery_Coach:
			//We want the Landmark tile to have a 0 degree rotation.
			ensureTileHasZeroRotation();
			addTile( TileType.Landmark_Cemetery_Coach );
			break;

		default:
			if( GameManager.Instance.getGameMode() == GameMode.Story )
			{
				addTile( roadSegment.endTile );
			}
			else
			{
				addTile( TileType.Straight );
			}
			break;
		}
	}*/

	/*private void addRandomLandmark()
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
				
			case TileType.Landmark_Defense_Tower:
				//We want the Landmark tile to have a 0 degree rotation.
				ensureTileHasZeroRotation();
				addTile( TileType.Landmark_Defense_Tower );
				addTile( TileType.Left );		
				addTile( TileType.Straight );		
				break;
				
			case TileType.Landmark_Zipline:
				//We want the Landmark tile to have a 0 degree rotation.
				ensureTileHasZeroRotation();
				addTile( TileType.Landmark_Zipline );
				addTile( TileType.Left );		
				break;

			default:
				addTile( randomLandmark );
				break;
		}
	}*/


	/*private void prepareTileList ( int levelToPrepare )
	{

		TileSubType beforePrepareListTileType = previousTileType;
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

	}*/

	/*private void addTileData( TileType type, SegmentTheme theme )
	{
		TileData tileData = new TileData();
		tileData.tileTheme = theme;
		tileData.tileType = type;
		levelTileList.Enqueue (tileData);
		previousTileRot = getTileRotation(0);
//PROBLEM
//previousTileType = type;
		//print ("addTileData: adding tile type " + tileData.tileType + " with theme " + tileData.tileTheme + " prev rot " + previousTileRot.eulerAngles );
	}*/

	/*private TileType getTileSubType( TileType type )
	{
		switch (type)
		{
		case TileType.None:
			return TileType.None;
			
		case TileType.Opening:
		case TileType.Opening2:
		case TileType.Opening3:
		case TileType.Opening4:
		case TileType.Opening5:
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
		case TileType.Landmark_Tomb_Start_2:
		case TileType.Landmark_Tomb_End:
		case TileType.Landmark_Tomb_Double:
		case TileType.Start_Fairyland:
		case TileType.Landmark_Cemetery_Coach:
		case TileType.Landmark_Graveyard:
		case TileType.Landmark_Graveyard_1:
		case TileType.Landmark_Graveyard_2:
		case TileType.Landmark_Graveyard_3:
		case TileType.Landmark_Graveyard_4:
		case TileType.Landmark_Graveyard_5:
		case TileType.Landmark_Graveyard_6:
		case TileType.Landmark_Graveyard_7:
		case TileType.Landmark_Graveyard_Start:
		case TileType.Landmark_Graveyard_End:
		case TileType.Landmark_Graveyard_Ghost:
		case TileType.Landmark_Test:
		case TileType.Landmark_Bog_Valley:
		case TileType.Landmark_Bog_Start:
		case TileType.Landmark_Treasure_Key:
		case TileType.Landmark_Collapsing_Bridge:
		case TileType.Landmark_Goblin_Loot:
		case TileType.Jungle_Start:
		case TileType.Landmark_Zipline:
			return TileType.Straight;

		case TileType.Left:
			return TileType.Left;
			
		case TileType.T_Junction:
		case TileType.Landmark_Defense_Tower:
		case TileType.Landmark_Windmill:
		case TileType.Right:
			return TileType.Right;
			
		default:
			Debug.LogWarning ("getTileSubType : unhandled tile type: " + type );
			return TileType.None;
		}
	}*/

	/*private void addRoadSegmentEndTile( TileType endTileType, SegmentTheme theme )
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

		case TileType.Landmark_Zipline:
			//We want the Landmark tile to have a 0 degree rotation.
			ensureTileHasZeroRotation2(theme);
			addTileData(TileType.Landmark_Zipline, theme );
			addTileData(TileType.Left, theme);
			break;

		case TileType.Landmark_Cemetery_Coach:
			//We want the Landmark tile to have a 0 degree rotation.
			ensureTileHasZeroRotation2(theme);
			addTileData( TileType.Landmark_Cemetery_Coach, theme );
			break;
			
		default:
			addTileData(endTileType, theme);
			break;
		}
	}*/

	/*
	//Add tiles such as to garanty that the tile added after these will have a zero Y rotation.
	private void ensureTileHasZeroRotation2( SegmentTheme theme)
	{
		float yRot = Mathf.Floor( previousTileRot.eulerAngles.y );
		switch (previousTileType)
		{
		case TileSubType.Left:
			if( yRot == 0 )
			{
				addTileData(TileType.Right, theme);
			}
			return;
			
		case TileSubType.Right:
			if( yRot == 0 )
			{
				addTileData(TileType.Left, theme);
			}
			return;
			
		case TileSubType.Straight:
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
	}*/

	/*private void addRandomTJunction2( TileType tJunctionTileType, SegmentTheme theme )
	{
		//We want the T-Junction tile to have a 0 degree rotation.
		ensureTileHasZeroRotation2( theme );
		//Adding T-Junction
		//The additional left and right tiles that are associated with a T-Junction
		//will be added at runtime.
		addTileData(tJunctionTileType, theme);
	}*/

	/*public class TileData
	{
		public TileType tileType = TileType.None;
		public SegmentTheme tileTheme;
	}*/

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

	/*private void addSegmentTile()
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
						if( currentThemePath == "Level/Tiles/Forest/" || currentThemePath == "Level/Tiles/Hell/" || currentThemePath == "Level/Tiles/Fairyland/" || currentThemePath == "Level/Tiles/Blizzard/")
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
	}*/

	/*private TileType getSegmentTile( SegmentTheme tileCreationTheme )
	{
		float previousRotY = Mathf.Floor( previousTileRot.eulerAngles.y );
		//Debug.Log("GenerateLevel-getSegmentTile: previous angle: " + previousRotY );

		//Straight section
		if( Random.value < 0.7f )
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
						if( tileCreationTheme == SegmentTheme.Forest || currentThemePath == "Level/Tiles/Fairyland/")
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
	}*/

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
	

	void newtileEntranceCrossedEndless()
	{
		if( endlessTileList.Count > 0 )
		{
			//Yes, we still have tiles in the queue
			addTileNew( endlessTileList.Dequeue() );
		}
		else
		{
			//We have run out of tiles
			TileGroup rtg = tileGroupManager.getRandomTileGroup( currentTheme );
			List <TileType> tiles = rtg.tileList;
			for( int j=0; j < tiles.Count; j++ )
			{
				Debug.Log("ENTRANCE RANDOM TILE  " + tiles[j].ToString() );
				endlessTileList.Enqueue(tiles[j]);
			}
			//Now that we have added additional tile, we can get a tile
			addTileNew( endlessTileList.Dequeue() );
		}
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
		//addTileInEndlessMode();
				newtileEntranceCrossedEndless();
			}
		}
		else
		{
/*
			//We are not in endless mode.
			//Add a tile from the next level if any
			if( levelTileList.Count > 0 )
			{
				TileData td = levelTileList.Dequeue();
				setCurrentTheme( td.tileTheme );
				//Debug.LogWarning("tileEntranceCrossed: Adding next level tile of type: " + td.tileType + " theme: " + td.tileTheme );
				if( td.tileType == TileType.T_Junction )
				{
					//Because T-Junction construction changes the previous position and rotation values, we cannot do it during the prepareTileList phase.
					addRandomTJunction( td.tileType );
				}
				else
				{
					addTile( td.tileType );
				}
			}
	*/
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
		//If the tile we just activated has a T-Junction or Landmark_Cemetery type
		//also enable the first tile on it's right side which has an index of + 5 compared to the T-Junction tile itself.
		SegmentInfo si = getSegmentInfo(worldRoadSegments[index]);
		if( si.tileType == TileType.T_Junction )
		{
			int firstTileToTheRight = index + 5;
			if( firstTileToTheRight < worldRoadSegments.Count ) worldRoadSegments[firstTileToTheRight].SetActive(true);
		}
		if( GameManager.Instance.getGameMode() == GameMode.Story )
		{
			/*if( si.isCheckpoint )
			{
				//If the next level is an episode, we do not need to prepare tiles for it.
				//Preparing new level tiles will cause a brief performance hit.
				//If the next level is an episode, this means the player will return to the map before accessing it anyway.
				if( ( seamlessLevelIndex + 1 ) < LevelManager.Instance.getNumberOfLevels() )
				{
					if( LevelManager.Instance.getLevelInfo( seamlessLevelIndex + 1 ).levelType != LevelType.Episode )
					{
						seamlessLevelIndex++;
						if( seamlessLevelIndex < LevelManager.Instance.getNumberOfLevels() )
						{
							//prepareTileList( seamlessLevelIndex );
						}
						else
						{
							Debug.LogWarning("Player is reaching end of the game.");
						}
					}
					else
					{
						Debug.LogWarning("GenerateLevel - onTileActivation: not preparing tile list for level," + (seamlessLevelIndex + 1) + ", because it is an episode level." );
					}
				}
			}*/
		}
	}

	TileType getTileType( GameObject tile )
	{
		return getSegmentInfo(tile).tileType;
	}

}
