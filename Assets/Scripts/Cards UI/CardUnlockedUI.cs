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
	[Header("Description Mode")]
	[SerializeField] TextMeshProUGUI modeButtonText;
	[SerializeField] Color competitionModeColor;
	[SerializeField] Color zombieModeColor;
	[SerializeField] Image background;
	[SerializeField] GameObject zombieModeDecorations;
	private bool isShowingCompetitionMode = true;

	private CardManager.CardData cd;

	#region Events
	//Event management used to notify other classes when this popup is displayed or hidden
	public delegate void CardUnlockedUIDisplayed( bool displayed );
	public static event CardUnlockedUIDisplayed cardUnlockedUIDisplayed;
	#endregion

	public void configureCard( CardManager.CardData cd )
	{
		//Store
		this.cd = cd;

		if( isShowingCompetitionMode )
		{
			modeButtonText.text = LocalizationManager.Instance.getText( "CARD_MODE_COMPETITION" );
			background.color = competitionModeColor;
			zombieModeDecorations.SetActive( false );
		}
		else
		{
			modeButtonText.text = LocalizationManager.Instance.getText( "CARD_MODE_ZOMBIE" );
			background.color = zombieModeColor;
			zombieModeDecorations.SetActive( true );
		}

		//Title
		string localizedCardName = LocalizationManager.Instance.getText( "CARD_NAME_" + cd.name.ToString().ToUpper() );
		topPanelText.text = string.Format(LocalizationManager.Instance.getText( "CARD_LEVEL_TITLE" ), 1, localizedCardName );

		//Card
		cardIcon.sprite = cd.icon;
		//Legendary cards have special effects
		cardIcon.material = cd.cardMaterial;

		cardManaText.text = cd.powerCost.ToString();

		//Description
		setCardDescription();

		//Rarity
		Color rarityColor;
		ColorUtility.TryParseHtmlString (CardManager.Instance.getCardColorHexValue(cd.rarity), out rarityColor);
		rarityIcon.color = rarityColor;
		rarityText.text = LocalizationManager.Instance.getText( "CARD_RARITY_" + cd.rarity.ToString() );

		//Configure card properties
		configureCardProperties( cd );
	}

	void setCardDescription()
	{
		//Description
		string localizedCardDescription = string.Empty;
		if( isShowingCompetitionMode )
		{
			localizedCardDescription = LocalizationManager.Instance.getText( "CARD_DESCRIPTION_" + cd.name.ToString().ToUpper() );
		}
		else
		{
			localizedCardDescription = LocalizationManager.Instance.getText( "CARD_DESCRIPTION_COOP_" + cd.name.ToString().ToUpper() );
			//Some cards have the exact same description for both modes.
			//If there is no coop description, simply use the one for competition.
			if( localizedCardDescription == "NOT FOUND" ) localizedCardDescription = LocalizationManager.Instance.getText( "CARD_DESCRIPTION_" + cd.name.ToString().ToUpper() );
		}
		descriptionText.text = localizedCardDescription;
	}

	void configureCardProperties( CardManager.CardData cd )
	{
		//Make sure we removed properties that were previously generated first
		for( int i = propertiesPanel.transform.childCount-1; i >= 0; i-- )
		{
			Transform child = propertiesPanel.transform.GetChild( i );
			GameObject.Destroy( child.gameObject );
		}

		int indexForBackground = 0;
		for( int i=0; i < cd.cardProperties.Count; i++ )
		{
			if( isShowingCompetitionMode )
			{
				if( cd.cardProperties[i].mode == CardPropertyMode.SHARED || cd.cardProperties[i].mode == CardPropertyMode.COMPETITION_ONLY )
				{
					createCardProperty( indexForBackground, cd.cardProperties[i], cd );
					indexForBackground++;
				}		
			}
			else
			{
				if( cd.cardProperties[i].mode == CardPropertyMode.SHARED || cd.cardProperties[i].mode == CardPropertyMode.COOP_ONLY )
				{
					createCardProperty( indexForBackground, cd.cardProperties[i], cd );
					indexForBackground++;
				}		
			}
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
		show( false );
	}

	public void show( bool value )
	{
		if( cardUnlockedUIDisplayed !=  null ) cardUnlockedUIDisplayed( value );
		gameObject.SetActive( value );
	}

	public void OnClickToggleMode()
	{
		isShowingCompetitionMode = !isShowingCompetitionMode;
		configureCard( cd );
	}

}
