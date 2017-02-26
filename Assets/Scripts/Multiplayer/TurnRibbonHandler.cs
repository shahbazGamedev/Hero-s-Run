using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnRibbonHandler : MonoBehaviour {

	[Header("General")]
	[SerializeField] Transform cardPanel;
	[SerializeField] GameObject cardPrefab;
	[Header("On Select")]
	[SerializeField] GameObject onSelectButton;
	[SerializeField] Image onSelectPlayerIcon;

	// Use this for initialization
	void Start ()
	{
		//Newly unlocked icons appear first.
		List<ProgressionManager.PlayerIconData> playerIconList = ProgressionManager.Instance.getSortedPlayerIconList();

		for( int i = 0; i < 4; i++ )
		{
			createPlayerIcon( i );
		}
	}

	void createPlayerIcon( int index )
	{
		ProgressionManager.PlayerIconData playerIconData = ProgressionManager.Instance.getPlayerIconDataByIndex( index );
		GameObject go = (GameObject)Instantiate(cardPrefab);
		go.transform.SetParent(cardPanel,false);
		Button playerIconButton = go.GetComponent<Button>();
		playerIconButton.onClick.RemoveListener(() => OnClickPlayerIcon(index));
		playerIconButton.onClick.AddListener(() => OnClickPlayerIcon(index));
		Image playerIconImage = go.GetComponent<Image>();
		playerIconImage.sprite = playerIconData.icon;
		playerIconData.rectTransform = go.GetComponent<RectTransform>();
	}

	public void OnClickPlayerIcon( int index )
	{
		print("OnClickPlayerIcon");
		RectTransform onSelectRectTransform = onSelectButton.GetComponent<RectTransform>();
		onSelectRectTransform.localScale = Vector3.one;

		ProgressionManager.PlayerIconData playerIconData = ProgressionManager.Instance.getPlayerIconDataByIndex( index );

		//Position on select game object on top of the selected entry
		onSelectButton.transform.SetParent( playerIconData.rectTransform, false );
		onSelectButton.SetActive( true );

		//Copy the icon and name
		onSelectPlayerIcon.sprite = playerIconData.icon;

		scaleUp();

	}

	void scaleUp()
	{
		CancelInvoke("scaleDown");
		LeanTween.cancel( gameObject );
		RectTransform onSelectRectTransform = onSelectButton.GetComponent<RectTransform>();
		LeanTween.scale( onSelectRectTransform, new Vector3( 1.2f, 1.2f, 1.2f ), 0.18f ).setEaseOutQuad();
		Invoke( "scaleDown", 0.36f );
	}
	
	void scaleDown()
	{
		//Make it the normal size after a few seconds
		RectTransform onSelectRectTransform = onSelectButton.GetComponent<RectTransform>();
		LeanTween.scale( onSelectRectTransform, Vector3.one, 0.18f ).setOnComplete(hide).setOnCompleteParam(gameObject).setEaseOutQuad();
	}

	void hide()
	{
		onSelectButton.SetActive( false );
	}

}
