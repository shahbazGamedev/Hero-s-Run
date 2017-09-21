using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardTurnRibbon : MonoBehaviour {

	public Image cardImage;
	public Image radialMask;
	public TextMeshProUGUI cardName;
	public TextMeshProUGUI powerCost;
	public TextMeshProUGUI additionalText; //Used for Stolen, Hack, etc.

	public void configureTurnRibbonCard ( CardManager.CardData cardData )
	{
		cardImage.sprite = cardData.icon;
		//Card name text and power cost text
		cardName.text = LocalizationManager.Instance.getText( "CARD_NAME_" + cardData.name.ToString().ToUpper() );
		powerCost.text = cardData.manaCost.ToString();
		
	}

	public void configureCardTexts ( string cardNameString, string powerCostString, string additionalTextString )
	{
		cardName.text = cardNameString;
		powerCost.text = powerCostString;
		additionalText.text = additionalTextString;
	}

	public void changeCardNameColor ( Color textColor )
	{
		cardName.color = textColor;
	}

	public void overrideCardImage ( Sprite overrideSprite )
	{
		cardImage.overrideSprite = overrideSprite;
	}
	
}
