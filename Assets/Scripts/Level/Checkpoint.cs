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
		audio.Play();
		LevelManager.Instance.incrementNextLevelToComplete();
		//Save the player stats before continuing
		PlayerStatsManager.Instance.savePlayerStats();
		Debug.LogWarning("Checkpoint activated " + gameObject.transform.parent.name );
		AchievementDisplay.activateDisplayFairy( LevelManager.Instance.getLevelInfo().LevelName + ": " + LocalizationManager.Instance.getText("CHECKPOINT_REACHED"), 0.35f, 5.5f );
	}
}
