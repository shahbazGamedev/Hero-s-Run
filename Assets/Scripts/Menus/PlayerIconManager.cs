using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PlayerIconManager : MonoBehaviour {

	[Header("General")]
	bool levelLoading = false;
	[SerializeField] Transform content;
	[SerializeField] GameObject playerIconPrefab;
	[Header("Top Right")]
	[SerializeField] Image currentPlayerIcon;
	[SerializeField] Text playerName;
	[Header("On Select")]
	[SerializeField] GameObject onSelectButton;
	[SerializeField] Image onSelectPlayerIcon;
	[SerializeField] Text onSelectPlayerName;

	// Use this for initialization
	void Start ()
	{
		int playerIconId = GameManager.Instance.playerProfile.getPlayerIconId();
		ProgressionManager.PlayerIconData playerIconData = ProgressionManager.Instance.getPlayerIconDataByUniqueId( playerIconId );
		currentPlayerIcon.sprite = playerIconData.icon;
		playerName.text = PlayerStatsManager.Instance.getUserName();

		//Higher batch number icons appear on top. This could be used to have newly released icons appear on top.
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
		
	}

	void createPlayerIcon( int index )
	{
		ProgressionManager.PlayerIconData playerIconData = ProgressionManager.Instance.getPlayerIconDataByIndex( index );
		GameObject go = (GameObject)Instantiate(playerIconPrefab);
		go.transform.SetParent(content,false);
		Button playerIconButton = go.GetComponent<Button>();
		playerIconButton.onClick.RemoveListener(() => OnClickPlayerIcon(index));
		playerIconButton.onClick.AddListener(() => OnClickPlayerIcon(index));
		Image playerIconImage = go.GetComponent<Image>();
		playerIconImage.sprite = playerIconData.icon;
		playerIconData.rectTransform = go.GetComponent<RectTransform>();
		Image[] playerIconNewRibbon = go.GetComponentsInChildren<Image>();
		playerIconNewRibbon[1].enabled = playerIconData.isNew;
	}

	public void OnClickPlayerIcon( int index )
	{
		RectTransform onSelectRectTransform = onSelectButton.GetComponent<RectTransform>();
		onSelectRectTransform.localScale = Vector3.one;

		ProgressionManager.PlayerIconData playerIconData = ProgressionManager.Instance.getPlayerIconDataByIndex( index );

		//Set the selected icon on top
		currentPlayerIcon.sprite = playerIconData.icon;

		//Position on select game object on top of the selected entry
		onSelectButton.transform.SetParent( playerIconData.rectTransform, false );
		onSelectButton.SetActive( true );

		//Copy the icon and name
		onSelectPlayerIcon.sprite = playerIconData.icon;
		onSelectPlayerName.text = playerIconData.name;

		//If it has a new ribbon, disable it
		Image[] playerIconNewRibbon = playerIconData.rectTransform.GetComponentsInChildren<Image>();
		playerIconData.isNew = false;
		playerIconNewRibbon[1].enabled = false;

		scaleUp();

		//Set this value in Player Profile. It will only be saved when the user exits the scene.
		//We don't want to be saving each time a user clicks on a icon.
		GameManager.Instance.playerProfile.setPlayerIconId( playerIconData.uniqueId );
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

	public void OnClickReturnToWorldMap()
	{
		//Save the player profile. The user may have changed his player icon.
		GameManager.Instance.playerProfile.serializePlayerprofile();
		StartCoroutine( loadScene(GameScenes.WorldMap) );
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
