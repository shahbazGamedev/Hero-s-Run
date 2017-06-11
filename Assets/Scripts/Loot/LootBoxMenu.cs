using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class LootBoxMenu : MonoBehaviour, IPointerDownHandler {

	[Header("Panels")]
	[SerializeField] GameObject masterPanel;

	[SerializeField] GameObject coinPanel;
	[SerializeField] GameObject gemPanel;
	[SerializeField] GameObject cardPanel;
	[SerializeField] GameObject playerIconPanel;

	[Header("Coin Panel")]
	[SerializeField] TextMeshProUGUI coinAmountText;
	[SerializeField] TextMeshProUGUI coinBalanceText;

	[Header("Gem Panel")]
	[SerializeField] TextMeshProUGUI gemAmountText;
	[SerializeField] TextMeshProUGUI gemBalanceText;

	[Header("Player Icon Panel")]
	[SerializeField] Image playerIconImage;
	[SerializeField] TextMeshProUGUI playerIconNameText;

	[Header("Card Panel")]
	[SerializeField] Image cardImage;
	[SerializeField] TextMeshProUGUI cardAmountText;
	[SerializeField] TextMeshProUGUI cardNameText;
	[SerializeField] Image rarityIcon;
	[SerializeField] TextMeshProUGUI rarityText;
	[SerializeField] TextMeshProUGUI cardLevelText;

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
	[SerializeField] TextMeshProUGUI progressBarText;

	[Header("Loot Counter")]
	[SerializeField] TextMeshProUGUI lootCounterText;

	List<LootBox.Loot> lootList;
	int lootCounter = 0;
	const float ANIMATION_DURATION = 0.75f;
	const float SHORT_ANIMATION_DURATION = 0.5f;

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
		//Display the number of loot items in the loot box
		lootCounterText.text = ( lootList.Count -1 ).ToString();
		StopAllCoroutines();
		StartCoroutine( giveLoot( lootList[lootCounter] ) );
		lootCounter++;
		masterPanel.SetActive (true );
	}

	IEnumerator giveLoot( LootBox.Loot loot )
	{
		switch( loot.type )
		{
			case LootType.COINS:
				activateLootPanel( LootType.COINS );
				//The yield is needed or else the spin number coroutine won't start because the text will not be active (it takes a frame).
				yield return new WaitForEndOfFrame();
				coinAmountText.text = "+" + loot.quantity.ToString();
				int coinBalance = GameManager.Instance.playerInventory.getCoinBalance();
				int newCoinBalance = coinBalance + loot.quantity;
				coinBalanceText.GetComponent<UISpinNumber>().spinNumber( "{0}", coinBalance, newCoinBalance, 2f, true );
				GameManager.Instance.playerInventory.addCoins( loot.quantity );
			break;

			case LootType.GEMS:
				activateLootPanel( LootType.GEMS );
				//The yield is needed or else the spin number coroutine won't start because the text will not be active (it takes a frame).
				yield return new WaitForEndOfFrame();
				gemAmountText.text = "+" + loot.quantity.ToString();
				int gemBalance = GameManager.Instance.playerInventory.getGemBalance();
				int newGemBalance = gemBalance + loot.quantity;
				gemBalanceText.GetComponent<UISpinNumber>().spinNumber( "{0}", gemBalance, newGemBalance, 2f, true );
				GameManager.Instance.playerInventory.addGems( loot.quantity );
			break;

			case LootType.CARDS:
				activateLootPanel( LootType.CARDS );
				//The yield is needed or else the spin number coroutine won't start because the text will not be active (it takes a frame).
				yield return new WaitForEndOfFrame();
				CardManager.CardData cd = CardManager.Instance.getCardByName( loot.cardName );
				PlayerDeck.PlayerCardData pcd = GameManager.Instance.playerDeck.getCardByName( loot.cardName );
				//Card Image
				cardImage.sprite = cd.icon;
				//Legendary cards may have special effects
				cardImage.material = cd.cardMaterial;
				//The amount of cards you are receiving
				cardAmountText.text = "x" + loot.quantity.ToString();
				//The localized name of the card
				cardNameText.text = LocalizationManager.Instance.getText( "CARD_NAME_" + pcd.name.ToString().ToUpper() );
				//Card rarity icon and text
				Color rarityColor;
				ColorUtility.TryParseHtmlString (CardManager.Instance.getCardColorHexValue(cd.rarity), out rarityColor);
				rarityIcon.color = rarityColor;
				rarityText.text = LocalizationManager.Instance.getText( "CARD_RARITY_" + cd.rarity.ToString() );
				//Card level
				cardLevelText.color = rarityColor;
				if( CardManager.Instance.isCardAtMaxLevel( pcd.level, cd.rarity ) )
				{
					cardLevelText.text = LocalizationManager.Instance.getText( "CARD_MAX_LEVEL");
				}
				else
				{
					cardLevelText.text = String.Format( LocalizationManager.Instance.getText( "CARD_LEVEL"), pcd.level.ToString() );
				}
				updateCardProgressBar( pcd, cd, loot.quantity );
				//Add to player deck
				GameManager.Instance.playerDeck.addCardFromLootBox( loot.cardName, loot.quantity );
			break;

			case LootType.PLAYER_ICON:
				activateLootPanel( LootType.PLAYER_ICON );
				playerIconImage.sprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( loot.uniqueItemID ).icon;
				playerIconNameText.text = LocalizationManager.Instance.getText( "PLAYER_ICON_" + loot.uniqueItemID.ToString() );
				GameManager.Instance.playerIcons.unlockPlayerIcon( loot.uniqueItemID );
			break;
			
			default:
				Debug.LogError("Give loot content encountered an unknown loot type: " + loot.type );
			break;
		}
		yield return new WaitForEndOfFrame();

	}

	void activateLootPanel( LootType type )
	{
		coinPanel.SetActive( false );
		gemPanel.SetActive( false );
		cardPanel.SetActive( false );
		playerIconPanel.SetActive( false );

		switch( type )
		{
			case LootType.COINS:
				coinPanel.SetActive( true );
			break;

			case LootType.GEMS:
				gemPanel.SetActive( true );
			break;

			case LootType.CARDS:
				cardPanel.SetActive( true );
			break;

			case LootType.PLAYER_ICON:
				playerIconPanel.SetActive( true );
			break;
		}
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
			GameManager.Instance.playerInventory.serializePlayerInventory( false );
			GameManager.Instance.playerDeck.serializePlayerDeck( false );
			GameManager.Instance.playerIcons.serializePlayerIcons( true );
			masterPanel.SetActive (false );
		}
	}

	void updateCardProgressBar( PlayerDeck.PlayerCardData pcd, CardManager.CardData cd, int numberCardsAdded )
	{
		int numberOfCardsForUpgrade;
		if( pcd.level + 1 <= CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ) )
		{
			numberOfCardsForUpgrade = CardManager.Instance.getNumberOfCardsRequiredForUpgrade( pcd.level + 1, cd.rarity );
		}
		else
		{
			numberOfCardsForUpgrade = CardManager.Instance.getNumberOfCardsRequiredForUpgrade( CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ), cd.rarity );
		}

		//Progress bar section
		if( CardManager.Instance.isCardAtMaxLevel( pcd.level, cd.rarity ) )
		{
			//MAXED OUT
			progressBarFill.color = CardManager.Instance.getCardUpgradeColor( CardUpgradeColor.MAXED_OUT );
			progressBarIndicator.color = Color.white;
			progressBarIndicator.overrideSprite = progressBarMaxLevelIndicator;
			//Slider is full
			progressBarSlider.value = 1f;
			progressBarText.text = string.Empty;
		}
		else
		{
			//NOT MAXED OUT
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

			//The number of cards has changed. Let's animate.
			float animationDuration;
			if( numberCardsAdded == 1 )
			{
				animationDuration = SHORT_ANIMATION_DURATION;
			}
			else
			{
				animationDuration = ANIMATION_DURATION;
			}
			float newNumberOfCards = pcd.quantity + numberCardsAdded;
			string cardsOwned = "{0}/" + numberOfCardsForUpgrade.ToString();
			progressBarText.GetComponent<UISpinNumber>().spinNumber( cardsOwned, pcd.quantity, newNumberOfCards, animationDuration, true, onCardIncrement );
			progressBarSlider.value = pcd.quantity/(float)numberOfCardsForUpgrade;
			float toValue = newNumberOfCards/numberOfCardsForUpgrade;
			if( toValue <= 1f ) progressBarSlider.GetComponent<UIAnimateSlider>().animateSlider( toValue, animationDuration );
		}
	}

	void onCardIncrement( int value)
	{
		print("onCardIncrement " + value );
	}
}
