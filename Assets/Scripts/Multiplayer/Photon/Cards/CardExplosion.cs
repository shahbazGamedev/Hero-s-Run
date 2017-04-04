using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Explosion card is a Rare card with 11 levels. The explosion is centered around the player activating the card. The player is immune to its effect.
/// </summary>
public class CardExplosion : Card {

	[SerializeField] float  baseDiameter = 10f;
	[SerializeField] float  diameterUpgradePerLevel = 0.25f;
	[SerializeField]  ParticleSystem zNukeEffect;
	int playerLayer = 8;
	int deviceLayer = 16;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardExplosionMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardExplosionMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Determine which players are affected by the explosion. The caster obviously is immune.
		float impactDiameter = baseDiameter + level * diameterUpgradePerLevel;
		blowUp( playerTransform, impactDiameter, photonViewID );

		//Do an explosion effect centered around the player
		this.photonView.RPC("cardExplosionRPC", PhotonTargets.All, playerTransform.position, impactDiameter );

	}

	int getExplosionMask()
	{
		int mask = 1 << playerLayer;
		mask |= 1 << deviceLayer;
		return mask;
	}

	/// <summary>
	/// Gets the dot product.
	/// </summary>
	/// <returns><c>true</c>, if the explosion is in front of the player, <c>false</c> otherwise.</returns>
	/// <param name="player">Player.</param>
	/// <param name="explosionLocation">Explosion location.</param>
	bool getDotProduct( Transform player, Vector3 explosionLocation )
	{
		Vector3 forward = player.TransformDirection(Vector3.forward);
		Vector3 toOther = explosionLocation - player.position;
		if (Vector3.Dot(forward, toOther) < 0)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	void blowUp( Transform player, float impactDiameter, int photonViewID )
	{
		Collider[] hitColliders = Physics.OverlapSphere( player.position, impactDiameter, getExplosionMask() );
		for( int i =0; i < hitColliders.Length; i++ )
		{
			//Verify that it is a player
			if( hitColliders[i].CompareTag("Player") )
			{
				//Verify that it is NOT the player activating the card
				if( hitColliders[i].GetComponent<PhotonView>().viewID != photonViewID )
				{
					//Verify that the player is not in the idle character state.
					//The player will be in the idle state after crossing the finish line for example.
					if( hitColliders[i].GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Idle )
					{
						//The explosion knocked down a player. Send him an RPC.
						if( getDotProduct( hitColliders[i].transform, player.position ) )
						{
							//Explosion is in front of player. He falls backward.
							hitColliders[i].GetComponent<PhotonView>().RPC("playerDied", PhotonTargets.All, DeathType.Obstacle );
						}
						else
						{
							//Explosion is behind player. He falls forward.
							hitColliders[i].GetComponent<PhotonView>().RPC("playerDied", PhotonTargets.All, DeathType.FallForward );
						}
					}
				}
			}
		}
	}
	#endregion

	public bool isOpponentNear( Transform caster, int level )
	{
		float impactDiameter = baseDiameter + level * diameterUpgradePerLevel;
		Collider[] hitColliders = Physics.OverlapSphere( caster.position, impactDiameter, getExplosionMask() );
		for( int i =0; i < hitColliders.Length; i++ )
		{
			//Verify that it is a player
			if( hitColliders[i].CompareTag("Player") )
			{
				//Verify that it is NOT the caster activating the card
				if( hitColliders[i].GetComponent<PhotonView>().viewID != caster.GetComponent<PhotonView>().viewID )
				{
					//Verify that the target is not in the Idle character state.
					//The target will be in the Idle state after crossing the finish line for example.
					if( hitColliders[i].GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Idle )
					{
						//We found at least one target for the explosion
						return true;
					}
				}
			}
		}
		//No target were found
		return false;
	}

	[PunRPC]
	void cardExplosionRPC( Vector3 explosionPosition, float impactDiameter )
	{
		ParticleSystem explosionEffect = GameObject.Instantiate( zNukeEffect );
		explosionEffect.transform.position = explosionPosition;
		explosionEffect.GetComponent<AudioSource>().Play ();
		explosionEffect.Play();
		blowUpLocalDevices(explosionPosition, impactDiameter );
	}

	/// <summary>
	/// Blows up local devices.
	/// In a perfect world, the MasterClient would decide if a device broke because of the explosion and send an RPC to the affected device.
	/// However, because the tiles containing the devices such as teleporters and jump pads are not network instantiated (and that would be overkill),
	/// I can't add a PhotonView component on the device (it throws an illegal view exception). Therefore, I can't send an RPC. 
	/// Doing it locally is an easy yet non-ideal workaround. It does have the benefit of reducing the number of RPCs required however.
	/// </summary>
	/// <param name="explosionPosition">Explosion position.</param>
	/// <param name="impactDiameter">Impact diameter.</param>
	void blowUpLocalDevices( Vector3 explosionPosition, float impactDiameter )
	{
		Collider[] hitColliders = Physics.OverlapSphere( explosionPosition, impactDiameter, getExplosionMask() );
		for( int i =0; i < hitColliders.Length; i++ )
		{
			if( hitColliders[i].CompareTag("Device") )
			{
				//The explosion can also break jump pads and teleporters
				hitColliders[i].GetComponent<Device>().changeDeviceState( DeviceState.Broken );
			}
		}
	}

}
