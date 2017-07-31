using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Teleporter. This implementation assumes that the teleporter tile group is composed of three 50 meter long tiles (Teleporter_Tx, Straight and Teleporter_Rx).
/// Therefore the distance between Tx and Rx is exactly 100 meters as both teleporter pads are in the center of their respective tile.
/// </summary>
public class Teleporter : Device {

	enum TeleporterType {
		Transmitter = 0,
		Receiver = 1
	}

	[SerializeField] int numberOfTilesSkippedBecauseOfTeleportation = 2; //important for the race position to be exact
	[SerializeField] TeleporterType type = TeleporterType.Transmitter;
	[Tooltip("The name of the move-to-center-lane game object. This game object is disabled when the teleporter type is set to Receiver.")]
	[SerializeField] GameObject moveToCenterLaneTrigger;

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
		if( type == TeleporterType.Transmitter && state == DeviceState.On )
		{
			if( other.CompareTag("Player") )
			{
				GetComponent<AudioSource>().Play();
				other.gameObject.GetComponent<PlayerInput>().teleport( new Vector3( transform.position.x, transform.position.y, transform.position.z + 100f), transform.eulerAngles.y );
				GenerateLevel generateLevel = GameObject.FindObjectOfType<GenerateLevel>();
				generateLevel.activateTilesAfterTeleport();
				other.GetComponent<PlayerRace>().tilesLeftBeforeReachingEnd -= numberOfTilesSkippedBecauseOfTeleportation;
			}
		}
	}
}
