using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrownLootBoxHandler : MonoBehaviour {

	[SerializeField] TextMeshProUGUI crownLootBoxText;
	[SerializeField] TextMeshProUGUI notEnoughCrownsText;
	[SerializeField] Slider crownsOwnedProgressBar;
	[SerializeField] GameObject toOpenText;
	public const int CROWNS_NEEDED_TO_OPEN = 10;
	const float ANIMATION_DURATION = 1.6f;
	string crownLootBoxOwned;

	// Use this for initialization
	void Start ()
	{
		crownLootBoxOwned = "{0}/" + CROWNS_NEEDED_TO_OPEN.ToString();
		int previousCrowns = GameManager.Instance.playerInventory.getLastDisplayedCrownBalance();
		int currentCrowns = GameManager.Instance.playerInventory.getCrownBalance();

		if( GameManager.Instance.playerInventory.getLastDisplayedCrownBalance() != GameManager.Instance.playerInventory.getCrownBalance() )
		{
			//The number of crowns has changed. Let's animate.
			crownLootBoxText.GetComponent<UISpinNumber>().spinNumber( crownLootBoxOwned, previousCrowns, currentCrowns, ANIMATION_DURATION, true, OnIncrement );
			crownsOwnedProgressBar.value = GameManager.Instance.playerInventory.getLastDisplayedCrownBalance()/(float)CROWNS_NEEDED_TO_OPEN;
			crownsOwnedProgressBar.GetComponent<UIAnimateSlider>().animateSlider( (float)currentCrowns/CROWNS_NEEDED_TO_OPEN, ANIMATION_DURATION );
			GameManager.Instance.playerInventory.saveLastDisplayedCrownBalance( currentCrowns );
		}
		else
		{
			//The value has not changed. Set the values directly.
			if( GameManager.Instance.playerInventory.getCrownBalance() == CROWNS_NEEDED_TO_OPEN )
			{
				//Player has enough crowns.
				setReadyToOpen();
			}
			else
			{
				//Player doesn't have enough crowns.
				crownLootBoxText.text = string.Format( crownLootBoxOwned, GameManager.Instance.playerInventory.getCrownBalance(), CROWNS_NEEDED_TO_OPEN );
				crownsOwnedProgressBar.value = GameManager.Instance.playerInventory.getCrownBalance()/(float)CROWNS_NEEDED_TO_OPEN;
				toOpenText.SetActive( true );
			}
		}
	}

	void OnIncrement( int value )
	{
		if( value == CROWNS_NEEDED_TO_OPEN )
		{
			//The spin number animation completed and we have enough crowns to open.
			Invoke( "setReadyToOpen", 0.5f ); //it looks better with a little delay before changing the text to "Open"
		}
	}

	void setReadyToOpen()
	{
		crownLootBoxText.text = LocalizationManager.Instance.getText("CROWN_LOOT_BOX_OPEN");
		crownLootBoxText.alignment = TextAlignmentOptions.Center;
		crownsOwnedProgressBar.value = 1f;
		toOpenText.SetActive( false );
	}

	public void OnClickOpenCrownLootBox()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		if( GameManager.Instance.playerInventory.getCrownBalance() == CROWNS_NEEDED_TO_OPEN )
		{
			//Player has enough crowns. Open loot box.
			GameManager.Instance.playerInventory.resetCrowns();
			GameManager.Instance.playerInventory.saveLastDisplayedCrownBalance( 0 );
			LootBoxClientManager.Instance.requestLootBox( LootBoxType.CROWN );
			crownLootBoxText.alignment = TextAlignmentOptions.Left;
			crownLootBoxText.text = string.Format( crownLootBoxOwned, GameManager.Instance.playerInventory.getCrownBalance(), CROWNS_NEEDED_TO_OPEN );
			crownsOwnedProgressBar.value = 0;
			toOpenText.SetActive( true );
		}
		else
		{
			CancelInvoke( "hideNotEnoughCrownsText" );
			//Player doesn't have enough crowns. Display a message.
			notEnoughCrownsText.text = string.Format( LocalizationManager.Instance.getText("CROWN_LOOT_BOX_NOT_ENOUGH_CROWNS"), CROWNS_NEEDED_TO_OPEN );
			notEnoughCrownsText.gameObject.SetActive( true );
			Invoke("hideNotEnoughCrownsText", 5f);
		}
	}
	
	void hideNotEnoughCrownsText()
	{
		notEnoughCrownsText.gameObject.SetActive( false );
	}
}
