using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsMenu : MonoBehaviour {

	[Header("Statistics Panel")]
	[SerializeField] RectTransform statisticEntriesPanel;
	[SerializeField] GameObject statisticEntryPrefab;
	[SerializeField] List<StatisticEntryIcon> statisticEntriesList = new List <StatisticEntryIcon>();
	[Header("Trophies")]
	[SerializeField] Text numberOfTrophiesText;

	// Use this for initialization
	void Start ()
	{	
		numberOfTrophiesText.text = GameManager.Instance.playerProfile.getTrophies().ToString("N0");
		configureEntries();
	}

	void configureEntries()
	{
		for( int i=0; i < statisticEntriesList.Count; i++ )
		{
			createStatisticEntry( i, statisticEntriesList[i].type, statisticEntriesList[i].icon, GameManager.Instance.playerStatistics.getStatisticData( statisticEntriesList[i].type ) );
		}
	}

	void createStatisticEntry( int index, StatisticDataType type, Sprite icon, int value )
	{
		GameObject go = (GameObject)Instantiate(statisticEntryPrefab);
		go.transform.SetParent(statisticEntriesPanel,false);
		go.GetComponent<StatisticEntryUI>().configureEntry( index, type, icon, value );
	}

	[System.Serializable]
	public class StatisticEntryIcon
	{
		public StatisticDataType type; 
		public Sprite icon;
	}

}
