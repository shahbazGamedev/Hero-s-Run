using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardUIUpgrade : MonoBehaviour, IPointerDownHandler {

	public void configureUpgradePanel( PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		gameObject.SetActive( true );
	}

    public void OnPointerDown(PointerEventData data)
    {
		print("Card Upgrade - OnPointerDown" );
		gameObject.SetActive( false );
		transform.parent.gameObject.SetActive( false );
    }
}
