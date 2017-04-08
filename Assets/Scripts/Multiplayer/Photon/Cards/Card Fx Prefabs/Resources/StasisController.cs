using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// See CardStasis for details.
/// </summary>
public class StasisController : MonoBehaviour {

	Transform affectedPlayerTransform;
	PlayerControl affectedPlayerControl;
	const float DISTANCE_ABOVE_GROUND = 2.5f;
	const float Y_POS_PLAYER_IN_SPHERE = -0.35f;

	#region Initialisation
	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		findAffectedPlayer( gameObject.GetPhotonView ().instantiationData );
	}

	void findAffectedPlayer(object[] data) 
	{
		int viewIdOfAffectedPlayer = (int) data[0];
		GameObject[] playersArray = GameObject.FindGameObjectsWithTag("Player");
		for( int i = 0; i < playersArray.Length; i ++ )
		{
			if( playersArray[i].GetPhotonView().viewID == viewIdOfAffectedPlayer )
			{
				//We found the spell's target
				affectedPlayerTransform = playersArray[i].transform;
	
				//Freeze the player's movement and remove player control.
				affectedPlayerControl = affectedPlayerTransform.GetComponent<PlayerControl>();
				affectedPlayerControl.enablePlayerMovement( false );
				affectedPlayerControl.enablePlayerControl( false );

				//We want the statis sphere to float above ground.
				affectedPlayerControl.gameObject.layer = 2; //change temporarily to Ignore Raycast
				RaycastHit hit;
				if (Physics.Raycast(new Vector3( transform.position.x, 10f, transform.position.z ), Vector3.down, out hit, 15.0F ))
				{
					transform.position = new Vector3( transform.position.x, hit.point.y + DISTANCE_ABOVE_GROUND, transform.position.z );
					//Also adjust the camera height
					if( GetComponent<PhotonView>().isMine && GetComponent<PlayerAI>() == null )
					{
						Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y + hit.point.y + DISTANCE_ABOVE_GROUND, Camera.main.transform.position.z);
					}
					affectedPlayerTransform.GetComponent<PlayerCamera>().positionCameraNow();
				}
				//Make the player a child of the Statis Sphere
				affectedPlayerTransform.SetParent( transform );
				affectedPlayerTransform.localPosition = new Vector3( 0, Y_POS_PLAYER_IN_SPHERE, 0 );
				affectedPlayerTransform.gameObject.layer = 8; //Restore to Player
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
				break;
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

		affectedPlayerTransform.SetParent( null );
		affectedPlayerTransform.GetComponent<Animator>().speed = 1f;
		affectedPlayerControl.fall();
		affectedPlayerControl.enablePlayerMovement( true );
		affectedPlayerControl.enablePlayerControl( true );
		Destroy( gameObject );
	}

}
