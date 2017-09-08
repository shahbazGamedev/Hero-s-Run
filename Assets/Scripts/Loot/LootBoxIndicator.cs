using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LootBoxIndicator : MonoBehaviour {

	[SerializeField] TextMeshProUGUI lootBoxOwnedText;
	const float ANIMATION_DURATION = 1.2f;
	string lootBoxOwned = "{0}";

	// Use this for initialization
	void Start ()
	{

		int currentLootBoxOwned = GameManager.Instance.playerInventory.getNumberOfLootBoxesOwned();

		if( currentLootBoxOwned > 0 )
		{

			//Player has loot boxes. Show the indicator.
			lootBoxOwnedText.transform.parent.gameObject.SetActive( true );

			//Verify if the number of loot boxes has changed since we were last in the main menu.
			int previousLootBoxOwned = GameManager.Instance.playerInventory.getLastDisplayedLootBoxesOwned();
	
			if( previousLootBoxOwned != currentLootBoxOwned )
			{
				//The number of loot boxes has changed. Let's animate.
				lootBoxOwnedText.GetComponent<UISpinNumber>().spinNumber( lootBoxOwned, previousLootBoxOwned, currentLootBoxOwned, ANIMATION_DURATION, true, OnIncrement );
				GameManager.Instance.playerInventory.saveLastDisplayedLootBoxesOwned( currentLootBoxOwned );
			}
			else
			{
				//The value has not changed. Set the values directly.
				lootBoxOwnedText.text = currentLootBoxOwned.ToString();
			}
		}
		else
		{
			//Player has no loot boxes. Hide the indicator.
			lootBoxOwnedText.transform.parent.gameObject.SetActive( false );
		}
	}

	void OnIncrement( int value )
	{
		//Maybe play a VFX each time the number increases
	}
}
