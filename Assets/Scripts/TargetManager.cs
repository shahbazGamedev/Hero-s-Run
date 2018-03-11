using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TargetManager : MonoBehaviour {

	public static TargetManager Instance;

	void Awake()
	{
		Instance = this;
	}

	#region Competition
	/// <summary>
	/// Returns true if the player is a valid target.
	/// A player is a valid target if:
	/// He is not dead, idle.
	/// He is not ziplinning (tested if testForZiplining is true).
	/// He is not using the Cloak card (tested if testForCloak is true).
	/// He is not teleporting (tested if testForTeleporting is true).
	/// </summary>
	/// <returns><c>true</c>, if the player is a valid target, <c>false</c> otherwise.</returns>
	/// <param name="player">Player.</param>
	/// <param name="testForZiplining">If set to <c>true</c> test for ziplining.</param>
	/// <param name="testForCloak">If set to <c>true</c> test for cloak.</param>
	/// <param name="testForTeleporting">If set to <c>true</c> test for teleporting.</param>
	public bool isPlayerValidTarget( Transform player, bool testForZiplining = true, bool testForCloak = true, bool testForTeleporting = true )
	{
		bool isValid = true;

		PlayerControl pc = player.GetComponent<PlayerControl>();

		//Is the player dying or idle or ziplining?
		if( pc.getCharacterState() == PlayerCharacterState.Dying || pc.getCharacterState() == PlayerCharacterState.Idle ) isValid = false;

		if( testForZiplining && pc.getCharacterState() == PlayerCharacterState.Ziplining ) isValid = false;

		PlayerSpell ps = player.GetComponent<PlayerSpell>();

		//Is the player using the Cloak card?
		if( testForCloak && ps.isCardActive(CardName.Cloak) ) isValid = false;

		//Is the player teleporting?
		if( testForTeleporting && ps.isBeingTeleported ) isValid = false;

		return isValid;
	}
	#endregion


	#region Coop
	/// <summary>
	/// Returns true if the creature is a valid target.
	/// A creature is a valid target if:
	/// It is not dead or immobilized.
	/// It is in front of the caster.
	/// </summary>
	/// <returns><c>true</c>, if the creature is a valid target, <c>false</c> otherwise.</returns>
	/// <param name="creature">Creature.</param>
	/// <param name="useDotProduct">If set to <c>true</c> use dot product.</param>
	/// <param name="caster">Caster.</param>
	public bool isCreatureTargetValid( Transform creature, bool useDotProduct = true, Transform caster = null )
	{
		bool isValid = false;
		ICreature creatureController = creature.GetComponent<ICreature>();
		if( creatureController != null && creatureController.getCreatureState() != CreatureState.Dying && creatureController.getCreatureState() != CreatureState.Immobilized )
		{
			isValid = true;
			if( useDotProduct ) isValid = isValid && getDotProduct( caster, creature );
		}
		return isValid;
	}
	#endregion


	#region Shared
	public bool isGeneralTargetValid( Transform potentialTarget, Transform caster )
	{
		bool valid = false;
   		switch (potentialTarget.gameObject.layer)
		{
	        case MaskHandler.playerLayer:
				valid = isPlayerValidTarget( potentialTarget );
				valid = valid && caster.name != potentialTarget.name;
				valid = valid && !GameManager.Instance.isCoopPlayMode();
				break;
	                
	        case MaskHandler.deviceLayer:
				//A device is a valid target if:
				//The device is not in the Broken state.
				//Additional we want to only destroy devices that are behind the player.
				//For example: We don't want to destroy the jump pad in front of us which we'll most likely want to use.
				//We do however want to destroy the jump pad if it is behind us to prevent others players from using it.
				Device dev = potentialTarget.GetComponent<Device>();
				valid = dev.state != DeviceState.Broken;
				valid = valid && !getDotProduct( caster, potentialTarget );
                break;

	        case MaskHandler.destructibleLayer:
				//A destructible object is a valid target if:
				//The destructible object is in the Functioning state.
				//You do not own the target. For example, if you create an Ice Wall, you don't want your Sentry to destroy it.
				CardSpawnedObject cso = potentialTarget.GetComponent<CardSpawnedObject>();
				valid = cso.spawnedObjectState == SpawnedObjectState.Functioning;
				valid = valid && caster.name != cso.getCasterName();
                break;

	        case MaskHandler.creatureLayer:
				valid = isCreatureTargetValid( potentialTarget, false );
                break;
		}
		//if( valid ) Debug.Log("isTargetValid " + potentialTarget.name );
		return valid;
	}

	/// <summary>
	/// Returns true if the target is in front of the caster, false otherwise.
	/// </summary>
	/// <returns><c>true</c>, if the target is in front of the caster, <c>false</c> otherwise.</returns>
	/// <param name="caster">Caster.</param>
	/// <param name="target">Target.</param>
	protected bool getDotProduct( Transform caster, Transform target )
	{
		Vector3 forward = caster.TransformDirection(Vector3.forward);
		Vector3 toOther = target.position - caster.position;
		if (Vector3.Dot(forward, toOther) < 0)
		{
			return false;
		}
		else
		{
			return true;
		}
	}
	#endregion
}
