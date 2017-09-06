using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BattleDeckSelector : MonoBehaviour {

	[SerializeField] CardCollectionManager cardCollectionManager;
	[SerializeField] Toggle battleDeckOneToggle;
	[SerializeField] Toggle battleDeckTwoToggle;
	[SerializeField] Toggle battleDeckThreeToggle;

	void Awake()
	{
		switch( GameManager.Instance.playerDeck.getActiveDeck())
		{
			case BattleDeck.BATTLE_DECK_ONE:
				battleDeckOneToggle.isOn = true;
				battleDeckTwoToggle.isOn = false;
				battleDeckThreeToggle.isOn = false;
			break;

			case BattleDeck.BATTLE_DECK_TWO:
				battleDeckOneToggle.isOn = false;
				battleDeckTwoToggle.isOn = true;
				battleDeckThreeToggle.isOn = false;
			break;

			case BattleDeck.BATTLE_DECK_THREE:
				battleDeckOneToggle.isOn = false;
				battleDeckTwoToggle.isOn = false;
				battleDeckThreeToggle.isOn = true;
			break;
		}
	}

	public void OnValueChangedDeck1()
	{
		if( GameManager.Instance.playerDeck.getActiveDeck() != BattleDeck.BATTLE_DECK_ONE )
		{
			GameManager.Instance.playerDeck.setActiveDeck( BattleDeck.BATTLE_DECK_ONE );
			//If card replacement is in progress, stop it.
			cardCollectionManager.stopCardReplacement();
			cardCollectionManager.initialize();
		}
	}

	public void OnValueChangedDeck2()
	{
		if( GameManager.Instance.playerDeck.getActiveDeck() != BattleDeck.BATTLE_DECK_TWO )
		{
			GameManager.Instance.playerDeck.setActiveDeck( BattleDeck.BATTLE_DECK_TWO );
			//If card replacement is in progress, stop it.
			cardCollectionManager.stopCardReplacement();
			cardCollectionManager.initialize();
		}
	}

	public void OnValueChangedDeck3()
	{
		if( GameManager.Instance.playerDeck.getActiveDeck() != BattleDeck.BATTLE_DECK_THREE )
		{
			GameManager.Instance.playerDeck.setActiveDeck( BattleDeck.BATTLE_DECK_THREE );
			//If card replacement is in progress, stop it.
			cardCollectionManager.stopCardReplacement();
			cardCollectionManager.initialize();
		}
	}

}
