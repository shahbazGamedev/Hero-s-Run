using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailPopup : MonoBehaviour {
	
	[Tooltip("TBC.")]
	[SerializeField] Text topPanelText;
	[SerializeField] GameObject card;

	public void configureCard (PlayerDeck.PlayerCardData pcd, CardManager.CardData cd)
	{
		topPanelText.text = string.Format("Level {0} {1}", pcd.level, pcd.name );
		card.GetComponent<CardUIDetails>().configureCard( pcd, cd );
	}

	public void OnClickHide()
	{
		gameObject.SetActive( false );
	}

}
