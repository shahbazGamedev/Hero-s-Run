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
	
	protected IEnumerator changeSprintBlendFactor( float endBlendFactor, float duration, PlayerControl playerControl )
	{
		float elapsedTime = 0;
		float startBlendFactor = playerControl.getSprintBlendFactor();
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			playerControl.setSprintBlendFactor( Mathf.Lerp( startBlendFactor, endBlendFactor, elapsedTime/duration ) );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		playerControl.setSprintBlendFactor( endBlendFactor );	
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
	/// Returns a random player target or null if none were found within the spell range.
	/// </summary>
	/// <returns>The random target.</returns>
	/// <param name="playerRace">Player race.</param>
	/// <param name="spellRange">Spell range.</param>
	protected Transform detectRandomTarget( PlayerRace playerRace, float spellRange )
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
			int random = Random.Range(0, potentialTargets.Count ); 
			return potentialTargets[random];
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

		GameObject boxColliderObject = GameObject.FindGameObjectWithTag("Finish Line");
		//If the tile with the finish line is not active, boxColliderObject will be null, so check for that.
		if( boxColliderObject != null )
		{
			BoxCollider boxCollider = boxColliderObject.GetComponent<BoxCollider>();
			if( boxCollider.bounds.Contains( objectPosition ) )
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
			//If boxColliderObject is null, that means the tile with the finish line is not yet active and therefore, we are far from the finish line.
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
				if( hitColliders[i].GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Idle )
				{
					//We found at least one target
					return true;
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
