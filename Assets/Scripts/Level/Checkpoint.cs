using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {

	public ParticleSystem effect;
	
	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" )
		{
			activateCheckpoint();
		}
	}

	void OnEnable ()
	{
		if( effect != null ) effect.Play();
	}

	void activateCheckpoint ()
	{
		//Reset the number of times the player died in the level
		PlayerStatsManager.Instance.resetTimesPlayerRevivedInLevel();
		GetComponent<AudioSource>().Play();
		LevelManager.Instance.incrementNextLevelToComplete();
		LevelManager.Instance.setLevelNumberOfLastCheckpoint (LevelManager.Instance.getNextLevelToComplete() );
		//Save the player stats before continuing
		PlayerStatsManager.Instance.savePlayerStats();
		Debug.LogWarning("Checkpoint activated " + gameObject.transform.parent.name );
		AchievementDisplay.achievementDisplay.activateDisplayFairy( LevelManager.Instance.getCurrentLevelName() + ": " + LocalizationManager.Instance.getText("CHECKPOINT_REACHED"), 5.5f );
	}
}
