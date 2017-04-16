using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardUIDetails : MonoBehaviour {

	public Image cardImage;
	public Text manaCost;

	public Image levelBackground;
	public Text levelText;

	public Image progressBarBackground;
	public Image progressBarArrow;
	public Slider progressBarSlider;
	public Text progressBarText;

	Color notEnoughCards = Color.blue;
	Color enoughCards = Color.green;

	// Use this for initialization
	public void configureCard (PlayerDeck.PlayerCardData pcd, CardManager.CardData cd)
	{
		cd.rectTransform = GetComponent<RectTransform>();

		cardImage.sprite = cd.icon;
		manaCost.text = cd.manaCost.ToString();
		Color color;
		ColorUtility.TryParseHtmlString (CardManager.Instance.getCardColorHexValue(cd.rarity), out color);
		levelBackground.color = color;
		levelText.text = pcd.level.ToString();

		int numberOfCardsForUpgrade = CardManager.Instance.getNumberOfCardsRequiredForUpgrade( pcd.level, cd.rarity );

		//Do I have enough cards to level up the card?
		if( pcd.quantity >= numberOfCardsForUpgrade )
		{
			progressBarBackground.color = enoughCards;
			progressBarArrow.color = enoughCards;
		}
		else
		{
			progressBarBackground.color = notEnoughCards;
			progressBarArrow.color = notEnoughCards;
		}
		progressBarSlider.value = pcd.quantity/(float)numberOfCardsForUpgrade;
		progressBarText.text = pcd.quantity.ToString() + "/" + numberOfCardsForUpgrade.ToString();
	}
	
}
