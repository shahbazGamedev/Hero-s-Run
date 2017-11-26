using UnityEngine;
using System.Collections;

public interface ICreature
{
	void knockback( Transform caster );
	CreatureState getCreatureState();
	void sideCollision();
	void victory( bool playWinSound );
	void deactivate();
	void shrink( Transform caster, bool value );
}
