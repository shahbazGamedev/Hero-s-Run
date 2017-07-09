using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// See CardStasis for details.
/// </summary>
public class StasisController : CardSpawnedObject {

	Transform affectedPlayerTransform;
	PlayerControl affectedPlayerControl;
	const float DISTANCE_ABOVE_GROUND = 3.5f;
	const float Y_POS_PLAYER_IN_SPHERE = -0.35f;

	#region Initialisation
	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		findAffectedPlayer( gameObject.GetPhotonView ().instantiationData );
	}

	void findAffectedPlayer(object[] data) 
	{
		int viewIdOfAffectedPlayer = (int) data[0];
		for( int i = 0; i < PlayerRace.players.Count; i ++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == viewIdOfAffectedPlayer )
			{
				//We found the spell's target
				affectedPlayerTransform = PlayerRace.players[i].transform;
				affectedPlayerTransform.GetComponent<Rigidbody>().isKinematic = true;
	
				affectedPlayerControl = affectedPlayerTransform.GetComponent<PlayerControl>();

				//If the player was using his jet packs, stop flying
				if( affectedPlayerControl.getCharacterState() == PlayerCharacterState.Flying ) affectedPlayerControl.GetComponent<PlayerSpell>().cancelJetPack();

				//If the player was ziplining when he got affected by stasis, detach him from the zipline.
				affectedPlayerControl.detachFromZipline();

				//Freeze the player's movement and remove player control.
				affectedPlayerControl.enablePlayerMovement( false );
				affectedPlayerControl.enablePlayerControl( false );

				//We want the statis sphere to float DISTANCE_ABOVE_GROUND above ground.
				//We add 0.1f because we do not want to start the raycast at the player's feet.
				//The Stasis prefab has the ignoreRaycast layer.
				affectedPlayerControl.gameObject.layer = MaskHandler.ignoreRaycastLayer; //change temporarily to Ignore Raycast
				RaycastHit hit;
				if (Physics.Raycast(new Vector3( affectedPlayerTransform.position.x, affectedPlayerTransform.position.y + 0.1f, affectedPlayerTransform.position.z ), Vector3.down, out hit, 30.0F ))
				{
					transform.position = new Vector3( transform.position.x, hit.point.y + DISTANCE_ABOVE_GROUND, transform.position.z );
				}
				else
				{
					Debug.LogWarning("StasisController-there is no ground below the affected player: " + affectedPlayerControl.name );
				}
				//Make the player a child of the Statis Sphere
				affectedPlayerTransform.SetParent( transform );
				affectedPlayerTransform.localPosition = new Vector3( 0, Y_POS_PLAYER_IN_SPHERE, 0 );
				affectedPlayerTransform.gameObject.layer = MaskHandler.playerLayer; //Restore to Player
				//Slow down the anim speed to give the impression of the player being stuck
				affectedPlayerTransform.GetComponent<Animator>().speed = 0.3f;
				affectedPlayerTransform.GetComponent<Animator>().Play( "Fall_Loop" );
				//Set the player state to Idle so that other spells don't affect the player while he is in Statis.
				affectedPlayerControl.setCharacterState( PlayerCharacterState.Idle );

				//If the player has a Sentry, it will be destroyed.
				affectedPlayerTransform.GetComponent<PlayerSpell>().cancelSentrySpell();

				//The Stasis Sphere has a limited lifespan which depends on the level of the Card.
				float spellDuration = (float) data[1];
				StartCoroutine( destroyStasisSphere( spellDuration ) );

				//Display the Stasis secondary icon on the minimap
				MiniMap.Instance.displaySecondaryIcon( affectedPlayerTransform.GetComponent<PhotonView>().viewID, (int) CardName.Stasis, spellDuration );
			}
		}
		if( affectedPlayerTransform != null )
		{
			Debug.Log("Stasis-The player affected by the Stasis Sphere is: " + affectedPlayerTransform.name );
		}
		else
		{
			Debug.LogError("StasisController error: could not find the target player with the Photon view id of " + viewIdOfAffectedPlayer );
		}
	}
	#endregion

	IEnumerator destroyStasisSphere( float delayBeforeSpellExpires )
	{
		yield return new WaitForSeconds(delayBeforeSpellExpires);
		MiniMap.Instance.hideSecondaryIcon( affectedPlayerTransform.gameObject );
		affectedPlayerTransform.SetParent( null );
		affectedPlayerTransform.GetComponent<Rigidbody>().isKinematic = false;
		affectedPlayerTransform.GetComponent<Animator>().speed = 1f;
		affectedPlayerControl.fall( true );
		affectedPlayerControl.enablePlayerMovement( true );
		affectedPlayerControl.enablePlayerControl( true );
		Destroy( gameObject );
	}

}
