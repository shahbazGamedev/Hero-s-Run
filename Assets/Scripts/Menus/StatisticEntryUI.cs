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
		if( index == 0 || index == 1 || index == 4 || index == 5 )
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
		else
		{
			propertyValue.text = value.ToString("N0");
		}
	}
}
