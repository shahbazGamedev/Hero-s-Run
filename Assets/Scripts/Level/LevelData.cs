using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DynamicFogAndMist;

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
	Countryside = 12
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
	
	public MultiplayerInfo getMultiplayerInfo( int multiplayerNumber )
	{
		if( multiplayerNumber < 0 || multiplayerNumber >= multiplayerList.Count )
		{
			Debug.LogError("LevelData-getMultiplayerInfo: multiplayer number specified, " + multiplayerNumber + ", is out of range." );
			return null;
		}
		else
		{		
			return multiplayerList[multiplayerNumber];
		}
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

	//Returns the level run start speed adjusted according to the difficulty level of the game.
	//The RunStartSpeed is higher in Heroic mode than in Normal mode for example.
	public float getRunStartSpeed()
	{
		float adjustedRunStartSpeed = LevelManager.Instance.getCurrentEpisodeInfo().RunStartSpeed;
		switch (PlayerStatsManager.Instance.getDifficultyLevel())
		{
			case DifficultyLevel.Normal:
			adjustedRunStartSpeed = adjustedRunStartSpeed; //Base value is Normal, so no multiplier
			break;
				
			case DifficultyLevel.Heroic:
			adjustedRunStartSpeed = adjustedRunStartSpeed * 1.15f;
			break;
				
			case DifficultyLevel.Legendary:
			adjustedRunStartSpeed = adjustedRunStartSpeed * 1.3f;
			break;
			
		}
		return adjustedRunStartSpeed;
		//return adjustedRunStartSpeed * Random.Range( 0.95f, 1f ); //Hack for testing
	}

	//Returns the level run acceleration adjusted according to the difficulty level of the game.
	//The RunAcceleration is higher in Heroic mode than in Normal mode for example.
	public float getRunAcceleration()
	{
		float adjustedRunAcceleration = LevelManager.Instance.getCurrentEpisodeInfo().RunAcceleration;
		switch (PlayerStatsManager.Instance.getDifficultyLevel())
		{

		case DifficultyLevel.Normal:
			adjustedRunAcceleration = adjustedRunAcceleration; //Base value is Normal, so no multiplier
			break;
			
		case DifficultyLevel.Heroic:
			adjustedRunAcceleration = adjustedRunAcceleration * 1.15f;
			break;
			
		case DifficultyLevel.Legendary:
			adjustedRunAcceleration = adjustedRunAcceleration * 1.3f;
			break;
			
		}
		return adjustedRunAcceleration;
	}

	//Returns the turn speed multiplier. To make turning easier, we slow down the player.
	//In Normal mode, the player is slowed a lot, in Heroic mode, a bit less, and not at all in Legendary mode.
	public float getRunSpeedTurnMultiplier()
	{
		switch (PlayerStatsManager.Instance.getDifficultyLevel())
		{
			case DifficultyLevel.Normal:
			return 0.9f;
				
			case DifficultyLevel.Heroic:
			return 0.95f;
				
			case DifficultyLevel.Legendary:
			return 1f;
			
			default:
			return 0.9f;
		}
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

	[System.Serializable]
	public class CircuitInfo
	{
		[Header("Race Circuit Parameters")]
		[Tooltip("The text ID of the race track.")]
		public string circuitTextID = "CIRCUIT_XXX";
		[Tooltip("The name to use for matchmaking. It must NOT have any underscore characters '_' because this cause Unity matchmaking to return no matches.")]
		public string matchName = string.Empty;
		[Tooltip("Bigger, rectangular image used at the top of the carousel.")]
		public Sprite circuitImage;
		[Tooltip("Square icon used at the beginning of the match.")]
		public Sprite circuitIcon;
		[Tooltip("Entry Fee")]
		public int entryFee = 0;		
	}

}
