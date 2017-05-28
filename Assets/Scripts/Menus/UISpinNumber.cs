using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISpinNumber : MonoBehaviour {

	TextMeshProUGUI text;

	void Start ()
	{
		text = GetComponent<TextMeshProUGUI>();
	}

	/// <summary>
	/// Spins the number.
	/// </summary>
	/// <param name="mainText">Main text. This method uses string.Format. The number will be inserted in the {0} location specified in the string.</param>
	/// <param name="fromValue">From value.</param>
	/// <param name="toValue">To value.</param>
	/// <param name="duration">Duration.</param>
	/// <param name="onFinish">On finish.</param>
	public void spinNumber ( string mainText, float fromValue, float toValue, float duration, bool onlySpinIfIncreasing, System.Action onFinish = null )
	{
		if( onlySpinIfIncreasing )
		{
			if( toValue > fromValue )
			{
				StartCoroutine( numberSpin(  mainText, fromValue, toValue, duration, onFinish ) );
			}
			else
			{
				text.text = string.Format( mainText, ((int)toValue).ToString("N0") );
			}
		}
		else
		{
			StartCoroutine( numberSpin(  mainText, fromValue, toValue, duration, onFinish ) );
		}
	}

	IEnumerator numberSpin( string mainText, float fromValue, float toValue, float duration, System.Action onFinish = null )
	{
		float startTime = Time.time;
		float elapsedTime = 0;
	
		float value = 0;
		int previousValue = -1;

		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;

			value =  Mathf.Lerp( fromValue, toValue, elapsedTime/duration );
			if( (int)value != previousValue )
			{
				text.text = string.Format( mainText, ((int)value).ToString("N0") );
				previousValue = (int)value;
			}
			yield return new WaitForEndOfFrame();  
	    }
		if( onFinish != null ) onFinish.Invoke();
	}

}
