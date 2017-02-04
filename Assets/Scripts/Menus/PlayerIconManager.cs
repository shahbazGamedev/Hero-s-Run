﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerIconManager : MonoBehaviour {

	[Header("General")]
	[SerializeField] Transform content;
	[SerializeField] GameObject playerIconPrefab;
	[SerializeField] List<PlayerIconData> playerIconList = new List<PlayerIconData>();

	// Use this for initialization
	void Start ()
	{
		PlayerIconData playerIconData;
		for( int i = 0; i < playerIconList.Count; i++ )
		{
			playerIconData = playerIconList[i];
			GameObject go = (GameObject)Instantiate(playerIconPrefab);
			go.transform.SetParent(content,false);
			Button playerIconButton = go.GetComponent<Button>();
			playerIconButton.onClick.RemoveAllListeners();
			playerIconButton.onClick.AddListener(() => OnClickPlayerIcon(i));
			Image playerIconImage = go.GetComponent<Image>();
			playerIconImage.sprite = playerIconData.icon;
			Text playerIconName = playerIconButton.GetComponentInChildren<Text>();
			playerIconName.text = playerIconData.name;
			//We don't want the text displayed right away
			playerIconName.gameObject.SetActive( false );
		}
		//Calculate the content length
		GridLayoutGroup glg = content.GetComponent<GridLayoutGroup>();
		//We have 3 player icons per row
		int numberOfRows = (int)Mathf.Ceil( playerIconList.Count/3f);
		int contentLength = numberOfRows * ( (int)glg.cellSize.y + (int)glg.spacing.y );
		print("numberOfRows " + numberOfRows + " " + contentLength );
		content.GetComponent<RectTransform>().sizeDelta = new Vector2( content.GetComponent<RectTransform>().rect.width, contentLength );
		
	}

	public void OnClickPlayerIcon( int index )
	{
		print("OnClickPlayerIcon " + index );
	}
	
	public void OnClickExit()
	{
		print("OnClickExit " );
	}

	[System.Serializable]
	public class PlayerIconData
	{
		public Sprite icon;
		public string name = "Overwatch Dark";
		public bool isNew = false;
	}
}
