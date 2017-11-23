﻿using UnityEngine;
using System.Collections;

interface ICreature
{
    void resetCreature();
	void knockback( Transform attacker );
	CreatureState getCreatureState();
	void sideCollision();
	void victory( bool playWinSound );
	void deactivate();

}
