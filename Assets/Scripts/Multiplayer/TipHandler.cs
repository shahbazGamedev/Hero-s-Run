using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipHandler : MonoBehaviour {

	[Header("Game Tips")]
	[SerializeField] Text tipDescription;
	List<string> tipList = new List<string>();

	void Start ()
	{
		populateTipList();
		displayRandomTip ();
	}

	void populateTipList()
	{
		string tip = "You can plough through an ice wall with raging Bull.";
		tipList.Add(tip);
		tip = "A player hit with Statis will lose his Sentry bot if he has one.";
		tipList.Add(tip);
		tip = "You can't play cards while ziplining.";
		tipList.Add(tip);
		tip = "You can invite a friend to race if you know their user name.";
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
