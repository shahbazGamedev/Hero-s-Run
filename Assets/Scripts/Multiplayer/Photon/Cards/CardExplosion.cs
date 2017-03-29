﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Explosion card is a Rare card with 11 levels. The explosion is centered around the player activating the card. The player is immune to its effect.
/// </summary>
public class CardExplosion : Photon.PunBehaviour {

	[SerializeField] float  baseDiameter = 10f;
	[SerializeField] float  diameterUpgradePerLevel = 0.25f;
	[SerializeField]  ParticleSystem zNukeEffect;
	int playerLayer = 2; //The player uses the Ignore Raycast layer which has an index of 2 and NOT the player layer which has an index of 8.
	int deviceLayer = 16;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardExplosionMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardExplosionMasterRPC( int level, int photonViewID )
	{
		//Find out which player activated card
		GameObject playerGameObject = null;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				playerGameObject = PlayerRace.players[i].gameObject;
			}
		}
		//Do an explosion effect centered on the player
		this.photonView.RPC("cardExplosionRPC", PhotonTargets.All, playerGameObject.name, playerGameObject.transform.position );
		//Determine which players are affected by the explosion. The caster obviously is immune.
		knockbackPlayers( playerGameObject.transform, level, photonViewID );
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

	void knockbackPlayers( Transform player, int level, int photonViewID )
	{
		float impactDiameter = baseDiameter + level * diameterUpgradePerLevel;
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
			else if( hitColliders[i].CompareTag("Device") )
			{
				hitColliders[i].GetComponent<Device>().changeDeviceState( DeviceState.Broken, player.name );
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
	void cardExplosionRPC( string casterName, Vector3 explosionPosition )
	{
		ParticleSystem explosionEffect = GameObject.Instantiate( zNukeEffect );
		explosionEffect.transform.position = explosionPosition;
		explosionEffect.GetComponent<AudioSource>().Play ();
		explosionEffect.Play();
		//Indicate on the minimap which card was played
		MiniMap.Instance.updateCardFeed( casterName, CardName.Explosion );
	}

}
