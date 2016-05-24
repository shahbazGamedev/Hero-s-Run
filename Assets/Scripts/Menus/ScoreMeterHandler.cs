using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScoreMeterHandler : MonoBehaviour {


	[Header("Score Meter")]
	public Text scoreText;
	public const float SCORE_SPIN_DURATION = 2f;
	string scoreString; //format is Score: <0>
	const float UPDATE_SEQUENCE_DELAY = 0.9f;

	void Awake()
	{
		scoreString = LocalizationManager.Instance.getText("MENU_SCORE");
	}
		
	public IEnumerator startUpdateSequence( LevelData.EpisodeInfo selectedEpisode )
	{
		//Wait for the post-level popup to have finished sliding in before spinning values
		yield return new WaitForSeconds(UPDATE_SEQUENCE_DELAY);
		StartCoroutine( spinScoreNumber( LevelManager.Instance.getScore() ) );
	}

	IEnumerator spinScoreNumber( int playerScore )
	{
		Debug.Log("spinScoreNumber " + playerScore );
		float startTime = Time.time;
		float elapsedTime = 0;
		float currentNumber = 0;

		int startValue = 0;

		while ( elapsedTime <= SCORE_SPIN_DURATION )
		{
			elapsedTime = Time.time - startTime;

			currentNumber =  Mathf.Lerp( startValue, playerScore, elapsedTime/SCORE_SPIN_DURATION );
			//Replace the string <0> by the score value
			scoreText.text = scoreString.Replace( "<0>", currentNumber.ToString("N0") );
			yield return new WaitForFixedUpdate();  
	    }
	}
}
