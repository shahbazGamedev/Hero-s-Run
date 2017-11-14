using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnRibbonHandler : MonoBehaviour {

	[Header("General")]
	[SerializeField] CardHandler cardHandler;
	[SerializeField] Transform cardPanel;
	[SerializeField] GameObject cardPrefab;
	[Tooltip("The text color to use for the card name if the card in the turn-ribbon is currently not effective e.g. you have a Health Boost card but your health is already at maximum.")]
	[SerializeField] Color cardNotEffectiveTextColor;
	[Header("Next Card")]
	[SerializeField] Image nextCardImage;
	CardManager.CardData nextCard;
	[SerializeField] Sprite blankCardSprite;
	[SerializeField] Sprite stolenCardSprite;
	[SerializeField] Sprite hackedCardSprite;
	[Header("Power Bar")]
	[SerializeField] PowerBar powerBar;

	public const int NUMBER_CARDS_IN_BATTLE_DECK = 8;
	public const float DELAY_BEFORE_NEXT_CARD_AVAILABLE = 1f;

	int [] cardIndexArray = new int[]{0,1,2,3,4,5,6,7};
	List<int> cardIndexList = new List<int>(NUMBER_CARDS_IN_BATTLE_DECK);

	List<CardManager.CardData> turnRibbonList = new List<CardManager.CardData>();
	List<Button> turnRibbonButtonList = new List<Button>();
	Queue<CardManager.CardData> cardQueue = new Queue<CardManager.CardData>();
	
	PlayerControl playerControl;
	PlayerRace playerRace;
	PlayerSpell playerSpell;
	PlayerHealth playerHealth;
	string userName;

	//Delegate used to communicate to other classes when the local player has played a card
	public delegate void CardPlayedEvent( CardName name, int level );
	public static event CardPlayedEvent cardPlayedEvent;

	// Use this for initialization
	void Start ()
	{
		cardIndexList.AddRange(cardIndexArray);

		List<PlayerDeck.PlayerCardData> battleDeckList = GameManager.Instance.playerDeck.getBattleDeck();

		//Populate turn-ribbon with 4 unique, random cards
		for( int i = 0; i < CardManager.CARDS_IN_TURN_RIBBON; i++ )
		{
			addCardToTurnRibbon( i, battleDeckList[getUniqueRandom()].name );
		}
		//Set next card with a unique, random card
		setNextCard( battleDeckList[getUniqueRandom()].name );

		//We are queuing 3 cards: 8 - 4 - 1 (card in battle decks - cards in turn ribbon - 1)
		for( int i = 0; i < 3; i++ )
		{
			addCardToQueue( battleDeckList[getUniqueRandom()].name );
		}
	}

	public void setPlayerControl( PlayerControl playerControl )
	{
		this.playerControl = playerControl;
		playerRace = playerControl.GetComponent<PlayerRace>();
		playerSpell = playerControl.GetComponent<PlayerSpell>();
		playerHealth = playerControl.GetComponent<PlayerHealth>();
		userName = GameManager.Instance.playerProfile.getUserName();
	}

	void addCardToTurnRibbon( int index, CardName cardName )
	{
		GameObject go = (GameObject)Instantiate(cardPrefab);
		go.transform.SetParent(cardPanel,false);
		Button cardButton = go.GetComponent<Button>();
		cardButton.name = "Button " + index.ToString();
		turnRibbonButtonList.Add(cardButton);
		cardButton.onClick.AddListener(() => OnClickCard( index ) );

		CardManager.CardData cardData = CardManager.Instance.getCardByName( cardName );
		go.GetComponent<CardTurnRibbon>().configureTurnRibbonCard( cardData );
		turnRibbonList.Add(cardData);
	}

	void setNextCard( CardName cardName )
	{
		CardManager.CardData cardData = CardManager.Instance.getCardByName( cardName );
		nextCardImage.GetComponent<CardTurnRibbon>().configureTurnRibbonCard( cardData );
		this.nextCard = cardData;
	}

	void addCardToQueue( CardName cardName )
	{
		CardManager.CardData cardData = CardManager.Instance.getCardByName( cardName );
		cardQueue.Enqueue( cardData );
	}

	void LateUpdate()
	{
		for( int i = 0; i < turnRibbonList.Count; i++ )
		{
			//If the race is not in progress, don't allow the player to play cards.
			//If the player cannot control his character (e.g. because of Stasis), don't allow him to play cards.
			//If the player has been Hacked, don't allow him to play cards.
			Image radialMask = turnRibbonButtonList[i].GetComponent<CardTurnRibbon>().radialMask;
			if( PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.IN_PROGRESS && playerControl.isPlayerControlEnabled() && !playerSpell.isCardActive(CardName.Hack) )
			{
				if( powerBar.hasEnoughPower( turnRibbonList[i].manaCost ) )
				{
					//We have enough power to play this card.
					radialMask.fillAmount = 0;
				}
				else
				{
					//We don't have enough power to play this card.
					float fillAmount = 1f - powerBar.getPowerAmount()/turnRibbonList[i].manaCost;
					if( fillAmount < 0 ) fillAmount = 0;
					radialMask.fillAmount = fillAmount;
				}
				//Verify if this card is effective.
				isEffective( i, turnRibbonList[i] );
			}
			else
			{
				radialMask.fillAmount = 1f;
			}
		}
	}

	/// <summary>
	/// If the card is effective, the card name will be white, if not, the card name will be red.
	/// For now, a card is not effective if it has the RANGE property, and the RANGE property is not infinite, and there no players within range.
	/// </summary>
	/// <param name="indexInTurnRibbon">Index in turn ribbon.</param>
	/// <param name="Card">Card.</param>
	void isEffective( int indexInTurnRibbon, CardManager.CardData card )
	{
		//These 2 lines are to test the impact of isEffective on the FPS
		DebugInfoType debugType = GameManager.Instance.playerDebugConfiguration.getDebugInfoType();
		if( debugType == DebugInfoType.FRAME_RATE_TEST ) return;

		if( card.doesCardHaveThisProperty( CardPropertyType.RANGE ) )
		{
			PlayerDeck.PlayerCardData pcd = GameManager.Instance.playerDeck.getCardByName( card.name );
			float range = card.getCardPropertyValue( CardPropertyType.RANGE, pcd.level );
			
			Button buttonOfCard = turnRibbonButtonList[indexInTurnRibbon];
			
			//This means that the RANGE is not infinite. Check for targets.
			if( range > 0 )
			{
				if( isThereAtLeastOnePlayerWithinRange( range ) )
				{
					//We have at least one target within range. Make the text white.
					buttonOfCard.GetComponent<CardTurnRibbon>().changeCardNameColor( Color.white );				
				}
				else
				{
					//We have no targets within range. Make the text red.
					buttonOfCard.GetComponent<CardTurnRibbon>().changeCardNameColor( cardNotEffectiveTextColor );
				}
			}
		}
		if( card.doesCardHaveThisProperty( CardPropertyType.HEALTH ) )
		{
			Button buttonOfCard = turnRibbonButtonList[indexInTurnRibbon];
			
			//Is the player at maximum health?
			if( playerHealth.isFullHealth() )
			{
				//Yes. Make the text red to indicate that it would be wasteful to use a health potion.
				buttonOfCard.GetComponent<CardTurnRibbon>().changeCardNameColor( cardNotEffectiveTextColor );
			}
			else
			{
				//No. Make the text white.
				buttonOfCard.GetComponent<CardTurnRibbon>().changeCardNameColor( Color.white );				
			}
		}
	}

	bool isThereAtLeastOnePlayerWithinRange( float range )
	{
		float sqrRange = range * range;
		for( int i =0; i < PlayerRace.players.Count; i++ )
		{
			//Ignore the local player
			if( PlayerRace.players[i] == playerRace ) continue;

			//Calculate the square magnitude to the other player
			float sqrMagnitude = Vector3.SqrMagnitude( playerRace.transform.position - PlayerRace.players[i].transform.position );

			//Is this player within range?
			if( sqrMagnitude > sqrRange ) continue;

			//Is the player dead or Idle or Ziplining? If so, ignore.
			if( PlayerRace.players[i].GetComponent<PlayerControl>().deathType != DeathType.Alive || PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Idle || PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Ziplining ) continue;

			//Is the player using the Cloak card? If so, ignore.
			if( PlayerRace.players[i].GetComponent<PlayerSpell>().isCardActive(CardName.Cloak) ) continue;

			//We found at least one target
			return true;
		}
		return false;
	}

	public void OnClickCard( int indexOfCardPlayed )
	{
		if( PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.IN_PROGRESS && playerControl.isPlayerControlEnabled() && !playerSpell.isCardActive(CardName.Hack) )
		{
			if( powerBar.hasEnoughPower( turnRibbonList[indexOfCardPlayed].manaCost ) )
			{
				//On which card did the player click?
				CardName cardName = turnRibbonList[indexOfCardPlayed].name;
		
				//Get data about the card - make sure NOT to modify the card data
				CardManager.CardData playedCard = CardManager.Instance.getCardByName( cardName );
		
				//Deduct the power
				powerBar.deductPower( playedCard.manaCost );
			
				Button buttonOfCardPlayed = turnRibbonButtonList[indexOfCardPlayed];

				//Disable the button to avoid multiple clicks
				buttonOfCardPlayed.interactable = false;

				//Play the card effect
				if( playedCard.usesOmniToolAnimation )
				{
					//Play the OmniTool animation before activating the card.
					//The animation callback will activate the card.
					playerControl.playOmniToolAnimation( playedCard.name );
				}
				else
				{
					//Activate the card directly
					activateCard( cardName );
				}

				//When you play the Steal card, it does not get replaced by the card in the Next slot but by the card
				//you are stealing.
				if( cardName != CardName.Steal )
				{
					//Temporarily replace the image on the button that was clicked by a blank image
					buttonOfCardPlayed.GetComponent<CardTurnRibbon>().overrideCardImage( blankCardSprite );
					//Card name text and power cost text
					buttonOfCardPlayed.GetComponent<CardTurnRibbon>().cardName.text = string.Empty;
					buttonOfCardPlayed.GetComponent<CardTurnRibbon>().powerCost.text = string.Empty;
			
					//Wait a little before moving the Next Card into the free ribbon slot
					StartCoroutine( moveNextCardIntoTurnRibbon( indexOfCardPlayed, playedCard ) );
			
					//Determine the level of the card since it may be modified by Supercharger.
					PlayerDeck.PlayerCardData playerCardData = GameManager.Instance.playerDeck.getCardByName( cardName );
					int level = playerCardData.level;
					if( playerSpell.isCardActive( CardName.Supercharger) )
					{
						int maxLevel = CardManager.Instance.getMaxCardLevelForThisRarity( playedCard.rarity );
						level = Mathf.Min( maxLevel, level + CardSupercharger.SUPERCHARGER_LEVEL_BOOST );
					}
		
					if( cardPlayedEvent != null ) cardPlayedEvent( cardName, level );
					playerSpell.playedCard( cardName );
				}
			}
		}
	}

	IEnumerator moveNextCardIntoTurnRibbon( int indexOfCardPlayed, CardManager.CardData playedCard )
	{
		yield return new WaitForSecondsRealtime( DELAY_BEFORE_NEXT_CARD_AVAILABLE );

		//Replace the image on the button that was clicked by the image of the Next card
		Button buttonOfCardPlayed = turnRibbonButtonList[indexOfCardPlayed];
		buttonOfCardPlayed.interactable = true;
		buttonOfCardPlayed.GetComponent<CardTurnRibbon>().cardImage.sprite = nextCard.icon;

		if( playerSpell.isCardActive(CardName.Hack) )
		{
			buttonOfCardPlayed.GetComponent<CardTurnRibbon>().overrideCardImage( hackedCardSprite );
			//Card name text and power cost text
			buttonOfCardPlayed.GetComponent<CardTurnRibbon>().configureCardTexts( string.Empty, string.Empty, LocalizationManager.Instance.getText("CARD_HACKED") );
			buttonOfCardPlayed.GetComponent<CardTurnRibbon>().additionalText.color = Color.magenta;
		}
		else
		{
			buttonOfCardPlayed.GetComponent<CardTurnRibbon>().overrideCardImage( null );
			//Card name text and power cost text
			buttonOfCardPlayed.GetComponent<CardTurnRibbon>().configureCardTexts( getCardNameAsString( nextCard.name ), nextCard.manaCost.ToString(), string.Empty );
			//Reset the color to white
			buttonOfCardPlayed.GetComponent<CardTurnRibbon>().changeCardNameColor( Color.white );
		}

		//In the turn-ribbon list, replace the card played by the card held in Next
		turnRibbonList.RemoveAt(indexOfCardPlayed);
		turnRibbonList.Insert( indexOfCardPlayed, nextCard );

		//Dequeue the oldest card and place it in Next
		CardManager.CardData oldestCardInQueue = cardQueue.Dequeue();
		setNextCard( oldestCardInQueue.name );

		//Enqueue the card that was just played
		cardQueue.Enqueue( playedCard );
		
	}

	public void activateCard( CardName cardName )
	{
		PlayerDeck.PlayerCardData playerCardData = GameManager.Instance.playerDeck.getCardByName( cardName );
		Debug.Log("TurnRibbonHandler-activateCard: playing card: " + cardName );
		cardHandler.activateCard( playerControl.GetComponent<PhotonView>(), cardName, userName, playerCardData.level );
		//Increase the card usage count. This is used to determine the player's favorite card.
		playerCardData.timesUsed++;
	}

	#region Player is Hacked
	public void playerIsHacked( bool isHacked )
	{
		for( int i = 0; i < turnRibbonList.Count; i++ )
		{
			if( isHacked )
			{
				//Temporarily replace the image on the button by a Hacked image
				Button buttonOfHackedCard = turnRibbonButtonList[i];
				buttonOfHackedCard.GetComponent<CardTurnRibbon>().overrideCardImage( hackedCardSprite );

				//Card name text and power cost text
				buttonOfHackedCard.GetComponent<CardTurnRibbon>().configureCardTexts( string.Empty,  string.Empty, LocalizationManager.Instance.getText("CARD_HACKED") );
				buttonOfHackedCard.GetComponent<CardTurnRibbon>().additionalText.color = Color.magenta;
			}
			else
			{
				CardName restoredCardName = turnRibbonList[i].name;
				//Get data about the card - make sure NOT to modify the card data
				CardManager.CardData restoredCard = CardManager.Instance.getCardByName( restoredCardName );
				//Restore the image on the button
				Button buttonOfRestoredCard = turnRibbonButtonList[i];
				buttonOfRestoredCard.GetComponent<CardTurnRibbon>().overrideCardImage( null );
				//Card name text and power cost text
				buttonOfRestoredCard.GetComponent<CardTurnRibbon>().configureCardTexts( getCardNameAsString( restoredCard.name ), restoredCard.manaCost.ToString(), string.Empty );
				buttonOfRestoredCard.GetComponent<CardTurnRibbon>().additionalText.color = Color.white;
			}
		}
	}
	#endregion

	#region Steal Card
	public CardName stealCard()
	{
		//Pick a random card from the turn ribbon
		int randomCardInTurnRibbon = Random.Range(0, turnRibbonList.Count);
		CardName stolenCardName = turnRibbonList[randomCardInTurnRibbon].name;

		//Get data about the card - make sure NOT to modify the card data
		CardManager.CardData stolenCard = CardManager.Instance.getCardByName( stolenCardName );
		//Temporarily replace the image on the button that was clicked by a blank image

		Button buttonOfCardPlayed = turnRibbonButtonList[randomCardInTurnRibbon];
		buttonOfCardPlayed.GetComponent<CardTurnRibbon>().overrideCardImage( stolenCardSprite );
		//Card name text and power cost text
		buttonOfCardPlayed.GetComponent<CardTurnRibbon>().configureCardTexts( string.Empty, string.Empty, LocalizationManager.Instance.getText("CARD_STOLEN") );

		//Wait a little before moving the Next Card into the free ribbon slot
		StartCoroutine( moveNextCardIntoTurnRibbon( randomCardInTurnRibbon, stolenCard ) );

		return stolenCardName;
	}

	public void replaceCard( CardName stolenCardName )
	{
		CardManager.CardData stealCard = CardManager.Instance.getCardByName( CardName.Steal );
		int stealCardIndex = turnRibbonList.IndexOf(stealCard);
		CardManager.CardData stolenCard = CardManager.Instance.getCardByName( stolenCardName );
		stolenCard.isStolenCard = true;
		turnRibbonList[stealCardIndex] = stolenCard;
		Debug.LogWarning("TurnRibbonHandler-replaceCard " + stealCard.name + " by " + stolenCardName );

		Button buttonOfCardPlayed = turnRibbonButtonList[stealCardIndex];
		buttonOfCardPlayed.interactable = true;
		buttonOfCardPlayed.GetComponent<CardTurnRibbon>().overrideCardImage( null );
		buttonOfCardPlayed.GetComponent<CardTurnRibbon>().cardImage.sprite = stolenCard.icon;
		//Card name text and power cost text
		buttonOfCardPlayed.GetComponent<CardTurnRibbon>().cardName.text = getCardNameAsString( stolenCard.name );
		buttonOfCardPlayed.GetComponent<CardTurnRibbon>().powerCost.text = stolenCard.manaCost.ToString();

	}
	#endregion

	int getUniqueRandom()
	{
		if(cardIndexList.Count == 0 )
		{
			Debug.LogError("TurnRibbonHandler-getUniqueRandom: cardIndexList is empty.");
			return -1;
		}
		int rand = Random.Range(0, cardIndexList.Count);
		int value = cardIndexList[rand];
		cardIndexList.RemoveAt(rand);
		return value;
	}

	public void increaseRefillRateForSupercharger()
	{
		powerBar.increaseRefillRateForSupercharger();
	}

	public void increaseRefillRate()
	{
		powerBar.increaseRefillRate();
	}

	public void resetRefillRate()
	{
		if( playerSpell.isCardActive(CardName.Supercharger) || playerRace.isPowerBoostActivated() ) return;
		powerBar.resetRefillRate();
	}

	string getCardNameAsString( CardName cardName )
	{
		return LocalizationManager.Instance.getText( "CARD_NAME_" + cardName.ToString().ToUpper() );
	}
}
