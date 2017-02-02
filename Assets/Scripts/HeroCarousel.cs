using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroCarousel : MonoBehaviour {

	[Header("General")]
	ScrollRect scrollRect;
	int currentIndex = 0;
	const float STEP_VALUE = 1f/3f;
	[Header("Left Top Corner")]
	[SerializeField] Image heroIcon;
	[SerializeField] Text heroName;

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
		
	}

	public void normalizedPosition ( Vector2 value )
	{
		//print("normalizedPosition " + value.x );
	}
	
}
