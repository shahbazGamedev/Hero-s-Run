using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DotsHandler : MonoBehaviour {

	[SerializeField] List<Image> dotsList = new List<Image>();

	public void OnValueChanged( Vector2 value )
	{
		float xPosition = (float) Math.Round( value.x, 1 );
		//print("value.x " + xPosition );

		for( int i = 0; i < dotsList.Count; i ++ )
		{
			if( dotsList.Count == 2 )
			{
				//leftmost entry
				if( xPosition == 0 ) 					
				{
					dotsList[0].color = Color.white;
				}
				//rightmost entry
				else if( xPosition == 1f ) 			
				{
					dotsList[1].color = Color.white;
				}
				else
				{
					dotsList[i].color = Color.gray;
				}
			}
			else if( dotsList.Count == 3 )
			{
				//leftmost entry
				if( xPosition == 0 )
				{
					dotsList[0].color = Color.white;
				}
				//middle entry
				else if( xPosition == 0.5f )
				{
					dotsList[1].color = Color.white;
				}
				//rightmost entry
				else if( xPosition == 1f )
				{
					dotsList[2].color = Color.white;
				}
				else
				{
					dotsList[i].color = Color.gray;
				}
			}
		}
	}

}
