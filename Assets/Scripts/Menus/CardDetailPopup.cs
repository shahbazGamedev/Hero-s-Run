using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class CardDetailPopup : MonoBehaviour {
	
	[SerializeField] CardCollectionManager cardCollectionManager;
	[SerializeField] ScrollRect horizontalScrollview;
	[SerializeField] Text topPanelText;
	[SerializeField] GameObject card;
	[Header("Rarity")]
	[SerializeField] Image rarityIcon;
	[SerializeField] Text rarityText;
	[Header("Description")]
	[SerializeField] Text descriptionText;
	[Header("Properties Panel")]
	[SerializeField] RectTransform propertiesPanel;
	[SerializeField] GameObject cardPropertyPrefab;
	[Header("XP Granted Panel")]
	[SerializeField] GameObject xpPanel;
	[SerializeField] Text xpTitleText;
	[SerializeField] Text xpValueText;
	[Header("Upgrade Button")]
	[SerializeField] Button upgradeButton;
	[SerializeField] Text upgradeButtonText;
	[SerializeField] Text upgradeCostText;
	[SerializeField] Image coinIcon; //The coin icon is hidden when the card is Maxed Out
	[Header("Use Button")]
	[SerializeField] Button useButton;
	[SerializeField] Text useButtonText;
	[Header("Not Enough Currency Popup")]
	[SerializeField] GameObject notEnoughCurrencyPopup;
	[Header("Card UI Upgrade")]
	[SerializeField]  CardUIUpgrade cardUIUpgrade;

	public void configureCard( GameObject go, PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		//Disable scrolling while popup is displayed
		horizontalScrollview.enabled = false;

		//Title
		string localizedCardName = LocalizationManager.Instance.getText( "CARD_NAME_" + pcd.name.ToString().ToUpper() );
		topPanelText.text = string.Format(LocalizationManager.Instance.getText( "CARD_LEVEL_TITLE" ), pcd.level, localizedCardName );
		//Card
		card.GetComponent<CardUIDetails>().configureCard( pcd, cd );
		//Description
		string localizedCardDescription = LocalizationManager.Instance.getText( "CARD_DESCRIPTION_" + pcd.name.ToString().ToUpper() );
		descriptionText.text = localizedCardDescription;
		//Rarity
		Color rarityColor;
		ColorUtility.TryParseHtmlString (CardManager.Instance.getCardColorHexValue(cd.rarity), out rarityColor);
		rarityIcon.color = rarityColor;
		rarityText.text = LocalizationManager.Instance.getText( "CARD_RARITY_" + cd.rarity.ToString() );
		//Configure card properties
		configureCardProperties( pcd, cd );
		//Upgrade button
		configureUpgradeButton( go, pcd, cd );
		//The Use button is only shown if the card is not in the battle deck
		List<CardManager.CardData> cardCollectionList = GameManager.Instance.playerDeck.getCardDeck( CardSortMode.BY_MANA_COST );
		bool isMemberOfCardCollection = cardCollectionList.Exists(cardData => cardData.name == cd.name );
		useButton.gameObject.SetActive( isMemberOfCardCollection  );
		if( isMemberOfCardCollection )
		{
			useButton.onClick.RemoveAllListeners();
			useButton.onClick.AddListener(() => OnClickUse( go, cd.name ));
		}

	}

	void configureCardProperties( PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		//Make sure we removed properties that were previously generated first
		for( int i = propertiesPanel.transform.childCount-1; i >= 0; i-- )
		{
			Transform child = propertiesPanel.transform.GetChild( i );
			GameObject.Destroy( child.gameObject );
		}

		for( int i=0; i < cd.cardProperties.Count; i++ )
		{
			createCardProperty( i, cd.cardProperties[i], pcd, cd );
		}
	}

	void createCardProperty( int index, CardManager.CardProperty cp, PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		GameObject go = (GameObject)Instantiate(cardPropertyPrefab);
		go.transform.SetParent(propertiesPanel,false);
		go.GetComponent<CardPropertyUI>().configureProperty( index, cp, pcd, cd );
	}

	/// <summary>
	/// Configures the XP panel. The XP panel is only shown when the player can upgrade.
	/// </summary>
	/// <param name="pcd">Pcd.</param>
	/// <param name="cd">Cd.</param>
	void configureXPPanel( PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		xpPanel.SetActive( true );
		xpTitleText.text = LocalizationManager.Instance.getText("CARD_XP_GRANTED_ON_UPGRADE");
		xpValueText.text = string.Format("{0:n0}", CardManager.Instance.getXPGainedAfterUpgrading( pcd.level + 1, cd.rarity ) );
	}

	/// <summary>
	/// Configures the upgrade button. To allow upgrading we need to validate these conditions:
	/// a) The card must not be maxed out
	/// b) The player has enough coin to pay.
	/// c) The player has enough cards for the upgrade.
	/// </summary>
	/// <param name="pcd"Player card data.</param>
	/// <param name="cd">Card data.</param>
	void configureUpgradeButton( GameObject go, PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		xpPanel.SetActive( false );
		coinIcon.gameObject.SetActive( true );
		upgradeButtonText.text = LocalizationManager.Instance.getText("CARD_UPGRADE");
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

		if( pcd.level + 1 <= CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ) )
		{
			//Do I have enough cards to level up the card?
			if( pcd.quantity >= numberOfCardsForUpgrade )
			{
				upgradeButton.onClick.RemoveAllListeners();
				upgradeButton.onClick.AddListener(() => OnClickUpgrade( go, cd.name, upgradeCost));

				//Does the player have enough coins?
				if( upgradeCost <= GameManager.Instance.playerInventory.getCoinBalance() )
				{
					//The player has enough coins.
					upgradeButton.interactable = true;
					upgradeCostText.color = upgradeButton.colors.normalColor;
					configureXPPanel( pcd, cd );
				}
				else
				{
					//The player does not have enough coins.
					//He can still click the button.
					//If he does, he will get a popup asking if wants to convert gems to coins.
					upgradeButton.interactable = true;
					upgradeCostText.color = Color.red;
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
			upgradeCostText.text = LocalizationManager.Instance.getText( "CARD_MAXED_OUT" );
			upgradeCostText.color = upgradeButton.colors.disabledColor;
			coinIcon.gameObject.SetActive( false );
		}
	}

	public void OnClickUpgrade( GameObject go, CardName card, int upgradeCost )
	{
		//Does the player have enough coins?
		if( upgradeCost <= GameManager.Instance.playerInventory.getCoinBalance() )
		{
			//The player has enough coins and does not need gems.
			GameManager.Instance.playerInventory.deductCoins( upgradeCost );
			GameManager.Instance.playerDeck.upgradeCardByOneLevel( card );
			//Display upgrade ceremony panel
			cardUIUpgrade.configureUpgradePanel( GameManager.Instance.playerDeck.getCardByName( card ), CardManager.Instance.getCardByName( card ) );
			//Update the Card Detail popup since some values have changed
			configureCard( go, GameManager.Instance.playerDeck.getCardByName( card ), CardManager.Instance.getCardByName( card ) );
			//Update the Card Collection entry as well
			go.GetComponent<CardUIDetails>().configureCard( GameManager.Instance.playerDeck.getCardByName( card ), CardManager.Instance.getCardByName( card ) );
		}
		else
		{
			//The player does not have enough coins.
			//Ask him if he wants to convert gems to coins.
			notEnoughCurrencyPopup.SetActive( true );
			int coinsAvailable = GameManager.Instance.playerInventory.getCoinBalance();
			int coinsMissing = upgradeCost - coinsAvailable;
			int gemsNeeded = (int) Math.Ceiling( coinsMissing/StoreManager.GEM_TO_COINS_RATIO );
			notEnoughCurrencyPopup.GetComponent<CardNotEnoughCurrencyPopup>().configureForNotEnoughCoins( go, card, coinsAvailable, gemsNeeded );
		}
	}

	public void OnClickUse( GameObject go, CardName card )
	{
		gameObject.SetActive( false );
		cardCollectionManager.replaceCard( go, card );
	}

	public void OnClickHide()
	{
		gameObject.SetActive( false );
		//Re-enable scrolling
		horizontalScrollview.enabled = true;
	}

}
