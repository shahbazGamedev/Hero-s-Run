using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroCardHandler : MonoBehaviour {

	[SerializeField] Button heroCardButton;
	[SerializeField] GameObject cardDetailPopup;
	[SerializeField] GameObject heroSelectionTopPanel;

	public void configureHeroCard( CardName cardName )
	{
		CardManager.CardData cd = CardManager.Instance.getCardByName(cardName);
		heroCardButton.onClick.RemoveAllListeners();
		heroCardButton.onClick.AddListener(() => OnClickCard( cd ));
		Image cardImage = heroCardButton.GetComponent<Image>();
		cardImage.sprite = cd.icon;
		//Legendary cards have special effects
		cardImage.material = cd.cardMaterial;
		TextMeshProUGUI cardNameText = GetComponentInChildren<TextMeshProUGUI>();
		string localizedCardName = LocalizationManager.Instance.getText( "CARD_NAME_" + cd.name.ToString().ToUpper() );
		cardNameText.text = localizedCardName;
	}
	
	public void OnClickCard( CardManager.CardData cd )
	{
		cardDetailPopup.GetComponent<CardUnlockedUI>().configureCard( cd );
		cardDetailPopup.GetComponent<CardUnlockedUI>().show( true );
		//Hide the top panel or else you will have 2 top panels and it does not look nice
		heroSelectionTopPanel.gameObject.SetActive( false );
	}

	public void OnClickHide()
	{
		cardDetailPopup.gameObject.SetActive( false );
		heroSelectionTopPanel.gameObject.SetActive( true );
	}

}
