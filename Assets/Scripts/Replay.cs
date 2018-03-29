using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_IOS
using UnityEngine.Apple.ReplayKit;

public class Replay : MonoBehaviour
{

	[SerializeField] Toggle recordingToggle;
	[SerializeField] Button previewButton;
 	[SerializeField] Button startBroadcastButton;
 	[SerializeField] TextMeshProUGUI broadcastStartStatus;
	private bool broadcastCallbackReceived = false;
	private bool broadcastStartSuccess = false;
	private string broadcastStartError = string.Empty;

	void Start()
	{
		if (ReplayKit.APIAvailable)
		{
			recordingToggle.isOn = LevelManager.Instance.isRecordingSelected;
			recordingToggle.gameObject.SetActive( true );
			previewButton.gameObject.SetActive( true );
			startBroadcastButton.gameObject.SetActive( ReplayKit.broadcastingAPIAvailable );
			broadcastStartStatus.text = string.Empty;
			broadcastStartStatus.gameObject.SetActive( ReplayKit.broadcastingAPIAvailable );
		}
		else
		{
			recordingToggle.gameObject.SetActive( false );
			previewButton.gameObject.SetActive( false );
			startBroadcastButton.gameObject.SetActive( false );
			broadcastStartStatus.gameObject.SetActive( false );
		}
	}

	public void OnToggle()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		LevelManager.Instance.isRecordingSelected = recordingToggle.isOn;
	}

	public void OnClickPreview()
	{
		//IMPORTANT:
		//1-It seems that ReplayKit.recordingAvailable has a value of false EVEN if a video is available.
		//2-Known Unity issue: after previewing the recording, the iOS status bar appears.
		print( "OnClickPreview ReplayKit.recordingAvailable " + ReplayKit.recordingAvailable );
		UISoundManager.uiSoundManager.playButtonClick();
		ReplayKit.Preview();
	}

	public void OnClickStartBroadcast()
	{
        ReplayKit.StartBroadcasting((bool success, string error) =>
		{
			//We will get the callback after a delay. The callback is called from OUTSIDE the main Unity thread. That's why we use the Update method to update the status text.
			broadcastCallbackReceived = true;
			broadcastStartSuccess = success;
			broadcastStartError = error;
		});

		UISoundManager.uiSoundManager.playButtonClick();
	}

	void Update()
	{
		if ( broadcastCallbackReceived && broadcastStartStatus.text == string.Empty )
		{
			if (broadcastStartSuccess)
			{
				broadcastStartStatus.text = "Broadcast successfully started.";
			}
			else
			{
				broadcastStartStatus.text = "Broadcast couldn't be started. Error: " + broadcastStartError;
			}
		}
	}

}
#endif