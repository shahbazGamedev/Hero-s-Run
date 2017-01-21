using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DotsHandler : MonoBehaviour {

	[SerializeField] List<Image> dotsList = new List<Image>();
	float scrollBarPreviousValue = 0;

	public void OnValueChanged( float value )
	{
		if( value != scrollBarPreviousValue )
		{
			scrollBarPreviousValue = value;
			for( int i = 0; i < dotsList.Count; i ++ )
			{
				if( i == value )
				{
					dotsList[i].color = Color.white;
				}
				else
				{
					dotsList[i].color = Color.gray;
				}
			}
			LevelManager.Instance.setCurrentMultiplayerLevel( (int) value );
 		}
	}
}
