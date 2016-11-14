using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class EpisodeProgressIndicator : MonoBehaviour {

	public GenerateLevel generateLevel;
	public Slider progressBar;
	public GameObject checkpointPrefab;
	public RectTransform handleSlideArea;

	void Start ()
	{
		createCheckpoints();
	}
	
	void createCheckpoints()
	{
		GameObject go;
		RectTransform rt;
		List<int> indexOfCheckpointTiles = generateLevel.getIndexOfCheckpointTiles();
		float sliderWidth = handleSlideArea.GetComponent<RectTransform>().rect.width;
		float numberOfTiles = (float) generateLevel.getNumberOfTiles();
		for( int i = 0; i < indexOfCheckpointTiles.Count; i++ )
		{
			go = (GameObject)Instantiate(checkpointPrefab);
			rt = go.GetComponent<RectTransform>();
			go.transform.SetParent( progressBar.transform, false );
			float xPosition = indexOfCheckpointTiles[i]/numberOfTiles * sliderWidth + 10f;
			rt.anchoredPosition = new Vector2( xPosition, rt.anchoredPosition.y);
			go.SetActive( true );
			Debug.Log("createCheckpoints index " + i + " xPosition " + xPosition + " % " + indexOfCheckpointTiles[i] + " sliderWidth " +  sliderWidth + " nbr tiles " + generateLevel.getNumberOfTiles() );
		}
	}

	// Update is called once per frame
	void Update ()
	{
		progressBar.value = generateLevel.getEpisodeProgress();
	}
}
