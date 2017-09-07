using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LootBoxMenu : MonoBehaviour, IPointerDownHandler {

	public static LootBoxMenu Instance;

	[Header("Panels")]
	[SerializeField] GameObject masterPanel;

	[SerializeField] GameObject coinPanel;
	[SerializeField] GameObject gemPanel;
	[SerializeField] GameObject cardPanel;
	[SerializeField] GameObject playerIconPanel;
	[SerializeField] GameObject voiceLinePanel;

	[Header("Coin Panel")]
	[SerializeField] TextMeshProUGUI coinAmountText;
	[SerializeField] TextMeshProUGUI coinBalanceText;

	[Header("Gem Panel")]
	[SerializeField] TextMeshProUGUI gemAmountText;
	[SerializeField] TextMeshProUGUI gemBalanceText;

	[Header("Player Icon Panel")]
	[SerializeField] Image playerIconImage;
	[SerializeField] TextMeshProUGUI playerIconNameText;

	[Header("Voice Line Panel")]
	[SerializeField] AudioWaveFormVisualizer audioWaveFormVisualizer;
	[SerializeField] TextMeshProUGUI voiceLineNameText;
	[SerializeField] Image heroIcon;
	[SerializeField] Toggle equipNowToggle;
	int voiceLineId = -1;
	HeroName heroName;

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
	int numberOfCardsForUpgrade;
	const float ANIMATION_DURATION = 0.75f;
	const float SHORT_ANIMATION_DURATION = 0.5f;

	void Awake ()
	{
		if(Instance)
		{
			DestroyImmediate(gameObject);
		}
		else
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

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
		//Reset
		lootCounter = 0;
		equipNowToggle.isOn = false;

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
				//Card Image
				cardImage.sprite = cd.icon;
				//Legendary cards may have special effects
				cardImage.material = cd.cardMaterial;
				//The amount of cards you are receiving
				cardAmountText.text = "x" + loot.quantity.ToString();
				//The localized name of the card
				cardNameText.text = LocalizationManager.Instance.getText( "CARD_NAME_" + cd.name.ToString().ToUpper() );
				//Card rarity icon and text
				Color rarityColor;
				ColorUtility.TryParseHtmlString (CardManager.Instance.getCardColorHexValue(cd.rarity), out rarityColor);
				rarityIcon.color = rarityColor;
				rarityText.text = LocalizationManager.Instance.getText( "CARD_RARITY_" + cd.rarity.ToString() );
				//Card level
				cardLevelText.color = rarityColor;
				//Does the player already own this card?
				PlayerDeck.PlayerCardData pcd = GameManager.Instance.playerDeck.getCardByName( loot.cardName );
				if( pcd == null )
				{
					//No, this is a new card.
					//Add it to the player's deck. Tag is as new so the "New" ribbon gets displayed.
					pcd = GameManager.Instance.playerDeck.addCard(  loot.cardName, 1, loot.quantity, BattleDeck.REMOVE_FROM_ALL_BATTLE_DECKS, false, true );
					updateCardProgressBar( pcd, cd, 0, loot.quantity );
				}
				else
				{
					//Yes, he already owns it
					//Add the cards obtained
					updateCardProgressBar( pcd, cd, pcd.quantity, loot.quantity );
					GameManager.Instance.playerDeck.changeCardQuantity(  pcd, loot.quantity );
				}
				//Save
				GameManager.Instance.playerDeck.serializePlayerDeck( true );

 				if( CardManager.Instance.isCardAtMaxLevel( pcd.level, cd.rarity ) )
				{
					cardLevelText.text = LocalizationManager.Instance.getText( "CARD_MAX_LEVEL");
				}
				else
				{
					cardLevelText.text = String.Format( LocalizationManager.Instance.getText( "CARD_LEVEL"), pcd.level.ToString() );
				}
				//Refresh the card collections
				//LootBoxMenu is used in multiple scenes. CardCollectionManager however is only available in the main menu.
				if( (GameScenes) SceneManager.GetActiveScene().buildIndex == GameScenes.MainMenu )
				{
					GameObject.FindObjectOfType<CardCollectionManager>().initialize ();
				}
			break;

			case LootType.PLAYER_ICON:
				activateLootPanel( LootType.PLAYER_ICON );
				playerIconImage.sprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( loot.uniqueItemID ).icon;
				playerIconNameText.text = LocalizationManager.Instance.getText( "PLAYER_ICON_" + loot.uniqueItemID.ToString() );
				GameManager.Instance.playerIcons.unlockPlayerIcon( loot.uniqueItemID );
			break;

			case LootType.VOICE_LINE:
				AudioClip clip = VoiceOverManager.Instance.getTauntClip( loot.heroName, loot.uniqueItemID );
				audioWaveFormVisualizer.initialize( clip );
				voiceLineNameText.text = "\"" + LocalizationManager.Instance.getText( "VO_TAUNT_" + loot.heroName.ToString().ToUpper() + "_" + loot.uniqueItemID.ToString() ) + "\"";
				equipNowToggle.onValueChanged.RemoveAllListeners();
				equipNowToggle.onValueChanged.AddListener ( (value) => { OnEquipNowToggle(loot); } );
				heroIcon.sprite = HeroManager.Instance.getHeroSprite( loot.heroName );
				GameManager.Instance.playerVoiceLines.unlockVoiceLineAndSave( loot.uniqueItemID );
				activateLootPanel( LootType.VOICE_LINE );
			break;
			
			default:
				Debug.LogError("Give loot content encountered an unknown loot type: " + loot.type );
			break;
		}
		yield return new WaitForEndOfFrame();

	}

	public void OnEquipNowToggle( LootBox.Loot loot )
	{
		UISoundManager.uiSoundManager.playButtonClick();
		if( equipNowToggle.isOn ) 
		{
			voiceLineId = loot.uniqueItemID;
			heroName = loot.heroName;
		}
		else
		{
			voiceLineId = -1;
		}
	}

	void activateLootPanel( LootType type )
	{
		coinPanel.SetActive( false );
		gemPanel.SetActive( false );
		cardPanel.SetActive( false );
		playerIconPanel.SetActive( false );
		voiceLinePanel.SetActive( false );

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

			case LootType.VOICE_LINE:
				voiceLinePanel.SetActive( true );
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
			//if the player decided to equip his new voice line (i.e. the value is not -1), set it in player profile before saving.
			if( equipNowToggle.isOn ) GameManager.Instance.playerVoiceLines.equipVoiceLine( voiceLineId, heroName );
			GameManager.Instance.playerInventory.serializePlayerInventory( false );
			GameManager.Instance.playerDeck.serializePlayerDeck( false );
			GameManager.Instance.playerIcons.serializePlayerIcons( false );
			GameManager.Instance.playerVoiceLines.serializeVoiceLines( true );
			masterPanel.SetActive (false );
		}
	}

	void updateCardProgressBar( PlayerDeck.PlayerCardData pcd, CardManager.CardData cd, int initialNumberOfCards, int numberCardsAdded )
	{
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
			if( initialNumberOfCards >= numberOfCardsForUpgrade )
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
			float newNumberOfCards = initialNumberOfCards + numberCardsAdded;
			string cardsOwned = "{0}/" + numberOfCardsForUpgrade.ToString();
			progressBarText.GetComponent<UISpinNumber>().spinNumber( cardsOwned, initialNumberOfCards, newNumberOfCards, animationDuration, true, onCardIncrement );
			progressBarSlider.value = initialNumberOfCards/(float)numberOfCardsForUpgrade;
			float toValue = newNumberOfCards/numberOfCardsForUpgrade;
			if( toValue > 1f ) toValue = 1f;
			progressBarSlider.GetComponent<UIAnimateSlider>().animateSlider( toValue, animationDuration );
		}
	}

	void onCardIncrement( int value)
	{
		if( value >= numberOfCardsForUpgrade )
		{
			progressBarFill.color = CardManager.Instance.getCardUpgradeColor( CardUpgradeColor.ENOUGH_CARDS_TO_UPGRADE );
			progressBarIndicator.color = CardManager.Instance.getCardUpgradeColor( CardUpgradeColor.ENOUGH_CARDS_TO_UPGRADE );
		}
		else
		{
			progressBarFill.color = CardManager.Instance.getCardUpgradeColor( CardUpgradeColor.NOT_ENOUGH_CARDS_TO_UPGRADE );
			progressBarIndicator.color = CardManager.Instance.getCardUpgradeColor( CardUpgradeColor.NOT_ENOUGH_CARDS_TO_UPGRADE );
		}
	}
}
