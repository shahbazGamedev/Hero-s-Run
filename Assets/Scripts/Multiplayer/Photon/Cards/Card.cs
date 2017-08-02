using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : Photon.PunBehaviour {

	public CardName  cardName;
	protected Vector3 spawnOffset = new Vector3( 0, 0, 10f );

	protected PlayerControl getPlayerControl( int photonViewID )
	{
		PlayerControl playerControl = null;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				playerControl = PlayerRace.players[i].GetComponent<PlayerControl>();
				break;
			}
		}
		return playerControl;
	}

	protected Transform getPlayerTransform( int photonViewID )
	{
		Transform playerTransform = null;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				playerTransform = PlayerRace.players[i].transform;
				break;
			}
		}
		return playerTransform;
	}

	/// <summary>
	/// Returns true if the player is immune to spells.
	/// A player is immune when in the Idle or Dying state.
	/// Having the Cloak card active also makes the player immune.
	/// </summary>
	/// <returns><c>true</c>, if the player is immune, <c>false</c> otherwise.</returns>
	/// <param name="player">Player.</param>
	protected bool isPlayerImmune( Transform player  )
	{
		if( player.GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Idle && player.GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Dying )
		{
			if( player.GetComponent<PlayerSpell>().isCardActive(CardName.Cloak) )
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else
		{
			return true;
		}
	}
	
	/// <summary>
	/// Returns the transform of the nearest player within spell range.
	/// </summary>
	/// <returns>The nearest target.</returns>
	/// <param name="playerRace">Player race.</param>
	/// <param name="spellRange">Spell range.</param>
	protected Transform detectNearestTarget( PlayerRace playerRace, float spellRange )
	{
		Transform nearestTarget = null;
		float nearestDistance = 100000;
		//Keep nearest target only
		for( int i =0; i < PlayerRace.players.Count; i++ )
		{
			//Ignore the caster
			if( PlayerRace.players[i] == playerRace ) continue;

			//Calculate the distance to the other player
			float distanceToTarget = Vector3.Distance( playerRace.transform.position, PlayerRace.players[i].transform.position );

			//Is this player within spell range?
			if( distanceToTarget > spellRange ) continue;

			//Is the player dead or Idle? If so, ignore.
			if( PlayerRace.players[i].GetComponent<PlayerControl>().deathType != DeathType.Alive || PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Idle ) continue;

			//Is the player using the Cloak card? If so, ignore.
			if( PlayerRace.players[i].GetComponent<PlayerSpell>().isCardActive(CardName.Cloak) ) continue;

			//Is it the closest player?
			if( distanceToTarget < nearestDistance )
			{
				nearestTarget = PlayerRace.players[i].transform;
				nearestDistance = distanceToTarget;
			}
		}
		return nearestTarget;
	}

	/// <summary>
	/// Returns a player target or null if none were found within the spell range.
	/// If multiple targets are within spell range, it will privilege returning a player that is in front of the caster.
	/// If all targets are behind the caster, it will return one at random.
	/// </summary>
	/// <returns>The random target.</returns>
	/// <param name="playerRace">Player race.</param>
	/// <param name="spellRange">Spell range.</param>
	protected Transform detectTarget( PlayerRace playerRace, float spellRange )
	{
		List<Transform> potentialTargets = new List<Transform>();
		
		for( int i =0; i < PlayerRace.players.Count; i++ )
		{
			//Ignore the caster
			if( PlayerRace.players[i] == playerRace ) continue;

			//Calculate the distance to the other player
			float distanceToTarget = Vector3.Distance( playerRace.transform.position, PlayerRace.players[i].transform.position );

			//Is this player within spell range?
			if( distanceToTarget > spellRange ) continue;

			//Is the player dead or Idle? If so, ignore.
			if( PlayerRace.players[i].GetComponent<PlayerControl>().deathType != DeathType.Alive || PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Idle ) continue;

			//Is the player using the Cloak card? If so, ignore.
			if( PlayerRace.players[i].GetComponent<PlayerSpell>().isCardActive(CardName.Cloak) ) continue;

			//We have a potential target
			potentialTargets.Add( PlayerRace.players[i].transform );
		}
		
		if( potentialTargets.Count == 0 )
		{
			return null;
		}
		else if ( potentialTargets.Count == 1 )
		{
			return potentialTargets[0];
		}
		else
		{
			//Privilege a player that is in front of you
			for( int i =0; i < potentialTargets.Count; i++ )
			{
				if( getDotProduct( playerRace.transform, potentialTargets[i].position ) ) return  potentialTargets[i];
			}
			//If we are here, it means that all targets are behind the player. Simply pick a random one.
			int random = Random.Range(0, potentialTargets.Count);
			return potentialTargets[random];				
		}
	}

	/// <summary>
	/// Gets the dot product.
	/// </summary>
	/// <returns><c>true</c>, if the other player is in front of the caster, <c>false</c> otherwise.</returns>
	/// <param name="caster">Caster.</param>
	/// <param name="otherPlayerPosition">The other player's position.</param>
	bool getDotProduct( Transform caster, Vector3 otherPlayerPosition )
	{
		Vector3 forward = caster.TransformDirection(Vector3.forward);
		Vector3 toOther = otherPlayerPosition - caster.position;
		if (Vector3.Dot(forward, toOther) < 0)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Returns true if there is a player target within spell range.
	/// </summary>
	/// <returns><c>true</c>, if there is a player target within range, <c>false</c> otherwise.</returns>
	/// <param name="caster">Caster.</param>
	/// <param name="level">Level.</param>
	public bool isThereATargetWithinRange( Transform caster, int level )
	{
		//See if there is a target
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		Transform nearestTarget = detectNearestTarget( caster.GetComponent<PlayerRace>(), cd.getCardPropertyValue( CardPropertyType.RANGE, level ) );

		return nearestTarget != null;
	}

	public bool isTargetInRange( PlayerRace playerRace, float aimRange )
	{
		for( int i =0; i < PlayerRace.players.Count; i++ )
		{
			//Ignore the potential sentry owner
			if( PlayerRace.players[i] == playerRace ) continue;

			//Calculate the distance to the other player
			float distanceToTarget = Vector3.Distance( playerRace.transform.position, PlayerRace.players[i].transform.position );

			//Is this player within aiming range?
			if( distanceToTarget > aimRange ) continue;

			//Is the player dead or Idle? If so, ignore.
			if( PlayerRace.players[i].GetComponent<PlayerControl>().deathType != DeathType.Alive || PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Idle ) continue;

			//Is the player using the Cloak card? If so, ignore.
			if( PlayerRace.players[i].GetComponent<PlayerSpell>().isCardActive(CardName.Cloak) ) continue;

			//We found at least one target
			return true;
		}
		return false;
	}

	public bool isAllowed( int photonViewID )
	{
		//Find out which player activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Verify that the player is not spawning the object inside the finish line trigger
		Vector3 objectPosition = playerTransform.TransformPoint( spawnOffset );

		GameObject finishLineColliderObject = GameObject.FindGameObjectWithTag("Finish Line");
		//If the tile with the finish line is not active, boxColliderObject will be null, so check for that.
		if( finishLineColliderObject != null )
		{
			Collider collider = finishLineColliderObject.GetComponent<Collider>();
			if( collider.bounds.Contains( objectPosition ) )
			{
				//Don't allow it
				return false;
			}
			else
			{
				return true;
			}
		}
		else
		{
			//If finishLineColliderObject is null, that means the tile with the finish line is not yet active and therefore, we are far from the finish line.
			//In this case, return true.
			return true;
		}
	}

	/// <summary>
	/// Returns true if an opponent is within the radius specified by this card.
	/// </summary>
	/// <returns><c>true</c>, if an opponent is near, <c>false</c> otherwise.</returns>
	/// <param name="caster">Caster.</param>
	/// <param name="level">Level.</param>
	public bool isOpponentNear( Transform caster, int level )
	{
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		float impactRadius = cd.getCardPropertyValue( CardPropertyType.RADIUS, level );
		Collider[] hitColliders = Physics.OverlapSphere( caster.position, impactRadius, MaskHandler.getMaskOnlyPlayer() );
		for( int i =0; i < hitColliders.Length; i++ )
		{
			//Verify that it is NOT the caster activating the card
			if( hitColliders[i].name != caster.name )
			{
				//Verify that the target is not in the Idle character state.
				//The target will be in the Idle state after crossing the finish line for example.
				//Make the sure the player is not dead.
				//Make sure the player does not have the Cloak card active.
				if( hitColliders[i].GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Idle && hitColliders[i].GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Dying )
				{
					if( !hitColliders[i].GetComponent<PlayerSpell>().isCardActive(CardName.Cloak) )
					{
						//We found at least one target
						return true;
					}
				}
			}
		}
		//No target were found
		return false;
	}

	#region Voice Overs activated by Cards
	public void playActivateCardVoiceOver( PhotonView casterPhotonView )
	{
		//Play the activate card voice over but only if there is a VO available to play. We don't want to send RPCs for nothing and waste bandwidth.
		if( casterPhotonView.GetComponent<PlayerVoiceOvers>().isSpellVoiceOverAvailable( cardName, false ) ) casterPhotonView.RPC( "activateCardVoiceOverRPC", PhotonTargets.All, (int)cardName );
	}
	#endregion
}
