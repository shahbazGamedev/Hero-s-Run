using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : Device {

	enum TeleporterType {
		Transmitter = 0,
		Receiver = 1
	}

	[Tooltip("Assuming that the teleporter tile group is composed of three tiles (Teleporter_Tx, Straight and Teleporter_Rx), this number should be 2. Assumes a tile depth of 1 for Teleporter_Rx and any tile between Teleporter_Tx and Teleporter_Rx.")]
	[SerializeField] int numberOfTilesSkippedBecauseOfTeleportation = 2; //important for the race position to be exact
	[SerializeField] TeleporterType type = TeleporterType.Transmitter;
	[Tooltip("The name of the move-to-center-lane game object. This game object is disabled when the teleporter type is set to Receiver.")]
	[SerializeField] GameObject moveToCenterLaneTrigger;
	[Tooltip("The particle effect to play when teleporting a player.")]
	[SerializeField] ParticleSystem activationEffect;

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
	
					float distanceBetweenTeleporters = numberOfTilesSkippedBecauseOfTeleportation * GenerateLevel.tileSize;
					Vector3 destinationTeleporterPosition = new Vector3( transform.position.x, transform.position.y, transform.position.z + distanceBetweenTeleporters);
					float destinationTeleporterYRotation = transform.eulerAngles.y;
	
					other.GetComponent<PhotonView>().RPC("teleportRPC", PhotonTargets.All, destinationTeleporterPosition, destinationTeleporterYRotation );
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

					PlayerControl playerControl = other.GetComponent<PlayerControl>();

					//We may have switched lanes because of the position change. Make sure the lane values are accurate.
					playerControl.recalculateCurrentLane();

					GameObject.FindObjectOfType<GenerateLevel>().activateTilesAfterTeleport();

					//Adjust the distance remaining
					float distanceBetweenTeleporters = numberOfTilesSkippedBecauseOfTeleportation * GenerateLevel.tileSize;
					playerControl.tileDistanceTraveled = playerControl.tileDistanceTraveled + distanceBetweenTeleporters;
					other.GetComponent<PlayerRace>().distanceTravelledOnThisTile = other.GetComponent<PlayerRace>().distanceTravelledOnThisTile - distanceBetweenTeleporters;

					other.GetComponent<PlayerSpell>().isBeingTeleported = false;

					playerControl.enablePlayerMovement( true );
				}
			}
		}
	}
}
