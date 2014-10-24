using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GalleryManager : MonoBehaviour {

	[Header("Frame")]
	public Text menuTitle;
	string menuTitleTextId = "GALLERY_TITLE";
	//Fairy
	[Header("Fairy")]
	public Text fairyCharacterName;
	public Text fairyCharacterText;
	string fairyCharacterNameTextId = "GALLERY_NAME_FAIRY";
	string fairyCharacterTextTextId  = "GALLERY_BIO_FAIRY";
	//Dark Queen
	[Header("Dark Queen")]
	public Text darkQueenCharacterName;
	public Text darkQueenCharacterText;
	string darkQueenCharacterNameTextId = "GALLERY_NAME_DARK_QUEEN";
	string darkQueenCharacterTextTextId  = "GALLERY_BIO_DARK_QUEEN";
	//Troll
	[Header("Troll")]
	public Text trollCharacterName;
	public Text trollCharacterText;
	string trollCharacterNameTextId = "GALLERY_NAME_TROLL";
	string trollCharacterTextTextId  = "GALLERY_BIO_TROLL";

	bool levelLoading = false;

	void Awake ()
	{
		//Reset
		levelLoading = false;

		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu

		//The activity indicator may have been started
		Handheld.StopActivityIndicator();

		//Title
		menuTitle.text = LocalizationManager.Instance.getText(menuTitleTextId);

		//Fairy
		//Character Name
		fairyCharacterName.text = LocalizationManager.Instance.getText(fairyCharacterNameTextId);

		//Character Bio
		string characterTextString = LocalizationManager.Instance.getText(fairyCharacterTextTextId);
		characterTextString = characterTextString.Replace("\\n", System.Environment.NewLine );
		fairyCharacterText.text = characterTextString;

		//Dark Queen
		//Character Name
		darkQueenCharacterName.text = LocalizationManager.Instance.getText(darkQueenCharacterNameTextId);
		
		//Character Bio
		characterTextString = LocalizationManager.Instance.getText(darkQueenCharacterTextTextId);
		characterTextString = characterTextString.Replace("\\n", System.Environment.NewLine );
		darkQueenCharacterText.text = characterTextString;
	
		//Troll
		//Character Name
		trollCharacterName.text = LocalizationManager.Instance.getText(trollCharacterNameTextId);
		
		//Character Bio
		characterTextString = LocalizationManager.Instance.getText(trollCharacterTextTextId);
		characterTextString = characterTextString.Replace("\\n", System.Environment.NewLine );
		trollCharacterText.text = characterTextString;
	
	}

	public void closeMenu()
	{
		StartCoroutine( close() );
	}

	IEnumerator close()
	{
		if( !levelLoading )
		{
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			Application.LoadLevel( 3 );
		}
	}
}
