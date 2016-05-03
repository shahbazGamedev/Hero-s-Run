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
	const float UPDATE_SEQUENCE_DELAY = 0.9f;

	void Awake()
	{
		scoreString = LocalizationManager.Instance.getText("MENU_SCORE");
	}
		
	void Start()
	{
		updatePositionOfStarMarkers();
	}

	public void updatePositionOfStarMarkers()
	{
		LevelData.EpisodeInfo selectedEpisode = LevelManager.Instance.getCurrentEpisodeInfo();
		Vector4 starsRequired = selectedEpisode.starsRequired;
		float sliderWidth = starMeterSlider.GetComponent<RectTransform>().rect.width;
		Debug.Log("updatePositionOfStarMarkers " + starsRequired );

		float xPosition = (starsRequired.x/starsRequired.w ) * sliderWidth;
		firstStar.anchoredPosition = new Vector2( xPosition,firstStar.anchoredPosition.y);

		xPosition = (starsRequired.y/starsRequired.w ) * sliderWidth;
		secondStar.anchoredPosition = new Vector2( xPosition,secondStar.anchoredPosition.y);

		xPosition = (starsRequired.z/starsRequired.w ) * sliderWidth;
		thirdStar.anchoredPosition = new Vector2( xPosition,thirdStar.anchoredPosition.y);

	}

	public IEnumerator startUpdateSequence( LevelData.EpisodeInfo selectedEpisode )
	{
		//Wait for the post-level popup to have finished sliding in before spinning values
		yield return new WaitForSeconds(UPDATE_SEQUENCE_DELAY);
		StartCoroutine( spinScoreNumber( LevelManager.Instance.getScore(), selectedEpisode.starsRequired.w ) );
	}

	IEnumerator spinScoreNumber( int playerScore, float maxScore )
	{
		Debug.Log("spinScoreNumber " + playerScore );
		float startTime = Time.time;
		float elapsedTime = 0;
		float currentNumber = 0;

		int startValue = 0;

		while ( elapsedTime <= scoreSpinDuration )
		{
			elapsedTime = Time.time - startTime;

			currentNumber =  Mathf.Lerp( startValue, playerScore, elapsedTime/scoreSpinDuration );
			starMeterSlider.value = currentNumber/maxScore;
			//Replace the string <0> by the score value
			starMeterScore.text = scoreString.Replace( "<0>", currentNumber.ToString("N0") );
			yield return new WaitForFixedUpdate();  
	    }		
	}

	void OnEnable()
	{
		PlayerStatsManager.playerInventoryChanged += PlayerInventoryChanged;
	}
	
	void OnDisable()
	{
		PlayerStatsManager.playerInventoryChanged -= PlayerInventoryChanged;
	}

	void PlayerInventoryChanged( PlayerInventoryEvent eventType, int newScore )
	{
		switch (eventType)
		{
			case PlayerInventoryEvent.Score_Changed:
				LevelData.EpisodeInfo selectedEpisode = LevelManager.Instance.getCurrentEpisodeInfo();
				Debug.Log("StarMeterHandler - PlayerInventoryChanged: Episode number is: " + LevelManager.Instance.getCurrentEpisodeNumber() );
				//Replace the string <0> by the score value
				starMeterScore.text = scoreString.Replace( "<0>", newScore.ToString("N0") );
				starMeterSlider.value = newScore/selectedEpisode.starsRequired.w;

			break;	        
		}
	}

}
