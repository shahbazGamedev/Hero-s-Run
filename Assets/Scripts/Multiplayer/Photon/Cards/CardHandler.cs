using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardHandler : MonoBehaviour {

	public void activateCard ( PhotonView casterPhotonView, CardName name, string userName, int level )
	{
		int photonViewId = casterPhotonView.viewID;

		//If the player or bot used the Supercharger card, he is casting every card for a short duration at a higher level than normal.
		//Increase the level but do not exceed the maximum level for this card.
		CardManager.CardData cd = CardManager.Instance.getCardByName( name );
		if( casterPhotonView.GetComponent<PlayerSpell>().isCardActive( CardName.Supercharger) )
		{
			print( casterPhotonView.gameObject.name + " is affected by supercharger. The normal card level for " + name.ToString() + " is " + level );
			int maxLevel = CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity );
			level = Mathf.Min( maxLevel, level + CardSupercharger.SUPERCHARGER_LEVEL_BOOST );
			print( "Adjusted level is " + level );
		}

		if( cd.isStolenCard )
		{
			//The maximum level for the stolen card is
			int maxLevel = CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity );
			//The level of Steal card in the player's deck is
			int stealCardLevel = GameManager.Instance.playerDeck.getCardByName(CardName.Steal).level;
			//We do not want to exceed the max level so take the smallest number of the two
			level = Mathf.Min( maxLevel, stealCardLevel );
		}

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
			case CardName.Grenade:
				CardGrenade cardGrenade = GetComponent<CardGrenade>();
				if( cardGrenade != null )
				{
					cardGrenade.activateCard( photonViewId,level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardGrenade component is not attached to the CardHandler in the Level scene.");
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
			case CardName.Trip_Mine:
				CardTripMine cardTripMine = GetComponent<CardTripMine>();
				if( cardTripMine != null )
				{
					cardTripMine.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardTripMine component is not attached to the CardHandler in the Level scene.");
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
			case CardName.Sentry:
				CardSentry cardSentry = GetComponent<CardSentry>();
				if( cardSentry != null )
				{
					cardSentry.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardSentry component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Stasis:
				CardStasis cardStasis = GetComponent<CardStasis>();
				if( cardStasis != null )
				{
					cardStasis.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardStasis component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Force_Field:
				CardForceField cardForceField = GetComponent<CardForceField>();
				if( cardForceField != null )
				{
					cardForceField.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardForceField component is not attached to the CardHandler in the Level scene.");
				}
			break;


			case CardName.Supercharger:
				CardSupercharger cardSupercharger = GetComponent<CardSupercharger>();
				if( cardSupercharger != null )
				{
					cardSupercharger.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardSupercharger component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Hack:
				CardHack cardHack = GetComponent<CardHack>();
				if( cardHack != null )
				{
					cardHack.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardHack component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Homing_Missile:
				CardHomingMissile cardHomingMissile = GetComponent<CardHomingMissile>();
				if( cardHomingMissile != null )
				{
					cardHomingMissile.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardHomingMissile component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Steal:
				CardSteal cardSteal = GetComponent<CardSteal>();
				if( cardSteal != null )
				{
					cardSteal.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardSteal component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Reflect:
				CardReflect cardReflect = GetComponent<CardReflect>();
				if( cardReflect != null )
				{
					cardReflect.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardReflect component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Smoke_Bomb:
				CardSmokeBomb cardSmokeBomb = GetComponent<CardSmokeBomb>();
				if( cardSmokeBomb != null )
				{
					cardSmokeBomb.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardSmokeBomb component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Shockwave:
				CardShockwave cardShockwave = GetComponent<CardShockwave>();
				if( cardShockwave != null )
				{
					cardShockwave.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardShockwave component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Cloak:
				CardCloak cardCloak = GetComponent<CardCloak>();
				if( cardCloak != null )
				{
					cardCloak.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardCloak component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Heal:
				CardHeal cardHeal = GetComponent<CardHeal>();
				if( cardHeal != null )
				{
					cardHeal.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardHeal component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case CardName.Armor:
				CardArmor cardArmor = GetComponent<CardArmor>();
				if( cardArmor != null )
				{
					cardArmor.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardArmor component is not attached to the CardHandler in the Level scene.");
				}
			break;
			default:
				Debug.LogWarning("CardHandler-The card name specified, " + name + ", is unknown.");
			break;
		}

		//Send message to minimap saying which hero played which card
		sendMinimapMessage( userName, (int)name );

		//Play the activate card voice over if there is one
		playActivateCardVoiceOver( casterPhotonView, name );
	}

	void sendMinimapMessage( string userName, int card )
	{
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			PlayerRace.players[i].getMinimapPhotonView().RPC( "minimapRPC", PhotonTargets.All, userName, card );
		}
	}

	void playActivateCardVoiceOver( PhotonView casterPhotonView, CardName name )
	{
		//Play the activate card voice over but only if there is one. We don't want to send RPCs for nothing and waste bandwidth.
		if( casterPhotonView.GetComponent<PlayerVoiceOvers>().isSpellVoiceOverAvailable( name ) ) casterPhotonView.RPC( "activateCardVoiceOverRPC", PhotonTargets.All, (int)name );
	}

	/// <summary>
	/// Verify if playing a card would be effective. Used by bots.
	/// </summary>
	/// <returns><c>true</c>, if card would be effective, <c>false</c> otherwise.</returns>
	/// <param name="caster">Caster.</param>
	/// <param name="name">Name.</param>
	/// <param name="level">Level.</param>
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
			case CardName.Grenade:
				return GetComponent<CardGrenade>().isOpponentNear( caster.transform, level );
			//Double jump is effective whenever you are trailing behind by a little
			case CardName.Double_Jump:
				if( !isCasterLeading( caster.GetComponent<PlayerRace>() ) ) return true;
			break;
			//Sprint is effective whenever you are trailing behind
			case CardName.Sprint:
				if( !isCasterLeading( caster.GetComponent<PlayerRace>() ) ) return true;
			break;
			//Firewall is effective whenever there is an opponent behind you and not too far
			//IceWall is effective whenever there is an opponent behind you and not too far
			case CardName.Force_Field:
				if( isCasterLeading( caster.GetComponent<PlayerRace>() ) )
				{
					return GetComponent<CardForceField>().isAllowed( caster.GetComponent<PhotonView>().viewID );
				}
			break;
			case CardName.Firewall:
				if( isCasterLeading( caster.GetComponent<PlayerRace>() ) )
				{
					return GetComponent<CardFirewall>().isAllowed( caster.GetComponent<PhotonView>().viewID );
				}
			break;
			case CardName.Smoke_Bomb:
				if( isCasterLeading( caster.GetComponent<PlayerRace>() ) )
				{
					return GetComponent<CardSmokeBomb>().isAllowed( caster.GetComponent<PhotonView>().viewID );
				}
			break;
			//Trip Mine is effective whenever there is an opponent behind you and not too far
			case CardName.Trip_Mine:
				if( isCasterLeading( caster.GetComponent<PlayerRace>() ) )
				{
					return GetComponent<CardTripMine>().isAllowed( caster.GetComponent<PhotonView>().viewID );
				}
			break;
			//Lightning is effective whenever your opponent is far ahead of you
			case CardName.Lightning:
				if( !isCasterLeading( caster.GetComponent<PlayerRace>() ) )
				{
					return GetComponent<CardLightning>().isThereATargetWithinRange( caster.transform, level );
				}
			break;
			//Shrink is always effective. It has infinite range.
			case CardName.Shrink:
				return true;
			//Linked Fate is effective whenever you are trailing behind
			case CardName.Linked_Fate:
				if( !isCasterLeading( caster.GetComponent<PlayerRace>() ) ) return true;
			break;
			//Sentry is effective whenever the Sentry would have a valid target within aim range
			case CardName.Sentry:
				CardManager.CardData cd = CardManager.Instance.getCardByName( CardName.Sentry );
				return GetComponent<CardSentry>().isTargetInRange( caster.GetComponent<PlayerRace>(), cd.getCardPropertyValue( CardPropertyType.AIM_RANGE, level ) );
			//Stasis is effective whenever your opponent is far ahead of you
			case CardName.Stasis:
				if( !isCasterLeading( caster.GetComponent<PlayerRace>() ) )
				{
					return GetComponent<CardStasis>().isThereATargetWithinRange( caster.transform, level );
				}
			break;
			//The following cards are effective whenever your opponent is far ahead of you
			case CardName.Homing_Missile:
			case CardName.Supercharger:
			case CardName.Hack:
				if( !isCasterLeading( caster.GetComponent<PlayerRace>() ) )
				{
					return true;
				}
			break;
			//Steal could be effective at any time
			case CardName.Steal:
				return true;
			//Reflect is a bit more effective when you are leading
			case CardName.Reflect:
				return isCasterLeading( caster.GetComponent<PlayerRace>() );
			//Shockwave is effective whenever there are opponents near you
			case CardName.Shockwave:
				return GetComponent<CardShockwave>().isOpponentNear( caster.transform, level );
			//Cloak is effective anytime
			case CardName.Cloak:
				return true;
			//Health Boost is effective when your current health is less than 80% of MAX. Health
			case CardName.Heal:
				return ( caster.GetComponent<PlayerHealth>().getHealth() < 0.8f * PlayerHealth.DEFAULT_HEALTH );
			//Adding armor is always a good idea.
			case CardName.Armor:
				return true;
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
