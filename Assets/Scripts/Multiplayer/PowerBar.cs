using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PowerBar : MonoBehaviour {

	[SerializeField] Slider powerBarSlider;
	[SerializeField] Image powerBarFill;
	public const int MAX_POWER_POINT = 10;
	public const int MIN_POWER_POINT = 0;
	public const int START_POWER_POINT = 0;
	public const float DEFAULT_POWER_REFILL_RATE = 2.8f; //Seconds needed to gain 1 power point
	public const float FAST_POWER_REFILL_RATE = 1.4f; //Seconds needed to gain 1 power point
	public float powerRefillRate = DEFAULT_POWER_REFILL_RATE;

	// Use this for initialization
	void Start ()
	{
		powerBarSlider.minValue = MIN_POWER_POINT;
		powerBarSlider.maxValue = MAX_POWER_POINT;
		powerBarSlider.value 	= START_POWER_POINT;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if( powerBarSlider.value < MAX_POWER_POINT && PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.IN_PROGRESS ) powerBarSlider.value = powerBarSlider.value + Time.deltaTime/powerRefillRate;
	}

	/// <summary>
	/// Returns true if the amount of power available is greater or equal to the power cost.
	/// </summary>
	/// <returns><c>true</c>, if enough power, <c>false</c> otherwise.</returns>
	/// <param name="powerCost">Power cost.</param>
	public bool hasEnoughPower( int powerCost )
	{
		if(powerCost <= powerBarSlider.value )
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public float getPowerAmount()
	{
		return powerBarSlider.value;
	}

	public void deductPower( int powerToRemove )
	{
		if( powerToRemove > powerBarSlider.value )
		{
			Debug.LogError("PowerBar-deducPower: the amount of power you want to deduct, " + powerToRemove + ", is bigger than the power available.");
		}
		else
		{
			powerBarSlider.value = powerBarSlider.value - powerToRemove;
		}
	}

	public void increaseRefillRate()
	{
		powerRefillRate = FAST_POWER_REFILL_RATE;
		powerBarFill.color = Color.red;
	}

	public void resetRefillRate()
	{
		powerRefillRate = DEFAULT_POWER_REFILL_RATE;
		powerBarFill.color = Color.green;
	}

}
