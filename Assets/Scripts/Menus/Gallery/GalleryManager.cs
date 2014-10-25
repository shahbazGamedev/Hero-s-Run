using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GalleryManager : MonoBehaviour {

	[Header("Frame")]
	public Text menuTitle;
	string menuTitleTextId = "GALLERY_TITLE";
	public Text characterName;
	public Text characterBio;

	//Fairy
	[Header("Fairy")]
	string fairyNameTextId = "GALLERY_NAME_FAIRY";
	string fairyBioTextId  = "GALLERY_BIO_FAIRY";
	public GameObject fairy3DGroup;

	//Dark Queen
	[Header("Dark Queen")]
	string darkQueenNameTextId = "GALLERY_NAME_DARK_QUEEN";
	string darkQueenBioTextId  = "GALLERY_BIO_DARK_QUEEN";
	public GameObject darkQueen3DGroup;

	//Troll
	[Header("Troll")]
	string trollNameTextId = "GALLERY_NAME_TROLL";
	string trollBioTextId  = "GALLERY_BIO_TROLL";
	public GameObject troll3DGroup;

	bool levelLoading = false;
	public ScrollRect characterBioScrollRect;

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
		characterName.text = LocalizationManager.Instance.getText(fairyNameTextId);

		//Character Bio
		string characterTextString = LocalizationManager.Instance.getText(fairyBioTextId);
		characterTextString = characterTextString.Replace("\\n", System.Environment.NewLine );
		characterBio.text = characterTextString;
	
	}

	public void OnValueChanged( float scrollBarPosition )
	{
		print ("Gallery Manager " + scrollBarPosition );

		//Reset the scroll rectangle with the character bio text to the top
		characterBioScrollRect.verticalNormalizedPosition = 1f;

		//Fairy
		if( scrollBarPosition == 0 )
		{
			//Character Name
			characterName.text = LocalizationManager.Instance.getText(fairyNameTextId);
			
			//Character Bio
			string characterTextString = LocalizationManager.Instance.getText(fairyBioTextId);
			characterTextString = characterTextString.Replace("\\n", System.Environment.NewLine );
			characterBio.text = characterTextString;

			//3D
			fairy3DGroup.SetActive( true );
			darkQueen3DGroup.SetActive( false );
			troll3DGroup.SetActive( false );

		}
		//Dark Queen
		else if(  scrollBarPosition == 0.5f )
		{
			//Character Name
			characterName.text = LocalizationManager.Instance.getText(darkQueenNameTextId);
			
			//Character Bio
			string characterTextString = LocalizationManager.Instance.getText(darkQueenBioTextId);
			characterTextString = characterTextString.Replace("\\n", System.Environment.NewLine );
			characterBio.text = characterTextString;
			
			//3D
			fairy3DGroup.SetActive( false );
			darkQueen3DGroup.SetActive( true );
			troll3DGroup.SetActive( false );
		}
		//Troll
		else
		{
			//Character Name
			characterName.text = LocalizationManager.Instance.getText(trollNameTextId);
			
			//Character Bio
			string characterTextString = LocalizationManager.Instance.getText(trollBioTextId);
			characterTextString = characterTextString.Replace("\\n", System.Environment.NewLine );
			characterBio.text = characterTextString;
			
			//3D
			fairy3DGroup.SetActive( false );
			darkQueen3DGroup.SetActive( false );
			troll3DGroup.SetActive( true );
		}
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
