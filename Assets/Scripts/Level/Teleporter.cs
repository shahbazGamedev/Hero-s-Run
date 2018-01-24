using System.Collections;
using UnityEngine;

public class Teleporter : Device {

	enum TeleporterType {
		Transmitter = 0,
		Receiver = 1
	}

	[Tooltip("The teleporter only works for players. The transmitter and the receiver pods must be on the same tile.")]
	[SerializeField] Transform teleporterRx;
	[SerializeField] TeleporterType type = TeleporterType.Transmitter;
	[Tooltip("The particle effect to play when teleporting a player.")]
	[SerializeField] ParticleSystem activationEffect;
	[Tooltip("Used to place the player on top of the pod after the teleport. Basically, this should be the height of the pod base.")]
	[SerializeField] float playerHeightForRx = 0.16f;
	[Tooltip("Delay used when teleporting.")]
	[SerializeField] float teleportDelay = 0.25f;

	new void Start ()
	{
		base.Start ();
	}

	void OnTriggerEnter(Collider other)
	{
		if( state == DeviceState.On )
		{
			if( type == TeleporterType.Transmitter )
			{
				//Remember that OnTriggerEnter can be called multiple times.
				//If this player is already being teleported, ignore the event.
				if( other.CompareTag("Player") && !other.GetComponent<PlayerSpell>().isBeingTeleported )
				{
					//Play a teleport VFX and sound
					GetComponent<AudioSource>().Play();
					activationEffect.Play();

					//The player stops moving until the RPC action gets processed.			
					other.GetComponent<PlayerControl>().enablePlayerMovement( false );

					//While waiting to be teleported, remove player control.
					other.GetComponent<PlayerControl>().enablePlayerControl( false );

					//The player might have been changing lanes when the teleportation was triggered.
					//Stop the change lane or else the player will continue to change lanes when he reaches the other side.
					other.GetComponent<PlayerControl>().stopSideMove();

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
		if( player.GetComponent<PhotonView>().isMine ) player.GetComponent<PhotonView>().RPC("teleportRPC", PhotonTargets.All, destinationTeleporterPosition );
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

		playerControl.enablePlayerControl( true );

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
		//Debug.LogWarning( "Teleporter-makePlayerInvisible for " + player.name + " Visible: " + isVisible );
	}

}
