﻿using UnityEngine;
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
	Sky_city_night = 14,
	Desert = 15,
	Desert_night = 16
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

/*
Fog notes
Symptoms

I am changing the fog settings of my built game scene at runtime using the RenderSettings.fog API to include fog
in my scene but it's not showing.

Cause

By default, shader variants used to handle Fog modes that are not used by any of the scenes are
not included in the game data. This is done to help cut down on shader data size.

Resolution

In the Graphics settings panel under Edit -> Project Settings -> Graphics there is a Fog modes drop down box.
By default, the drop down is set to Automatic. This will strip the Fogging variants of the shader if it is not
found on any of the scenes. You can set these to be overridden by setting the drop down property to Manual.
*/
public class LevelData : MonoBehaviour {
	
	public List<LightingData> lightingDataList = new List<LightingData>();
	public List<EpisodeInfo> episodeList = new List<EpisodeInfo>();
	public List<MultiplayerInfo> multiplayerList = new List<MultiplayerInfo>();

	public const int NUMBER_OF_EPISODES = 9;
	//This should be the directional light in the scene
	GameObject Sun;
	Material skyBoxMaterial;

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
		LightingData lightingData = lightingDataList.Find( ld => ld.sunType == sunType );

		if( Sun != null )
		{
			skyBoxMaterial = null;
			float lightIntensity;
			float shadowStrength = 0f;
			Quaternion sunDirection;
			RenderSettings.ambientLight = new Color(0,0,0);
			RenderSettings.fog = false;

			switch (sunType)
			{
			case SunType.Morning:
				lightIntensity = 0.74f;
				Sun.GetComponent<Light>().shadows = LightShadows.Soft;
				shadowStrength = 0.3f;
				sunDirection = Quaternion.Euler( 78f,75f,4f );
				Sun.GetComponent<Light>().color = Color.white;
				break;
				
			case SunType.Noon:
				lightIntensity = 0.58f;
				Sun.GetComponent<Light>().shadows = LightShadows.Soft;
				shadowStrength = 0.5f;
				sunDirection = Quaternion.Euler( 78f,75f,4f );
				Sun.GetComponent<Light>().color = Color.white;
				break;
				
			case SunType.Afternoon:
				lightIntensity = 0.74f;
				Sun.GetComponent<Light>().shadows = LightShadows.Soft;
				shadowStrength = 0.4f;
				sunDirection = Quaternion.Euler( 78f,75f,4f );
				Sun.GetComponent<Light>().color = Color.white;
				break;
								
			case SunType.Desert:
				//Directional light
				Sun.GetComponent<Light>().color = lightingData.sunColor;

				lightIntensity = 0.7f;
				Sun.GetComponent<Light>().shadows = LightShadows.Soft;
				sunDirection = Quaternion.Euler( 45.57f,-329f,-159.54f );
				shadowStrength = 1f;

				//Sky box
				RenderSettings.sun = Sun.GetComponent<Light>();
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
				RenderSettings.ambientSkyColor = lightingData.skyColor;
				RenderSettings.ambientEquatorColor = lightingData.equatorColor;
				RenderSettings.ambientGroundColor = lightingData.groundColor;
				RenderSettings.reflectionIntensity = 0.614f;

				//Fog
				RenderSettings.fog = true;
				RenderSettings.fogMode = FogMode.Linear;
				RenderSettings.fogColor = lightingData.fogColor;
				RenderSettings.fogStartDistance = -1.4f;
				RenderSettings.fogEndDistance = 84.93f;
				break;

			case SunType.Desert_night:
				//Directional light
				Sun.GetComponent<Light>().color = lightingData.sunColor;
				lightIntensity = 0.65f;
				Sun.GetComponent<Light>().shadows = LightShadows.Soft;
				sunDirection = Quaternion.Euler( 45.57f,-329f,-159.54f );
				shadowStrength = 0.6f;

				//Sky box
				RenderSettings.sun = Sun.GetComponent<Light>();
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
				RenderSettings.ambientSkyColor = lightingData.skyColor;
				RenderSettings.ambientEquatorColor = lightingData.equatorColor;
				RenderSettings.ambientGroundColor = lightingData.groundColor;

				RenderSettings.reflectionIntensity = 0.614f;

				//Fog
				RenderSettings.fog = true;
				RenderSettings.fogMode = FogMode.Linear;
				RenderSettings.fogColor = lightingData.fogColor;
				RenderSettings.fogStartDistance = -1.4f;
				RenderSettings.fogEndDistance = 100f;
				break;

			case SunType.Blizzard:
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
				lightIntensity = 0.65f;
				Sun.GetComponent<Light>().shadows = LightShadows.None;
				sunDirection = Quaternion.Euler( 32f,13f,-63f );
				Sun.GetComponent<Light>().color = Color.white;
				break;

			case SunType.Overcast:
				lightIntensity = 1.2f;
				Sun.GetComponent<Light>().shadows = LightShadows.None;
				sunDirection = Quaternion.Euler( 38.28f,119.5f,87.52f );
				Sun.GetComponent<Light>().color = new Color(0.623f,0.729f,0.882f); //bluish
				break;

			case SunType.Elfland:
				lightIntensity = 0.75f;
				shadowStrength = 0.3f;
				Sun.GetComponent<Light>().shadows = LightShadows.None;
				sunDirection = Quaternion.Euler( 38.28f,119.5f,87.52f );
				Sun.GetComponent<Light>().color = new Color(1f,0.788f,0.647f); //orange
				break;

			case SunType.Hell:
				lightIntensity = 0.3f;
				Sun.GetComponent<Light>().shadows = LightShadows.None;
				sunDirection = Quaternion.Euler( 32f,13f,-63f );
				RenderSettings.ambientLight = new Color(0.13f,0.21f,0.3f);
				Sun.GetComponent<Light>().color = Color.white;
				break;

			case SunType.Cemetery:
				lightIntensity = 0.28f;
				Sun.GetComponent<Light>().shadows = LightShadows.None;
				sunDirection = Quaternion.Euler( 32f,13f,-63f );
				Sun.GetComponent<Light>().color = new Color(0.623f,0.729f,0.882f); //bluish
				RenderSettings.ambientLight = new Color(0.13f,0.21f,0.3f);
				Sun.GetComponent<Light>().color = Color.white;
				break;

			case SunType.Sky_city:
				lightIntensity = 1.15f;
				Sun.GetComponent<Light>().shadows = LightShadows.Soft;
				shadowStrength = 0.42f;
				sunDirection = Quaternion.Euler( 80f,119f,42f );

				RenderSettings.ambientSkyColor = new Color(0.764f, 0.764f, 0.764f ); //greyish
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
				RenderSettings.ambientIntensity = 0.6f;

				//RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
				//RenderSettings.ambientIntensity = 1f;

				RenderSettings.fog = false;
				RenderSettings.fogMode = FogMode.Linear;
				RenderSettings.fogColor = lightingData.fogColor; //bluish
				RenderSettings.fogStartDistance = 100f;
				RenderSettings.fogEndDistance = 120f;

				break;

			case SunType.Sky_city_night:
				lightIntensity = 2.5f;
				Sun.GetComponent<Light>().shadows = LightShadows.None;
				Sun.GetComponent<Light>().color = lightingData.sunColor; //bluish
				shadowStrength = 0.1f;
				sunDirection = Quaternion.Euler( 122.7f,-55.8f,30.7f );

				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
				RenderSettings.ambientIntensity = 2f;

				RenderSettings.fog = false;
				RenderSettings.fogMode = FogMode.Linear;
				RenderSettings.fogColor = lightingData.fogColor; //dark purple
				RenderSettings.fogStartDistance = 100f;
				RenderSettings.fogEndDistance = 120f;

				break;

			case SunType.Countryside:
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
				lightIntensity = 0.54f;
				Sun.GetComponent<Light>().shadows = LightShadows.Soft;
				shadowStrength = 0.62f;
				sunDirection = Quaternion.Euler( 78f,75f,4f );
				Sun.GetComponent<Light>().color = Color.white;
				break;
			}
			skyBoxMaterial = getSkyBox( sunType );
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

	/// <summary>
	/// Gets a random map excluding coop maps and the training map.
	/// For testing, use the override in the debug menu to specify which map you want to play in.
	/// </summary>
	/// <returns>The random map.</returns>
	public MultiplayerInfo getRandomMap()
	{
		if( GameManager.Instance.playerDebugConfiguration.getOverrideMap() != -1 )
		{
			//For testing, use the override to specify which map you want to play in.
			return multiplayerList[GameManager.Instance.playerDebugConfiguration.getOverrideMap()];
		}
		else
		{
			List<MultiplayerInfo> competitionList = multiplayerList.FindAll( entry => entry.isCoop == false ).ToList();
			if( competitionList.Count > 0 )
			{
				//0 is the training map. We want to exclude it.
				int random = Random.Range( 1, competitionList.Count);
				return competitionList[random];
			}
			else
			{
				Debug.LogError("LevelData-getRandomMap was called but no MultiplayerInfo with isCoop set to false has been found. Returning null." );
				return null;
			}
		}
	}

	/// <summary>
	/// Gets a random coop map.
	/// For testing, use the override in the debug menu to specify which map you want to play in.
	/// </summary>
	/// <returns>The random map.</returns>
	public MultiplayerInfo getRandomCoopMap()
	{
		if( GameManager.Instance.playerDebugConfiguration.getOverrideMap() != -1 )
		{
			//For testing, use the override to specify which map you want to play in.
			return multiplayerList[GameManager.Instance.playerDebugConfiguration.getOverrideMap()];
		}
		else
		{
			Debug.LogError("LevelData-getRandomCoopMap was called but no MultiplayerInfo with isCoop set to true has been found. Returning null." );
			return null;
		}
	}

	public MultiplayerInfo getMapByName( string mapName )
	{
		for( int i =0; i < multiplayerList.Count; i++ )
		{
			if( multiplayerList[i].circuitInfo.mapName == mapName )
			{	
				return multiplayerList[i];
			}
		}
		return null;
	}

	/// <summary>
	/// Gets the sorted race track list. It is sorted in ascending order based on the number of trophies required.
	/// </summary>
	/// <returns>The sorted race track list.</returns>
	/// <param name="excludeCoop">If set to <c>true</c> exclude coop tracks.</param>
	public List<MultiplayerInfo> getSortedRaceTrackList( bool excludeCoop )
	{
		if( excludeCoop )
		{
			//Return only non-coop tracks.
			return multiplayerList.FindAll( entry => entry.isCoop == false ).ToList();
		}
		else
		{
			//Return all tracks, including coop tracks.
			return multiplayerList;
		}
	}

	[System.Serializable]
	public class MultiplayerInfo
	{
		[Header("Multiplayer Parameters")]
		[Tooltip("Editor map name is not used at runtime. It's only used to make the data easier to read in the editor.")]
		public string editorMapName;
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
		[Range(0,1)]
		[Tooltip("Percentage chance of rain. Set to 0 if you do not want any rain. The level tiles must have rain particle systems with the RainActivator component for this to work.")]
		public float rainChance;
		[Tooltip("The rain clip will use the main ambience audio source.")]
		public AudioClip rainAudio;
		[Tooltip("Set to true if this level is for the coop mode.")]
		public bool isCoop = false;
		
	}

	[System.Serializable]
	public class CircuitInfo
	{
		[Header("Map Parameters")]
		[Tooltip("The name to use for matchmaking. It must NOT have any underscore characters '_'.")]
		public string mapName = string.Empty;
		[Tooltip("Bigger rectangular image.")]
		public Sprite circuitImage;
		[Tooltip("The map number is displayed in various UI elements. The training map is 0.")]
		public int mapNumber;
		[Tooltip("The background color is used in various UI elements to match the main color of the map image.")]
		public Color backgroundColor;
		[Tooltip("Spawn height")]
		public float spawnHeight = 0;
	}

	[System.Serializable]
	public class LightingData
	{
		public SunType sunType; 
		public Material skyBox;
		public Color sunColor;
		public Color skyColor;
		public Color equatorColor;
		public Color groundColor;
		public Color fogColor;
	}

	Material getSkyBox( SunType sunType )
	{
		LightingData lightingData = lightingDataList.Find( ld => ld.sunType == sunType );
		if( lightingData != null )
		{
			return lightingData.skyBox;
		}
		else
		{
			Debug.LogError("LevelData-getSkyBox: There is no lighting data associated with the sun type: " + sunType );
			return null;
		}
	}

}
