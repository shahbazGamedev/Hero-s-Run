using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CardUIDetails : MonoBehaviour {

	[Header("General")]
	[Tooltip("The card image.")]
	[SerializeField] Image cardImage;
	[Tooltip("The mana needed to use the card.")]
	[SerializeField] Text manaCost;

	[Header("Level")]
	[Tooltip("The level text is displayed on top of the card image. For example: 'Level 5' or 'Max Level'. The text color varies with the card rarity.")]
	[SerializeField] Text levelText;

	[Header("Progress Bar")]
	[Tooltip("The color of the slider fill varies depending on whether the card is: not ready to be upgraded, ready to to be upgraded or maxed out.")]
	[SerializeField] Image progressBarFill;
	[Tooltip("The indicator varies depending on whether the card is not ready to be upgraded, ready to to be upgraded or maxed out. If it is not ready to be upgraded, it is an arrow. If it is ready to be upgraded, it is an arrow bouncing up and down. If it is maxed out, it displays the sprite specified by progressBarMaxLevelIndicator.")]
	[SerializeField] Image progressBarIndicator;	
	[Tooltip("The sprite to use when the card is Maxed Out.")]
	[SerializeField] Sprite progressBarMaxLevelIndicator;
	[Tooltip("The slider used to show the progress before the card can be upgraded.")]
	[SerializeField] Slider progressBarSlider;
	[Tooltip("The text displayed on top of the progress bar. If the player has 23 cards and needs 50 to upgrade, it will display '23/50'.")]
	[SerializeField] Text progressBarText;

	public void configureCard (PlayerDeck.PlayerCardData pcd, CardManager.CardData cd)
	{
		//security Check
		if( pcd.level > CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ) )
		{
			Debug.LogError("CardUIDetails-The level for the card " +  cd.name + " is above the maximum allowed which is " + CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ) + ". Resetting the card's level to the maximum value allowed." );
			pcd.level = CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity );
			GameManager.Instance.playerDeck.serializePlayerDeck( true );
		}

		//We need to save the RectTransform in order to know where to place the card overlay when the card is clicked.
		cd.rectTransform = cardImage.GetComponent<RectTransform>();

		//Card image and mana cost
		cardImage.sprite = cd.icon;

		//Legendary cards have special effects
		cardImage.material = cd.cardMaterial;

		if( manaCost != null ) manaCost.text = cd.manaCost.ToString();

		//Level section
		//Level background
		Color rarityColor;
		ColorUtility.TryParseHtmlString (CardManager.Instance.getCardColorHexValue(cd.rarity), out rarityColor);
		if( levelText != null ) levelText.color = rarityColor;

		//Level text and numberOfCardsForUpgrade
		int numberOfCardsForUpgrade;
		if( pcd.level + 1 <= CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ) )
		{
			numberOfCardsForUpgrade = CardManager.Instance.getNumberOfCardsRequiredForUpgrade( pcd.level + 1, cd.rarity );
			if( levelText != null ) levelText.text = String.Format( LocalizationManager.Instance.getText( "CARD_LEVEL"), pcd.level.ToString() );
		}
		else
		{
			numberOfCardsForUpgrade = CardManager.Instance.getNumberOfCardsRequiredForUpgrade( CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ), cd.rarity );
			if( levelText != null ) levelText.text = LocalizationManager.Instance.getText( "CARD_MAX_LEVEL");
		}

		//Progress bar section
		if( pcd.level + 1 < CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ) )
		{
			//Do I have enough cards to level up the card?
			if( pcd.quantity >= numberOfCardsForUpgrade )
			{
				progressBarFill.color = CardManager.Instance.getCardUpgradeColor( CardUpgradeColor.ENOUGH_CARDS_TO_UPGRADE );
				progressBarIndicator.color = CardManager.Instance.getCardUpgradeColor( CardUpgradeColor.ENOUGH_CARDS_TO_UPGRADE );
			}
			else
			{
				progressBarFill.color = CardManager.Instance.getCardUpgradeColor( CardUpgradeColor.NOT_ENOUGH_CARDS_TO_UPGRADE );
				progressBarIndicator.color = CardManager.Instance.getCardUpgradeColor( CardUpgradeColor.NOT_ENOUGH_CARDS_TO_UPGRADE );
			}
			progressBarIndicator.overrideSprite = null;
		}
		else
		{
			progressBarFill.color = CardManager.Instance.getCardUpgradeColor( CardUpgradeColor.MAXED_OUT );
			progressBarIndicator.color = Color.white;
			progressBarIndicator.overrideSprite = progressBarMaxLevelIndicator;
		}
		progressBarSlider.value = pcd.quantity/(float)numberOfCardsForUpgrade;
		progressBarText.text = pcd.quantity.ToString() + "/" + numberOfCardsForUpgrade.ToString();
	}
	
}
