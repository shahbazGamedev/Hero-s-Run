using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroCarousel : MonoBehaviour {

	[Header("General")]
	ScrollRect scrollRect;
	public int currentIndex = 0;
	const float STEP_VALUE = 1f/3f;
	[Header("Left Top Corner")]
	[SerializeField] Image heroIcon;
	[SerializeField] Text heroName;
	[Header("Active Ability")]
	[SerializeField] Image activeAbilityIcon;
	[SerializeField] Text activeAbilityTitle;
	[Header("Passive Ability")]
	[SerializeField] Image passiveAbilityIcon;
	[SerializeField] Text passiveAbilityTitle;

	// Use this for initialization
	void Awake () {
		
		scrollRect = GetComponent<ScrollRect>();
		configureHeroDetails();
	}

	public void previousItem()
	{
		currentIndex--;
		if( currentIndex < 0 ) currentIndex = 3;
		scrollRect.horizontalNormalizedPosition = currentIndex * STEP_VALUE;
		configureHeroDetails();
	}

	public void nextItem()
	{
		currentIndex++;
		if( currentIndex > 3 )
		{
			currentIndex = 0;
		}
		scrollRect.horizontalNormalizedPosition = currentIndex * STEP_VALUE;
		configureHeroDetails();
	}

	void configureHeroDetails()
	{
		HeroManager.HeroCharacter hero = HeroManager.Instance.getHeroCharacter( currentIndex );
		heroIcon.sprite = hero.icon;
		heroName.text = hero.name;
		//configure abilities
		HeroManager.HeroAbility activeAbility = HeroManager.Instance.getHeroAbility( hero.activeAbilityEffect );
		activeAbilityIcon.sprite = activeAbility.icon;
		activeAbilityTitle.text = LocalizationManager.Instance.getText( "ABILITY_TITLE_" + activeAbility.abilityEffect.ToString() );
		HeroManager.HeroAbility passiveAbility = HeroManager.Instance.getHeroAbility( hero.passiveAbilityEffect );
		passiveAbilityIcon.sprite = passiveAbility.icon;;
		passiveAbilityTitle.text  = LocalizationManager.Instance.getText( "ABILITY_TITLE_" + passiveAbility.abilityEffect.ToString() );

	}

	public void normalizedPosition ( Vector2 value )
	{
		//print("normalizedPosition " + value.x );
	}
	
}
