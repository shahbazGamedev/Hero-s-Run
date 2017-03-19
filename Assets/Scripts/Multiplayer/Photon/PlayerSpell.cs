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
	#endregion

	PlayerControl playerControl;
	PlayerSounds playerSounds;

	// Use this for initialization
	void Awake ()
	{
		playerControl = GetComponent<PlayerControl>();
		playerSounds = GetComponent<PlayerSounds>();
	}

	#region Shrink spell
	[PunRPC]
	void shrinkSpell( string casterName, float spellDuration )
	{
		playerSounds.playSound( shrinkSound, false );
		ParticleSystem shrinkEffect = ParticleSystem.Instantiate( shrinkParticleSystem, transform );
		shrinkEffect.transform.localPosition = new Vector3( 0, 1f, 0 );
		shrinkEffect.Play();
		StartCoroutine( shrink( new Vector3( 0.3f, 0.3f, 0.3f ), 1.25f, spellDuration ) );
		//Indicate on the minimap which card was played
		MiniMap.Instance.updateCardFeed( casterName, CardName.Shrink );
	}

	IEnumerator shrink( Vector3 endScale, float shrinkDuration, float spellDuration )
	{
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
	#endregion

	#region Linked Fate spell
	[PunRPC]
	void cardLinkedFateRPC( string casterName, float spellDuration )
	{
		if( gameObject.name == casterName )
		{
			castLinkedFate = true;
		}
		else
		{
			affectedByLinkedFate = true;
		}
		//Indicate on the minimap which card was played
		MiniMap.Instance.updateCardFeed( casterName, CardName.Linked_Fate );

		//For the player affected by Linked Fate, change the color of his icon on the map
		if( affectedByLinkedFate ) MiniMap.Instance.changeColorOfRadarObject( GetComponent<PlayerControl>(), Color.magenta );

		//Cancel the spell once the duration has run out
		Invoke("cancelLinkedFateSpell", spellDuration );

		Debug.LogWarning("PlayerSpell cardLinkedFateRPC received-affectedByLinkedFate: " + affectedByLinkedFate + " castLinkedFate: " + castLinkedFate + " name: " + gameObject.name + " caster: " + casterName );
	}

	void cancelLinkedFateSpell()
	{
		if( affectedByLinkedFate ) MiniMap.Instance.changeColorOfRadarObject( GetComponent<PlayerControl>(), Color.white );
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

	public void playerDied()
	{
		if( castLinkedFate && PhotonNetwork.isMasterClient && GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Dying )
		{
			Debug.LogWarning("PlayerSpell-playerDied: affectedByLinkedFate: " + affectedByLinkedFate + " castLinkedFate: " + castLinkedFate + " name: " + gameObject.name );

			//Kill all players with the affectedByLinkedFate flag. Ignore the caster (who is dead anyway).
			for( int i = 0; i < PlayerRace.players.Count; i++ )
			{
				if( PlayerRace.players[i].GetComponent<PlayerSpell>().isAffectedByLinkedFate() && !PlayerRace.players[i].GetComponent<PlayerSpell>().hasCastLinkedFate() )
				{
					Debug.LogWarning("PlayerSpell-playerDied LOOP " + PlayerRace.players[i].GetComponent<PlayerSpell>().isAffectedByLinkedFate() + " " + PlayerRace.players[i].GetComponent<PlayerSpell>().hasCastLinkedFate() + " " + PlayerRace.players[i].name );
					PlayerRace.players[i].GetComponent<PhotonView>().RPC("playerDied", PhotonTargets.AllViaServer, DeathType.Obstacle );
					//Reset the color
					MiniMap.Instance.changeColorOfRadarObject( PlayerRace.players[i].GetComponent<PlayerControl>(), Color.white );
				}
			}
		}
	}
	#endregion


}
