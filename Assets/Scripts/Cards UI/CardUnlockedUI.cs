using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUnlockedUI : MonoBehaviour {

	[SerializeField] Text topPanelText;
	[SerializeField] Image cardIcon;
	[SerializeField] Text cardManaText;
	[Header("Rarity")]
	[SerializeField] Image rarityIcon;
	[SerializeField] Text rarityText;
	[Header("Description")]
	[SerializeField] Text descriptionText;
	[Header("Properties Panel")]
	[SerializeField] RectTransform propertiesPanel;
	[SerializeField] GameObject cardPropertyPrefab;

	[Header("Fields on Sector Change popup")]
	[SerializeField] GameObject closeButton;
	[SerializeField] TextMeshProUGUI sectorNumberText;

	public void configureCard( CardManager.CardData cd )
	{
		//Title
		string localizedCardName = LocalizationManager.Instance.getText( "CARD_NAME_" + cd.name.ToString().ToUpper() );
		topPanelText.text = string.Format(LocalizationManager.Instance.getText( "CARD_LEVEL_TITLE" ), 1, localizedCardName );

		//Card
		cardIcon.sprite = cd.icon;
		cardManaText.text = cd.manaCost.ToString();

		//Description
		string localizedCardDescription = LocalizationManager.Instance.getText( "CARD_DESCRIPTION_" + cd.name.ToString().ToUpper() );
		descriptionText.text = localizedCardDescription;

		//Rarity
		Color rarityColor;
		ColorUtility.TryParseHtmlString (CardManager.Instance.getCardColorHexValue(cd.rarity), out rarityColor);
		rarityIcon.color = rarityColor;
		rarityText.text = LocalizationManager.Instance.getText( "CARD_RARITY_" + cd.rarity.ToString() );

		//Configure card properties
		configureCardProperties( cd );
	}

	void configureCardProperties( CardManager.CardData cd )
	{
		//Make sure we removed properties that were previously generated first
		for( int i = propertiesPanel.transform.childCount-1; i >= 0; i-- )
		{
			Transform child = propertiesPanel.transform.GetChild( i );
			GameObject.Destroy( child.gameObject );
		}

		for( int i=0; i < cd.cardProperties.Count; i++ )
		{
			createCardProperty( i, cd.cardProperties[i], cd );
		}
	}

	void createCardProperty( int index, CardManager.CardProperty cp, CardManager.CardData cd )
	{
		GameObject go = (GameObject)Instantiate(cardPropertyPrefab);
		go.transform.SetParent(propertiesPanel,false);
		go.GetComponent<CardPropertyUI>().configureProperty( index, cp, 1, cd, false );
	}

	public void OnClickHide()
	{
		sectorNumberText.gameObject.SetActive( true );
		closeButton.SetActive( true );
		gameObject.SetActive( false );
	}
}
