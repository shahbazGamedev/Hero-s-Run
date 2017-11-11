using UnityEngine;
using System.Collections;

public class TorchHandler : MonoBehaviour {

	bool hasBeenTriggered = false;

	void OnEnable()
	{
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
		if( !LevelManager.Instance.getEnableTorches() )
		{
			extinguishTorch();
		}
	}
	
	void OnDisable()
	{
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
	}
	
	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.ExtinguishTorch && !hasBeenTriggered )
		{
			hasBeenTriggered = true;
			extinguishTorch();
		}
	}
	
	void extinguishTorch()
	{
		transform.Find("torch fire").GetComponent<ParticleSystem>().Stop();
		transform.Find("torch light").GetComponent<Light>().enabled = false;
	}
}
