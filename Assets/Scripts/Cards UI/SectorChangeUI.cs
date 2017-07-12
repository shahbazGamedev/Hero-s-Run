using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SectorChangeUI : MonoBehaviour {

	[Header("Sector Changed")]
	[SerializeField] GameObject closeButton;
	[SerializeField] TextMeshProUGUI sectorNumberText;
	[SerializeField] TextMeshProUGUI sectorNameText;
	[SerializeField] TextMeshProUGUI trophiesNeededText;
	[SerializeField] Image sectorImage;
	[SerializeField] Image sectorBackground;

	// Use this for initialization
	void Start ()
	{
		//Determine the player's current sector.
		LevelData.MultiplayerInfo multiplayerInfo = LevelManager.Instance.getLevelData().getRaceTrackByTrophies();

		//Now that we know the sector, initialize the various UI elements.
 		sectorNumberText.text = string.Format( LocalizationManager.Instance.getText( "SECTOR_NUMBER" ), multiplayerInfo.circuitInfo.sectorNumber );
		string sectorName = LocalizationManager.Instance.getText( "SECTOR_" + multiplayerInfo.circuitInfo.sectorNumber.ToString() );
		sectorNameText.text = sectorName;
		trophiesNeededText.text = multiplayerInfo.trophiesNeededToUnlock.ToString() + "+";
		sectorImage.sprite = multiplayerInfo.circuitInfo.circuitImage;
		sectorBackground.color = multiplayerInfo.circuitInfo.backgroundColor;
	}

	void OnEnable()
	{
		CardUnlockedUI.cardUnlockedUIDisplayed += CardUnlockedUIDisplayed;
	}

	void OnDisable()
	{
		CardUnlockedUI.cardUnlockedUIDisplayed -= CardUnlockedUIDisplayed;
	}

	void CardUnlockedUIDisplayed( bool displayed )
	{
		//The card unlocked UI popup looks better if the close button and sector info is hidden. It feels less cluttered.
		closeButton.SetActive( !displayed );
		sectorNumberText.gameObject.SetActive( !displayed );
	}

	public void OnClickHide()
	{
		gameObject.SetActive( false );
	}
}
