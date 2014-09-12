﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SunType 
{
	Morning = 0,
	Noon = 1,
	Afternoon = 2,
	Sunset = 3,
	Night = 4,
	Hell = 5,
	Overcast = 6,
	Cemetery = 7
}

public class LevelData : MonoBehaviour {
	
	public string FinalDestinationName = "KING_CASTLE";
	public List<LevelInfo> levelList = new List<LevelInfo>();

	public List<LevelInfo> getLevelList()
	{
		return levelList;
	}

	public LevelInfo getLevelInfo( int levelNumber )
	{
		if( levelNumber < 0 || levelNumber >= levelList.Count )
		{
			Debug.LogError("LevelData-getLeveInfo: level number specified, " + levelNumber + ", is out of range." );
			return null;
		}
		else
		{		
			return levelList[levelNumber];
		}
	}
	
	public int getNumberOfLevels()
	{
		return levelList.Count;
	}
	
	//This method sets the skybox material as well as the light intensity and direction 
	//for the current level. The skybox materials should be located under the Skybox directory under Resources.
	public void setSunParameters( SunType sunType )
	{
		//This should be the directional light in the scene
		GameObject Sun = GameObject.FindGameObjectWithTag("Sunlight");
		if( Sun != null )
		{
			Material skyBoxMaterial;
			float lightIntensity;
			float shadowStrength;
			Quaternion sunDirection;
			string skyBoxName;
			RenderSettings.ambientLight = new Color(0,0,0);

			switch (sunType)
			{
			case SunType.Morning:
				skyBoxName = "CartoonSkybox";
				lightIntensity = 0.54f;
				shadowStrength = 0.3f;
				sunDirection = Quaternion.Euler( 78f,75f,4f );
				break;
				
			case SunType.Noon:
				skyBoxName = "CartoonSkybox";
				lightIntensity = 0.58f;
				shadowStrength = 0.5f;
				sunDirection = Quaternion.Euler( 78f,75f,4f );
				break;
				
			case SunType.Afternoon:
				skyBoxName = "CartoonSkybox";
				lightIntensity = 0.54f;
				shadowStrength = 0.4f;
				sunDirection = Quaternion.Euler( 78f,75f,4f );
				break;
				
			case SunType.Sunset:
				skyBoxName = "Skybox_sunset";
				lightIntensity = 0.44f;
				shadowStrength = 0.4f;
				sunDirection = Quaternion.Euler( 32f,13f,-63f );
				break;
				
			case SunType.Night:
				skyBoxName = "Skybox_Cartoon_Night";
				lightIntensity = 0.34f;
				shadowStrength = 0f;
				Sun.light.shadows = LightShadows.None;
				sunDirection = Quaternion.Euler( 32f,13f,-63f );
				break;

			case SunType.Overcast:
				skyBoxName = "Overcast2 Skybox";
				lightIntensity = 1.2f;
				shadowStrength = 0f;
				Sun.light.shadows = LightShadows.None;
				sunDirection = Quaternion.Euler( 38.28f,119.5f,87.52f );
				Sun.light.color = new Color(0.623f,0.729f,0.882f); //bluish
				break;

			case SunType.Hell:
				skyBoxName = "Skybox Hell";
				lightIntensity = 0.34f;
				shadowStrength = 0f;
				Sun.light.shadows = LightShadows.None;
				sunDirection = Quaternion.Euler( 32f,13f,-63f );
				RenderSettings.ambientLight = new Color(0.13f,0.21f,0.3f);
				break;

			case SunType.Cemetery:
				skyBoxName = "Skybox Cemetery";
				lightIntensity = 0.28f;
				shadowStrength = 0f;
				Sun.light.shadows = LightShadows.None;
				sunDirection = Quaternion.Euler( 32f,13f,-63f );
				Sun.light.color = new Color(0.623f,0.729f,0.882f); //bluish
				RenderSettings.ambientLight = new Color(0.13f,0.21f,0.3f);
				break;

			default:
				skyBoxName = "CartoonSkybox";
				lightIntensity = 0.54f;
				shadowStrength = 0.62f;
				sunDirection = Quaternion.Euler( 78f,75f,4f );
				break;
			}
			skyBoxMaterial = Resources.Load( "Skybox/" + skyBoxName ) as Material;
			if( skyBoxMaterial != null )
			{
				RenderSettings.skybox = skyBoxMaterial;
				GameObject cutSceneCamera = GameObject.Find("CutsceneCamera");
				Skybox skyBox = (Skybox) cutSceneCamera.GetComponent("Skybox");
				skyBox.material = skyBoxMaterial;
				//A value of 1f means sky is almost completely white, a value of 0 means the sky is darker.
				//Not used for now.
				//skyBoxMaterial.SetColor( "_Tint", new Color( 118f/255f, 118f/255f, 118f/255f, 1f ) );

			}
			else
			{
				Debug.LogError("LevelData-setSunParameters : Unable to find the appropriate skybox: " + skyBoxName + " in the Skybox folder of the Resources folder." );
			}
			
			Sun.light.intensity = lightIntensity;
			Sun.transform.rotation = sunDirection;
			Sun.light.shadowStrength = shadowStrength;
			Debug.Log("LevelData-setSunParameters : parameters: " + Sun.name + " " + Sun.light.intensity + " " +  Sun.light.shadowStrength );

		}
		else
		{
			Debug.LogError("LevelData-setSunParameters : The level scene must contain a directional light called 'Sunlight'." );
		}

	}

	[System.Serializable]
	public class LevelInfo
	{
		[Header("Level Parameters")]
		[Tooltip("Text ID of level name. The name appears on road signs and in the pause menu.")]
		public string LevelName = "Level Name Text ID";
		[Tooltip("Text ID of level description. The description appears in the pause menu.")]
		public string LevelDescription = "LEVEL_DESCRIPTION_0";
		[Tooltip("Specifies whether the level should allow Tap To Play right away or wait for a callback. For example, in the opening level, we want to wait a few seconds before the player can start playing.")]
		public bool waitForTapToPlay = false;
		[Tooltip("Indicates whether or not this level is a tutorial. Tutorial levels are generated by code and do not use road segments.")]
		public bool isTutorial = false;
		[Tooltip("The x and y coordinates where the level icon will be displayed on the world map.")]
		public Vector2 MapCoordinates;
		[Tooltip("Deprecated")]
		public bool isSectionEnd = false;
		[Tooltip("Deprecated")]
		public Vector2 sectionIconMapCoordinates;
		[Tooltip("Deprecated")]
		public string sectionName = "";
		[Tooltip("The ambience sound for the level. It plays in addition to the music. It is optional.")]
		public AudioClip AmbienceSound;
		[Tooltip("Whether or not to include a surrounding plane. The plane can represent an ocean for example.")]
		public bool includeSurroundingPlane = true;
		[Tooltip("Which material to use for the surrounding plane.")]
		public Material surroundingPlaneMaterial;
		[Tooltip("The type of sun for the level. The sun type will not change until the player starts a new section. The sun type determines characteristics such ambient light, directional light rotation, color, intensity and shadows and skybox material.")]
		public SunType sunType = SunType.Afternoon;
		[Tooltip("Whether or not the level has fog.")]
		public bool hasFog = false;
		[Tooltip("The fog tint.")]
		public Color fogTint = new Color( 96,91,91, 255 );
		[Tooltip("The fog opacity.")]
		[Range(0, 1f)]
		public float fogFade = 0.128f;
		[Tooltip("The fog follows the player. The fogHeightDelta value allows you to adjust the fog height versus the player's height. if the delta is set to 30, the fog center will be 30 meters above the player.")]
		public float fogHeightDelta = 30f;
		[Tooltip("The player's initial run speed in m/sec.")]
		public float RunStartSpeed = 10f;
		[Tooltip("How fast will the player accelerate.")]
		public float RunAcceleration = 0.09f;
		[Tooltip("The number of tiles between power ups. Zero means there is no power up in that level.")]
		public int powerUpDensity = 4;
		[Tooltip("The type of tile that the player will start on.")]
		public TileType startTile = TileType.Start;
		[Tooltip("The length in meters of the level. This value is calculated when the level is created.")]
		public float lengthInMeters = 0;
		[Header("Road Segments")]
		public List<RoadSegment> roadSegmentList = new List<RoadSegment>();

		//Returns the level run start speed adjusted according to the difficulty level of the game.
		//The RunStartSpeed is higher in Heroic mode than in Normal mode for example.
		public float getRunStartSpeed()
		{
			float adjustedRunStartSpeed = RunStartSpeed;
			switch (PlayerStatsManager.Instance.getDifficultyLevel())
			{
				case DifficultyLevel.Normal:
				adjustedRunStartSpeed = RunStartSpeed; //Base value is Normal, so no multiplier
				break;
					
				case DifficultyLevel.Heroic:
				adjustedRunStartSpeed = RunStartSpeed * 1.2f;
				break;
					
				case DifficultyLevel.Legendary:
				adjustedRunStartSpeed = RunStartSpeed * 1.4f;
				break;
				
			}
			return adjustedRunStartSpeed;
		}

		//Returns the level run acceleration adjusted according to the difficulty level of the game.
		//The RunAcceleration is higher in Heroic mode than in Normal mode for example.
		public float getRunAcceleration()
		{
			float adjustedRunAcceleration = RunAcceleration;
			switch (PlayerStatsManager.Instance.getDifficultyLevel())
			{

			case DifficultyLevel.Normal:
				adjustedRunAcceleration = RunAcceleration; //Base value is Normal, so no multiplier
				break;
				
			case DifficultyLevel.Heroic:
				adjustedRunAcceleration = RunAcceleration * 1.2f;
				break;
				
			case DifficultyLevel.Legendary:
				adjustedRunAcceleration = RunAcceleration * 1.4f;
				break;
				
			}
			return adjustedRunAcceleration;
		}

	}
	
	[System.Serializable]
	public class RoadSegment
	{
		public SegmentTheme theme;
		public int NumberOfTiles;
		public TileType endTile = TileType.End;
	}

}
