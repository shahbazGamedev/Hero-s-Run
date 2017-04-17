using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardUIDetails : MonoBehaviour {

	[Header("General")]
	[Tooltip("The card image.")]
	[SerializeField] Image cardImage;
	[Tooltip("The mana needed to use the card.")]
	[SerializeField] Text manaCost;

	[Header("Level")]
	[Tooltip("The level background is displayed on top of the card. The color of the background varies with the card rarity.")]
	[SerializeField] Image levelBackground;
	[Tooltip("The level text is displayed on top of the level background. For example: 'Level 5' or 'Max Level'.")]
	[SerializeField] Text levelText;

	[Header("Progress Bar")]
	[Tooltip("The progress bar is displayed below the card. The color of the background varies depending on whether the card is: not ready to be upgraded, ready to to be upgraded or maxed out.")]
	[SerializeField] Image progressBarBackground;
	[Tooltip("The indicator varies depending on whether the card is not ready to be upgraded, ready to to be upgraded or maxed out. If it is not ready to be upgraded, it is an arrow. If it is ready to be upgraded, it is an arrow bouncing up and down. If it is maxed out, it displays the sprite specified by progressBarMaxLevelIndicator.")]
	[SerializeField] Image progressBarIndicator;	
	[Tooltip("The sprite to use when the card is Maxed Out.")]
	[SerializeField] Sprite progressBarMaxLevelIndicator;
	[Tooltip("The slider used to show the progress before the card can be upgraded.")]
	[SerializeField] Slider progressBarSlider;
	[Tooltip("The text displayed on top of the progress bar. If the player has 23 cards and needs 50 to upgrade, it will display '23/50'.")]
	[SerializeField] Text progressBarText;

	Color NOT_ENOUGH_CARDS_TO_UPGRADE = Color.blue;
	Color ENOUGH_CARDS_TO_UPGRADE = Color.green;
	Color MAXED_OUT = Color.red;

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
		cd.rectTransform = GetComponent<RectTransform>();

		//Card image and mana cost
		cardImage.sprite = cd.icon;
		manaCost.text = cd.manaCost.ToString();

		//Level section
		//Level background
		Color color;
		ColorUtility.TryParseHtmlString (CardManager.Instance.getCardColorHexValue(cd.rarity), out color);
		levelBackground.color = color;

		//Level text and numberOfCardsForUpgrade
		int numberOfCardsForUpgrade;
		if( pcd.level + 1 < CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ) )
		{
			numberOfCardsForUpgrade = CardManager.Instance.getNumberOfCardsRequiredForUpgrade( pcd.level + 1, cd.rarity );
			levelText.text = pcd.level.ToString();
		}
		else
		{
			numberOfCardsForUpgrade = CardManager.Instance.getNumberOfCardsRequiredForUpgrade( CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ), cd.rarity );
			levelText.text = "Max Level";
		}

		//Progress bar section
		if( pcd.level + 1 < CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ) )
		{
			//Do I have enough cards to level up the card?
			if( pcd.quantity >= numberOfCardsForUpgrade )
			{
				progressBarBackground.color = ENOUGH_CARDS_TO_UPGRADE;
				progressBarIndicator.color = ENOUGH_CARDS_TO_UPGRADE;
			}
			else
			{
				progressBarBackground.color = NOT_ENOUGH_CARDS_TO_UPGRADE;
				progressBarIndicator.color = NOT_ENOUGH_CARDS_TO_UPGRADE;
			}
		}
		else
		{
			progressBarBackground.color = MAXED_OUT;
			progressBarIndicator.color = Color.white;
			progressBarIndicator.overrideSprite = progressBarMaxLevelIndicator;
		}
		progressBarSlider.value = pcd.quantity/(float)numberOfCardsForUpgrade;
		progressBarText.text = pcd.quantity.ToString() + "/" + numberOfCardsForUpgrade.ToString();
	}
	
}
