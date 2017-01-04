using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JournalSceneManager : MonoBehaviour {

	bool levelLoading = false;

	// Use this for initialization
	void Start () {
		
	}
	
	public void OnClickCloseMenu()
	{
		StartCoroutine( close() );
	}

	IEnumerator close()
	{
		if( !levelLoading )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			GameManager.Instance.setGameState(GameState.WorldMapNoPopup);
			SceneManager.LoadScene( (int)GameScenes.WorldMap );
		}
	}
}
