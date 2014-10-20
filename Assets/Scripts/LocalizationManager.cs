using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//This class is a Singleton
//Currently EFIGS languages are supported.
public class LocalizationManager {


	private static LocalizationManager localizationManager = null;
	string dictionaryPath = "Localization/GameText";
	Dictionary<string, string> gameText;
	int languageIndex;


	public static LocalizationManager Instance
	{
        get
		{
			if (localizationManager == null)
			{

				localizationManager = new LocalizationManager();

            }
			return localizationManager;
        }
    }

	private int getLanguage()
	{
		int languageIndex;
		SystemLanguage language = Application.systemLanguage;
		Debug.Log("The device is running in the " + language + " language." );
		switch (language)
		{
		case SystemLanguage.English:
			languageIndex = 1;
			break;
			
		case SystemLanguage.French:
			languageIndex = 2;
			break;
			
		case SystemLanguage.Italian:
			languageIndex = 3;
			break;
			
		case SystemLanguage.German:
			languageIndex = 4;
			break;

		case SystemLanguage.Spanish:
			languageIndex = 5;
			break;

		default:
			//The language is not supported. Default to English.
			languageIndex = 1;
			Debug.LogWarning("LocalizationManager-setLanguage: the device language " + language + " is not supported. Defaulting to English."  );
			break;
		}
		return languageIndex;
	}


	public void initialize()
	{
		languageIndex = getLanguage();

		TextAsset csvFile =  Resources.Load(dictionaryPath) as TextAsset;
		if( csvFile != null )
		{
			gameText = csvLoader.PopulateDictionary( csvFile, languageIndex );
		}
		else
		{
			Debug.LogError( "LocalizationManager-initializeGameText: Unable to open file : " + dictionaryPath );
		}
	}

	public string getText( string textID )
	{
		if( gameText != null )
		{
			string result = null;
			if( gameText.TryGetValue(textID, out result) )
			{
				//success!
				return result;
			}
			else
			{
				//failure!
				Debug.LogWarning("LocalizationManager-getText: unable to find text for : " + textID );
				return "NOT FOUND";
			}
		}
		else
		{
			return "NOT INITIALISED";
		}
	}

}
