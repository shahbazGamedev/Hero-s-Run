using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
 
public class HorizontalScrollSnap : MonoBehaviour, IEndDragHandler
{
 	ScrollRect scrollRect;

	void Awake()
	{
		scrollRect = GetComponent<ScrollRect>();
	}

    /// <summary>
    /// End drag event
    /// </summary>
    public void OnEndDrag (UnityEngine.EventSystems.PointerEventData eventData)
    {
		StartCoroutine( snapToPosition( 0.4f ) );
    }

	IEnumerator snapToPosition( float duration )
	{
		print("snapToPosition " + scrollRect.horizontalNormalizedPosition );
		float elapsedTime = 0;
		
		float startHorizontalPosition = scrollRect.horizontalNormalizedPosition;
		float endHorizontalPosition = 0;

		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			scrollRect.horizontalNormalizedPosition = Mathf.Lerp( startHorizontalPosition, endHorizontalPosition, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		scrollRect.horizontalNormalizedPosition = 0;
		print("snapToPosition after " + scrollRect.horizontalNormalizedPosition );
	}

}

