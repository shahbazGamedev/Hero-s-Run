using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class StarMeterHandler : MonoBehaviour {


	[Header("Star Meter")]
	public Slider starMeterSlider;
	public Text starMeterScore;
	public RectTransform firstStar;
	public RectTransform secondStar;
	public RectTransform thirdStar;
	public float scoreSpinDuration = 2f;
	string scoreString; //format is Score: <0>

	void Awake()
	{
		scoreString = LocalizationManager.Instance.getText("MENU_SCORE");

	}
	public void updateValues(LevelData.EpisodeInfo selectedEpisode )
	{
		updatePositionOfStars( selectedEpisode );
		//Wait for the panel to have finished sliding in before spinning values
		Invoke("startUpdateSequence", 0.9f );
	}
	
	void updatePositionOfStars(LevelData.EpisodeInfo selectedEpisode )
	{
		Vector4 starsRequired = selectedEpisode.starsRequired;
		float sliderWidth = starMeterSlider.GetComponent<RectTransform>().rect.width;

		Debug.Log("updatePositionOfStars " + starsRequired );

		float xPosition = (starsRequired.x/starsRequired.w ) * sliderWidth;
		firstStar.anchoredPosition = new Vector2( xPosition,firstStar.anchoredPosition.y);

		xPosition = (starsRequired.y/starsRequired.w ) * sliderWidth;
		secondStar.anchoredPosition = new Vector2( xPosition,secondStar.anchoredPosition.y);

		xPosition = (starsRequired.z/starsRequired.w ) * sliderWidth;
		thirdStar.anchoredPosition = new Vector2( xPosition,thirdStar.anchoredPosition.y);

	}
	void startUpdateSequence()
	{
		StartCoroutine( spinScoreNumber( 50000 ) );
	}

	IEnumerator spinScoreNumber( int playerScore )
	{
		float startTime = Time.time;
		float elapsedTime = 0;
		float currentNumber = 0;

		int startValue = 0;

		while ( elapsedTime <= scoreSpinDuration )
		{
			elapsedTime = Time.time - startTime;

			currentNumber =  Mathf.Lerp( startValue, playerScore, elapsedTime/scoreSpinDuration );
			starMeterSlider.value = currentNumber/100000f;
			//Replace the string <0> by the score value
			starMeterScore.text = scoreString.Replace( "<0>", currentNumber.ToString("N0") );
			yield return new WaitForFixedUpdate();  
	    }		
	}

}
