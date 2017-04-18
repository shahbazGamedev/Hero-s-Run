using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailPopup : MonoBehaviour {
	
	[Tooltip("TBC.")]
	[SerializeField] Text topPanelText;
	[SerializeField] GameObject card;
	[Header("Rarity")]
	[SerializeField] Image rarityIcon;
	[SerializeField] Text rarityText;
	[Header("Description")]
	[SerializeField] Text descriptionText;
	[Header("Upgrade Button")]
	[SerializeField] Button upgradeButton;
	[SerializeField] Text upgradeButtonText;
	[SerializeField] Text upgradeCostText;

	public void configureCard( PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		//Title
		string localizedCardName = LocalizationManager.Instance.getText( "CARD_NAME_" + pcd.name.ToString().ToUpper() );
		topPanelText.text = string.Format("Level {0} {1}", pcd.level, localizedCardName );
		//Card
		card.GetComponent<CardUIDetails>().configureCard( pcd, cd );
		//Description
		string localizedCardDescription = LocalizationManager.Instance.getText( "CARD_DESCRIPTION_" + pcd.name.ToString().ToUpper() );
		descriptionText.text = localizedCardDescription;
		//Rarity
		Color rarityColor;
		ColorUtility.TryParseHtmlString (CardManager.Instance.getCardColorHexValue(cd.rarity), out rarityColor);
		rarityIcon.color = rarityColor;
		rarityText.text = cd.rarity.ToString();
		//Upgrade button
		configureUpgradeButton( pcd, cd );
	}

	/// <summary>
	/// Configures the upgrade button. To allow upgrading we need to validate these conditions:
	/// a) The card must not be maxed out
	/// b) The player has enough coin to pay.
	/// c) The player has enough cards for the upgrade.
	/// </summary>
	/// <param name="pcd"Player card data.</param>
	/// <param name="cd">Card data.</param>
	void configureUpgradeButton( PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		upgradeButtonText.text = "Upgrade";
		int numberOfCardsForUpgrade;
		int upgradeCost;
		if( pcd.level + 1 < CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ) )
		{
			numberOfCardsForUpgrade = CardManager.Instance.getNumberOfCardsRequiredForUpgrade( pcd.level + 1, cd.rarity );
		 	upgradeCost= CardManager.Instance.getCoinsRequiredForUpgrade( pcd.level + 1, cd.rarity );
		}
		else
		{
			numberOfCardsForUpgrade = CardManager.Instance.getNumberOfCardsRequiredForUpgrade( CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ), cd.rarity );
		 	upgradeCost= CardManager.Instance.getCoinsRequiredForUpgrade(CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ), cd.rarity );
		}

		if( pcd.level + 1 < CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ) )
		{
			//Do I have enough cards to level up the card?
			if( pcd.quantity >= numberOfCardsForUpgrade )
			{
				//Does the player have enough coins?
				if( upgradeCost <= PlayerStatsManager.Instance.getCurrentCoins() )
				{
					//The player has enough coins.
					upgradeButton.interactable = true;
					upgradeCostText.color = upgradeButton.colors.normalColor;
				}
				else
				{
					//The player does not have enough coins.
					upgradeButton.interactable = false;
					upgradeCostText.color = upgradeButton.colors.disabledColor;
				}
			}
			else
			{
				//The player does have enough cards.
				upgradeButton.interactable = false;
				upgradeCostText.color = upgradeButton.colors.disabledColor;
			}
			upgradeCostText.text = string.Format("{0:n0}", upgradeCost );
		}
		else
		{
			//The card is already maxed out.
			upgradeButton.interactable = false;
			upgradeCostText.text = "Maxed Out";
			upgradeCostText.color = upgradeButton.colors.disabledColor;
		}
	}

	public void OnClickUpgrade()
	{
		Debug.Log("Upgrading card.");
	}

	public void OnClickHide()
	{
		gameObject.SetActive( false );
	}

}
