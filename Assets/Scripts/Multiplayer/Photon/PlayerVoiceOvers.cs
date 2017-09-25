using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrazyMinnow.SALSA;


public class PlayerVoiceOvers : MonoBehaviour {

	[Header("Voice Overs")]
	[SerializeField] AudioSource voiceOverAudioSource;
	public Salsa3D headSalsa3D = null;
	List<VoiceOverManager.VoiceOverData> voiceOverList = new List<VoiceOverManager.VoiceOverData>();

	// Use this for initialization
	public void initializeVoiceOvers ( HeroName heroName )
	{
		voiceOverList = VoiceOverManager.Instance.getHeroVoiceOverList( heroName );	
	}

	// Called by OnSkinCreated so that PlayerVoiceOver can have access to the component in charge of lip-sync.
	public void setLipSyncComponent ( Salsa3D headSalsa3D )
	{
		this.headSalsa3D = headSalsa3D;	
	}

	[PunRPC]
	void activateCardVoiceOverRPC( int name )
    {
		playVoiceOver( VoiceOverType.VO_Casting_Spell, (CardName) name );
	}

	public bool isSpellVoiceOverAvailable( CardName card, bool playOnActivationOnly = true )
	{
		//Do we have one or more VOs that match?
		List<VoiceOverManager.VoiceOverData> vodList = voiceOverList.FindAll(vo => vo.type == VoiceOverType.VO_Casting_Spell && vo.cardName == card && vo.playOnActivationOnly == playOnActivationOnly );

		return vodList.Count > 0;
	}

	public void playVoiceOver( VoiceOverType voiceOverType, CardName card = CardName.None )
	{
		//Don't interrupt the current VO for another one.
		if( voiceOverAudioSource.isPlaying ) return;

		//Do we have one or more VOs that match?
		List<VoiceOverManager.VoiceOverData> vodList = voiceOverList.FindAll(vo => ( vo.type == voiceOverType && vo.cardName == card ) );

		if( vodList.Count > 0 )
		{
			if( vodList.Count == 1 )
			{
				playVO( vodList[0].clip );
			}
			else
			{
				//We have multiple entries that match. Let's play a random one.
				int random = Random.Range( 0, vodList.Count );
				playVO(  vodList[random].clip );
			}
		}
	}

	public float getPitch() 
	{
		return voiceOverAudioSource.pitch;	
	}

	public void setPitch( float pitch ) 
	{
		voiceOverAudioSource.pitch = pitch;	
	}

	public void resetPitch() 
	{
		voiceOverAudioSource.pitch = 1f;
	}

	/// <summary>
	/// Plays the taunt clip locally and sends an RPC so that the remote players play it as well.
	/// </summary>
	/// <param name="clip">Clip.</param>
	/// <param name="sex">Sex.</param>
	/// <param name="voiceLineId">Voice line identifier.</param>
	public void playTaunt ( AudioClip clip, int voiceLineId )
	{
		playVO( clip );
		GetComponent<PhotonView>().RPC("playTauntRPC", PhotonTargets.Others, voiceLineId );
	}

	[PunRPC]
	void playTauntRPC( int uniqueId )
    {
		//Find the clip to play
		VoiceOverManager.VoiceOverData equippedVoiceLine = VoiceOverManager.Instance.getAllTaunts ().Find(vo => vo.uniqueId == uniqueId );
		if( equippedVoiceLine != null )
		{
			playVO( equippedVoiceLine.clip );
		}
	}

	void playVO( AudioClip clip )
	{
		if( headSalsa3D != null )
		{
			headSalsa3D.SetAudioClip(clip);
			headSalsa3D.Play();
		}
		else
		{
			voiceOverAudioSource.PlayOneShot( clip );
		}

	}

	/// <summary>
	/// Stops the audio source.
	/// If the player dies, we don't want him to continue talking.
	/// </summary>
	public void stopAudioSource()
	{
		if( headSalsa3D != null ) headSalsa3D.Stop();
		voiceOverAudioSource.Stop();
	}
	
}
