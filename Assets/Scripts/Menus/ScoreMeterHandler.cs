using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScoreMeterHandler : MonoBehaviour {


	[Header("Score Meter")]
	public Text scoreText;
	public const float SCORE_SPIN_DURATION = 1.8f;
	public string labelTextID;

	public void resetScore()
	{
		string labelString = LocalizationManager.Instance.getText(labelTextID);
		//Replace the string <0> by 0
		scoreText.text = labelString.Replace( "<0>", "0" );
	}

	public IEnumerator spinScoreNumber( int number, System.Action onFinish = null  )
	{
		string labelString = LocalizationManager.Instance.getText(labelTextID);
		float startTime = Time.time;
		float elapsedTime = 0;
		float currentNumber = 0;

		int startValue = 0;

		while ( elapsedTime <= SCORE_SPIN_DURATION )
		{
			elapsedTime = Time.time - startTime;

			currentNumber =  Mathf.Lerp( startValue, number, elapsedTime/SCORE_SPIN_DURATION );
			//Replace the string <0> by the number
			scoreText.text = labelString.Replace( "<0>", currentNumber.ToString("N0") );
			yield return new WaitForFixedUpdate();  
	    }
		if( onFinish != null ) onFinish.Invoke();
	}

}
