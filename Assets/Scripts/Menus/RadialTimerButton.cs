using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class RadialTimerButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler {

	[SerializeField] Image radialImageMask;
	[SerializeField] float duration = 2f;
	[SerializeField] float delayBeforeCallingOnClick = 1f;
	public bool isActive = true;

	void Awake()
	{
		//The button should not be interactable.
		//If it were interactable, you could get an OnClick event if the player released the button even if the timer had not completed.
		GetComponent<Button>().interactable = false;
	}

	public void OnPointerDown(PointerEventData eventData )
	{
		if( isActive) StartCoroutine( animate() );
	}

	IEnumerator animate()
	{
		float startTime = Time.time;
		float elapsedTime = 0;

		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;

			radialImageMask.fillAmount =  Mathf.Lerp( 0, 1f, elapsedTime/duration );
			yield return new WaitForEndOfFrame();  
	    }
		radialImageMask.fillAmount =  0;
		Invoke( "CallOnClick", delayBeforeCallingOnClick );
	}

	public void OnPointerUp(PointerEventData eventData )
	{
		StopAllCoroutines();
		radialImageMask.fillAmount =  0;
	}

	void CallOnClick()
	{
		//This code will call the OnClick () event defined for this button in the Editor.
		GetComponent<Button>().interactable = true;
		PointerEventData pointer = new PointerEventData(EventSystem.current);
		ExecuteEvents.Execute( gameObject, pointer, ExecuteEvents.pointerClickHandler);
		GetComponent<Button>().interactable = false;
	}

}
