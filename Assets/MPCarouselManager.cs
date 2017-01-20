using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MPCarouselManager : MonoBehaviour {

	//Variables for swipe
	bool touchStarted = false;
	Vector2 touchStartPos;
	float MINIMUM_HORIZONTAL_DISTANCE = 0.09f * Screen.width; //How many pixels you need to swipe horizontally to change page.
	public Scrollbar scrollbarIndicator;

	const int INDEX_OF_LAST_CIRCUIT = 1;
	int indexOfDisplayedCircuit = 0; 			//0 is the Royal Run, 1 is the Practice Run
	int previousIndexOfDisplayedCircuit = 0; 

	// Use this for initialization
	void Start () {
		
	}
	
	void Update ()
	{
		handleSwipes();
		#if UNITY_EDITOR
		handleKeyboard();
		#endif
	}

	void handleKeyboard()
	{
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.LeftArrow) ) 
		{
			sideSwipe( false );
		}
		else if ( Input.GetKeyDown (KeyCode.RightArrow) ) 
		{
			sideSwipe( true );
		}
	}

	void handleSwipes()
	{
		//Verify if the player swiped across the screen
		if (Input.touchCount > 0)
		{
           	Touch touch = Input.GetTouch( 0 );

			switch (touch.phase)
			{
			case TouchPhase.Began:
				touchStarted = true;
				touchStartPos = touch.position;
				break;
				
			case TouchPhase.Ended:
				if (touchStarted)
				{
					touchStarted = false;
				}
				break;
				
			case TouchPhase.Canceled:
				touchStarted = false;
				break;
				
			case TouchPhase.Stationary:
				break;
				
			case TouchPhase.Moved:
				if (touchStarted)
				{
					TestForSwipeGesture(touch);
				}
				break;
			}
		}	
		
	}

	void TestForSwipeGesture(Touch touch)
	{
		Vector2 lastPos = touch.position;
		float distance = Vector2.Distance(lastPos, touchStartPos);
		
		if (distance > MINIMUM_HORIZONTAL_DISTANCE )
		{
			touchStarted = false;
			float dy = lastPos.y - touchStartPos.y;
			float dx = lastPos.x - touchStartPos.x;
			
			float angle = Mathf.Rad2Deg * Mathf.Atan2(dx, dy);
			
			angle = (360 + angle - 45) % 360;
			
			if (angle < 90)
			{
				//player swiped RIGHT
				sideSwipe( true );
			}
			else if (angle < 180)
			{
				//player swiped DOWN
				//Ignore
			}
			else if (angle < 270)
			{
				//player swiped LEFT
				sideSwipe( false );
			}
			else
			{
				//player swiped UP
				//Ignore
			}
		}
	}

	void sideSwipe( bool isGoingRight )
	{
		if( isGoingRight )
		{
			indexOfDisplayedCircuit++;
			if( indexOfDisplayedCircuit > INDEX_OF_LAST_CIRCUIT ) indexOfDisplayedCircuit = 1;
		}
		else
		{
			indexOfDisplayedCircuit--;
			if( indexOfDisplayedCircuit < 0 ) indexOfDisplayedCircuit = INDEX_OF_LAST_CIRCUIT;
		}
		OnValueChanged( indexOfDisplayedCircuit );

	}

	void OnValueChanged( int newIndex )
	{
		if( newIndex == previousIndexOfDisplayedCircuit ) return; //Nothing has changed. Ignore.
		previousIndexOfDisplayedCircuit = newIndex;
		//Update the scrollbar indicator which is not interactable
		scrollbarIndicator.value = (float)newIndex/INDEX_OF_LAST_CIRCUIT;
	}
}
