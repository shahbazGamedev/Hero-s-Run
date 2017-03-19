using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsMenu : MonoBehaviour {

	[SerializeField] Text numberRacesRunText;				//Player at least started the race, may did not finish it.
	[SerializeField] Text numberRacesWonText;				//Player completed race in first position.

	[SerializeField] Text distanceTravelledLifetimeText;	//Sum of the distance travelled for all the completed races.
	[SerializeField] Text numberOfDeathsLifetimeText;		//Sum of all the times the player died during a race, whether it was completed or not.
	[SerializeField] Text perfectRacesLifetimeText;			//Sum of all the races where the player did not die once during a completed race.

	[SerializeField] Text currentWinStreakText;				//The number of races won in a row. Resets as soon as you lose or abandon a race.
	[SerializeField] Text bestWinStreakLifetimeText;		//The player's best win streak ever.

	// Use this for initialization
	void Start ()
	{	
		PlayerStatistics playerStatistics = GameManager.Instance.playerStatistics;

		numberRacesRunText.text = numberRacesRunText.text + ": " + playerStatistics.numberRacesRun;
		numberRacesWonText.text = numberRacesWonText.text + ": " + playerStatistics.numberRacesWon;

		distanceTravelledLifetimeText.text = distanceTravelledLifetimeText.text + ": " + playerStatistics.distanceTravelledLifetime.ToString("N1") + "M";
		numberOfDeathsLifetimeText.text = numberOfDeathsLifetimeText.text + ": " + playerStatistics.numberOfDeathsLifetime;
		perfectRacesLifetimeText.text = perfectRacesLifetimeText.text + ": " + playerStatistics.perfectRacesLifetime;

		currentWinStreakText.text = currentWinStreakText.text + ": " + playerStatistics.currentWinStreak;
		bestWinStreakLifetimeText.text = bestWinStreakLifetimeText.text + ": " + playerStatistics.bestWinStreakLifetime;
	}
	
}
