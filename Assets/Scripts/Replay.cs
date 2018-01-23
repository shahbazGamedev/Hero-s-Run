using UnityEngine;
using UnityEngine.UI;
#if UNITY_IOS
using UnityEngine.Apple.ReplayKit;

public class Replay : MonoBehaviour
{

	[SerializeField] Toggle recordingToggle;
	[SerializeField] Button previewButton;
 
	void Start()
	{
		if (ReplayKit.APIAvailable)
		{
			recordingToggle.isOn = LevelManager.Instance.isRecordingSelected;
			recordingToggle.gameObject.SetActive( true );
			previewButton.gameObject.SetActive( true );
		}
		else
		{
			recordingToggle.gameObject.SetActive( false );
			previewButton.gameObject.SetActive( false );
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
}
#endif