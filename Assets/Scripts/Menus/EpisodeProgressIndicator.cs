using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class EpisodeProgressIndicator : MonoBehaviour {

	public GenerateLevel generateLevel;
	public GameObject checkpointPrefab;
	public GameObject playerIconPrefab;
	RectTransform playerIcon;
	private RectTransform progressBarPanel;
	private float progressBarLength;

	void Awake ()
	{
		progressBarPanel = GetComponent<RectTransform>();
		progressBarLength = progressBarPanel.rect.width;
		createCheckpoints();
		createPlayerIcon(); //do this after because of the sort order
	}
	
	void createCheckpoints()
	{
		GameObject go;
		RectTransform rt;
		List<int> indexOfCheckpointTiles = generateLevel.getIndexOfCheckpointTiles();
		float numberOfTiles = (float) generateLevel.getNumberOfTiles();
		for( int i = 0; i < indexOfCheckpointTiles.Count; i++ )
		{
			go = (GameObject)Instantiate(checkpointPrefab);
			rt = go.GetComponent<RectTransform>();
			go.transform.SetParent( progressBarPanel, false );
			float xPosition = indexOfCheckpointTiles[i]/numberOfTiles * progressBarLength;
			rt.anchoredPosition = new Vector2( xPosition, rt.anchoredPosition.y);
			go.SetActive( true );
		}
	}

	void createPlayerIcon()
	{
		GameObject go;
		go = (GameObject)Instantiate(playerIconPrefab);
		go.SetActive( true );
		playerIcon = go.GetComponent<RectTransform>();
		go.transform.SetParent( progressBarPanel, false );
		playerIcon.anchoredPosition = new Vector2( 0, playerIcon.anchoredPosition.y);
	}

	public void updatePlayerIconPosition ()
	{
		float xPosition = generateLevel.getEpisodeProgress() * progressBarLength;
		playerIcon.anchoredPosition = new Vector2( xPosition, playerIcon.anchoredPosition.y);
	}
}
