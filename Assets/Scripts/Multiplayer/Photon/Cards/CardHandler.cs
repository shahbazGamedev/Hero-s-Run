using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardHandler : MonoBehaviour {

	public void activateCard ( int photonViewId, CardName name, int level)
	{
		switch (name)
		{
			case CardName.Raging_Bull:
				CardSpeedBoost cardSpeedBoost = GetComponent<CardSpeedBoost>();
				if( cardSpeedBoost != null )
				{
					cardSpeedBoost.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardSpeedBoost component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Explosion:
				CardExplosion cardExplosion = GetComponent<CardExplosion>();
				if( cardExplosion != null )
				{
					cardExplosion.activateCard( photonViewId,level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardExplosion component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Double_Jump:
				CardDoubleJump cardDoubleJump = GetComponent<CardDoubleJump>();
				if( cardDoubleJump != null )
				{
					cardDoubleJump.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardDoubleJump component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Sprint:
				CardSprint cardSprint = GetComponent<CardSprint>();
				if( cardSprint != null )
				{
					cardSprint.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardSprint component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Firewall:
				CardFirewall cardFirewall = GetComponent<CardFirewall>();
				if( cardFirewall != null )
				{
					cardFirewall.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardFirewall component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Lightning:
				CardLightning cardLightning = GetComponent<CardLightning>();
				if( cardLightning != null )
				{
					cardLightning.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardLightning component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Shrink:
				CardShrink cardShrink = GetComponent<CardShrink>();
				if( cardShrink != null )
				{
					cardShrink.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardShrink component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Glyph:
				CardGlyph cardGlyph = GetComponent<CardGlyph>();
				if( cardGlyph != null )
				{
					cardGlyph.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardGlyph component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Linked_Fate:
				CardLinkedFate cardLinkedFate = GetComponent<CardLinkedFate>();
				if( cardLinkedFate != null )
				{
					cardLinkedFate.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardLinkedFate component is not attached to the CardHandler in the Level scene.");
				}
			break;
			default:
				Debug.LogWarning("CardHandler-The card name specified, " + name + ", is unknown.");
			break;
		}
	}

	public bool isCardEffective ( GameObject caster, CardName name, int level )
	{
		bool effective = false;
		switch (name)
		{
			//Raging bull is effective whenever you are trailing behind
			case CardName.Raging_Bull:
				if( !isCasterLeading( caster.GetComponent<PlayerRace>() ) ) return true;
			break;
			//Explosion is effective whenever an opponent is near you
			case CardName.Explosion:
				return GetComponent<CardExplosion>().isOpponentNear( caster.transform, level );
			break;
			//Double jump is effective whenever you are trailing behind by a little
			case CardName.Double_Jump:
				if( !isCasterLeading( caster.GetComponent<PlayerRace>() ) ) return true;
			break;
			//Sprint is effective whenever you are trailing behind
			case CardName.Sprint:
				if( !isCasterLeading( caster.GetComponent<PlayerRace>() ) ) return true;
			break;
			//Firewall is effective whenever there is an opponent behind you and not too far
			case CardName.Firewall:
				if( isCasterLeading( caster.GetComponent<PlayerRace>() ) )
				{
					return GetComponent<CardFirewall>().willSpellBeEffective( caster.transform, level );
				}
			break;
			//Glyph is effective whenever there is an opponent behind you and not too far
			case CardName.Glyph:
				if( isCasterLeading( caster.GetComponent<PlayerRace>() ) )
				{
					return GetComponent<CardGlyph>().willSpellBeEffective( caster.transform, level );
				}
			break;
			//Lightning is effective whenever your opponent is far ahead of you
			case CardName.Lightning:
				if( !isCasterLeading( caster.GetComponent<PlayerRace>() ) )
				{
					Transform nearestTarget = GetComponent<CardLightning>().detectNearestTarget( caster.transform, level, caster.GetComponent<PhotonView>().viewID );
					if( nearestTarget != null ) return true;
				}
			break;
			//Shrink is effective whenever your opponent is far ahead of you
			case CardName.Shrink:
				if( !isCasterLeading( caster.GetComponent<PlayerRace>() ) )
				{
					Transform nearestTarget = GetComponent<CardShrink>().detectNearestTarget( caster.transform, level, caster.GetComponent<PhotonView>().viewID );
					if( nearestTarget != null ) return true;
				}
			break;
			default:
				Debug.LogWarning("CardHandler-The card name specified, " + name + ", is unknown.");
			break;
		}
		return effective;
	}

	bool isCasterLeading( PlayerRace caster )
	{
		bool isLeading = true;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			//Ignore the caster
			if( PlayerRace.players[i] == caster ) continue;
			if( PlayerRace.players[i].racePosition < caster.racePosition ) return false;
		}
		return isLeading;
	}

}
