using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeroCardHandler : MonoBehaviour {

	[SerializeField] Button heroCardButton;
	[SerializeField] GameObject cardDetailPopup;

	public void configureHeroCard( CardName cardName )
	{
		CardManager.CardData cd = CardManager.Instance.getCardByName(cardName);
		heroCardButton.onClick.RemoveAllListeners();
		heroCardButton.onClick.AddListener(() => OnClickCard( cd ));
		Image cardImage = heroCardButton.GetComponent<Image>();
		cardImage.sprite = cd.icon;
		//Legendary cards have special effects
		cardImage.material = cd.cardMaterial;

	}
	
	public void OnClickCard( CardManager.CardData cd )
	{
		cardDetailPopup.GetComponent<CardUnlockedUI>().configureCard( cd );
		cardDetailPopup.GetComponent<CardUnlockedUI>().show( true );
	}
}
