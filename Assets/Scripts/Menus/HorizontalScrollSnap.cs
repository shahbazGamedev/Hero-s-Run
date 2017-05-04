using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
 
public class HorizontalScrollSnap : MonoBehaviour, IEndDragHandler, IBeginDragHandler
{
 	ScrollRect scrollRect;

	float lowerBoundaryR = 0.2f;
	float upperBoundaryR = 0.7f;

	float lowerBoundaryL = 0.3f;
	float upperBoundaryL = 0.8f;

	float lowerDestination = 0;
	float centerDestination = 0.5f;
	float upperDestination = 1f;

	float horizontalNormalizedPositionOnBeginDrag;

	void Awake()
	{
		scrollRect = GetComponent<ScrollRect>();
	}

    public void OnBeginDrag (UnityEngine.EventSystems.PointerEventData eventData)
    {
 		horizontalNormalizedPositionOnBeginDrag = scrollRect.horizontalNormalizedPosition;
	}

    public void OnEndDrag (UnityEngine.EventSystems.PointerEventData eventData)
    {
 		StartCoroutine( snapToPosition( 0.2f ) );
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
		bool isGoingRight = currentPosition - horizontalNormalizedPositionOnBeginDrag > 0;
		if( isGoingRight )
		{
			if( currentPosition >= upperBoundaryR )
			{
				return upperDestination;
			}
			else if( currentPosition <= lowerBoundaryR )
			{
				return lowerDestination;
			}
			else
			{
				return centerDestination;
			}
		}
		else
		{
			if( currentPosition <= upperBoundaryL && currentPosition >= lowerBoundaryL )
			{
				return centerDestination;
			}
			else if( currentPosition < lowerBoundaryL )
			{
				return lowerDestination;
			}
			else
			{
				return upperDestination;
			}
		}
	}
}

