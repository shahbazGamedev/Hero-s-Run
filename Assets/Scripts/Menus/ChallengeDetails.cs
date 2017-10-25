using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChallengeDetails : MonoBehaviour {

	public Text numberOfChallengersText;

	public void configure ( int episodeNumber )
	{
		int numberOfChallengers = GameManager.Instance.challengeBoard.getNumberOfUnstartedChallenges( episodeNumber );
		if( numberOfChallengers == 0 )
		{
			gameObject.SetActive( false );
		}
		else
		{
			gameObject.SetActive( true );
			numberOfChallengersText.text = numberOfChallengers.ToString();
		}
	}
}
