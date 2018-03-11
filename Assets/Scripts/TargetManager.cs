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
	/// <param name="caster">Caster.</param>
	/// <param name="creature">Creature.</param>
	public bool isCreatureTargetValid( Transform caster, Transform creature )
	{
		bool isValid = false;
		ICreature creatureController = creature.GetComponent<ICreature>();
		if( creatureController != null && creatureController.getCreatureState() != CreatureState.Dying && creatureController.getCreatureState() != CreatureState.Immobilized )
		{
			isValid = true;
		}
		isValid = isValid && getDotProduct( caster, creature );
		return isValid;
	}
	#endregion


	#region Shared
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
