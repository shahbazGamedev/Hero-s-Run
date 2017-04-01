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
			sex = HeroManager.Instance.getBotHeroCharacter( LevelManager.Instance.selectedBotHeroIndex ).sex;
		}
		voiceOverList = VoiceOverManager.Instance.getVoiceOverList( sex );
		
	}

	public void playVoiceOver( VoiceOverType voiceOverType, CardName card = CardName.None )
	{
		//Don't interrupt the current VO for another one.
		if( voiceOverAudioSource.isPlaying ) return;

		//Do we have a VO that matches?
		VoiceOverManager.VoiceOverData vod = voiceOverList.Find(vo => ( vo.type == voiceOverType && vo.cardName == card ) );
		if( vod != null ) voiceOverAudioSource.PlayOneShot( vod.clip );

	}
	
}
