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

	public void configureProperty( int index, CardManager.CardProperty cp, PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
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
		else
		{
			propertyValue.text = string.Format( cd.getCardPropertyValue( cp.type, pcd.level ).ToString() + " {0}", CardManager.Instance.getCardPropertyValueType( cp.type ) );
		}
	}
}
