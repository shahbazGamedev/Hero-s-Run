using UnityEngine;
using System.Collections;

interface ICreature
{
    void resetCreature();
	void knockback();
	CreatureState getCreatureState();
}
