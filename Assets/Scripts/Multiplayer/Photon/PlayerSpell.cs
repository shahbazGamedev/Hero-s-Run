using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

/// <summary>
/// This class handles all spells that affect the player such as Shrink.
/// </summary>
public class PlayerSpell : PunBehaviour {

	#region Shrink spell
	[SerializeField] AudioClip shrinkSound;
	[SerializeField] ParticleSystem shrinkParticleSystem;
	float runSpeedBeforeSpell;
	#endregion

	#region Linked Fate spell
	bool affectedByLinkedFate = false;
	bool castLinkedFate = false;
	[SerializeField] AudioClip linkedFateSound;
	#endregion

	#region Hack
	public bool affectedByHack = false;
	#endregion

	#region Supercharger
	bool affectedBySupercharger = false;
	#endregion

	#region Sentry spell
	SentryController sentryController;
	#endregion

	PlayerControl playerControl;
	PlayerSounds playerSounds;
	PlayerVoiceOvers playerVoiceOvers;

	//Delegate used to communicate to other classes when an enemy has played a special card such as Hack
	public delegate void CardPlayedByOpponentEvent( CardName name, float duration );
	public static event CardPlayedByOpponentEvent cardPlayedByOpponentEvent;

	// Use this for initialization
	void Awake ()
	{
		playerControl = GetComponent<PlayerControl>();
		playerSounds = GetComponent<PlayerSounds>();
		playerVoiceOvers = GetComponent<PlayerVoiceOvers>();
	}

	#region Shrink spell
	[PunRPC]
	void shrinkSpellRPC( float spellDuration )
	{
		playerSounds.playSound( shrinkSound, false );
		ParticleSystem shrinkEffect = ParticleSystem.Instantiate( shrinkParticleSystem, transform );
		shrinkEffect.transform.localPosition = new Vector3( 0, 1f, 0 );
		shrinkEffect.Play();
		StartCoroutine( shrink( new Vector3( 0.3f, 0.3f, 0.3f ), 1.25f, spellDuration ) );
		if( cardPlayedByOpponentEvent != null && GetComponent<PhotonView>().isMine ) cardPlayedByOpponentEvent( CardName.Shrink, spellDuration );
	}

	IEnumerator shrink( Vector3 endScale, float shrinkDuration, float spellDuration )
	{
		playerVoiceOvers.playVoiceOver(VoiceOverType.VO_Spell, CardName.Shrink, false );
		playerControl.setAllowRunSpeedToIncrease( false );
		runSpeedBeforeSpell = playerControl.getSpeed();

		float elapsedTime = 0;
		Vector3 startScale = transform.localScale;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			transform.localScale = Vector3.Lerp( startScale, endScale, elapsedTime/shrinkDuration );
			playerControl.runSpeed = runSpeedBeforeSpell * transform.localScale.y;

			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < shrinkDuration );
		transform.localScale = endScale;	
		StartCoroutine( enlarge( new Vector3( 1f, 1f, 1f ), 1.1f, spellDuration ) );
	}

	IEnumerator enlarge( Vector3 endScale, float shrinkDuration, float spellDuration )
	{
		yield return new WaitForSeconds( spellDuration );
		float elapsedTime = 0;
		Vector3 startScale = transform.localScale;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			transform.localScale = Vector3.Lerp( startScale, endScale, elapsedTime/shrinkDuration );
			playerControl.runSpeed = runSpeedBeforeSpell * transform.localScale.y;

			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < shrinkDuration );
		transform.localScale = endScale;	
		playerControl.setAllowRunSpeedToIncrease( true );
		playerControl.runSpeed = runSpeedBeforeSpell;
	}

	void cancelShrinkSpell()
	{
		//Are we shrunk?
		if( transform.localScale.y != 1f )
		{
			//If we died while shrunk, do nothing i.e. stay small, that's fine.
			//If we crossed the finish line while shrunk, enlarge the player quickly back to his normal size.
			if( playerControl.deathType == DeathType.Alive )
			{
				StartCoroutine( enlarge( new Vector3( 1f, 1f, 1f ), 0.9f, 0 ) );
			}
		}
	}
	#endregion

	#region Linked Fate spell
	[PunRPC]
	void cardLinkedFateRPC( string casterName, float spellDuration )
	{
		//Play a creepy sound
		playerSounds.playSound( linkedFateSound, false );

		if( gameObject.name == casterName )
		{
			castLinkedFate = true;
		}
		else
		{
			affectedByLinkedFate = true;
			if( cardPlayedByOpponentEvent != null && GetComponent<PhotonView>().isMine ) cardPlayedByOpponentEvent( CardName.Linked_Fate, spellDuration );
		}

		//For the player affected by Linked Fate, change the color of his icon on the map
		//The local player does not have an icon on the minimap.
		//If a bot cast the linked fate spell, do not attempt to change the icon color since it does not exist.
		if( affectedByLinkedFate &&  GameManager.Instance.getPlayMode() != PlayMode.PlayAgainstEnemy ) MiniMap.Instance.changeColorOfRadarObject( GetComponent<PlayerControl>(), Color.magenta );

		//Cancel the spell once the duration has run out
		Invoke("cancelLinkedFateSpell", spellDuration );

		Debug.LogWarning("PlayerSpell cardLinkedFateRPC received-affectedByLinkedFate: " + affectedByLinkedFate + " castLinkedFate: " + castLinkedFate + " name: " + gameObject.name + " caster: " + casterName );
	}

	void cancelLinkedFateSpell()
	{
		if( affectedByLinkedFate &&  GameManager.Instance.getPlayMode() != PlayMode.PlayAgainstEnemy ) MiniMap.Instance.changeColorOfRadarObject( GetComponent<PlayerControl>(), Color.white );
		affectedByLinkedFate = false;
		castLinkedFate = false;
	}


	public bool isAffectedByLinkedFate()
	{
		return affectedByLinkedFate;
	}

	public bool hasCastLinkedFate()
	{
		return castLinkedFate;
	}
	#endregion

	#region Sentry Spell
	public void registerSentry( SentryController sentryController )
	{
		this.sentryController = sentryController;
	}

	public void cancelSentrySpell()
	{
		if( sentryController != null )
		{
			sentryController.destroySpawnedObjectNow();
			sentryController = null;
		}
	}
	#endregion

	#region Hack
	[PunRPC]
	void cardHackRPC( float spellDuration )
	{
		if( cardPlayedByOpponentEvent != null && GetComponent<PhotonView>().isMine ) cardPlayedByOpponentEvent( CardName.Hack, spellDuration );

		affectedByHack = true;
		//Cancel the hack effect once the duration has run out
		Invoke("cancelHack", spellDuration );

		//Display the Hacked secondary icon on the minimap
		MiniMap.Instance.displaySecondaryIcon( GetComponent<PhotonView>().viewID, (int) CardName.Hack, spellDuration );

		//To Do
		//Add a reddish glow and electric sparks to the omni-tool so it appears broken.

		print("PlayerSpell cardHackRPC received-affectedByHack: " + affectedByHack + " name: " + gameObject.name );
	}

	void cancelHack()
	{
		affectedByHack = false;
		print("PlayerSpell cancelHack for " + gameObject.name );
	}

	public bool isAffectedByHack()
	{
		return affectedByHack;
	}
	#endregion

	#region Supercharger
	[PunRPC]
	void cardSuperchargerRPC( float spellDuration )
	{
		affectedBySupercharger = true;
		//Cancel the supercharger effect once the duration has run out
		Invoke("cancelSupercharger", spellDuration );

		//Display the Supercharger secondary icon on the minimap
		MiniMap.Instance.displaySecondaryIcon( GetComponent<PhotonView>().viewID, (int) CardName.Supercharger, spellDuration );

		//To Do
		//Display a Supercharger timer on the HUD so the player knows when it is going to run out.
		//Add a cool glow to the omni-tool
		print("PlayerSpell cardSuperchargerRPC received " + gameObject.name );
	}

	void cancelSupercharger()
	{
		affectedBySupercharger = false;
		print("PlayerSpell cancelSupercharger for " + gameObject.name );
	}

	public bool isAffectedBySupercharger()
	{
		return affectedBySupercharger;
	}
	#endregion

	public void cancelAllSpells()
	{
		StopAllCoroutines();
		CancelInvoke();
		cancelLinkedFateSpell();
		cancelSentrySpell();
		cancelShrinkSpell();
		cancelSupercharger();
		cancelHack();
	}

	public void playerDied()
	{
		if( castLinkedFate && PhotonNetwork.isMasterClient && GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Dying )
		{
			//Kill all players with the affectedByLinkedFate flag. Ignore the caster (who is dead anyway).
			for( int i = 0; i < PlayerRace.players.Count; i++ )
			{
				if( PlayerRace.players[i].GetComponent<PlayerSpell>().isAffectedByLinkedFate() && !PlayerRace.players[i].GetComponent<PlayerSpell>().hasCastLinkedFate() )
				{
					PlayerRace.players[i].GetComponent<PhotonView>().RPC("playerDied", PhotonTargets.AllViaServer, DeathType.Obstacle );
					//Reset the color
					if( GameManager.Instance.getPlayMode() != PlayMode.PlayAgainstEnemy) MiniMap.Instance.changeColorOfRadarObject( PlayerRace.players[i].GetComponent<PlayerControl>(), Color.white );
				}
			}
		}
		//Cancel any active spells if the player dies
		cancelAllSpells();
	}

}
