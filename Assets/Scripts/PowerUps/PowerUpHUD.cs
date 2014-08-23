using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PowerUpHUD : MonoBehaviour {

	public GUIStyle powerUpStyle;
	Vector2 powerupButtonSize = new Vector2( 0.09f * Screen.width, 0.09f * Screen.width);
	float margin = Screen.width * 0.04f;
	Hashtable slideOptions = new Hashtable();
	Vector2 slideStartDest;
	Vector2 slideEndDest;
	float slideDuration = 0.6f;
	Rect iconRect;
	Rect inventoryRect;
	Dictionary<PowerUpType, PowerUpDisplay> powerUpDisplayList = new Dictionary<PowerUpType, PowerUpDisplay>(4);

	void Awake () {
		iconRect = new Rect( 0,0,powerupButtonSize.x, powerupButtonSize.y);
		inventoryRect = new Rect(  powerupButtonSize.x * 0.85f,0, powerupButtonSize.x * 0.8f, powerupButtonSize.y);
		slideOptions.Clear();
		slideOptions.Add("ease", LeanTweenType.easeOutQuad);
	}
	
	public void activateDisplay( PowerUpManager.PowerUpData pud )
	{
		PowerUpDisplay displayData;
		//Reset rect position
		if( powerUpDisplayList.ContainsKey(pud.powerUpType) )
		{
			//Update it
			displayData = powerUpDisplayList[pud.powerUpType];
			displayData.groupRect = new LTRect( -powerupButtonSize.x, getHeight(displayData.index), powerupButtonSize.x * 2f, powerupButtonSize.y );
		}
		else
		{
			//Create it
			displayData = new PowerUpDisplay();
			displayData.index = powerUpDisplayList.Count;
			displayData.groupRect = new LTRect( -powerupButtonSize.x, getHeight(displayData.index), powerupButtonSize.x * 2f, powerupButtonSize.y );
			powerUpDisplayList.Add (pud.powerUpType, displayData );
		}
		slideStartDest = new Vector2( margin, getHeight(displayData.index) );
		LeanTween.move( displayData.groupRect, slideStartDest, slideDuration, slideOptions );
	}

	//At what height should the icon be displayed
	float getHeight( int index )
	{
		return (0.2f + index * 0.08f ) * Screen.height;
	}

	void OnGUI ()
	{
		foreach(KeyValuePair<PowerUpType, PowerUpDisplay> pair in powerUpDisplayList) 
		{
			displayPowerUp( pair.Key, pair.Value);
		}
	}

	void displayPowerUp( PowerUpType type, PowerUpDisplay displayData )
	{
		GUI.BeginGroup(displayData.groupRect.rect);
		GUI.DrawTextureWithTexCoords(iconRect, PowerUpDisplayData.getPowerUpImage(), PowerUpDisplayData.getPowerUpTexCoord(type) );
		if( type == PlayerStatsManager.Instance.getPowerUpSelected() )
		{
			GUI.Label( inventoryRect, PlayerStatsManager.Instance.getPowerUpQuantity(type).ToString(), powerUpStyle );
		}
		GUI.EndGroup();
	}
	
	public void slideDisplayOut( PowerUpType type )
	{
		//Carefull: when using LeanTween on the same gameObject (like the player) not to use the same function
		//name in different classes as you may end up having an onComplete event in one class calling
		//the function in the other class.
		if( powerUpDisplayList.ContainsKey( type) )
		{
			PowerUpDisplay displayData = powerUpDisplayList[type];
			slideEndDest = new Vector2( -powerupButtonSize.x-inventoryRect.width, getHeight(displayData.index) );
			LeanTween.move( displayData.groupRect, slideEndDest, slideDuration, slideOptions );
		}
	}

	public void hideImmediately( PowerUpType type )
	{
		if( powerUpDisplayList.ContainsKey( type) )
		{
			powerUpDisplayList.Remove(type);
		}
	}

	public void hideImmediately()
	{
		powerUpDisplayList.Clear();
	}
	
	public class PowerUpDisplay
	{
		public Texture2D icon;
		public LTRect groupRect;
		public int index = 0;
	}

}
