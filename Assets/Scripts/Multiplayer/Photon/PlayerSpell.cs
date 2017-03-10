using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

/// <summary>
/// This class handles all spells that affect the player such as Shrink.
/// </summary>
public class PlayerSpell : PunBehaviour {

	[SerializeField] AudioClip shrinkSound;
	PlayerControl playerControl;
	PlayerSounds playerSounds;
	float runSpeedBeforeSpell;

	// Use this for initialization
	void Awake ()
	{
		playerControl = GetComponent<PlayerControl>();
		playerSounds = GetComponent<PlayerSounds>();
	}

	[PunRPC]
	void shrinkSpell( float spellDuration )
	{
		playerSounds.playSound( shrinkSound, false );
		StartCoroutine( shrink( new Vector3( 0.3f, 0.3f, 0.3f ), 1.25f, spellDuration ) );
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
}
