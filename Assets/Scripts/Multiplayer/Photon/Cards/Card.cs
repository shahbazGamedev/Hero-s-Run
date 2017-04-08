using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : Photon.PunBehaviour {

	[SerializeField] float  baseDuration = 5f;
	[SerializeField] float  durationUpgradePerLevel = 1f;
	[SerializeField] float  baseRange = 100f;
	[SerializeField] float  rangeUpgradePerLevel = 10f;

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
	/// Returns true if there is a player target within spell range.
	/// </summary>
	/// <returns><c>true</c>, if there is a player target within range, <c>false</c> otherwise.</returns>
	/// <param name="caster">Caster.</param>
	/// <param name="level">Level.</param>
	public bool isThereATargetWithinRange( Transform caster, int level )
	{
		//See if there is a target
		Transform nearestTarget = detectNearestTarget( caster.GetComponent<PlayerRace>(), getRange( level ) );

		return nearestTarget != null;
	}

	public float getRange( int level )
	{
		return baseRange + level *  rangeUpgradePerLevel;
	}

	public float getDuration( int level )
	{
		return baseDuration + level *  durationUpgradePerLevel;
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

}
