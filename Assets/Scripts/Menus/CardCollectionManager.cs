using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

class CardCollectionManager : MonoBehaviour {

	[Header("General")]
	bool levelLoading = false;
	[SerializeField] Transform content;
	[SerializeField] GameObject cardPrefab;
	[Header("Top Right")]
	[SerializeField] Image currentPlayerIcon;
	[SerializeField] Text playerName;
	[Header("On Select")]
	[SerializeField] GameObject onSelectButton;
	[SerializeField] Image onSelectPlayerIcon;
	[SerializeField] Text onSelectPlayerName;
	[Header("Texts")]
	[SerializeField] Text menuTitle;
	public PlayerDeck playerDeck;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();
		/*int playerIconId = GameManager.Instance.playerProfile.getPlayerIconId();
		ProgressionManager.PlayerIconData playerIconData = ProgressionManager.Instance.getPlayerIconDataByUniqueId( playerIconId );
		currentPlayerIcon.sprite = playerIconData.icon;
		playerName.text = PlayerStatsManager.Instance.getUserName();

		//Newly unlocked icons appear first.
		List<ProgressionManager.PlayerIconData> playerIconList = ProgressionManager.Instance.getSortedPlayerIconList();

		for( int i = 0; i < playerIconList.Count; i++ )
		{
			createPlayerIcon( i );
		}
		//Calculate the content length
		GridLayoutGroup glg = content.GetComponent<GridLayoutGroup>();
		//We have 3 player icons per row
		int numberOfRows = (int)Mathf.Ceil( playerIconList.Count/3f);
		int contentLength = numberOfRows * ( (int)glg.cellSize.y + (int)glg.spacing.y ) + glg.padding.top;
		content.GetComponent<RectTransform>().sizeDelta = new Vector2( content.GetComponent<RectTransform>().rect.width, contentLength );
		
		//Localise
		menuTitle.text = LocalizationManager.Instance.getText( "PLAYER_ICON_MENU_TITLE" );*/

		List<PlayerDeck.PlayerCardData> battleDeckList = GameManager.Instance.playerDeck.getBattleDeck();
		for( int i = 0; i < battleDeckList.Count; i++ )
		{
			createCard( i, battleDeckList[i] );
		}

	}

	void createCard( int index, PlayerDeck.PlayerCardData pcd )
	{
		GameObject go = (GameObject)Instantiate(cardPrefab);
		CardManager.CardData cd = CardManager.Instance.getCardByName( pcd.name );
		go.transform.SetParent(content,false);
		Button cardButton = go.GetComponent<Button>();
		cardButton.onClick.RemoveListener(() => OnClickPlayerIcon(index,pcd, cd));
		cardButton.onClick.AddListener(() => OnClickPlayerIcon(index,pcd, cd));
		go.GetComponent<CardUIDetails>().configureCard( pcd, cd );
	}

	public void OnClickPlayerIcon( int index, PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		RectTransform onSelectRectTransform = onSelectButton.GetComponent<RectTransform>();
		onSelectRectTransform.localScale = Vector3.one;
		onSelectRectTransform.localPosition = Vector3.zero;

		//Position on select game object on top of the selected entry
		onSelectButton.transform.SetParent( cd.rectTransform, false );
		onSelectButton.SetActive( true );

		//Copy the icon and name
		onSelectPlayerIcon.sprite = cd.icon;
		onSelectPlayerName.text = cd.name.ToString();

		scaleUp();

		/*if( !playerIconData.isLocked)
		{
			//Set the selected icon on top
			currentPlayerIcon.sprite = playerIconData.icon;

			//Set this value in Player Profile. It will only be saved when the user exits the scene.
			//We don't want to be saving each time a user clicks on a icon.
			GameManager.Instance.playerProfile.setPlayerIconId( playerIconData.uniqueId );
		}*/
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

	public void OnClickOpenMainMenu()
	{
		//Save the player profile. The user may have changed his player icon.
		GameManager.Instance.playerProfile.serializePlayerprofile();
		StartCoroutine( loadScene(GameScenes.MainMenu) );
	}

	IEnumerator loadScene(GameScenes value)
	{
		if( !levelLoading )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)value );
		}
	}

}
