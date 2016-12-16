using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDMultiplayer : MonoBehaviour {

	//Sound to play every second during countdown
	public AudioClip beep;
	public Text goText;

	//Event management used to notify players to start running
	public delegate void StartRunningEvent();
	public static event StartRunningEvent startRunningEvent;

	//Called by the server to initiate a 3-2-1 Go! countdown and begin the race
	public void startCountdown ()
 	{
		StartCoroutine( countdown() );
	}
	
	//Valid only in multiplayer
	IEnumerator countdown()
	{
		//For the 3,2,1 countdown
		goText.rectTransform.eulerAngles = new Vector3( 0,0,0 );
		goText.gameObject.SetActive( true );
		int countdown = 3;
		while (countdown > 0)
		{
			goText.text = countdown.ToString();
			UISoundManager.uiSoundManager.playAudioClip( beep );
			yield return new WaitForSecondsRealtime( 1.0f );
			countdown --;
			if( countdown == 0 )
			{
				//Tell the players to start running
				if(startRunningEvent != null) startRunningEvent();
				//Display a Go! message and hide after a few seconds
				goText.rectTransform.eulerAngles = new Vector3( 0,0,4 );
				goText.text = LocalizationManager.Instance.getText("GO");
				Invoke ("hideGoText", 1.5f );
			}
		}
	}

	void hideGoText()
	{
		goText.gameObject.SetActive( false );
	}
}
