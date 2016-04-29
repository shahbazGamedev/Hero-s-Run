using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PowerUpTimer : BaseClass {

	public RectTransform timerHand;
	const float COMPLETE_ROTATION_IN_DEGREES = 315f;
	private IEnumerator coroutine;
	
	public void startTimer( float duration )
	{
		gameObject.SetActive( true );
		if( coroutine != null ) StopCoroutine( coroutine );
		coroutine = startTimerThread( duration );
		StartCoroutine( coroutine );
	}

	public void stopTimer()
	{
		if( coroutine != null ) StopCoroutine( coroutine );
		gameObject.SetActive( false );
	}

	IEnumerator startTimerThread( float duration )
	{
		float angle = 0;
		Vector3 angleVector = Vector3.zero;
		float elapsed = 0;
		
		do
		{
			elapsed = elapsed + Time.deltaTime;
			angle = elapsed/duration * COMPLETE_ROTATION_IN_DEGREES;
			angleVector.Set(0,0, angle );
			timerHand.localEulerAngles = angleVector;
			yield return _sync();
		} while ( elapsed < duration );
		//Duration has expired.
		gameObject.SetActive( false );
	}
}
