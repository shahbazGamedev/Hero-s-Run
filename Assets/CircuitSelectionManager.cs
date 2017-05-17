using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CircuitSelectionManager : MonoBehaviour {

	[Header("Race Track Details")]
	[SerializeField] Transform raceTrackHolder;
	[SerializeField] GameObject raceTrackPrefab;
	bool levelLoading = false;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();
		createRaceTracks();
		adjustContentHeight();
	}

	void adjustContentHeight()
	{
		//Adjust the length of the content based on the number of entries.
		//All entries have the same height.
		//There is spacing between the entries.
		float top = raceTrackHolder.GetComponent<VerticalLayoutGroup>().padding.top;
		float elementHeight = raceTrackPrefab.GetComponent<LayoutElement>().preferredHeight;
		float spacing = raceTrackHolder.GetComponent<VerticalLayoutGroup>().spacing;
		float buffer = 50;
		float contentHeight = top + buffer + raceTrackHolder.childCount * elementHeight + ( raceTrackHolder.childCount - 1 ) * spacing;
		raceTrackHolder.GetComponent<RectTransform>().sizeDelta = new Vector2( raceTrackHolder.GetComponent<RectTransform>().sizeDelta.x, contentHeight );
	}

	#region Race Tracks
	void createRaceTracks()
	{
		List<LevelData.MultiplayerInfo> raceTrackList = LevelManager.Instance.getLevelData().getSortedRaceTrackList();
		for( int i = 0; i < raceTrackList.Count; i++ )
		{
			createRaceTrack( i, raceTrackList[i] );
		}
	}

	void createRaceTrack( int index, LevelData.MultiplayerInfo info )
	{
		GameObject go = (GameObject)Instantiate(raceTrackPrefab);
		go.transform.SetParent(raceTrackHolder,false);
		Button raceTrackButton = go.GetComponent<Button>();
		raceTrackButton.onClick.RemoveAllListeners();
		raceTrackButton.onClick.AddListener(() => OnClickRaceTrack( info ));
		go.GetComponent<RaceTrackUI>().configure( index, info );
	}

	public void OnClickRaceTrack( LevelData.MultiplayerInfo info )
	{
		LevelManager.Instance.setSelectedCircuit( info );
		StartCoroutine( loadScene(GameScenes.Matchmaking) );
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
	#endregion

}
