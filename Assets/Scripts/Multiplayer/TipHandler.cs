using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipHandler : MonoBehaviour {

	[Header("Game Tips")]
	[SerializeField] Text tipTitle;
	[SerializeField] Text tipDescription;
	List<string> tipList = new List<string>();

	void Start ()
	{
		populateTipList();
		displayRandomTip ();
	}

	void populateTipList()
	{
		string tip = "I like coffee especially in the morning.";
		tipList.Add(tip);
		tip = "I also like a cold beer on a hot summer day.";
		tipList.Add(tip);
		tip = "But enjoy the most a good glass of red wine.";
		tipList.Add(tip);
	}

	void displayRandomTip ()
	{
		if( tipList.Count > 0 )
		{
			tipDescription.text = tipList[ Random.Range(0, tipList.Count) ];
		}
		else
		{
			Debug.LogError("TipHandler-the tip list is empty.");
			tipDescription.text = string.Empty;
		}
	}
}
