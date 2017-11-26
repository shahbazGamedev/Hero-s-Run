using System.Collections;
using UnityEngine;

public class PlayerHealth : Photon.PunBehaviour {

	public const int DEFAULT_HEALTH = 100;
	public int currentHealth = DEFAULT_HEALTH;
	public const int MAXIMUM_ARMOR = 100;
	public int currentArmor = 0;
	HealthBarHandler healthBarHandler;
	PlayerAI playerAI;

	// Use this for initialization
	void Start ()
	{
		playerAI = GetComponent<PlayerAI>();
		healthBarHandler = HUDMultiplayer.hudMultiplayer.getHealthBarHandler();
	}

	#region Health
	public void deductHealth ( int amountToDeduct, PlayerControl playerWhoCausedDamage = null )
	{
		if( playerWhoCausedDamage )
		{
			Debug.Log("deductHealth " + gameObject.name + " amountToDeduct " + amountToDeduct + " currentHealth " + currentHealth + " playerWhoCausedDamage: " + playerWhoCausedDamage.name );
		}
		else
		{
			Debug.Log("deductHealth " + gameObject.name + " amountToDeduct " + amountToDeduct + " currentHealth " + currentHealth);
		}

		if( amountToDeduct <= 0 )
		{
			Debug.LogWarning("PlayerHealth: you cannot deduct from health a value less or equal to 0.");
		}
		else
		{

			//If the player has armor, deduct from armor before deducting from health
			if( currentArmor > 0 )
			{
				if( currentArmor >= amountToDeduct )
				{
					//The player has enough armor to absorb all of the damage
					removeArmor( amountToDeduct );
					return;
				}
				else
				{
					//The player can only absorb a portion of the damage.
					//Deduct the current armor from the damage.
					//All the armor is used up.
					amountToDeduct = amountToDeduct - currentArmor;
					removeArmor( currentArmor );
				}
			}

			//Never deduct more than the current health
			amountToDeduct = Mathf.Min( currentHealth, amountToDeduct );

			int newHealth = currentHealth - amountToDeduct;

			if( newHealth == 0 )
			{
				//Player died because his health is zero.
				GetComponent<PlayerControl>().killPlayer( DeathType.NO_MORE_HEALTH );
				//Give the kill to the player who caused the death blow
				if( playerWhoCausedDamage != null ) playerWhoCausedDamage.incrementKillCounter();
			}
			if( PhotonNetwork.isMasterClient ) this.photonView.RPC("changeHealthRPC", PhotonTargets.All, newHealth );
		}
	}

	public void deductAllHealth ()
	{
		//Debug.Log("deductAllHealth " + gameObject.name  );
		if( PhotonNetwork.isMasterClient ) this.photonView.RPC("changeHealthRPC", PhotonTargets.All, 0 );
	}

	public void resetHealth ()
	{
		//Debug.Log("resetHealth " + gameObject.name  );
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
		//Debug.Log("changeHealthRPC received " + gameObject.name + " currentHealth " + currentHealth + " new health " + newHealth );
		if( photonView.isMine && playerAI == null )
		{
			if( newHealth == DEFAULT_HEALTH )
			{
				healthBarHandler.resetHealth();
			}
			else
			{
				healthBarHandler.changeHealth( currentHealth, newHealth );
				//If the player is not dead and took damage, flash an effect on the HUD.
				if( newHealth < currentHealth && newHealth != 0 ) StartCoroutine( HUDMultiplayer.hudMultiplayer.displayDamageEffect() );
			}
		}
		currentHealth = newHealth;
	}
	#endregion

	#region Armor
	public void addArmor( int armorAdded )
	{
		if( armorAdded > MAXIMUM_ARMOR )
		{
			Debug.LogWarning("PlayerHealth: you cannot add more than " + MAXIMUM_ARMOR + " armor." );
		}
		else if( armorAdded <= 0 )
		{
			Debug.LogWarning("PlayerHealth: you cannot add zero or less armor." );
		}
		else
		{
			//Never go over the maximum allowed
			armorAdded = Mathf.Min( armorAdded, MAXIMUM_ARMOR - currentArmor );

			int newArmor = currentArmor + armorAdded;

			//Dont't send an RPC if the value has not changed
			if( newArmor != currentArmor )
			{
				if( PhotonNetwork.isMasterClient ) this.photonView.RPC("addArmorRPC", PhotonTargets.All, newArmor );
			}
		}
	}

	[PunRPC]
	void addArmorRPC( int newArmor )
	{
		Debug.Log("addArmorRPC received " + gameObject.name + " new armor " + newArmor );
		if( photonView.isMine && playerAI == null )
		{
			healthBarHandler.changeArmor( currentArmor, newArmor );
		}
		currentArmor = newArmor;
	}

	public void removeArmor( int armorToRemove )
	{
		if( armorToRemove <= 0 )
		{
			Debug.LogWarning("PlayerHealth: you cannot remove zero or less armor." );
		}
		else if( armorToRemove > MAXIMUM_ARMOR )
		{
			Debug.LogWarning("PlayerHealth: you cannot remove more than " + MAXIMUM_ARMOR + " armor." );
		}
		else
		{
			//Never remove more than current armor
			if( armorToRemove > currentArmor ) armorToRemove = currentArmor;

			int newArmor = currentArmor - armorToRemove;

			//Dont't send an RPC if the value has not changed
			if( newArmor != currentArmor )
			{
				if( PhotonNetwork.isMasterClient ) this.photonView.RPC("removeArmorRPC", PhotonTargets.All, newArmor );
			}
		}
	}

	[PunRPC]
	void removeArmorRPC( int newArmor )
	{
		Debug.Log("removeArmorRPC received " + gameObject.name );
		if( photonView.isMine && playerAI == null )
		{
			healthBarHandler.changeArmor( currentArmor, newArmor );
		}
		currentArmor = newArmor;
	}

	public void removeAllArmor()
	{
		if( PhotonNetwork.isMasterClient ) this.photonView.RPC("removeAllArmorRPC", PhotonTargets.All );
	}

	[PunRPC]
	void removeAllArmorRPC()
	{
		Debug.Log("removeAllArmorRPC received " + gameObject.name );
		if( photonView.isMine && playerAI == null )
		{
			healthBarHandler.removeAllArmor();
		}
		currentArmor = 0;
	}

	public int getArmor ()
	{
		return currentArmor;
	}
	#endregion

	#if UNITY_EDITOR
	void Update()
	{
		handleKeyboard();
	}

	private void handleKeyboard()
	{
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.Q) ) 
		{
			addArmor( 0 );
		}
		else if ( Input.GetKeyDown (KeyCode.W) ) 
		{
			addArmor( 100 );
		}
		else if ( Input.GetKeyDown (KeyCode.E) ) 
		{
			addArmor( 40 );
		}
		else if ( Input.GetKeyDown (KeyCode.R) ) 
		{
			addArmor( 1000 );
		}
		else if ( Input.GetKeyDown (KeyCode.T) ) 
		{
			removeAllArmor();
		}
		else if ( Input.GetKeyDown (KeyCode.Y) ) 
		{
			removeArmor( 20 );
		}
		else if ( Input.GetKeyDown (KeyCode.U) ) 
		{
			removeArmor( -1 );
		}
		else if ( Input.GetKeyDown (KeyCode.I) ) 
		{
			removeArmor( 120 );
		}
		else if ( Input.GetKeyDown (KeyCode.S) ) 
		{
			deductHealth( 0 );
		}
		else if ( Input.GetKeyDown (KeyCode.D) ) 
		{
			deductHealth( 1000 );
		}
		else if ( Input.GetKeyDown (KeyCode.F) ) 
		{
			deductHealth( 10 );
		}
		else if ( Input.GetKeyDown (KeyCode.G) ) 
		{
			resetHealth();
		}
	}
	#endif

}
