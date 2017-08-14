using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {

	public ParticleSystem effect;
	
	void OnTriggerEnter(Collider other)
	{
		if( other.CompareTag("Player") )
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
		LevelManager.Instance.incrementNumberOfCheckpointsPassed();
		int numberOfCoinsAtCheckpoint = LevelManager.Instance.getScore();
		LevelManager.Instance.setCoinsAtLastCheckpoint( numberOfCoinsAtCheckpoint );
		//Save the player stats before continuing
		PlayerStatsManager.Instance.savePlayerStats();
		Debug.Log("Checkpoint activated " + gameObject.transform.parent.name );
		//EPISODE_NAME_X is the text ID to use to get the localised episode name where X is the episode name indexed starting at 1.
		int episodeNumber = LevelManager.Instance.getCurrentEpisodeNumber();
		string episodeNumberString = (episodeNumber + 1).ToString();
		string episodeName = LocalizationManager.Instance.getText("EPISODE_NAME_" + episodeNumberString );
		DialogManager.dialogManager.activateDisplayFairy( episodeName + ": " + LocalizationManager.Instance.getText("CHECKPOINT_REACHED"), 5.5f );
	}
}
