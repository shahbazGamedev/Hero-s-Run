using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DotsHandler : MonoBehaviour {

	[SerializeField] List<Image> dotsList = new List<Image>();
	[SerializeField] bool travelsLeft = true;
	[SerializeField] Color colorOn = Color.white;
	[SerializeField] Color colorOff = Color.gray;
	public int activePanel;

	void Start()
	{ 
		if( travelsLeft )
		{
			activePanel = dotsList.Count - 1;
		}
		else
		{
			activePanel = 0;
		}
	}

	public void OnValueChanged( Vector2 value )
	{
		float xPosition = (float) Math.Round( value.x, 1 );
		float increment = 1f/(dotsList.Count-1);
		for( float i = 0; i <= 1f; i = i + increment )
		{
			int index = (int) (i * (dotsList.Count-1));
			if( i == xPosition )
			{
				dotsList[index].color = colorOn;
				activePanel = index;
			}
			else			
			{
				dotsList[index].color = colorOff;
			}
		}
	}

}
