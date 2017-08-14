using System.Collections;
using UnityEngine;

public class PlayerHealth : Photon.PunBehaviour {

	public const int DEFAULT_HEALTH = 100;
	public int currentHealth = DEFAULT_HEALTH;
	HealthBarHandler healthBarHandler;

	// Use this for initialization
	void Start ()
	{
		healthBarHandler = HUDMultiplayer.hudMultiplayer.getHealthBarHandler();
		//Invoke("test", 10f) ;
	}

	void test ()
	{
		healthBarHandler.deductHealth( currentHealth, currentHealth - 50 );
	}

	public void deductHealth ( int amountToDeduct )
	{
		if( PhotonNetwork.isMasterClient )
		{
			if( amountToDeduct <= 0 )
			{
				Debug.LogWarning("PlayerHealth: you cannot deduct from health a value less or equal to 0.");
			}
			else
			{
				//Never deduct more than the current health
				amountToDeduct = Mathf.Min( currentHealth, amountToDeduct );

				int newHealth = currentHealth - amountToDeduct;

				healthBarHandler.deductHealth( currentHealth, newHealth );

				currentHealth = newHealth;

				if( newHealth == 0 )
				{
					//Player died because his health is zero.
					GetComponent<PlayerControl>().killPlayer( DeathType.NO_MORE_HEALTH );
				}
				else
				{
					this.photonView.RPC("changeHealthRPC", PhotonTargets.Others, newHealth );
				}
			}
		}
	}

	public void resetHealth ()
	{
		healthBarHandler.resetHealth();
		currentHealth = DEFAULT_HEALTH;
	}

	[PunRPC]
	void changeHealthRPC( int newHealth )
	{
		Debug.Log("changeHealthRPC received " + gameObject.name + " new health " + newHealth );
		currentHealth = newHealth;
	}
	
}
