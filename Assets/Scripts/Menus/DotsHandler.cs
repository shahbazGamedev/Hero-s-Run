using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DotsHandler : MonoBehaviour {

	[SerializeField] List<Image> dotsList = new List<Image>();
	public int activePanel; //0 means left most, 1 means center or rightmost, 2 means rightmost

	void Start()
	{
		if( dotsList.Count == 2 )
		{
			activePanel = 0;
		}
		else if( dotsList.Count == 3 )
		{
			activePanel = 2;
		}
	}

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
					activePanel = 0;
				}
				//rightmost entry
				else if( xPosition == 1f ) 			
				{
					dotsList[1].color = Color.white;
					activePanel = 1;
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
					activePanel = 0;
				}
				//middle entry
				else if( xPosition == 0.5f )
				{
					dotsList[1].color = Color.white;
					activePanel = 1;
				}
				//rightmost entry
				else if( xPosition == 1f )
				{
					dotsList[2].color = Color.white;
					activePanel = 2;
				}
				else
				{
					dotsList[i].color = Color.gray;
				}
			}
		}
	}

}
