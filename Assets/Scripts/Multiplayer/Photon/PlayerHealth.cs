using System.Collections;
using UnityEngine;

public class PlayerHealth : Photon.PunBehaviour {

	public const int DEFAULT_HEALTH = 100;
	public int currentHealth = DEFAULT_HEALTH;
	HealthBarHandler healthBarHandler;
	PlayerAI playerAI;

	// Use this for initialization
	void Start ()
	{
		playerAI = GetComponent<PlayerAI>();
		healthBarHandler = HUDMultiplayer.hudMultiplayer.getHealthBarHandler();
	}

	public void deductAllHealth ()
	{
		if( PhotonNetwork.isMasterClient ) this.photonView.RPC("changeHealthRPC", PhotonTargets.All, 0 );
	}

	public void deductHealth ( int amountToDeduct )
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

			if( newHealth == 0 )
			{
				//Player died because his health is zero. Note that killPlayer checks for isMasterClient
				GetComponent<PlayerControl>().killPlayer( DeathType.NO_MORE_HEALTH );
			}
			else
			{
				if( PhotonNetwork.isMasterClient ) this.photonView.RPC("changeHealthRPC", PhotonTargets.All, newHealth );
			}
		}
	}

	public void resetHealth ()
	{
		if( PhotonNetwork.isMasterClient ) this.photonView.RPC("changeHealthRPC", PhotonTargets.All, DEFAULT_HEALTH );
	}

	public int getHealth ()
	{
		return currentHealth;
	}

	public bool isFullHealth()
	{
		return currentHealth == DEFAULT_HEALTH;
	}

	[PunRPC]
	void changeHealthRPC( int newHealth )
	{
		Debug.Log("changeHealthRPC received " + gameObject.name + " new health " + newHealth );
		if( photonView.isMine && playerAI == null )
		{
			if( newHealth == DEFAULT_HEALTH )
			{
				healthBarHandler.resetHealth();
			}
			else
			{
				healthBarHandler.changeHealth( currentHealth, newHealth );
			}
		}
		currentHealth = newHealth;
	}
	
}
