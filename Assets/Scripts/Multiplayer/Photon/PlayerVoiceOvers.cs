using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerVoiceOvers : MonoBehaviour {

	[Header("Voice Overs")]
	[SerializeField] AudioSource voiceOverAudioSource;
	List<VoiceOverManager.VoiceOverData> voiceOverList = new List<VoiceOverManager.VoiceOverData>();

	// Use this for initialization
	void Start ()
	{
		string heroName;
		if( GetComponent<PlayerAI>() == null )
		{
			//We're the player
			heroName = HeroManager.Instance.getHeroCharacter( GameManager.Instance.playerProfile.selectedHeroIndex ).name;
		}
		else
		{
			//We're a bot
			heroName = GetComponent<PlayerAI>().botHero.name;
		}
		voiceOverList = VoiceOverManager.Instance.getHeroVoiceOverList( heroName );
		
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
				voiceOverAudioSource.PlayOneShot( vodList[0].clip );
			}
			else
			{
				//We have multiple entries that match. Let's play a random one.
				int random = Random.Range( 0, vodList.Count );
				voiceOverAudioSource.PlayOneShot(  vodList[random].clip );
			}
		}
	}

	/// <summary>
	/// Plays the taunt clip locally and sends an RPC so that the remote players play it as well.
	/// </summary>
	/// <param name="clip">Clip.</param>
	/// <param name="sex">Sex.</param>
	/// <param name="voiceLineId">Voice line identifier.</param>
	public void playTaunt ( AudioClip clip, int voiceLineId )
	{
		voiceOverAudioSource.PlayOneShot( clip );
		GetComponent<PhotonView>().RPC("playTauntRPC", PhotonTargets.Others, voiceLineId );
	}

	[PunRPC]
	void playTauntRPC( int uniqueId )
    {
		//Find the clip to play
		VoiceOverManager.VoiceOverData equippedVoiceLine = VoiceOverManager.Instance.getAllTaunts ().Find(vo => vo.uniqueId == uniqueId );
		if( equippedVoiceLine != null )
		{
			voiceOverAudioSource.PlayOneShot( equippedVoiceLine.clip );
		}
	}

	/// <summary>
	/// Stops the audio source.
	/// If the player dies, we don't want him to continue talking.
	/// </summary>
	public void stopAudioSource()
	{
		voiceOverAudioSource.Stop();
	}
	
}
