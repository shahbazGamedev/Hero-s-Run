using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityDetails : MonoBehaviour {

	[Header("Ability Details")]
	[SerializeField] HeroCarousel heroCarousel;
	[SerializeField] Image abilityIcon;
	[SerializeField] Text abilityType;
	[SerializeField] Text abilityTitle;
	[SerializeField] Text abilityDescription;
	[SerializeField] Text exitButtonText;

	void Start()
	{
		exitButtonText.text = LocalizationManager.Instance.getText( "ABILITY_EXIT" );
	}

	public void configureActiveAbilityDetails()
	{
		HeroManager.HeroCharacter hero = HeroManager.Instance.getHeroCharacter( heroCarousel.currentIndex );
		HeroManager.HeroAbility ability = HeroManager.Instance.getHeroAbility( hero.activeAbilityEffect );
		configureAbilityDetails( ability );
	}

	public void configurePassiveAbilityDetails()
	{
		HeroManager.HeroCharacter hero = HeroManager.Instance.getHeroCharacter( heroCarousel.currentIndex );
		HeroManager.HeroAbility ability = HeroManager.Instance.getHeroAbility( hero.passiveAbilityEffect );
		configureAbilityDetails( ability );
	}

	void configureAbilityDetails( HeroManager.HeroAbility ability )
	{
		abilityIcon.sprite = ability.icon;
		abilityType.text = "<color=orange>" + LocalizationManager.Instance.getText( "ABILITY_TYPE_" + ability.type.ToString() ) + "</color>";
		abilityTitle.text = LocalizationManager.Instance.getText( "ABILITY_TITLE_" + ability.abilityEffect.ToString() );
		abilityDescription.text = LocalizationManager.Instance.getText( "ABILITY_DESC_" + ability.abilityEffect.ToString() );

	}

}
