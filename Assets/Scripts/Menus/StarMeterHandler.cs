using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class StarMeterHandler : MonoBehaviour {


	[Header("Star Meter")]
	public Slider starMeterSlider;
	public Text coinMeterScore;
	public RectTransform StarMarkersPanel;
	public RectTransform leftStarMarker;
	public RectTransform middleStarMarker;
	public RectTransform rightStarMarker;
	public Image leftStar;
	public Image middleStar;
	public Image rightStar;
	public Color32 starReceivedColor;
	public float scoreSpinDuration = 2f;
	string scoreString; //format is Stars: <0>
	const float UPDATE_SEQUENCE_DELAY = 0.9f;
	//The end of the star slider is 10% longer than the highest number of stars for that episode (the z value).
	float maxNumberOfStars = 0;

	void Awake()
	{
		scoreString = LocalizationManager.Instance.getText("MENU_COINS");
		//Replace the string <0> by 0 initially
		coinMeterScore.text = scoreString.Replace( "<0>", LevelManager.Instance.getCoinsAtLastCheckpoint().ToString() );
	}
		
	void Start()
	{
		updatePositionOfStarMarkers();
	}

	void updatePositionOfStarMarkers()
	{
		LevelData.EpisodeInfo selectedEpisode = LevelManager.Instance.getCurrentEpisodeInfo();
		Vector3 starsRequired = selectedEpisode.coinsRequired;
		maxNumberOfStars = 1.1f * starsRequired.z;
		float sliderWidth = StarMarkersPanel.rect.width;

		float xPosition = (starsRequired.x/maxNumberOfStars ) * sliderWidth;
		leftStarMarker.anchoredPosition = new Vector2( xPosition,leftStarMarker.anchoredPosition.y);

		xPosition = (starsRequired.y/maxNumberOfStars ) * sliderWidth;
		middleStarMarker.anchoredPosition = new Vector2( xPosition,middleStarMarker.anchoredPosition.y);

		xPosition = (starsRequired.z/maxNumberOfStars ) * sliderWidth;
		rightStarMarker.anchoredPosition = new Vector2( xPosition,rightStarMarker.anchoredPosition.y);

	}

	public IEnumerator startUpdateSequence( LevelData.EpisodeInfo selectedEpisode )
	{
		//Wait for the post-level popup to have finished sliding in before spinning values
		yield return new WaitForSeconds(UPDATE_SEQUENCE_DELAY);
		StartCoroutine( spinScoreNumber( LevelManager.Instance.getScore(), maxNumberOfStars ) );
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
			coinMeterScore.text = scoreString.Replace( "<0>", currentNumber.ToString("N0") );
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
				//Replace the string <0> by the score value
				coinMeterScore.text = scoreString.Replace( "<0>", newScore.ToString("N0") );
				starMeterSlider.value = newScore/maxNumberOfStars;
				updateDisplayStars( newScore, selectedEpisode );
			break;	        
		}
	}

	void updateDisplayStars( int newScore, LevelData.EpisodeInfo selectedEpisode )
	{
		int numberOfStars = 0;

		if ( newScore >= selectedEpisode.coinsRequired.x && newScore < selectedEpisode.coinsRequired.y )
		{
			numberOfStars = 1;
		}
		else if ( newScore >= selectedEpisode.coinsRequired.y && newScore < selectedEpisode.coinsRequired.z )
		{
			numberOfStars = 2;
		}
		else if ( newScore >= selectedEpisode.coinsRequired.z )
		{
			numberOfStars = 3;
		}

		switch (numberOfStars)
		{
			case 0:
			    //Do nothing
				break;
			case 1:
				leftStar.color = starReceivedColor;
				break;
			case 2:
				leftStar.color = starReceivedColor;
				middleStar.color = starReceivedColor;
				break;
			case 3:
				leftStar.color = starReceivedColor;
				middleStar.color = starReceivedColor;
				rightStar.color = starReceivedColor;
				break;
		}
 	
	}

}
