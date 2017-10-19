using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ResultsScreenHandler : MonoBehaviour {

	[SerializeField] RectTransform resultsHolder;
	[SerializeField] GameObject resultPrefab;

	public void showResults()
	{
		adjustSizeOfResultsScreen( PlayerRace.players.Count );
		for(int i=0; i<PlayerRace.players.Count;i++)
		{
			//For each player, create a result entry
			createResultEntry();
		}
	}

	void adjustSizeOfResultsScreen( int playerCount )
	{
		float singleEntryHeight = resultPrefab.GetComponent<RectTransform>().sizeDelta.y;
		float spacing = GetComponent<VerticalLayoutGroup>().spacing;
		float desiredHeight = playerCount * singleEntryHeight + ( playerCount - 1 ) * spacing;
		resultsHolder.sizeDelta = new Vector2( resultsHolder.sizeDelta.x, desiredHeight );
		print("ResultsScreenHandler-adjustSizeOfResultsScreen " + desiredHeight + " playerCount " + playerCount );
	}

	void createResultEntry()
	{
		GameObject go = (GameObject)Instantiate(resultPrefab);
		go.transform.SetParent(resultsHolder,false);
		go.GetComponent<ResultEntry>().configureEntry( 1, 69, 268, "Natasha", null, "3:37" );
	}

}
