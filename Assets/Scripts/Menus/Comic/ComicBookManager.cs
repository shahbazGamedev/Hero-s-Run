using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ComicBookManager : MonoBehaviour {

	public Canvas episodeScreen;
	public Button tapToContinue;
	Animator anim;
	int currentComicBookStrip = 0;
	public int levelToLaunchIndex = 0;
	public List<Image> stripList = new List<Image>(5);
	bool levelLoading = false;

	// Use this for initialization
	void Awake () {
	
		Screen.orientation = ScreenOrientation.LandscapeLeft;
		Invoke ("enableAutoRotation", 0.5f );
		anim = episodeScreen.gameObject.GetComponent<Animator>();
	}

	void enableAutoRotation()
	{
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
		Screen.autorotateToLandscapeLeft = true;
		Screen.autorotateToLandscapeRight = true;
		
		Screen.orientation = ScreenOrientation.AutoRotation;
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
