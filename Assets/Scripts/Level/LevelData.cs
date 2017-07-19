using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DynamicFogAndMist;
using System.Linq;

public enum SunType 
{
	Morning = 0,
	Noon = 1,
	Afternoon = 2,
	Sunset = 3,
	Night = 4,
	Hell = 5,
	Overcast = 6,
	Cemetery = 7,
	Elfland = 8,
	Blizzard = 9,
	Jungle = 10,
	Caves = 11,
	Countryside = 12,
	Sky_city = 13,
	Sky_city_night = 14

}

public enum LevelType 
{
	Episode = 0,
	Normal = 1
}

public enum RewardType
{
	None=0,
	Coins=1,	
	PowerUp = 2,
	Life = 3,
	Customization = 4	//Future implementation
}

public class LevelData : MonoBehaviour {
	
	public List<EpisodeInfo> episodeList = new List<EpisodeInfo>();
	public List<MultiplayerInfo> multiplayerList = new List<MultiplayerInfo>();

	public const int NUMBER_OF_EPISODES = 9;
	//This should be the directional light in the scene
	GameObject Sun;
	public Material skyBoxMaterial; //needed so that the cutscene camera can access it

	public void initialise()
	{
		Sun = GameObject.FindGameObjectWithTag("Sunlight");
	}

	public EpisodeInfo getEpisodeInfo( int episodeNumber )
	{
		if( episodeNumber < 0 || episodeNumber >= episodeList.Count )
		{
			Debug.LogError("LevelData-getEpisodeInfo: episode number specified, " + episodeNumber + ", is out of range." );
			return null;
		}
		else
		{		
			return episodeList[episodeNumber];
		}
	}

	public int getNumberOfMultiplayerLevels()
	{
		return multiplayerList.Count;
	}

	//This method sets the skybox material as well as the light intensity and direction 
	//for the current level. The skybox materials should be located under the Skybox directory under Resources.
	public void setSunParameters( SunType sunType )
	{
		if( Sun != null )
		{
			skyBoxMaterial = null;
			float lightIntensity;
			float shadowStrength = 0f;
			Quaternion sunDirection;
			string skyBoxName;
			RenderSettings.ambientLight = new Color(0,0,0);
			RenderSettings.fog = false;

			switch (sunType)
			{
			case SunType.Morning:
				skyBoxName = "CartoonSkybox";
				lightIntensity = 0.74f;
				Sun.GetComponent<Light>().shadows = LightShadows.Soft;
				shadowStrength = 0.3f;
				sunDirection = Quaternion.Euler( 78f,75f,4f );
				Sun.GetComponent<Light>().color = Color.white;
				break;
				
			case SunType.Noon:
				skyBoxName = "CartoonSkybox";
				lightIntensity = 0.58f;
				Sun.GetComponent<Light>().shadows = LightShadows.Soft;
				shadowStrength = 0.5f;
				sunDirection = Quaternion.Euler( 78f,75f,4f );
				Sun.GetComponent<Light>().color = Color.white;
				break;
				
			case SunType.Afternoon:
				skyBoxName = "CartoonSkybox";
				lightIntensity = 0.74f;
				Sun.GetComponent<Light>().shadows = LightShadows.Soft;
				shadowStrength = 0.4f;
				sunDirection = Quaternion.Euler( 78f,75f,4f );
				Sun.GetComponent<Light>().color = Color.white;
				break;
								
			case SunType.Blizzard:
				skyBoxName = "Blizzard";
				lightIntensity = 0.3f;
				Sun.GetComponent<Light>().shadows = LightShadows.Soft;
				shadowStrength = 0.6f;
				sunDirection = Quaternion.Euler( 80f,119f,42f );
				Sun.GetComponent<Light>().color = new Color(0.796f,0.796f,0.796f); //greyish
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
				RenderSettings.ambientIntensity = 1.1f;
				RenderSettings.fog = false;
				RenderSettings.fogMode = FogMode.Linear;
				RenderSettings.fogColor = new Color(0.447f,0.698f,0.917f); //blueish
				RenderSettings.fogStartDistance = 20f;
				RenderSettings.fogEndDistance = 80f;

				break;

			case SunType.Jungle:
				skyBoxName = "Jungle";
				lightIntensity = 1.15f;
				Sun.GetComponent<Light>().shadows = LightShadows.Soft;
				shadowStrength = 0.6f;
				sunDirection = Quaternion.Euler( 80f,119f,42f );
				RenderSettings.ambientSkyColor = new Color(0.764f, 0.764f, 0.764f ); //greyish
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
				RenderSettings.ambientIntensity = 0.6f;
				RenderSettings.fog = false;
				RenderSettings.fogMode = FogMode.Linear;
				RenderSettings.fogColor = new Color(0.172f, 0.654f, 0.654f ); //greenish
				RenderSettings.fogStartDistance = 35f;
				RenderSettings.fogEndDistance = 120f;

				break;

			case SunType.Caves:
				skyBoxName = "None";
				lightIntensity = 0.27f;
				Sun.GetComponent<Light>().color = new Color(0.855f,0.855f,0.855f); //light grey
				Sun.GetComponent<Light>().shadows = LightShadows.None;
				sunDirection = Quaternion.Euler( 69f,83f,68f );
				RenderSettings.skybox = null;	
				RenderSettings.ambientSkyColor = new Color(0.353f, 0.222f, 0.052f ); //brownish
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
				RenderSettings.ambientIntensity = 1f;
				RenderSettings.fog = false;
				RenderSettings.fogMode = FogMode.Linear;
				RenderSettings.fogColor = new Color(0, 0.466f, 0.56f ); //greenish
				RenderSettings.fogStartDistance = 30f;
				RenderSettings.fogEndDistance = 90f;
				break;

			case SunType.Night:
				skyBoxName = "Skybox_Cartoon_Night";
				lightIntensity = 0.65f;
				Sun.GetComponent<Light>().shadows = LightShadows.None;
				sunDirection = Quaternion.Euler( 32f,13f,-63f );
				Sun.GetComponent<Light>().color = Color.white;
				break;

			case SunType.Overcast:
				skyBoxName = "Overcast2 Skybox";
				lightIntensity = 1.2f;
				Sun.GetComponent<Light>().shadows = LightShadows.None;
				sunDirection = Quaternion.Euler( 38.28f,119.5f,87.52f );
				Sun.GetComponent<Light>().color = new Color(0.623f,0.729f,0.882f); //bluish
				break;

			case SunType.Elfland:
				skyBoxName = "Overcast2 Skybox";
				lightIntensity = 0.75f;
				shadowStrength = 0.3f;
				Sun.GetComponent<Light>().shadows = LightShadows.None;
				sunDirection = Quaternion.Euler( 38.28f,119.5f,87.52f );
				Sun.GetComponent<Light>().color = new Color(1f,0.788f,0.647f); //orange
				break;

			case SunType.Hell:
				skyBoxName = "Skybox Hell";
				lightIntensity = 0.3f;
				Sun.GetComponent<Light>().shadows = LightShadows.None;
				sunDirection = Quaternion.Euler( 32f,13f,-63f );
				RenderSettings.ambientLight = new Color(0.13f,0.21f,0.3f);
				Sun.GetComponent<Light>().color = Color.white;
				break;

			case SunType.Cemetery:
				skyBoxName = "Skybox Cemetery";
				lightIntensity = 0.28f;
				Sun.GetComponent<Light>().shadows = LightShadows.None;
				sunDirection = Quaternion.Euler( 32f,13f,-63f );
				Sun.GetComponent<Light>().color = new Color(0.623f,0.729f,0.882f); //bluish
				RenderSettings.ambientLight = new Color(0.13f,0.21f,0.3f);
				Sun.GetComponent<Light>().color = Color.white;
				break;

			case SunType.Sky_city:
				skyBoxName = "Skybox_backgound";

				lightIntensity = 1.15f;
				Sun.GetComponent<Light>().shadows = LightShadows.Soft;
				shadowStrength = 0.42f;
				sunDirection = Quaternion.Euler( 80f,119f,42f );

				RenderSettings.ambientSkyColor = new Color(0.764f, 0.764f, 0.764f ); //greyish
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
				RenderSettings.ambientIntensity = 0.6f;

				//RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
				//RenderSettings.ambientIntensity = 1f;

				RenderSettings.fog = true;
				RenderSettings.fogMode = FogMode.Linear;
				RenderSettings.fogColor = new Color(109f/255f, 169f/255f, 226f/255f ); //bluish
				RenderSettings.fogStartDistance = 100f;
				RenderSettings.fogEndDistance = 120f;

				break;

			case SunType.Sky_city_night:
				skyBoxName = "Skybox_backgound_night";

				lightIntensity = 2.5f;
				Sun.GetComponent<Light>().shadows = LightShadows.None;
				Sun.GetComponent<Light>().color = new Color(44f/255f,96f/255f,146f/255f); //purple
				shadowStrength = 0.1f;
				sunDirection = Quaternion.Euler( 122.7f,-55.8f,30.7f );

				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
				RenderSettings.ambientIntensity = 2f;

				RenderSettings.fog = true;
				RenderSettings.fogMode = FogMode.Linear;
				RenderSettings.fogColor = new Color( 2f/255f, 52f/255f, 105f/255f ); //dark purple
				RenderSettings.fogStartDistance = 100f;
				RenderSettings.fogEndDistance = 120f;

				break;

			case SunType.Countryside:
				skyBoxName = "Countryside";
				lightIntensity = 0.8f;
				Sun.GetComponent<Light>().shadows = LightShadows.Soft;
				shadowStrength = 0.75f;
				sunDirection = Quaternion.Euler( 35f,-140f,-27f );
				Sun.GetComponent<Light>().color = new Color(0.913f,0.898f,0.776f); //light yellow
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
				RenderSettings.ambientGroundColor = new Color(0.71f,0.794f,0.642f);
				RenderSettings.ambientEquatorColor = new Color(0.794f,0.742f,0.455f);
				RenderSettings.ambientSkyColor = new Color(0.506f,0.633f,0.748f);
				RenderSettings.ambientIntensity = 1f;
				RenderSettings.fog = false;
				RenderSettings.fogMode = FogMode.ExponentialSquared;
				RenderSettings.fogColor = new Color(0.792f,0.823f,0.764f); //greenish
				break;

			default:
				skyBoxName = "CartoonSkybox";
				lightIntensity = 0.54f;
				Sun.GetComponent<Light>().shadows = LightShadows.Soft;
				shadowStrength = 0.62f;
				sunDirection = Quaternion.Euler( 78f,75f,4f );
				Sun.GetComponent<Light>().color = Color.white;
				break;
			}
			if( skyBoxName != "None" ) skyBoxMaterial = Resources.Load( "Skybox/" + skyBoxName ) as Material;
			if( skyBoxMaterial != null )
			{
				RenderSettings.skybox = skyBoxMaterial;
			}
			
			Sun.GetComponent<Light>().intensity = lightIntensity;
			Sun.transform.rotation = sunDirection;
			Sun.GetComponent<Light>().shadowStrength = shadowStrength;
		}
		else
		{
			Debug.LogError("LevelData-setSunParameters : The level scene must contain a directional light called 'Sunlight'." );
		}

	}

	public void setFogParameters( SunType sunType )
	{
		DynamicFog dynamicFog = Camera.main.GetComponent<DynamicFog>();
		switch (sunType)
		{
			case SunType.Blizzard:
				//Fog Properties
				dynamicFog.alpha = 0.6f;
				dynamicFog.noiseStrength = 0.4f;
				dynamicFog.distance = 0.01f;
				dynamicFog.distanceFallOff = 0.04f;
				dynamicFog.maxDistance = 1f;
				dynamicFog.maxDistanceFallOff = 0f;
				dynamicFog.height = 20f;
				dynamicFog.heightFallOff = 1f;

				dynamicFog.turbulence = 0.4f;
				dynamicFog.speed = 0.005f;
				dynamicFog.color = new Color( 0.89f, 0.89f, 0.89f );
				dynamicFog.color2 = new Color( 0.89f, 0.89f, 0.89f );
		
				//Sky Properties
				dynamicFog.skyHaze = 350f;
				dynamicFog.skySpeed = 0.3f;
				dynamicFog.skyNoiseStrength = 0.6f;
				dynamicFog.skyAlpha = 0;
				break;

		}
		//Force the materials to update
		dynamicFog.UpdateMaterialProperties();
	}

	[System.Serializable]
	public class EpisodeInfo
	{
		[Header("Episode Parameters")]
		[Tooltip("Episode name is not used at runtime. It is only used to make the data easier to read in the editor.")]
		public string episodeName = "Episode Name";
		[Tooltip("Image used at the top of the pre-level popup.")]
		public Sprite preLevelSprite;
		[Tooltip("Total number of chest keys for episode.")]
		public int numberOfChestKeys = 0;
		[Tooltip("Coins required to reach one, two, or three stars for the episode.")]
		public Vector3 coinsRequired = new Vector3( 10000f, 33000f, 50000f );
		[Tooltip("The type of sun for the level. The sun type will not change until the player starts a new section. The sun type determines characteristics such ambient light, directional light rotation, color, intensity and shadows and skybox material.")]
		public SunType sunType = SunType.Afternoon;
		[Tooltip("Specifies whether the level should allow Tap To Play right away or wait for a callback. For example, in the opening level, we want to wait a few seconds before the player can start playing.")]
		public bool waitForTapToPlay = false;
		[Tooltip("The player's initial run speed in m/sec.")]
		public float RunStartSpeed = 18f;
		[Tooltip("How fast will the player accelerate.")]
		public float RunAcceleration = 0.13f;
		[Tooltip("The number of tiles between power ups. Zero means there is no power up in that level.")]
		public int powerUpDensity = 4;
		[Tooltip("The ambience sound for the level. It plays in addition to the music. It is optional.")]
		public AudioClip mainAmbienceTrack;
		[Tooltip("A secondary ambience sound for the level. It plays in addition to the music. It is optional.")]
		public AudioClip secondaryAmbienceTrack;
		[Tooltip("The quiet music track for the level. It plays in addition to the ambience. It is optional.")]
		public AudioClip quietMusicTrack;
		[Tooltip("The action music track for the level. It plays on top of the quiet music when triggered, usually during a combat sequence. It is optional.")]
		public AudioClip actionMusicTrack;
		[Tooltip("Whether or not to include a surrounding plane. The plane can represent an ocean for example.")]
		public bool includeSurroundingPlane = false;
		[Tooltip("Which material to use for the surrounding plane.")]
		public Material surroundingPlaneMaterial;
		[Header("Dynamic Fog")]
		public bool isFogEnabled = false;
		[Header("Tile Groups")]
		public List<TileGroupType> tileGroupList = new List<TileGroupType>();
	}

	public MultiplayerInfo getRaceTrackByName( string raceTrackName )
	{
		for( int i =0; i < multiplayerList.Count; i++ )
		{
			if( multiplayerList[i].circuitInfo.raceTrackName == raceTrackName )
			{	
				return multiplayerList[i];
			}
		}
		return null;
	}

	/// <summary>
	/// Gets the appropriate multiplayer info based on the number of trophies the player has.
	/// </summary>
	/// <returns>The multiplayer info based on the number of trophies the player has.</returns>
	public MultiplayerInfo getRaceTrackByTrophies()
	{
		return getRaceTrackByTrophies( GameManager.Instance.playerProfile.getTrophies() );
	}

	/// <summary>
	/// Gets the appropriate multiplayer info based on the number of trophies specified.
	/// If the tutorial has not been completed, it will return the tutorial track.
	/// Race track 0 is the tutorial.
	/// Race Track 1 is unlocked upon completing the tutorial.
	/// 400 Trophy unlocks Race Track 2.
	/// 800 Trophy unlocks Race Track 3.
	/// 1,200 Trophy unlocks Race Track 4.
	/// 1,600 Trophy unlocks Race Track 5.
	/// 2,000 Trophy unlocks Race Track 6.
	/// </summary>
	/// <returns>The appropriate multiplayer info based on the number of trophies specified</returns>
	/// <param name="numberOfTrophies">Number of trophies.</param>
	public MultiplayerInfo getRaceTrackByTrophies( int numberOfTrophies )
	{
		if( numberOfTrophies < 0 )
		{
			Debug.LogError("LevelData-getRaceTrackByTrophies: The number of trophies must be greater than 0." );
			return null;
		}
		MultiplayerInfo multiplayerInfo = null;
		if( !GameManager.Instance.playerProfile.hasCompletedTutorial() )
		{
			multiplayerInfo = multiplayerList[0];
		}
		else
		{
			List<MultiplayerInfo> firstElement = new List<MultiplayerInfo>();
			firstElement.Add( multiplayerList[0] ); //exclude the tutorial track
			var sortedList = multiplayerList.Except(firstElement).OrderByDescending( entry => entry.trophiesNeededToUnlock );
			foreach( MultiplayerInfo mi in sortedList )
			{
				if( numberOfTrophies >= mi.trophiesNeededToUnlock )
				{
					multiplayerInfo = mi;
					//print( "getRaceTrackByTrophies-the one we want is " + mi.circuitInfo.raceTrackName + " " + mi.trophiesNeededToUnlock + " needed: " + numberOfTrophies );
					break;
				}
			}
		}
		return multiplayerInfo;
	}

	/// <summary>
	/// Gets the sorted race track list. It is sorted in ascending order based on the number of trophies required.
	/// </summary>
	/// <returns>The sorted race track list.</returns>
	public List<MultiplayerInfo> getSortedRaceTrackList()
	{
		return multiplayerList.OrderBy( entry => entry.trophiesNeededToUnlock ).ToList();
	}

	[System.Serializable]
	public class MultiplayerInfo
	{
		[Header("Multiplayer Parameters")]
		[Tooltip("Circuit name is not used at runtime. It is only used to make the data easier to read in the editor.")]
		public string circuitName = "Circuit Name";
		public CircuitInfo circuitInfo = new CircuitInfo();
		[Tooltip("The type of sun for the level. The sun type will not change until the player starts a new section. The sun type determines characteristics such ambient light, directional light rotation, color, intensity and shadows and skybox material.")]
		public SunType sunType = SunType.Afternoon;
		[Tooltip("The player's initial run speed in m/sec.")]
		public float RunStartSpeed = 18f;
		[Tooltip("The ambience sound for the level. It plays in addition to the music. It is optional.")]
		public AudioClip mainAmbienceTrack;
		[Tooltip("A secondary ambience sound for the level. It plays in addition to the music. It is optional.")]
		public AudioClip secondaryAmbienceTrack;
		[Tooltip("The quiet music track for the level. It plays in addition to the ambience. It is optional.")]
		public AudioClip quietMusicTrack;
		[Tooltip("The action music track for the level. It plays on top of the quiet music when triggered, usually during a combat sequence. It is optional.")]
		public AudioClip actionMusicTrack;
		[Tooltip("Whether or not to include a surrounding plane. The plane can represent an ocean for example.")]
		public bool includeSurroundingPlane = false;
		[Tooltip("Which material to use for the surrounding plane.")]
		public Material surroundingPlaneMaterial;
		[Header("Dynamic Fog")]
		public bool isFogEnabled = false;
		[Header("Tile Groups")]
		[Tooltip("Defines the number of tile groups for the level including the end tile group. If the number specified is bigger than the number of tile groups in tileGroupList plus one (for the end tile group), random tile groups will be added.")]
		public int numberOfTileGroups = 25;
		[Tooltip("Defines the mandatory tile groups for the level. It should not contain an end tile group. It is preferable, but not mandatory, that it contains a start tile group.")]
		public List<TileGroupType> tileGroupList = new List<TileGroupType>();
		[Tooltip("A list of end tile groups. A random tile group from this list will be added at the end of the level. It should not be empty.")]
		public List<TileGroupType> endTileGroupList = new List<TileGroupType>();
		[Tooltip("Tile size. Either 50 for Jousting or 36.4 for all other.")]
		public float tileSize;
		[Header("Other")]
		[Tooltip("Trophies needed to unlock race track.")]
		public int trophiesNeededToUnlock;
		[Tooltip("Coins awarded on victory. This number varies per track.")]
		public int coinsAwardedOnVictory;
		
	}

	[System.Serializable]
	public class CircuitInfo
	{
		[Header("Sector Parameters")]
		[Tooltip("The name to use for matchmaking. It must NOT have any underscore characters '_'.")]
		public string raceTrackName = string.Empty;
		[Tooltip("Bigger rectangular image.")]
		public Sprite circuitImage;
		[Tooltip("Square icon used at the beginning of the match.")]
		public Sprite circuitIcon;
		[Tooltip("The sector number is displayed in various UI elements. The training sector is 0.")]
		public int sectorNumber;
		[Tooltip("The background color is used in various UI elements to match the main color of the sector image.")]
		public Color backgroundColor;
	}

}
