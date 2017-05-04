using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
 
public class HorizontalScrollSnap : MonoBehaviour, IEndDragHandler
{
 	ScrollRect scrollRect;

	float lowerBoundary = 0.25f;
	float centerBoundary = 0.5f;
	float upperBoundary = 0.75f;

	float lowerDestination = 0;
	float centerDestination = 0.5f;
	float upperDestination = 1f;

	void Awake()
	{
		scrollRect = GetComponent<ScrollRect>();
	}

    /// <summary>
    /// End drag event
    /// </summary>
    public void OnEndDrag (UnityEngine.EventSystems.PointerEventData eventData)
    {
		StartCoroutine( snapToPosition( 0.24f ) );
    }

	IEnumerator snapToPosition( float duration )
	{
		float elapsedTime = 0;
		
		float startHorizontalPosition = scrollRect.horizontalNormalizedPosition;
		float endHorizontalPosition = getDesiredPosition( scrollRect.horizontalNormalizedPosition );

		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			scrollRect.horizontalNormalizedPosition = Mathf.Lerp( startHorizontalPosition, endHorizontalPosition, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
	}

	float getDesiredPosition( float currentPosition )
	{
		if( currentPosition >= upperBoundary )
		{
			return upperDestination;
		}
		else if( currentPosition > lowerBoundary && currentPosition < upperBoundary )
		{
			return centerDestination;
		}
		else
		{
			return lowerDestination;
		}
	}
}

