using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroCarousel : MonoBehaviour {

	[Header("General")]
	public int currentIndex = 0; //corresponds to center icon
	Color fadedDescriptionTextColor;
	[Header("3 carousel images")]
	[SerializeField] Image leftIcon;
	[SerializeField] Image centerIcon;
	[SerializeField] Image rightIcon;
	[Header("Left Top Corner")]
	[SerializeField] Image heroIcon;
	[SerializeField] Text heroName;
	[Header("Active Ability")]
	[SerializeField] Image activeAbilityIcon;
	[SerializeField] Text activeAbilityTitle;
	[SerializeField] Text activeAbilityDescription;
	[Header("Passive Ability")]
	[SerializeField] Image passiveAbilityIcon;
	[SerializeField] Text passiveAbilityTitle;
	[SerializeField] Text passiveAbilityDescription;
	[Header("Hero Skin")]
	[SerializeField] List<GameObject> heroSkinList = new List<GameObject>();
	GameObject previousSkin = null;
	int maxHeroIndex = 0;

	// Use this for initialization
	void Awake () {
		
		maxHeroIndex = HeroManager.Instance.getNumberOfHeroes() - 1;
		fadedDescriptionTextColor = new Color( activeAbilityDescription.color.r, activeAbilityDescription.color.g, activeAbilityDescription.color.b, 0 );
		configureHeroDetails();
	}

	public void previousItem()
	{
		currentIndex--;
		if( currentIndex < 0 ) currentIndex = maxHeroIndex;
		configureHeroDetails();
	}

	public void nextItem()
	{
		currentIndex++;
		if( currentIndex > maxHeroIndex )
		{
			currentIndex = 0;
		}
		configureHeroDetails();
	}

	void updateCarouselImages()
	{
		int initialCurrentIndex = currentIndex;

		//Center Icon
		HeroManager.HeroCharacter hero = HeroManager.Instance.getHeroCharacter( initialCurrentIndex );
		centerIcon.sprite = hero.icon;

		//Right Icon
		initialCurrentIndex = currentIndex + 1;
		if( initialCurrentIndex > maxHeroIndex ) initialCurrentIndex = 0;
		hero = HeroManager.Instance.getHeroCharacter( initialCurrentIndex );
		rightIcon.sprite = hero.icon;

		//Left Icon
		initialCurrentIndex = currentIndex - 1;
		if( initialCurrentIndex < 0 ) initialCurrentIndex = maxHeroIndex;
		hero = HeroManager.Instance.getHeroCharacter( initialCurrentIndex );
		leftIcon.sprite = hero.icon;
	}

	void configureHeroDetails()
	{
		CancelInvoke("hideActiveAbilityDescription");
		CancelInvoke("hidePassiveAbilityDescription");
		activeAbilityDescription.color = fadedDescriptionTextColor;
		passiveAbilityDescription.color = fadedDescriptionTextColor;

		HeroManager.HeroCharacter hero = HeroManager.Instance.getHeroCharacter( currentIndex );
		heroIcon.sprite = hero.icon;
		heroName.text = hero.name;
		//configure skin
		configureSkin( heroSkinList[hero.skinIndex] );
		//configure abilities
		//Active
		HeroManager.HeroAbility activeAbility = HeroManager.Instance.getHeroAbility( hero.activeAbilityEffect );
		activeAbilityIcon.sprite = activeAbility.icon;
		activeAbilityTitle.text = LocalizationManager.Instance.getText( "ABILITY_TITLE_" + activeAbility.abilityEffect.ToString() );
		activeAbilityDescription.text = LocalizationManager.Instance.getText( "ABILITY_DESC_" + activeAbility.abilityEffect.ToString() );
		//Passive
		HeroManager.HeroAbility passiveAbility = HeroManager.Instance.getHeroAbility( hero.passiveAbilityEffect );
		passiveAbilityIcon.sprite = passiveAbility.icon;
		passiveAbilityTitle.text  = LocalizationManager.Instance.getText( "ABILITY_TITLE_" + passiveAbility.abilityEffect.ToString() );
		passiveAbilityDescription.text = LocalizationManager.Instance.getText( "ABILITY_DESC_" + passiveAbility.abilityEffect.ToString() );
		updateCarouselImages();

	}

	void configureSkin( GameObject selectedSkin )
	{
		if( previousSkin != null ) previousSkin.SetActive( false );
		selectedSkin.SetActive( true );
		previousSkin = selectedSkin;
	}

	public void OnClickShowActiveAbilityDescription()
	{
		LeanTween.cancel( gameObject );
		CancelInvoke("hideActiveAbilityDescription");
		UISoundManager.uiSoundManager.playButtonClick();
		LeanTween.alphaText( activeAbilityDescription.GetComponent<RectTransform>(), 1f, 0.4f );
		Invoke( "hideActiveAbilityDescription", 4f );
	}

	public void OnClickShowPassiveAbilityDescription()
	{
		LeanTween.cancel( gameObject );
		CancelInvoke("hidePassiveAbilityDescription");
		UISoundManager.uiSoundManager.playButtonClick();
		LeanTween.alphaText( passiveAbilityDescription.GetComponent<RectTransform>(), 1f, 0.4f );
		Invoke( "hidePassiveAbilityDescription", 4f );
	}

	void hideActiveAbilityDescription()
	{
		LeanTween.alphaText( activeAbilityDescription.GetComponent<RectTransform>(), 0f, 0.4f );
	}

	void hidePassiveAbilityDescription()
	{
		LeanTween.alphaText( passiveAbilityDescription.GetComponent<RectTransform>(), 0f, 0.4f );
	}

}
