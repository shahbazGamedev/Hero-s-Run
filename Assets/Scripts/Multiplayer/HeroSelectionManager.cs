using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HeroSelectionManager : MonoBehaviour {

	[Header("Hero Selection Manager")]
	const float VOICE_LINES_HORIZONTAL_POSITION = -852;
	[SerializeField] RectTransform horizontalContent;
	bool levelLoading = false;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();
	}

	public void OnClickReturnToMainMenu()
	{
		//Save the selection
		GameManager.Instance.playerProfile.serializePlayerprofile();
		StartCoroutine( loadScene(GameScenes.MainMenu) );
	}

	public void OnClickScrollToVoiceLines()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		StartCoroutine( scrollToVoiceLines( 0.4f ) );
	}

	IEnumerator scrollToVoiceLines( float duration )
	{
		float elapsedTime = 0;
		
		Vector2 startHorizontalPosition = horizontalContent.anchoredPosition;
		Vector2 endHorizontalPosition = new Vector2( VOICE_LINES_HORIZONTAL_POSITION, horizontalContent.anchoredPosition.y );

		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			horizontalContent.anchoredPosition = Vector2.Lerp( startHorizontalPosition, endHorizontalPosition, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		horizontalContent.anchoredPosition = new Vector2( VOICE_LINES_HORIZONTAL_POSITION, horizontalContent.anchoredPosition.y );
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
