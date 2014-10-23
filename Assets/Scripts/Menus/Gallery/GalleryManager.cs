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
		LocalizationManager.Instance.initialize();
		//The activity indicator may have been started
		Handheld.StopActivityIndicator();
		menuTitle.text = LocalizationManager.Instance.getText(menuTitleTextId);
		characterName.text = LocalizationManager.Instance.getText(characterNameTextId);
		characterText.text = LocalizationManager.Instance.getText(characterTextTextId);
	}
}
