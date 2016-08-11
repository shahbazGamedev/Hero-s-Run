using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Creature : BaseClass {

	protected CreatureState creatureState = CreatureState.Idle;

	public CreatureState getCreatureState()
	{
		return creatureState;
	}

	public void setCreatureState( CreatureState state )
	{
		creatureState = state;
	}

}
