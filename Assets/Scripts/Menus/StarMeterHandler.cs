using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class StarMeterHandler : MonoBehaviour {


	[Header("Star Meter")]
	public Slider starMeterSlider;
	public Text starMeterScore;
	public float maxNumberStart = 75000; //for testing
	string scoreString; //format is Score: <0>

	void Awake()
	{
		scoreString = LocalizationManager.Instance.getText("MENU_SCORE");

	}
	public void updateValues(LevelData.EpisodeInfo selectedEpisode )
	{
		Debug.Log("Updating Star Meter.");
		//Wait for the panel to have finished sliding in before spinning values
		Invoke("startUpdateSequence", 0.9f );
	}
	
	void startUpdateSequence()
	{
		StartCoroutine( spinScoreNumber( 50000 ) );
	}

	IEnumerator spinScoreNumber( int playerScore )
	{
		float duration = 2.2f;
		float startTime = Time.time;
		float elapsedTime = 0;
		float currentNumber = 0;

		int startValue = 0;

		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;

			currentNumber =  Mathf.Lerp( startValue, playerScore, elapsedTime/duration );
			starMeterSlider.value = currentNumber/maxNumberStart;
			//Replace the string <0> by the score value
			starMeterScore.text = scoreString.Replace( "<0>", currentNumber.ToString("N0") );
			yield return new WaitForFixedUpdate();  
	    }		
	}

}
