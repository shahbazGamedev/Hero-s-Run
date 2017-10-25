using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class csvLoader {

	//Parses the CSV file and populates a dictionary where the key is the text ID and the value, the text in the language of the device.
	//The CSV file should have the following columns:
	//TEXT_ID,ENGLISH,FRENCH,ITALIAN,GERMAN,SPANISH
	//The file needs to use UTF-16 encoding.
	//The language index is a value from 0 to 4, where 0 is English, 1 is French, etc.
	//The header row of the CSV file (row is 0) is not saved.
	//If the language is not found, it will default to English.
	public static Dictionary<string, string> PopulateDictionary ( TextAsset csvFile, int languageIndex )
	{

		string[] rows = csvFile.text.Split("\n"[0]);

		Dictionary<string, string> gameText = new Dictionary<string, string>(rows.Length);

		string[] rowContent;
		//Don't import the CSV value header (i.e. row zero)
		for( int i=1; i < rows.Length; i++ )
		{
			//The # character at the beginning of a row is used to indicate a row header or comment. These are not imported in the game.
			if( !rows[i].StartsWith("#") )
			{
				rowContent = rows[i].Split(","[0]);
				string text = rowContent[languageIndex].Trim();
				//Commas in the text are identified by <comma>
				text = text.Replace("<comma>", "," );
				//Debug.Log( "row: " + i + " " + rowContent[0] + ", " + rowContent[languageIndex] );
				if( !gameText.ContainsKey( rowContent[0].Trim() ) )
				{
					gameText.Add( rowContent[0].Trim (), text );
				}
				else
				{
					Debug.LogWarning("csvLoader-The game text identifier, " + rowContent[0].Trim () + ", already exists in the dictionary. Skipping it.");
				}
			}
		}
		return gameText;
	}
}
