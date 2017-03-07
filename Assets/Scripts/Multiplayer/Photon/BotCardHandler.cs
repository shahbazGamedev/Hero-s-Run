using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotCardHandler : MonoBehaviour {

	HeroManager.BotHeroCharacter botHero;
	List<PlayerDeck.PlayerCardData> battleDeck;

	// Use this for initialization
	void Start ()
	{
		//Get and store the bot that was selected in MPNetworkLobbyManager and saved in LevelManager.
		botHero = HeroManager.Instance.getBotHeroCharacter( LevelManager.Instance.selectedBotHeroIndex );
	
		//Get and store the battle deck for this hero
		battleDeck = getBattleDeck();
		
	}

	List<PlayerDeck.PlayerCardData> getBattleDeck()
	{
		List<PlayerDeck.PlayerCardData> battleDeck = botHero.botCardDataList.FindAll( card => card.inBattleDeck == true );
		Debug.Log("Cards in bot battle deck:\n" );
		for( int i = 0; i < battleDeck.Count; i++ )
		{
			Debug.Log("Card " + i + " " +  battleDeck[i].name );
		}
		return battleDeck;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
