using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Emotion {

	Happy = 1,
	Sad = 2,
	Surprised = 3,
	Disapointed = 4
}

public class SentryController : MonoBehaviour {

	[Header("Sentry Sound Effects")]
	[SerializeField] AudioSource audioSource;
	[SerializeField] List<SentrySoundData> sentrySoundList = new List<SentrySoundData>();
	private GameObject myOwner;
	private Transform myOwnerTransform;
	private PlayerControl myOwnerPlayerControl;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		findOwner( gameObject.GetPhotonView ().instantiationData );
	}

	void findOwner(object[] data) 
	{
		int viewIdOfOwner = (int) data[0];
		GameObject[] playersArray = GameObject.FindGameObjectsWithTag("Player");
		for( int i = 0; i < playersArray.Length; i ++ )
		{
			if( playersArray[i].GetPhotonView().viewID == viewIdOfOwner )
			{
				myOwner = playersArray[i];
				myOwnerTransform = myOwner.transform;
				myOwnerPlayerControl = myOwner.GetComponent<PlayerControl>();
				transform.SetParent( myOwnerTransform );
				playSoundEffect( Emotion.Happy );
				break;
			}
		}
		if( myOwner != null )
		{
			Debug.Log("SentryController-The owner of this sentry is: " + myOwner.name );
		}
		else
		{
			Debug.LogError("SentryController error: could not find the sentry owner with the Photon view id of " + viewIdOfOwner );
		}
	}

	public void playSoundEffect( Emotion emotion )
	{
		//Don't interrupt the current sound effect for another one.
		if( audioSource.isPlaying ) return;

		//Do we have one or more sound effects that match?
		List<SentrySoundData> availableSoundsList = sentrySoundList.FindAll(soundClip => ( soundClip.emotion == emotion ) );

		if( availableSoundsList.Count > 0 )
		{
			if( availableSoundsList.Count == 1 )
			{
				audioSource.PlayOneShot( availableSoundsList[0].clip );
			}
			else
			{
				//We have multiple entries that match. Let's play a random one.
				int random = Random.Range( 0, availableSoundsList.Count );
				audioSource.PlayOneShot(  availableSoundsList[random].clip );
			}
		}
	}

	/// <summary>
	/// Stops the audio source.
	/// </summary>
	public void stopAudioSource()
	{
		audioSource.Stop();
	}

	[System.Serializable]
	public class SentrySoundData
	{
		public Emotion emotion;
		public AudioClip clip;
	}

}
