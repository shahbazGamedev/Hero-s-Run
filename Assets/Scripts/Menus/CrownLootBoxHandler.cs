using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrownLootBoxHandler : MonoBehaviour {

	[SerializeField] TextMeshProUGUI crownsOwned;
	[SerializeField] TextMeshProUGUI notEnoughCrownsText;
	[SerializeField] Slider crownsOwnedProgressBar;
	public const int CROWNS_NEEDED_TO_OPEN = 10;
	const float ANIMATION_DURATION = 1.6f;

	// Use this for initialization
	void Start ()
	{
		string crownLootBoxOwned = "{0}/" + CROWNS_NEEDED_TO_OPEN.ToString();
		int previousCrowns = GameManager.Instance.playerInventory.getLastDisplayedCrownBalance();
		int currentCrowns = GameManager.Instance.playerInventory.getCrownBalance();

		if( GameManager.Instance.playerInventory.getLastDisplayedCrownBalance() != GameManager.Instance.playerInventory.getCrownBalance() )
		{
			//The number of crowns has changed. Let's animate.
			crownsOwned.GetComponent<UISpinNumber>().spinNumber( crownLootBoxOwned, previousCrowns, currentCrowns, ANIMATION_DURATION, true );
			crownsOwnedProgressBar.value = GameManager.Instance.playerInventory.getLastDisplayedCrownBalance()/(float)CROWNS_NEEDED_TO_OPEN;
			crownsOwnedProgressBar.GetComponent<UIAnimateSlider>().animateSlider( (float)currentCrowns/CROWNS_NEEDED_TO_OPEN, ANIMATION_DURATION );
			GameManager.Instance.playerInventory.saveLastDisplayedCrownBalance( currentCrowns );
		}
		else
		{
			//The value has not changed. Set the values directly.
			crownsOwned.text = string.Format( crownLootBoxOwned, GameManager.Instance.playerInventory.getCrownBalance(), CROWNS_NEEDED_TO_OPEN );
			crownsOwnedProgressBar.value = GameManager.Instance.playerInventory.getCrownBalance()/(float)CROWNS_NEEDED_TO_OPEN;
		}
	}

	public void OnClickOpenCrownLootBox()
	{
		if( GameManager.Instance.playerInventory.getCrownBalance() == CROWNS_NEEDED_TO_OPEN )
		{
			//Player has enough crowns. Open loot box.


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
