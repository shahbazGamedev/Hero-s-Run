using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VoiceLinesMenu : MonoBehaviour {

	[Header("General")]
	[SerializeField] Transform voiceLineHolder;
	[SerializeField] GameObject voiceLinePrefab;

	[Header("Top Section")]
	[SerializeField] Image heroIcon;
	[SerializeField] TextMeshProUGUI heroName;

	[Header("Entry")]
	[SerializeField] Sprite lockIcon;
	[SerializeField] Sprite equippedIcon;
	[SerializeField] TextMeshProUGUI voiceLineText;
	[SerializeField] AudioClip lockedSound;

	// Use this for initialization
	void Start ()
	{
		HeroManager.HeroCharacter hero = HeroManager.Instance.getHeroCharacter( GameManager.Instance.playerProfile.selectedHeroIndex );
		updateHero( hero );
		createVoiceLines( hero );
	}

	void OnEnable()
	{
		HeroCarousel.heroChangedEvent += HeroChangedEvent;
	}
	
	void OnDisable()
	{
		HeroCarousel.heroChangedEvent -= HeroChangedEvent;
	}

	void HeroChangedEvent( int selectedHeroIndex )
	{
		HeroManager.HeroCharacter hero = HeroManager.Instance.getHeroCharacter( selectedHeroIndex );
		updateHero( hero );
		createVoiceLines( hero );
	}

	void updateHero( HeroManager.HeroCharacter hero )
	{
		heroIcon.sprite = hero.icon;
		heroName.text = hero.name;
	}

	void createVoiceLines( HeroManager.HeroCharacter hero )
	{
		//Remove previous voice lines
		for( int i = voiceLineHolder.transform.childCount-1; i >= 1; i-- )
		{
			Transform child = voiceLineHolder.transform.GetChild( i );
			GameObject.Destroy( child.gameObject );
		}
		//Get all of the voice lines that the player has in his inventory
		List<PlayerVoiceLines.VoiceLineData> playerVoiceOverList = GameManager.Instance.playerVoiceLines.getVoiceLinesForHero ( hero.name ); 

		//Get all of the taunts that exist for that hero
		List<VoiceOverManager.VoiceOverData> allHeroTaunts = VoiceOverManager.Instance.getHeroTaunts ( hero.name );

		//Indicate number of voice lines found versus total number of voice lines
		int unlockedCount = GameManager.Instance.playerVoiceLines.getUnlockedCountForHero( hero.name );
		int total = allHeroTaunts.Count;

		voiceLineText.text = string.Format( LocalizationManager.Instance.getText( "VOICE_LINES" ), unlockedCount, total );

	
		for( int i = 0; i < playerVoiceOverList.Count; i++ )
		{
			createVoiceLine( i, hero.name, playerVoiceOverList[i] );
		}

		//Calculate the content length
		VerticalLayoutGroup vlg = voiceLineHolder.GetComponent<VerticalLayoutGroup>();
		int contentLength = ( playerVoiceOverList.Count + 1 )* ( (int)vlg.preferredHeight + (int)vlg.spacing ) + vlg.padding.top;
		voiceLineHolder.GetComponent<RectTransform>().sizeDelta = new Vector2( voiceLineHolder.GetComponent<RectTransform>().rect.width, contentLength );		
	}

	void createVoiceLine( int index, string heroName, PlayerVoiceLines.VoiceLineData vo )
	{
		GameObject go = (GameObject)Instantiate(voiceLinePrefab);
		go.transform.SetParent(voiceLineHolder,false);
		Button voiceLineButton = go.GetComponent<Button>();
		voiceLineButton.onClick.RemoveAllListeners();
		voiceLineButton.onClick.AddListener(() => OnClickVoiceLine( vo ));
		TextMeshProUGUI[] texts = voiceLineButton.GetComponentsInChildren<TextMeshProUGUI>();
		texts[0].text =  LocalizationManager.Instance.getText( "VO_TAUNT_" + heroName.ToString().ToUpper() + "_" + vo.uniqueId.ToString() );
		Image[] images = voiceLineButton.GetComponentsInChildren<Image>();
		//We have 3 possibilities:
		//The voice line is locked
		//The voice line is equipped (and therefore unlocked)
		//The voice line is not locked and not equipped
		if( vo.isLocked )
		{
			images[1].sprite = lockIcon;
		}
		else
		{
			if( vo.isEquipped )
			{
				images[1].sprite = equippedIcon;
			}
			else
			{
				images[1].gameObject.SetActive( false );
			}
		}
	}

	void OnClickVoiceLine( PlayerVoiceLines.VoiceLineData vo )
	{
		if( vo.isLocked )
		{
			GetComponent<AudioSource>().PlayOneShot( lockedSound );
		}
		else
		{
			if( vo.isEquipped )
			{
				AudioClip clip = VoiceOverManager.Instance.getTauntClip( vo.heroName, vo.uniqueId );
				GetComponent<AudioSource>().PlayOneShot( clip );
			}
			else
			{
				GameManager.Instance.playerVoiceLines.equipVoiceLine( vo.uniqueId, vo.heroName );
				GameManager.Instance.playerVoiceLines.serializeVoiceLines( true );
				AudioClip clip = VoiceOverManager.Instance.getTauntClip( vo.heroName, vo.uniqueId );
				GetComponent<AudioSource>().PlayOneShot( clip );

				HeroManager.HeroCharacter hero = HeroManager.Instance.getHeroCharacter( GameManager.Instance.playerProfile.selectedHeroIndex );
				createVoiceLines( hero );
			}
		}
	}


}
