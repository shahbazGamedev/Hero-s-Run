using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarHandler : MonoBehaviour {

	[Header("Health Bar")]
	[SerializeField] Slider healthBar;
	[SerializeField] TextMeshProUGUI healthNumber;
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
		//Only hide the health bar if the player who crossed the finish line is not a bot.
		if( !isBot ) healthBar.gameObject.SetActive( false );
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
		healthBar.GetComponent<UIAnimateSlider>().animateSlider( newHealth, animationSpeed, onFinish );
		healthNumber.GetComponent<UISpinNumber>().spinNumber( "{0}", currentHealth, newHealth, animationSpeed, false );			
	}

	public void resetHealth ()
	{
		healthNumber.text = PlayerHealth.DEFAULT_HEALTH.ToString();
		healthBar.value = PlayerHealth.DEFAULT_HEALTH;			
	}
	
}
