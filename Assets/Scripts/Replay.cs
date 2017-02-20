using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_IOS
using UnityEngine.iOS;
using UnityEngine.Apple.ReplayKit;

public class Replay : MonoBehaviour
{
	public Text recordingText;
	public Button toggleRecordingButton;
	public Button previewButton;

	void Awake()
	{
		if( !ReplayKit.APIAvailable ) Destroy( gameObject );
		toggleRecordingButton.gameObject.SetActive( PlayerStatsManager.Instance.getShowRecordButton() );
		recordingText.text = "Not Recording";
		previewButton.gameObject.SetActive( false );
		
	}
 
	public void toggleRecording()
	{
		try
		{
			if( ReplayKit.isRecording )
			{
				recordingText.text = "Not Recording";
				ReplayKit.StopRecording();
			}
			else
			{
				recordingText.text = "Recording ...";
				ReplayKit.StartRecording();
			}
		}
   		catch (Exception e)
		{
			Debug.LogError( "Replay exception: " +  e.ToString() + " ReplayKit.lastError: " + ReplayKit.lastError );
    	}
		Debug.Log("Replay: is broadcast API available " + ReplayKit.broadcastingAPIAvailable );
	}

	public void preview()
	{
		if (ReplayKit.recordingAvailable)
		{
			previewButton.gameObject.SetActive( false );
			ReplayKit.Preview();
		}
	}

	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
	}

	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
	}
	
	void GameStateChange( GameState previousState, GameState newState )
	{
		if( newState == GameState.PostLevelPopup )
		{
			ReplayKit.StopRecording();
			ReplayKit.Discard();
		}
		else if( newState == GameState.Paused )
		{
			toggleRecordingButton.gameObject.SetActive( false );
			previewButton.gameObject.SetActive( ReplayKit.recordingAvailable );
		}
		else if( newState == GameState.Normal )
		{
			toggleRecordingButton.gameObject.SetActive( PlayerStatsManager.Instance.getShowRecordButton() );
		}
		else
		{
			toggleRecordingButton.gameObject.SetActive( false );
		}
	}

}
#endif