using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardPropertyUI : MonoBehaviour {

	[SerializeField] Image propertyBackground;
	[SerializeField] Image propertyIcon;
	[SerializeField] Text propertyTitle;
	[SerializeField] Text propertyValue;
	Color lightBackground = new Color( 180f/255f, 180f/255f, 180f/255f, 0.5f );
	Color darkerBackground = Color.gray;

	public void configureProperty( int index, CardManager.CardProperty cp, PlayerDeck.PlayerCardData pcd, CardManager.CardData cd, bool displayIncrease )
	{
		//Alternate light and dark backgrounds to increase legibility
		if( index == 0 || index == 1 || index == 4 || index == 5 )
		{
			propertyBackground.color = darkerBackground;
		}
		else
		{
			propertyBackground.color = lightBackground;
		}
		propertyIcon.sprite = CardManager.Instance.getCardPropertyIcon( cp.type );
		propertyTitle.text = LocalizationManager.Instance.getText( "CARD_PROPERTIES_" + cp.type.ToString() );
		if( cp.type == CardPropertyType.ACCURACY )
		{
			//Convert to percentage
			float valueAsPercentage = 1f - cd.getCardPropertyValue( cp.type, pcd.level );
			propertyValue.text = string.Format("{0:P}", valueAsPercentage );
		}
		else if( cp.type == CardPropertyType.RANGE )
		{
			float range = cd.getCardPropertyValue( cp.type, pcd.level );
			if( range == -1f )
			{
				//Range is infinite
				propertyValue.text = LocalizationManager.Instance.getText( "CARD_PROPERTIES_INFINITE" );
			}
			else
			{
				propertyValue.text = string.Format( range.ToString() + " {0}", CardManager.Instance.getCardPropertyValueType( cp.type ) );
			}
		}
		else if( cp.type == CardPropertyType.TARGET )
		{
			CardPropertyTargetType targetType = cd.getCardPropertyTargetType();
			propertyValue.text = LocalizationManager.Instance.getText( "CARD_PROPERTIES_TARGET_" + targetType.ToString() );
		}
		else
		{
			propertyValue.text = string.Format( cd.getCardPropertyValue( cp.type, pcd.level ).ToString() + " {0}", CardManager.Instance.getCardPropertyValueType( cp.type ) );
		}
		if( displayIncrease ) displayPropertyIncrease( cp, pcd, cd );
	}

	void displayPropertyIncrease( CardManager.CardProperty cp, PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		//At this point, the card ALREADY has been upgraded by one level
		if( cp.type == CardPropertyType.ACCURACY )
		{
			//Convert to percentage
			float previousValueAsPercentage = 1f - cd.getCardPropertyValue( cp.type, pcd.level - 1 );
			float currentValueAsPercentage = 1f - cd.getCardPropertyValue( cp.type, pcd.level );
			float increase = currentValueAsPercentage - previousValueAsPercentage;
			propertyValue.text = propertyValue.text + "  <color=#1CF26DFF>+" + string.Format("{0:P}", increase ) + "</color>";
		}
		else if( cp.type == CardPropertyType.RANGE )
		{
			float range = cd.getCardPropertyValue( cp.type, pcd.level );
			if( range != -1f )
			{
				//Range is not infinite
				float previousValue = cd.getCardPropertyValue( cp.type, pcd.level - 1 );
				float currentValue = cd.getCardPropertyValue( cp.type, pcd.level );
				float increase = currentValue - previousValue;
				propertyValue.text = propertyValue.text + "  <color=#1CF26DFF>+" + increase.ToString("N0") + "</color>";
			}
		}
		else if( cp.type == CardPropertyType.TARGET )
		{
			//Do nothing
		}
		else
		{
			float previousValue = cd.getCardPropertyValue( cp.type, pcd.level - 1 );
			float currentValue = cd.getCardPropertyValue( cp.type, pcd.level );
			float increase = currentValue - previousValue;
			propertyValue.text = propertyValue.text + "  <color=#1CF26DFF>+" + increase.ToString("0.##") + "</color>";
		}
	}
}
