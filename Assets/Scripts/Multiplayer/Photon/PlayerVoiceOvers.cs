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
		Sex sex;
		if( GetComponent<PlayerAI>() == null )
		{
			//We're the player
			sex = HeroManager.Instance.getHeroCharacter( GameManager.Instance.playerProfile.selectedHeroIndex ).sex;
		}
		else
		{
			//We're a bot
			sex = GetComponent<PlayerAI>().botHero.sex;
		}
		voiceOverList = VoiceOverManager.Instance.getVoiceOverList( sex );
		
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

	public void playTaunt ()
	{
		//Do we have one or more VOs that match?
		List<VoiceOverManager.VoiceOverData> vodList = voiceOverList.FindAll(vo => ( vo.type == VoiceOverType.VO_Taunt ) );

		if( vodList.Count > 0 )
		{
			if( vodList.Count == 1 )
			{
				voiceOverAudioSource.PlayOneShot( vodList[0].clip );
				int voIndex = voiceOverList.IndexOf(vodList[0]);
				GetComponent<PhotonView>().RPC("playTauntRPC", PhotonTargets.Others, voIndex );
			}
			else
			{
				//We have multiple entries that match. Let's play a random one.
				int random = Random.Range( 0, vodList.Count );
				voiceOverAudioSource.PlayOneShot(  vodList[random].clip );
				int voIndex = voiceOverList.IndexOf(vodList[random]);
				GetComponent<PhotonView>().RPC("playTauntRPC", PhotonTargets.Others, voIndex );
			}
		}
	}

	[PunRPC]
	void playTauntRPC( int index )
    {
		voiceOverAudioSource.PlayOneShot( voiceOverList[index].clip );
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
