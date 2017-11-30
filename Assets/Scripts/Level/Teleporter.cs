using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : Device {

	enum TeleporterType {
		Transmitter = 0,
		Receiver = 1
	}

	[Tooltip("The transmitter and the receiver must be on the same tile.")]
	[SerializeField] Transform teleporterRx;
	[SerializeField] TeleporterType type = TeleporterType.Transmitter;
	[Tooltip("The name of the move-to-center-lane game object. This game object is disabled when the teleporter type is set to Receiver.")]
	[SerializeField] GameObject moveToCenterLaneTrigger;
	[Tooltip("The particle effect to play when teleporting a player.")]
	[SerializeField] ParticleSystem activationEffect;
	const float playerHeightForRx = 0.16f;
	const float teleportDelay = 0.25f;

	new void Start ()
	{
		base.Start ();
		if( type == TeleporterType.Receiver )
		{
			//The move to center lane trigger for bots is not needed for the receiver
			moveToCenterLaneTrigger.SetActive( false );
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if( state == DeviceState.On )
		{
			if( type == TeleporterType.Transmitter )
			{
				if( other.CompareTag("Player") && other.GetComponent<PhotonView>().isMine )
				{
					//Play a teleport VFX and sound
					GetComponent<AudioSource>().Play();
					activationEffect.Play();

					//The player stops moving until the RPC action gets processed.			
					other.GetComponent<PlayerControl>().enablePlayerMovement( false );

					other.GetComponent<PlayerSpell>().isBeingTeleported = true;

					StartCoroutine( teleportAfterDelay( other.transform ) );	
				}
			}
			else if( type == TeleporterType.Receiver )
			{
				//Only play the activation effects if you were teleported (and not just because you ran through the rx teleporter).
				if( other.CompareTag("Player") && other.GetComponent<PlayerSpell>().isBeingTeleported )
				{
					//Play a teleport VFX and sound
					GetComponent<AudioSource>().Play();
					activationEffect.Play();

					StartCoroutine( allowMoveAfterDelay( other.transform ) );	

				}
			}
		}
	}

	IEnumerator teleportAfterDelay( Transform player )
	{
		yield return new WaitForSeconds( teleportDelay );
		makePlayerInvisible( player, false );
		yield return new WaitForSeconds( teleportDelay );
		Vector3 destinationTeleporterPosition = new Vector3( teleporterRx.position.x, teleporterRx.position.y + playerHeightForRx, teleporterRx.position.z );
		player.GetComponent<PhotonView>().RPC("teleportRPC", PhotonTargets.All, destinationTeleporterPosition );
	}

	IEnumerator allowMoveAfterDelay( Transform player )
	{
		PlayerControl playerControl = player.GetComponent<PlayerControl>();
		playerControl.stumble();

		yield return new WaitForSeconds( 0.4f );

		if( !player.GetComponent<PlayerSpell>().isCardActive( CardName.Cloak ) ) makePlayerInvisible( player, true );

		yield return new WaitForSeconds( teleportDelay );

		player.GetComponent<PlayerSpell>().isBeingTeleported = false;

		//We may have switched lanes because of the position change. Make sure the lane values are accurate.
		playerControl.recalculateCurrentLane();

		playerControl.enablePlayerMovement( true );
	}

	void makePlayerInvisible( Transform player, bool isVisible )
	{
		Transform heroSkin = player.Find("Hero Skin");
		SkinnedMeshRenderer[] smr = heroSkin.GetComponentsInChildren<SkinnedMeshRenderer>();
		for( int i = 0; i < smr.Length; i++ )
		{
			smr[i].enabled = isVisible;
		} 
		player.GetComponent<PlayerVisuals>().enablePlayerShadow( isVisible );
	}

}
