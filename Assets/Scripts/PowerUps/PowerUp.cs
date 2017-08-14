using UnityEngine;
using System.Collections;

public enum ActivationType {

	NotSet = 0,
	Consumable = 1, 	//requires the player to double-tap to activate it
	Automatic = 2		//activates automatically
}

public class PowerUp : MonoBehaviour {
	
	//Power Up common properties
	public PowerUpType powerUpType = PowerUpType.None;
	public ActivationType activationType = ActivationType.NotSet;

	static PowerUpManager powerUpManager;
	
	void Awake()
	{
		GameObject powerUpManagerObject = GameObject.FindGameObjectWithTag("PowerUpManager");
		powerUpManager = powerUpManagerObject.GetComponent<PowerUpManager>();
	}

	void OnTriggerEnter(Collider other)
	{
		if( other.CompareTag("Player") )
		{
			if( activationType == ActivationType.Consumable )
			{
				//Add it to our inventory
				powerUpManager.pickUpPowerUp( powerUpType );
			}
			else if( activationType == ActivationType.Automatic )
			{
				//Activate it right away
				powerUpManager.activatePowerUp( powerUpType );
			}
			Destroy( gameObject );
		}
    }
}
