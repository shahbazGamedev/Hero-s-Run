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

	// Use this for initialization
	void Start ()
	{
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
