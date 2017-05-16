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

	[Header("Online Players")]
	[SerializeField] Text numberOnlinePlayers;

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

	void Start ()
	{
		InvokeRepeating("getNumberOfOnlinePlayers", 0f, 5f );
	}
	
	void getNumberOfOnlinePlayers()
	{
		//The player needs to be connected to the Internet, connected to Photon and not be in offline mode.
		//Note: if PhotonNetwork.offlineMode is true, PhotonNetwork.connected will also be true, hence the extra test.
		if( Application.internetReachability != NetworkReachability.NotReachable && PhotonNetwork.connected && !PhotonNetwork.offlineMode )
		{
			//The count of players currently using this application (available on MasterServer in 5sec intervals).
			//This is the total for ALL tracks.
			numberOnlinePlayers.text = PhotonNetwork.countOfPlayers.ToString();

		}
		else
		{
			numberOnlinePlayers.text = LocalizationManager.Instance.getText( "CIRCUIT_NOT_AVAILABLE" );
		}
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
