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
	[SerializeField] GameObject newSectorRibbon;

	// Use this for initialization
	void Start ()
	{
		int currentSector = GameManager.Instance.playerProfile.getCurrentSector();
		//Now that we know the sector, initialize the various UI elements.
 		sectorNumberText.text = string.Format( LocalizationManager.Instance.getText( "SECTOR_NUMBER" ), currentSector );
		string sectorName = LocalizationManager.Instance.getText( "SECTOR_" + currentSector.ToString() );
		sectorNameText.text = sectorName;
		trophiesNeededText.text = SectorManager.Instance.getTrophiesRequired( currentSector ).ToString() + "+";
		sectorImage.sprite = SectorManager.Instance.getSectorImage( currentSector );
		sectorBackground.color = SectorManager.Instance.getSectorColor( currentSector );;
		newSectorRibbon.SetActive( PlayerRaceManager.Instance.sectorStatus == SectorStatus.WENT_UP_AND_NEW );
		//Reset value
		PlayerRaceManager.Instance.sectorStatus = SectorStatus.NO_CHANGE;
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
