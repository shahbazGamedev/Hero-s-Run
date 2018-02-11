using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

class PlayerIconManager : MonoBehaviour {

	[Header("General")]
	[SerializeField] Transform content;
	[SerializeField] GameObject playerIconPrefab;
	[Header("On Select")]
	[SerializeField] GameObject onSelectButton;
	[SerializeField] Image onSelectPlayerIcon;
	[SerializeField] Text onSelectPlayerName;
	[Header("Name and Icon")]
	[SerializeField] Image playerIcon;
	[SerializeField] TextMeshProUGUI playerNameText;

	List<PlayerIcons.PlayerIconData> sortedPlayerIconList;

	// Use this for initialization
	void Start ()
	{
		//Newly unlocked icons appear first.
		sortedPlayerIconList = GameManager.Instance.playerIcons.getSortedPlayerIconList();

		for( int i = 0; i < sortedPlayerIconList.Count; i++ )
		{
			createPlayerIcon( i );
		}
		//Calculate the content length
		GridLayoutGroup glg = content.GetComponent<GridLayoutGroup>();
		//We have 3 player icons per row
		int numberOfRows = (int)Mathf.Ceil( sortedPlayerIconList.Count/3f);
		int contentLength = numberOfRows * ( (int)glg.cellSize.y + (int)glg.spacing.y ) + glg.padding.top;
		content.GetComponent<RectTransform>().sizeDelta = new Vector2( content.GetComponent<RectTransform>().rect.width, contentLength );		
	}

	void createPlayerIcon( int index )
	{
		PlayerIcons.PlayerIconData playerIconData = sortedPlayerIconList[index];
		Sprite sprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( playerIconData.uniqueId ).icon;
		GameObject go = (GameObject)Instantiate(playerIconPrefab);
		go.transform.SetParent(content,false);
		Button playerIconButton = go.GetComponent<Button>();
		playerIconButton.onClick.RemoveListener(() => OnClickPlayerIcon(index));
		playerIconButton.onClick.AddListener(() => OnClickPlayerIcon(index));
		Image playerIconImage = go.GetComponent<Image>();
		playerIconImage.sprite = sprite;
		playerIconData.rectTransform = go.GetComponent<RectTransform>();
		Image[] playerIconNewRibbon = go.GetComponentsInChildren<Image>();
		playerIconNewRibbon[1].enabled = playerIconData.isNew;		//new ribbon
		playerIconNewRibbon[2].enabled = playerIconData.isLocked;	//mask when locked
		playerIconNewRibbon[3].enabled = playerIconData.isLocked;	//lock icon
	}

	public void OnClickPlayerIcon( int index )
	{
		RectTransform onSelectRectTransform = onSelectButton.GetComponent<RectTransform>();
		onSelectRectTransform.localScale = Vector3.one;

		PlayerIcons.PlayerIconData playerIconData = sortedPlayerIconList[index];
		Sprite sprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( playerIconData.uniqueId ).icon;

		//Position on select game object on top of the selected entry
		onSelectButton.transform.SetParent( playerIconData.rectTransform, false );
		onSelectButton.SetActive( true );

		//Copy the icon and name
		onSelectPlayerIcon.sprite = sprite;
		onSelectPlayerName.text = LocalizationManager.Instance.getText( "PLAYER_ICON_" + playerIconData.uniqueId.ToString() );

		//If it has a new ribbon, disable it
		Image[] playerIconNewRibbon = playerIconData.rectTransform.GetComponentsInChildren<Image>();
		playerIconData.isNew = false;
		playerIconNewRibbon[1].enabled = false;
		playerIconData.isNew = false;
		GameManager.Instance.playerIcons.serializePlayerIcons( true );

		scaleUp();

		if( !playerIconData.isLocked)
		{
			//Set this value in Player Profile.
			GameManager.Instance.playerProfile.setPlayerIconId( playerIconData.uniqueId );
			//Update the player icon at the top right
			playerIcon.sprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( GameManager.Instance.playerProfile.getPlayerIconId() ).icon;
			//Save the player profile. The user may have changed his player icon.
			GameManager.Instance.playerProfile.serializePlayerprofile( true );
		}
	}

	void scaleUp()
	{
		CancelInvoke("scaleDown");
		LeanTween.cancel( gameObject );
		RectTransform onSelectRectTransform = onSelectButton.GetComponent<RectTransform>();
		LeanTween.scale( onSelectRectTransform, new Vector3( 1.2f, 1.2f, 1.2f ), 0.2f );
		Invoke( "scaleDown", 2f );
	}
	
	void scaleDown()
	{
		//Make it the normal size after a few seconds
		RectTransform onSelectRectTransform = onSelectButton.GetComponent<RectTransform>();
		LeanTween.scale( onSelectRectTransform, Vector3.one, 0.2f ).setOnComplete(hide).setOnCompleteParam(gameObject);;
	}

	void hide()
	{
		onSelectButton.SetActive( false );
	}

}
