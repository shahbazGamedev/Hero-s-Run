using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarHandler : MonoBehaviour {

	[Header("Health Bar")]
	[SerializeField] GameObject healthBarHolder;	//Parent of the 3 radial images.
	[SerializeField] Image lowHealthRadial; 		//The first 3 boxes (out of 10) are orange to indicate low health (30/100). The other 7 are gray.
	[SerializeField] Image fullHealthRadial; 		//All 10 boxes are blue. Blue means healthy.
	[SerializeField] Image armorRadial;				//the armor indicator sits on top of the full health radial.
	const float ANIMATION_DURATION_NORMAL = 0.8f;
	const float ANIMATION_DURATION_FAST = 0.3f;

	void OnEnable()
	{
		PlayerRace.crossedFinishLine += CrossedFinishLine;
	}
	
	void OnDisable()
	{
		PlayerRace.crossedFinishLine -= CrossedFinishLine;
	}

	void CrossedFinishLine( Transform player, int officialRacePosition, bool isBot )
	{
		//Only hide the health and armor bars if the player who crossed the finish line is not a bot.
		if( !isBot )
		{
			healthBarHolder.SetActive( false );
		}
	}

	public void changeHealth (int currentHealth, int newHealth, System.Action onFinish = null )
	{
		float animationSpeed;
		if( newHealth == 0 )
		{
			//Player will die because his health is zero so animate super fast.
			animationSpeed = ANIMATION_DURATION_FAST;
		}
		else
		{
			animationSpeed = ANIMATION_DURATION_NORMAL;
		}
		//Low and full radial images should always be in sync
		float fillAmount = newHealth/(float)PlayerHealth.DEFAULT_HEALTH;
		lowHealthRadial.GetComponent<UIAnimateRadialImage>().animateFillAmount( fillAmount, animationSpeed, onFinish );
		fullHealthRadial.GetComponent<UIAnimateRadialImage>().animateFillAmount( fillAmount, animationSpeed, onFinish );
	}

	public void resetHealth ()
	{
		lowHealthRadial.fillAmount = 1f;			
		fullHealthRadial.fillAmount = 1f;			
	}

	public void changeArmor (int currentArmor, int newArmor, System.Action onFinish = null )
	{
		float fillAmount = newArmor/(float)PlayerHealth.MAXIMUM_ARMOR;
		armorRadial.GetComponent<UIAnimateRadialImage>().animateFillAmount( newArmor, ANIMATION_DURATION_FAST, onFinish );
	}

	public void removeAllArmor ()
	{
		armorRadial.fillAmount = 0;	
	}
	
}
