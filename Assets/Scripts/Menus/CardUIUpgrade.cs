using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class CardUIUpgrade : MonoBehaviour, IPointerDownHandler
{
	[Tooltip("The card image.")]
	[SerializeField] Image cardImage;
	[Tooltip("The card name.")]
	[SerializeField] Text cardName;

	[Header("Level")]
	[Tooltip("The level text is displayed on top of the card image. For example: 'Level 5' or 'Max Level'. The text color varies with the card rarity.")]
	[SerializeField] Text levelText;

	[Header("Properties Panel")]
	[SerializeField] RectTransform propertiesPanel;
	[SerializeField] GameObject cardPropertyPrefab;

	public void configureUpgradePanel( PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		//Card name
		string localizedCardName = LocalizationManager.Instance.getText( "CARD_NAME_" + pcd.name.ToString().ToUpper() );
		cardName.text = localizedCardName;

		//Card image
		cardImage.sprite = cd.icon;
		//Legendary cards have special effects
		cardImage.material = cd.cardMaterial;

		//Level section
		//Level background
		Color rarityColor;
		ColorUtility.TryParseHtmlString (CardManager.Instance.getCardColorHexValue(cd.rarity), out rarityColor);
		if( levelText != null ) levelText.color = rarityColor;

		//Level text
		if( pcd.level + 1 < CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ) )
		{
			if( levelText != null ) levelText.text = String.Format( LocalizationManager.Instance.getText( "CARD_LEVEL"), pcd.level.ToString() );
		}
		else
		{
			if( levelText != null ) levelText.text = LocalizationManager.Instance.getText( "CARD_MAX_LEVEL");
		}

		//Configure card properties
		configureCardProperties( pcd, cd );

		gameObject.SetActive( true );
	}

	void configureCardProperties( PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		if( propertiesPanel == null ) return;

		//Make sure we removed properties that were previously generated first
		for( int i = propertiesPanel.transform.childCount-1; i >= 0; i-- )
		{
			Transform child = propertiesPanel.transform.GetChild( i );
			GameObject.Destroy( child.gameObject );
		}

		for( int i=0; i < cd.cardProperties.Count; i++ )
		{
			createCardProperty( i, cd.cardProperties[i], pcd, cd );
		}
	}

	void createCardProperty( int index, CardManager.CardProperty cp, PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		GameObject go = (GameObject)Instantiate(cardPropertyPrefab);
		go.transform.SetParent(propertiesPanel,false);
		go.GetComponent<CardPropertyUI>().configureProperty( index, cp, pcd, cd, true );
	}

	/// <summary>
	/// Raises the pointer down event. If the player clicks anywhere, it dismisses this popup and its parent.
	/// </summary>
	/// <param name="data">Data.</param>
    public void OnPointerDown(PointerEventData data)
    {
		gameObject.SetActive( false );
		transform.parent.gameObject.SetActive( false );
    }
}
