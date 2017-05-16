using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CarouselEntry : MonoBehaviour {

	[Header("Race Circuit Parameters")]
	[SerializeField] string raceTrackName;
	public Text circuitName;
	public Image circuitImage;

	[SerializeField] Text raceButtonText;

	bool levelLoading = false;
	
	// Use this for initialization
	void Awake ()
	{
		//Configuration
		LevelData levelData = LevelManager.Instance.getLevelData();
		LevelData.CircuitInfo circuitInfo = levelData.getRaceTrackByName( raceTrackName ).circuitInfo;

		//Circuit
		circuitName.text = LocalizationManager.Instance.getText( circuitInfo.circuitTextID );
		circuitImage.sprite = circuitInfo.circuitImage;

		//Common to all carousel entries
		raceButtonText.text = LocalizationManager.Instance.getText( "MENU_CONFIRM" );
	}

	public void OnClickShowMatchmaking()
	{
		LevelData.MultiplayerInfo selectedCircuit = LevelManager.Instance.getLevelData().getRaceTrackByName( raceTrackName ); 
		LevelManager.Instance.setSelectedCircuit( selectedCircuit );
		StartCoroutine( loadScene(GameScenes.Matchmaking) );
	}

	IEnumerator loadScene(GameScenes value)
	{
		if( !levelLoading )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)value );
		}
	}

}
