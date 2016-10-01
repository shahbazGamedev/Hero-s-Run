using UnityEngine;
using System.Collections;
using System;

public enum RequestDataType {
	Unknown = 0,
	Ask_Give_Life = 1,
	Accept_Give_Life = 2
}
public class AppRequestData {

	//format example: "570437323049720_1378641986"
	public string appRequestID = "";
	//Format example: Bob
	public string fromFirstName = "";
	//Format example: Smith
	public string fromLastName = "";
	//Format example: ":"1378641986"
	public string fromID = "";
	//Format examples:
	//a) "Section,11" 	- ask player to help to unlock section starting at level 11
	//b) "Ask Lfe,1" 	- ask player to send him one life
	//c) "Give Life,1" 	- send player one life
	public RequestDataType dataType = RequestDataType.Unknown;
	public int dataNumber = 0; 	//e.g. number of lives, level number of section (this is the shield number which is indexed starting a 1, not the level number which is indexed starting at 0)
	//date field are returned as ISO-8601 formatted strings from the App Request and are stored as DateTime objects.
	public DateTime created_time;
	public bool hasBeenProcessed = false; 

	public void printAppRequestData()
	{
		string printStr = appRequestID + " " + fromFirstName  + " " + fromLastName + " " + fromID + " " + dataType + " " + dataNumber + " " + created_time;
		Debug.Log( "AppRequestData: " + printStr );
		//Example:
		//AppRequestData: 634435613405942_120734471723307 Commander Sheppard 130364490758658 Ask_Give_Life 1 09/30/2016 22:09:42
	}

	//Stores the dataType as an enum
	public void setRequestDataType( string dataTypeStr )
	{
		switch (dataTypeStr)
		{
		case "Ask_Give_Life":
			dataType = RequestDataType.Ask_Give_Life;
			break;
		case "Accept_Give_Life":
			dataType = RequestDataType.Accept_Give_Life;
			break;
		default:
			Debug.LogWarning("AppRequestData-getRequestDataType: unknown data type specified: " + dataType );
			break;
		}

	}
}
