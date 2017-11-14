using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class HeroSelectionManager : MonoBehaviour, IPointerDownHandler {

	[Header("Hero Selection Manager")]
	[SerializeField] GameObject voiceLinesPanel;
	bool levelLoading = false;

	// Use this for initialization
	void Start ()
	{
		voiceLinesPanel.SetActive( false );
		Handheld.StopActivityIndicator();
	}

	public void OnClickReturnToMainMenu()
	{
		//Save the selection
		GameManager.Instance.playerProfile.serializePlayerprofile();
		StartCoroutine( loadScene(GameScenes.MainMenu) );
	}

	public void OnClickDisplayVoiceLines()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		voiceLinesPanel.SetActive( !voiceLinesPanel.activeSelf );
	}

	public void OnPointerDown(PointerEventData eventData )
	{
		//If the player clicks on the voice line panel outside of the button area, dismiss the panel.
		if( voiceLinesPanel.activeSelf ) voiceLinesPanel.SetActive( false );
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
