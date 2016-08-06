using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Creature : BaseClass {

	public Image mapIconPrefab;
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
