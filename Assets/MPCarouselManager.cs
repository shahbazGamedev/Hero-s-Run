using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MPCarouselManager : MonoBehaviour {

	//Variables for swipe
	bool touchStarted = false;
	Vector2 touchStartPos;
	float MINIMUM_HORIZONTAL_DISTANCE = 0.09f * Screen.width; //How many pixels you need to swipe horizontally to change page.
	public Scrollbar scrollbarIndicator;

	const int INDEX_OF_LAST_CIRCUIT = 1;
	int indexOfDisplayedCircuit = 0; 			//0 is the Royal Run, 1 is the Practice Run
	int previousIndexOfDisplayedCircuit = 0; 

	StoreManager storeManager;
	LevelData levelData;
	bool levelLoading = false;

	void Awake ()
	{
		SceneManager.LoadScene( (int)GameScenes.Store, LoadSceneMode.Additive );

		//Get the level data. Level data has the parameters for all the levels of the game.
		levelData = LevelManager.Instance.getLevelData();
	}

	// Use this for initialization
	void Start ()
	{
		GameObject storeManagerObject = GameObject.FindGameObjectWithTag("Store");
		storeManager = storeManagerObject.GetComponent<StoreManager>();

		Handheld.StopActivityIndicator();
	}

	void OnValueChanged( int newIndex )
	{
		if( newIndex == previousIndexOfDisplayedCircuit ) return; //Nothing has changed. Ignore.
		previousIndexOfDisplayedCircuit = newIndex;
		//Update the scrollbar indicator which is not interactable
		scrollbarIndicator.value = (float)newIndex/INDEX_OF_LAST_CIRCUIT;
	}

	public void OnClickShowShop()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		storeManager.showStore( StoreTab.Shop, StoreReason.None );
	}

	public void OnClickReturnToWorldMap()
	{
		StartCoroutine( close() );
	}

	IEnumerator close()
	{
		if( !levelLoading )
		{
			Debug.Log("MPCarouselManager - returning to world map.");
			UISoundManager.uiSoundManager.playButtonClick();
			levelLoading = true;
			GameManager.Instance.setGameState(GameState.WorldMapNoPopup);
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)GameScenes.WorldMap );
		}
	}


}
