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
		numberOfTrophiesText.text = GameManager.Instance.playerInventory.getTrophyBalance().ToString("N0");
		configureEntries();
	}

	void configureEntries()
	{
		List<PlayerStatistics.StatisticData> statisticEntriesList = GameManager.Instance.playerStatistics.getStatisticEntriesList();

		for( int i=0; i < statisticEntriesList.Count; i++ )
		{
			createStatisticEntry( i, statisticEntriesList[i].type, getIcon( statisticEntriesList[i].type ), statisticEntriesList[i].value  );
		}
	}

	void createStatisticEntry( int index, StatisticDataType type, Sprite icon, int value )
	{
		GameObject go = (GameObject)Instantiate(statisticEntryPrefab);
		go.transform.SetParent(statisticEntriesPanel,false);
		go.GetComponent<StatisticEntryUI>().configureEntry( index, type, icon, value );
	}

	Sprite getIcon( StatisticDataType type )
	{
		return statisticEntriesList.Find( data => data.type == type).icon;
	}	

	[System.Serializable]
	public class StatisticEntryIcon
	{
		public StatisticDataType type; 
		public Sprite icon;
	}

}
