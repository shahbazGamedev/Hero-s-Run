using UnityEngine;
using System.Collections;

public class FriendData
{

	public string first_name;
	//isSelected is true if the entry corresponding to this friend has been selected (i.e. the toggle button is checked) in the Offer Lives popup.
	public bool isSelected = false;

	public void printFriendData()
	{
		string printStr = "first_name " + first_name;
		Debug.Log( "FriendData: " + printStr );
	}

}
