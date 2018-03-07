using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardSaleUI : MonoBehaviour {

	[Header("Card Sale")]
	[SerializeField] TextMeshProUGUI cardName;
	[SerializeField] TextMeshProUGUI cardRarity;
	[SerializeField] TextMeshProUGUI cardQuantityText;
	[SerializeField] GameObject card;
	[SerializeField] GameObject progressBar;
	[SerializeField] GameObject costHolder;
	[SerializeField] TextMeshProUGUI costText;
	[SerializeField] GameObject purchasedHolder;

	public void configureCard ( PlayerDeck.PlayerCardData pcd, CardManager.CardData cd, int cardQuantity, int cost )
	{
		//Card name
		cardName.text = LocalizationManager.Instance.getText( "CARD_NAME_" + pcd.name.ToString().ToUpper() );
		//Card rarity
		cardRarity.text = LocalizationManager.Instance.getText( "CARD_RARITY_" + cd.rarity.ToString().ToUpper() );
		//Card quantity
		cardQuantityText.text = string.Format( LocalizationManager.Instance.getText( "STORE_QUANTITY" ), cardQuantity );
		//Listener
		Button cardButton = GetComponent<Button>();
		cardButton.onClick.RemoveAllListeners();
		cardButton.onClick.AddListener(() => OnClickCardOnSale( cd, cardQuantity, cost ));
		//Card
		card.GetComponent<CardUIDetails>().configureCard( pcd, cd );
		//Cost
		costText.text = cost.ToString();
	}

	public void OnClickCardOnSale( CardManager.CardData cd, int cardQuantity, int cost )
	{
		print("CardSaleUI " + cd.name );
		bool hasEnoughFunds = true;
		if( hasEnoughFunds )
		{
			progressBar.SetActive( false );
			purchasedHolder.SetActive( true );
			purchasedHolder.GetComponent<UIBounceEffect>().scaleUp();
			costHolder.SetActive( false );
			GetComponent<Button>().interactable = false;
			grantCards( cd, cardQuantity );
			GameManager.Instance.playerInventory.deductCoins( cost );
		}
		else
		{
			//Show purchase currency popup.
			//...
		}
	}
	
	void grantCards( CardManager.CardData cd, int cardQuantity )
	{
		//Does the player already own this card?
		PlayerDeck.PlayerCardData pcd = GameManager.Instance.playerDeck.getCardByName( cd.name );
		if( pcd == null )
		{
			//No, this is a new card.
			//Add it to the player's deck. Tag as new so the "New" ribbon gets displayed.
			//Since it's a new card, the level will be 1.
			//By using BattleDeck.REMOVE_FROM_ALL_BATTLE_DECKS, we are not assigning this new card toa deck. We will let the player do that.
			pcd = GameManager.Instance.playerDeck.addCard(  cd.name, 1, cardQuantity, BattleDeck.REMOVE_FROM_ALL_BATTLE_DECKS, HeroManager.Instance.isHeroCard( cd.name ), true );
		}
		else
		{
			//Yes, he already owns it
			//Add the cards obtained
			GameManager.Instance.playerDeck.changeCardQuantity(  pcd, cardQuantity );
		}
		//Save
		GameManager.Instance.playerDeck.serializePlayerDeck( true );

		//Refresh the card collections
		GameObject.FindObjectOfType<CardCollectionManager>().initialize ();
	}
}
