using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class CardUIUpgrade : MonoBehaviour, IPointerDownHandler
{
	[Header("General")]
	[Tooltip("The card image.")]
	[SerializeField] Image cardImage;
	[Tooltip("The card name.")]
	[SerializeField] Text cardName;

	[Header("Level")]
	[Tooltip("The level text is displayed on top of the card image. For example: 'Level 5' or 'Max Level'. The text color varies with the card rarity.")]
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
	[Header("Properties Panel")]
	[SerializeField] RectTransform propertiesPanel;
	[SerializeField] GameObject cardPropertyPrefab;

	Color NOT_ENOUGH_CARDS_TO_UPGRADE = Color.blue;
	Color ENOUGH_CARDS_TO_UPGRADE = Color.green;
	Color MAXED_OUT = Color.red;

	public void configureUpgradePanel( PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		gameObject.SetActive( true );

		//Card name
		string localizedCardName = LocalizationManager.Instance.getText( "CARD_NAME_" + pcd.name.ToString().ToUpper() );
		cardName.text = localizedCardName;

		//Card image
		cardImage.sprite = cd.icon;


		//Level section
		//Level background
		Color rarityColor;
		ColorUtility.TryParseHtmlString (CardManager.Instance.getCardColorHexValue(cd.rarity), out rarityColor);
		if( levelText != null ) levelText.color = rarityColor;

		//Level text and numberOfCardsForUpgrade
		int numberOfCardsForUpgrade;
		if( pcd.level + 1 < CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ) )
		{
			numberOfCardsForUpgrade = CardManager.Instance.getNumberOfCardsRequiredForUpgrade( pcd.level + 1, cd.rarity );
			if( levelText != null ) levelText.text = String.Format( "Level {0}", pcd.level.ToString() );
		}
		else
		{
			numberOfCardsForUpgrade = CardManager.Instance.getNumberOfCardsRequiredForUpgrade( CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ), cd.rarity );
			if( levelText != null ) levelText.text = "Max Level";
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

		//Configure card properties
		configureCardProperties( pcd, cd );
	}

	void configureCardProperties( PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		if( propertiesPanel == null ) return;

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

    public void OnPointerDown(PointerEventData data)
    {
		gameObject.SetActive( false );
		transform.parent.gameObject.SetActive( false );
    }
}
