using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PowerUpTimer : MonoBehaviour {

	public Text timeRemaining;
	private IEnumerator coroutine;
	
	public void startTimer( float duration )
	{
		gameObject.SetActive( true );
		if( coroutine != null ) StopCoroutine( coroutine );
		coroutine = startCountdown( duration );
		StartCoroutine( coroutine );
	}

	public void stopTimer()
	{
		if( coroutine != null ) StopCoroutine( coroutine );
		gameObject.SetActive( false );
	}

	IEnumerator startCountdown( float duration )
	{
		int startValue = (int)duration;
		int countdown = startValue;
		Debug.Log( "startCountdown " + startValue );
		while (countdown > 0)
		{
			if( countdown < 10 )
			{
				timeRemaining.text = "0:0" + countdown.ToString();
			}
			else
			{
				timeRemaining.text = "0:" + countdown.ToString();
			}
			//Slow time may be happening. We still want the countdown to be one second between count
			yield return new WaitForSeconds(1.0f);
			countdown --;
		}
		//Duration has expired.
		gameObject.SetActive( false );
		
	}

}
