using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StatisticEntryUI : MonoBehaviour {

	[SerializeField] Image propertyBackground;
	[SerializeField] Image propertyIcon;
	[SerializeField] Text propertyTitle;
	[SerializeField] Text propertyValue;
	Color lightBackground = new Color( 180f/255f, 180f/255f, 180f/255f, 0.5f );
	Color darkerBackground = Color.gray;

	public void configureEntry( int index, StatisticDataType type, Sprite icon, int value )
	{
		//Alternate light and dark backgrounds to increase legibility
		if( index == 0 || index == 1 || index == 4 || index == 5 || index == 8 || index == 9 )
		{
			propertyBackground.color = darkerBackground;
		}
		else
		{
			propertyBackground.color = lightBackground;
		}
		propertyIcon.sprite = icon;
		propertyTitle.text = LocalizationManager.Instance.getText( "STATISTICS_" + type.ToString() );
		if( type == StatisticDataType.DISTANCE_TRAVELED_LIFETIME )
		{
			propertyValue.text = value.ToString("N1") + "M";
		}
		else if( type == StatisticDataType.WIN_LOSS_RATIO )
		{
			float winLossRatio = GameManager.Instance.playerStatistics.getWinLoss();
			if( winLossRatio > 0 )
			{
				propertyValue.text = string.Format( "{0:P1}", winLossRatio );
			}
			else
			{
				propertyValue.text = LocalizationManager.Instance.getText( "STATISTICS_NOT_AVAILABLE" );
			}
		}
		else if( type == StatisticDataType.FAVORITE_CARD )
		{
			propertyValue.text = LocalizationManager.Instance.getText( "CARD_NAME_" + GameManager.Instance.playerDeck.getFavoriteCard().ToString().ToUpper() );
		}
		else
		{
			propertyValue.text = value.ToString("N0");
		}
	}
}
