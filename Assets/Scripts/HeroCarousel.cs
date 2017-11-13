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
	[Header("Hero Skin")]
	[SerializeField] List<GameObject> heroSkinList = new List<GameObject>();
	GameObject previousSkin = null;
	int maxHeroIndex = 0;
	[Header("Card reserved For Hero")]
	[SerializeField] HeroCardHandler heroCardhandler;

	//Delegate used to communicate to other classes when the selected hero has changed.
	public delegate void HeroChangedEvent( int selectedHeroIndex, GameObject heroSkin );
	public static event HeroChangedEvent heroChangedEvent;

	// Use this for initialization
	void Awake () {
		
		maxHeroIndex = HeroManager.Instance.getNumberOfHeroes() - 1;
	}

	void Start()
	{
		currentIndex = GameManager.Instance.playerProfile.selectedHeroIndex;
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
		
		//Remember the selected hero as we will need to access it later
		GameManager.Instance.playerProfile.selectedHeroIndex = currentIndex;

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
		HeroManager.HeroCharacter hero = HeroManager.Instance.getHeroCharacter( currentIndex );
		heroIcon.sprite = hero.icon;
		heroName.text = hero.name.ToString();
		//configure skin
		configureSkin( heroSkinList[hero.skinIndex] );
		updateCarouselImages();
		//configure card
		heroCardhandler.configureHeroCard( hero.reservedCard );
		if( heroChangedEvent != null ) heroChangedEvent( currentIndex, heroSkinList[hero.skinIndex] );
	}

	void configureSkin( GameObject selectedSkin )
	{
		if( previousSkin != null ) previousSkin.SetActive( false );
		selectedSkin.SetActive( true );
		previousSkin = selectedSkin;
	}
}
