using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HellTextManager : MonoBehaviour {

	public TextMesh textLine1;
	public TextMesh textLine2;
	public TextMesh textLine3;
	public int maximumCharactersPerLine = 18;
	//List of text ids. A text from this list will be selected to appear on the associated banner.
	static List<string> textList = new List<string>(4);
	static int textIndex = 0;

	void Awake()
	{
		assembleTexts();
		splitTextIntoThreeLines(getFunnyText());
	}

	void assembleTexts()
	{
		if( textList.Count == 0 )
		{
			textList.Add("HELL_BANNER_BINGO");
			textList.Add("HELL_BANNER_BRUNCH");
			textList.Add("HELL_BANNER_ARMS_DEALER");
			textList.Add("HELL_BANNER_BILLIONS_SERVED");
		 }
	}

	string getFunnyText()
	{
		if( textIndex == textList.Count ) textIndex = 0;
		string selectedText = textList[textIndex];
		textIndex++;
		return LocalizationManager.Instance.getText(selectedText);
	}

	void splitTextIntoThreeLines( string text )
	{
		int currentWordIndex = 0;
		int currentCharacterCount = 0;
		string currentLine1 = System.String.Empty;
		string currentLine2 = System.String.Empty;
		string currentLine3 = System.String.Empty;

		string[] words = text.Split(' ');
		//Add a space at the end of each word except the last one
		for( int i = 0; i < words.Length - 1; i++ )
		{
			words[i] = words[i] + " ";
		}

		if( words != null && words.Length > 1 )
		{
			while ( currentCharacterCount <= maximumCharactersPerLine && currentWordIndex < words.Length )
			{
				if( words[currentWordIndex].Length + currentCharacterCount <= maximumCharactersPerLine )
				{
					//Add the word to the current line
					currentCharacterCount = currentCharacterCount + words[currentWordIndex].Length;
					currentLine1 = currentLine1 + words[currentWordIndex];
					currentWordIndex++;
				}
				else
				{
					break;
				}
			}
			currentCharacterCount = 0;
			while ( currentCharacterCount <= maximumCharactersPerLine && currentWordIndex < words.Length )
			{
				if( words[currentWordIndex].Length + currentCharacterCount <= maximumCharactersPerLine )
				{
					//Add the word to the current line
					currentCharacterCount = currentCharacterCount + words[currentWordIndex].Length;
					currentLine2 = currentLine2 + words[currentWordIndex];
					currentWordIndex++;
				}
				else
				{
					break;
				}
			}
			currentCharacterCount = 0;
			while ( currentCharacterCount <= maximumCharactersPerLine && currentWordIndex < words.Length )
			{
				if( words[currentWordIndex].Length + currentCharacterCount <= maximumCharactersPerLine )
				{
					//Add the word to the current line
					currentCharacterCount = currentCharacterCount + words[currentWordIndex].Length;
					currentLine3 = currentLine3 + words[currentWordIndex];
					currentWordIndex++;
				}
				else
				{
					break;
				}
			}

		}
		textLine1.text = currentLine1;
		textLine2.text = currentLine2;
		textLine3.text = currentLine3;
	}
}
