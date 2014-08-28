using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PauseMenu : MonoBehaviour {

	Vector2 popupSize = new Vector2( Screen.width * 0.9f, Screen.height * 0.85f );
	Rect popupRect;
	public GUIStyle descriptionStyle;
	public GUIStyle selectorStyle;
	public GUIStyle previousButtonStyle;
	public GUIStyle nextButtonStyle;
	Vector2 areaSize;
	Rect areaRect;

	PopupHandler popupHandler;
	GUIContent levelDescriptionContent;
	GUIContent buttonText = new GUIContent( System.String.Empty );
	
	HUDHandler hudHandler;

	float margin;
	Vector2 buttonSize = new Vector2( Screen.width * 0.16f, Screen.width * 0.16f );
	Vector2 powerUpSize = new Vector2( Screen.width * 0.22f, Screen.width * 0.22f );
	float buttonMargin;

	// Use this for initialization
	void Awake () {
		hudHandler = GetComponent<HUDHandler>();

		Transform CoreManagers = GameObject.FindGameObjectWithTag("CoreManagers").transform;
		popupHandler = CoreManagers.GetComponent<PopupHandler>();
		popupRect = new Rect( (Screen.width - popupSize.x) * 0.5f, (Screen.height - popupSize.y) * 0.5f , popupSize.x, popupSize.y );


		areaSize = new Vector2( popupSize.x * 0.8f, popupSize.y * 0.9f );
		margin = (popupSize.x - areaSize.x) * 0.5f;
		areaRect = new Rect( margin, popupSize.y * 0.18f, areaSize.x, areaSize.y );
		buttonMargin = (popupSize.x - (2 * buttonSize.x) - powerUpSize.x - (2 * margin) ) * 0.5f;

		//Adjust font sizes based on screen resolution
		PopupHandler.changeFontSizeBasedOnResolution( descriptionStyle );
		PopupHandler.changeFontSizeBasedOnResolution( selectorStyle );

	}
	
	//This function is called by WorldMapHandler when the player has selected a level.
	public void setLevelToLoad( int levelToLoad )
	{
		//Now that we know what level to load, we can retrieve the level description text.
		levelDescriptionContent = new GUIContent( LevelManager.Instance.getLevelDescription(levelToLoad) + "\n" );
	}
	
	public void renderPauseMenu()
	{
		PowerUpType powerUpSelected = PlayerStatsManager.Instance.getPowerUpSelected();
		popupHandler.setPopupRect( popupRect );
		popupHandler.drawCloseButton( closeLevelMenu );
		GUILayout.BeginArea( areaRect );
		GUILayout.BeginVertical();
		GUILayout.Label( levelDescriptionContent, descriptionStyle );
		GUILayout.EndVertical();
		GUILayout.EndArea();
		float height = popupSize.y * 0.46f;
		//Left arrow
		if( GUI.Button( new Rect(margin,height,buttonSize.x,buttonSize.y), System.String.Empty, previousButtonStyle) && !LevelManager.Instance.isTutorialActive() )
		{
			PowerUpType newPowerUp = PowerUpDisplayData.getPreviousPowerUp();
			PlayerStatsManager.Instance.setPowerUpSelected( newPowerUp );
		}
		//Power-up icon
		GUI.DrawTextureWithTexCoords(new Rect(margin+buttonSize.x+buttonMargin,height - (powerUpSize.y-buttonSize.y) * 0.5f,powerUpSize.x,powerUpSize.y), PowerUpDisplayData.getPowerUpImage(), PowerUpDisplayData.getPowerUpTexCoord(powerUpSelected) );
		//Power-up quantity
		GUI.Label( new Rect(margin+buttonSize.x+buttonMargin+powerUpSize.x,height+buttonSize.y-40,50,50), PlayerStatsManager.Instance.getPowerUpQuantity( powerUpSelected).ToString(), selectorStyle );
		//Right arrow
		if( GUI.Button( new Rect(margin+buttonSize.x+2*buttonMargin+powerUpSize.x,height,buttonSize.x,buttonSize.y), System.String.Empty, nextButtonStyle) && !LevelManager.Instance.isTutorialActive() )
		{
			PowerUpType newPowerUp = PowerUpDisplayData.getNextPowerUp();
			PlayerStatsManager.Instance.setPowerUpSelected( newPowerUp );
		}
		//Power-up name
		GUI.Label( new Rect(margin+buttonSize.x+buttonMargin,popupSize.y * 0.63f,powerUpSize.x,50), PowerUpDisplayData.getPowerUpName(powerUpSelected), selectorStyle );

		//Power-up how to use
		int originalSize = selectorStyle.fontSize;
		selectorStyle.fontSize = (int)(selectorStyle.fontSize * 0.65f);
		GUI.Label( new Rect(margin+buttonSize.x+buttonMargin,popupSize.y * 0.67f,powerUpSize.x,50), LocalizationManager.Instance.getText("POWER_UP_HOW_TO"), selectorStyle );
		selectorStyle.fontSize = originalSize;

		Rect areaRect3 = new Rect( (popupSize.x - areaSize.x)/2, popupSize.y * 0.75f, areaSize.x, areaSize.y );
		GUILayout.BeginArea( areaRect3 );
		GUILayout.BeginVertical();
		buttonText.text = LocalizationManager.Instance.getText("MENU_RESUME");
		popupHandler.drawButton( buttonText, resumeGame );
		buttonText.text = LocalizationManager.Instance.getText("MENU_HOME");
		popupHandler.drawButton( buttonText, goToHomeMenu );
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	public void closeLevelMenu()
	{
		popupHandler.closePopup();
		resumeGame();
	}
	
	public void resumeGame()
	{
		hudHandler.pauseGame();
	}
	
	public void goToHomeMenu()
	{
		hudHandler.goToHomeMenu();
	}

}
