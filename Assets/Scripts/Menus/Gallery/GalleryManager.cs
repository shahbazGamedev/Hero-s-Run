using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GalleryManager : MonoBehaviour {

	//The UI elements have text and need to be localized
	public Text menuTitle;
	public Text characterName;
	public Text characterText;
	public string menuTitleTextId = "GALLERY_TITLE";
	public string characterNameTextId = "GALLERY_NAME_FAIRY";
	public string characterTextTextId  = "GALLERY_BIO_FAIRY";

	void Awake ()
	{
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu

		//The activity indicator may have been started
		Handheld.StopActivityIndicator();

		//Title
		menuTitle.text = LocalizationManager.Instance.getText(menuTitleTextId);

		//Character Name
		characterName.text = LocalizationManager.Instance.getText(characterNameTextId);

		//Character Bio
		string characterTextString = LocalizationManager.Instance.getText(characterTextTextId);
		characterTextString = characterTextString.Replace("\\n", System.Environment.NewLine );
		characterText.text = characterTextString;
	}
}
