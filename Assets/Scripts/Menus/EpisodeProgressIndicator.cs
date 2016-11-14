using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class EpisodeProgressIndicator : MonoBehaviour {

	public GenerateLevel generateLevel;
	public GameObject checkpointPrefab;
	public RectTransform playerIcon;
	private RectTransform progressBarPanel;
	private float progressBarLength;

	void Start ()
	{
		progressBarPanel = GetComponent<RectTransform>();
		progressBarLength = progressBarPanel.rect.width;
		createCheckpoints();
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
			Debug.Log("createCheckpoints index " + i + " xPosition " + xPosition + " checkpoint index " + indexOfCheckpointTiles[i] + " progressBarLength " +  progressBarLength + " nbr tiles " + generateLevel.getNumberOfTiles() );
		}
	}

	// Update is called once per frame
	void Update ()
	{
		float xPosition = generateLevel.getEpisodeProgress() * progressBarLength;
		playerIcon.anchoredPosition = new Vector2( xPosition, playerIcon.anchoredPosition.y);
	}
}
