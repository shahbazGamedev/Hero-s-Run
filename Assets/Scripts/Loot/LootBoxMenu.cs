using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class LootBoxMenu : MonoBehaviour, IPointerDownHandler {

	[SerializeField] GameObject lootPanel;
	[SerializeField] CardUIDetails cardUIDetails;
	[SerializeField] Image lootSprite;
	[SerializeField] TextMeshProUGUI lootNameText;
	[SerializeField] TextMeshProUGUI lootAmountText;
	[SerializeField] TextMeshProUGUI lootCounterText;
	[SerializeField] GameObject cardProgressPanel;
	[SerializeField] Sprite coinSprite;
	[SerializeField] Sprite gemSprite;
	[SerializeField] Sprite coinCardSprite;
	[SerializeField] Sprite gemCardSprite;
	[SerializeField] GameObject rarityPanel;
	[SerializeField] TextMeshProUGUI rarityText;
	[SerializeField] Image rarityIcon;
	[SerializeField] GameObject currencyPanel;
	[SerializeField] Image currencyIcon;
	[SerializeField] TextMeshProUGUI currencyAmountText;

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

	[Header("Level")]
	[Tooltip("The level text is displayed on top of the card image. For example: 'Level 5' or 'Max Level'. The text color varies with the card rarity.")]
	[SerializeField] TextMeshProUGUI levelText;
	List<LootBox.Loot> lootList;
	int lootCounter = 0;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();
	}

	void OnEnable()
	{
		LootBoxClientManager.lootBoxGrantedEvent += LootBoxGrantedEvent;
	}

	void OnDisable()
	{
		LootBoxClientManager.lootBoxGrantedEvent -= LootBoxGrantedEvent;
	}

	public void LootBoxGrantedEvent( LootBox lootBox )
	{
		lootCounter = 0;
		lootList = lootBox.getLootList();
		lootBox.print();
		GameManager.Instance.playerProfile.setLastFreeLootBoxOpenedTime( DateTime.UtcNow );
		//Schedule a local notification to remind the player of when his next free loot box will be available
		NotificationServicesHandler.Instance.scheduleFreeLootBoxNotification(240);
		//Display the number of loot items in the loot box
		lootCounterText.text = lootList.Count.ToString();
		StopAllCoroutines();
		StartCoroutine( giveLoot( lootList[lootCounter] ) );
		lootCounter++;
		lootPanel.SetActive (true );
	}

	IEnumerator giveLoot( LootBox.Loot loot )
	{
		lootSprite.material = null;
		switch( loot.type )
		{
			case LootType.COINS:
				levelText.color = Color.white;
				levelText.gameObject.SetActive( true );
				levelText.text = LocalizationManager.Instance.getText( "LOOT_BOX_YOU_HAVE" );
				rarityPanel.SetActive( false );
				currencyPanel.SetActive( true );
				cardProgressPanel.SetActive( false );
				yield return new WaitForEndOfFrame();
				currencyIcon.sprite = coinSprite;
				lootNameText.text = LocalizationManager.Instance.getText( "STORE_COINS_TITLE" );
				displayLootReceived( "+" + loot.quantity.ToString(), coinCardSprite );
				int coinBalance = GameManager.Instance.playerInventory.getCoinBalance();
				int newCoinBalance = coinBalance + loot.quantity;
				currencyAmountText.gameObject.SetActive( true);
				currencyAmountText.GetComponent<UISpinNumber>().spinNumber( "{0}", coinBalance, newCoinBalance, 2f, true );
				GameManager.Instance.playerInventory.addCoins( loot.quantity );
			break;

			case LootType.GEMS:
				levelText.color = Color.white;
				levelText.gameObject.SetActive( true );
				levelText.text = LocalizationManager.Instance.getText( "LOOT_BOX_YOU_HAVE" );
				cardProgressPanel.SetActive( false );
				currencyPanel.SetActive( true );
				yield return new WaitForEndOfFrame();
				currencyIcon.sprite = gemSprite;
				rarityPanel.SetActive( false );
				lootNameText.text = LocalizationManager.Instance.getText( "STORE_GEMS_TITLE" );
				int gemBalance = GameManager.Instance.playerInventory.getGemBalance();
				int newGemBalance = gemBalance + loot.quantity;
				currencyAmountText.gameObject.SetActive( true);
				currencyAmountText.GetComponent<UISpinNumber>().spinNumber( "{0}", gemBalance, newGemBalance, 2f, true );
				GameManager.Instance.playerInventory.addGems( loot.quantity );
				displayLootReceived( "+" + loot.quantity.ToString(), gemCardSprite );
			break;

			case LootType.CARDS:
				levelText.gameObject.SetActive( true );
				cardProgressPanel.SetActive( true );
				currencyPanel.SetActive( false );
				rarityPanel.SetActive( true );
				GameManager.Instance.playerDeck.addCardFromLootBox( loot.cardName, loot.quantity );
				CardManager.CardData cd = CardManager.Instance.getCardByName( loot.cardName );
				PlayerDeck.PlayerCardData pcd = GameManager.Instance.playerDeck.getCardByName( loot.cardName );
				cardUIDetails.configureCard( pcd, cd );
				//Card name
				string localizedCardName = LocalizationManager.Instance.getText( "CARD_NAME_" + pcd.name.ToString().ToUpper() );
				lootNameText.text = localizedCardName;
				displayLootReceived( "+" + loot.quantity.ToString() );
				//Level section
				//Level background
				Color rarityColor;
				ColorUtility.TryParseHtmlString (CardManager.Instance.getCardColorHexValue(cd.rarity), out rarityColor);
				if( levelText != null ) levelText.color = rarityColor;
		
				//Level text
				if( pcd.level + 1 < CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ) )
				{
					if( levelText != null ) levelText.text = String.Format( LocalizationManager.Instance.getText( "CARD_LEVEL"), pcd.level.ToString() );
				}
				else
				{
					if( levelText != null ) levelText.text = LocalizationManager.Instance.getText( "CARD_MAX_LEVEL");
				}
				//Rarity
				ColorUtility.TryParseHtmlString (CardManager.Instance.getCardColorHexValue(cd.rarity), out rarityColor);
				rarityIcon.color = rarityColor;
				rarityText.text = LocalizationManager.Instance.getText( "CARD_RARITY_" + cd.rarity.ToString() );
				updateCardProgressBar( pcd, cd );
				lootSprite.material = cd.cardMaterial;

			break;

			case LootType.PLAYER_ICON:
				cardProgressPanel.SetActive( false );
				currencyPanel.SetActive( false );
				rarityPanel.SetActive( false );
				lootNameText.text = LocalizationManager.Instance.getText( "PLAYER_ICON_MENU_TITLE" );
				GameManager.Instance.playerIcons.unlockPlayerIcon( loot.uniqueItemID );
				string iconName = LocalizationManager.Instance.getText( "PLAYER_ICON_" + loot.uniqueItemID.ToString() );
				displayLootReceived( iconName, ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( loot.uniqueItemID ).icon );
				levelText.gameObject.SetActive( false );
			break;
			
			default:
				Debug.LogError("Give loot content encountered an unknown loot type: " + loot.type );
			break;
		}
		yield return new WaitForEndOfFrame();

	}

	void displayLootReceived( string text, Sprite sprite = null )
	{
		if( sprite != null ) lootSprite.sprite = sprite;
		lootAmountText.text = text;
	}

	public void OnPointerDown(PointerEventData data)
	{
		if( lootCounter < lootList.Count )
		{
			StopAllCoroutines();
			StartCoroutine( giveLoot( lootList[lootCounter] ) );
			lootCounter++;
			lootCounterText.text = (lootList.Count - lootCounter).ToString();

		}
		else
		{
			//Save
			GameManager.Instance.playerInventory.serializePlayerInventory();
			GameManager.Instance.playerDeck.serializePlayerDeck(true);
			lootPanel.SetActive (false );
		}
	}

	void updateCardProgressBar( PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		//Level text and numberOfCardsForUpgrade
		int numberOfCardsForUpgrade;
		if( pcd.level + 1 < CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ) )
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
				progressBarBackground.color = ENOUGH_CARDS_TO_UPGRADE;
				progressBarIndicator.color = ENOUGH_CARDS_TO_UPGRADE;
			}
			else
			{
				progressBarBackground.color = NOT_ENOUGH_CARDS_TO_UPGRADE;
				progressBarIndicator.color = NOT_ENOUGH_CARDS_TO_UPGRADE;
			}
			progressBarIndicator.overrideSprite = null;
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
