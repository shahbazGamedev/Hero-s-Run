using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PowerBar : MonoBehaviour {

	[SerializeField] Image powerBarFill;
	[SerializeField] Color normalPowerBarFillColor;
	[SerializeField] Color emergencyPowerEngagedFillColor;
	[SerializeField] Color superchargerPowerBarFillColor;
	[SerializeField] RectTransform powerBarLitTipRect;  //Should be anchored bottom, center
	[SerializeField] Image powerBarLitTip;

	public const float MAX_POWER_POINT = 10f;
	public const float START_POWER_POINT = 0;
	public const float DEFAULT_POWER_REFILL_RATE = 2.8f; //Seconds needed to gain 1 power point
	const float SUPERCHARGER_POWER_REFILL_RATE = 1.8f; //Seconds needed to gain 1 power point
	public const float FAST_POWER_REFILL_RATE = 1.4f; //Seconds needed to gain 1 power point
	float powerBarFillLength;
	float powerRefillRate = DEFAULT_POWER_REFILL_RATE;
	float power = START_POWER_POINT; //Value between 0 and MAX_POWER_POINT

	// Use this for initialization
	void Start ()
	{
		powerBarFillLength = powerBarFill.GetComponent<RectTransform>().sizeDelta.y;
		updatePower ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if( power < MAX_POWER_POINT && PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.IN_PROGRESS )
		{
			power = power + Time.deltaTime/powerRefillRate;
			updatePower ();
		}
	}

	void updatePower ()
	{
		powerBarFill.fillAmount = power/MAX_POWER_POINT;
		if( power > 0 )
		{
			if( !powerBarLitTipRect.gameObject.activeSelf ) powerBarLitTipRect.gameObject.SetActive( true );
			//Position the light at the tip of the power bar
			powerBarLitTipRect.anchoredPosition = new Vector2( powerBarLitTipRect.anchoredPosition.x, powerBarFillLength * powerBarFill.fillAmount );
		}
		else
		{
			if( powerBarLitTipRect.gameObject.activeSelf ) powerBarLitTipRect.gameObject.SetActive( false );
		}
	}

	/// <summary>
	/// Returns true if the amount of power available is greater or equal to the power cost.
	/// </summary>
	/// <returns><c>true</c>, if enough power, <c>false</c> otherwise.</returns>
	/// <param name="powerCost">Power cost.</param>
	public bool hasEnoughPower( int powerCost )
	{
		return (powerCost <= power );
	}

	/// <summary>
	/// Gets the power amount, which is a value between 0 and MAX_POWER_POINT.
	/// </summary>
	/// <returns>The power amount</returns>
	public float getPowerAmount()
	{
		return power;
	}

	public void deductPower( int powerToRemove )
	{
		if( powerToRemove > power )
		{
			Debug.LogError("PowerBar-deducPower: the amount of power you want to deduct, " + powerToRemove + ", is bigger than the power available.");
		}
		else
		{
			power = power - powerToRemove;
			updatePower ();
		}
	}

	public void increaseRefillRateForSupercharger()
	{
		powerRefillRate = SUPERCHARGER_POWER_REFILL_RATE;
		powerBarFill.color = superchargerPowerBarFillColor;
		powerBarLitTip.color = superchargerPowerBarFillColor;
	}

	public void increaseRefillRate()
	{
		powerRefillRate = FAST_POWER_REFILL_RATE;
		powerBarFill.color = emergencyPowerEngagedFillColor;
		powerBarLitTip.color = emergencyPowerEngagedFillColor;
	}

	public void resetRefillRate()
	{
		powerRefillRate = DEFAULT_POWER_REFILL_RATE;
		powerBarFill.color = normalPowerBarFillColor;
		powerBarLitTip.color = normalPowerBarFillColor;
	}

}
