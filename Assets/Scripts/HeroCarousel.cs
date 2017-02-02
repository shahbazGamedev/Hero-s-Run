using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroCarousel : MonoBehaviour {

	ScrollRect scrollRect;
	int currentIndex = 0;
	const float STEP_VALUE = 1f/3f;

	// Use this for initialization
	void Awake () {
		
		scrollRect = GetComponent<ScrollRect>();
	}

	public void previousItem()
	{
		currentIndex--;
		if( currentIndex < 0 ) currentIndex = 3;
		scrollRect.horizontalNormalizedPosition = currentIndex * STEP_VALUE;
	}

	public void nextItem()
	{
		currentIndex++;
		if( currentIndex > 3 )
		{
			currentIndex = 0;
		}
		scrollRect.horizontalNormalizedPosition = currentIndex * STEP_VALUE;
	}

	public void normalizedPosition ( Vector2 value )
	{
		//print("normalizedPosition " + value.x );
	}
	
	// Update is called once per frame
	public void OnClickConfirm()
	{
		print("Confirm button pressed.");
	}
}
