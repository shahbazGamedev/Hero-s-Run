using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ComicBookManager : MonoBehaviour {

	public Canvas episodeScreen;
	public Button tapToContinue;
	public Text tapToContinueText;
	Animator anim;
	int currentComicBookStrip = 0;
	public int levelToLaunchIndex = 0;
	public List<Image> stripList = new List<Image>(5);
	bool levelLoading = false;

	// Use this for initialization
	void Awake () {
	
		Screen.autorotateToPortrait = true;
		Screen.autorotateToPortraitUpsideDown = false;
		Screen.autorotateToLandscapeLeft = true;
		Screen.autorotateToLandscapeRight = true;		
		Screen.orientation = ScreenOrientation.AutoRotation;

		Invoke ("displayFirstStrip", 0.5f );
		anim = episodeScreen.gameObject.GetComponent<Animator>();
		tapToContinueText.text = LocalizationManager.Instance.getText("TAP_TO_CONTINUE");
		resetComicStrips();
	}

	void displayFirstStrip()
	{

		//The initial strip is all black. We are now ready to display the first strip with art.
		displayNextComicStrip();
		tapToContinue.gameObject.SetActive( true );
		//anim.SetTrigger("FadeOut");
	}
	
	public void displayNextComicStrip()
	{
		currentComicBookStrip++;
		print ( "displayNextComicStrip " + currentComicBookStrip + " " + stripList.Count );
		if( currentComicBookStrip < stripList.Count )
		{
			for( int i = 0; i <  stripList.Count; i++ )
			{
				if( i == currentComicBookStrip )
				{
					//Activate
					stripList[i].gameObject.SetActive( true );
				}
				else
				{
					//Deactivate
					stripList[i].gameObject.SetActive( false );
				}
			}
		}
		if( currentComicBookStrip == ( stripList.Count - 1 ) )
		{
			tapToContinue.gameObject.SetActive( false );
			//Revert to protrait mode BEFORE loading level
			Screen.orientation = ScreenOrientation.Portrait;

			//Open level
			StartCoroutine( loadLevel() );
		}
	}

	public void resetComicStrips()
	{
		currentComicBookStrip = 0;
		tapToContinue.gameObject.SetActive( false );
		for( int i = 0; i <  stripList.Count; i++ )
		{
			if( i == currentComicBookStrip )
			{
				//Activate
				stripList[i].gameObject.SetActive( true );
			}
			else
			{
				//Deactivate
				stripList[i].gameObject.SetActive( false );
			}
		}
	}

	IEnumerator loadLevel()
	{
		if( !levelLoading )
		{
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			LevelManager.Instance.forceNextLevelToComplete( levelToLaunchIndex );
			Application.LoadLevel( 4 );
		}
		
	}

}
