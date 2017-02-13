using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CircuitSelectionManager : MonoBehaviour {

	[SerializeField] ScrollRect carouselScrollRect;
	bool levelLoading = false;
	public List<CarouselEntry> carouselEntryList = new List<CarouselEntry>(2);
	[SerializeField] Scrollbar scrollbar;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();
		GameManager.Instance.setMultiplayerMode( true );
		carouselScrollRect.horizontalNormalizedPosition = 0; //Make sure it is on the far left completely or the dot won't light up
	}

	public void OnClickShowStore()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		StoreManager.Instance.showStore( StoreTab.Store, StoreReason.None );
	}

	public void OnClickShowHeroSelection()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		LevelManager.Instance.setCurrentMultiplayerLevel( (int) scrollbar.value );
		CarouselEntry selected = carouselEntryList[LevelManager.Instance.getCurrentMultiplayerLevel() ];
		LevelManager.Instance.selectedRaceDetails = selected;
		StartCoroutine( loadHeroSelection() );
	}

	public void OnClickReturnToWorldMap()
	{
		StartCoroutine( loadWorldMap() );
	}

	IEnumerator loadHeroSelection()
	{
		if( !levelLoading )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)GameScenes.HeroSelection );
		}
	}

	IEnumerator loadWorldMap()
	{
		if( !levelLoading )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			levelLoading = true;
			GameManager.Instance.setMultiplayerMode( false );
			GameManager.Instance.setGameState(GameState.WorldMapNoPopup);
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)GameScenes.WorldMap );
		}
	}

}
