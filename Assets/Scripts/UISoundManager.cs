using UnityEngine;
using System.Collections;

public class UISoundManager : MonoBehaviour {

	public static UISoundManager uiSoundManager = null;

	// Audio - GUI
	AudioSource uiAudioSource;
	[SerializeField] AudioClip buttonClick;

	void Awake ()
	{
		uiSoundManager = this;
		DontDestroyOnLoad( gameObject );

		uiAudioSource = GetComponent<AudioSource>();

		//Used to play UI sounds for the HUD and all menus
		uiAudioSource.ignoreListenerPause = true;

	}

	public void playButtonClick()
	{
		//uiAudioSource.PlayOneShot( buttonClick );
	}

	public void playAudioClip( AudioClip clip )
	{
		if( clip != null ) uiAudioSource.PlayOneShot( clip );
	}
}
