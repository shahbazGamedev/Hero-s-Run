using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour {

	[SerializeField] Slider manaSlider;
	public const int MAX_MANA_POINT = 10;
	public const int MIN_MANA_POINT = 0;
	public const int START_MANA_POINT = 5;
	public const float MANA_REFILL_RATE = 2.8f; //Seconds needed to gain 1 mana point

	// Use this for initialization
	void Start ()
	{
		manaSlider.minValue = MIN_MANA_POINT;
		manaSlider.maxValue = MAX_MANA_POINT;
		manaSlider.value 	= START_MANA_POINT;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if( manaSlider.value < MAX_MANA_POINT && PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.IN_PROGRESS ) manaSlider.value = manaSlider.value + Time.deltaTime/MANA_REFILL_RATE;
	}

	/// <summary>
	/// Returns true if the amount of mana available is greater or equal to the mana cost.
	/// </summary>
	/// <returns><c>true</c>, if enough mana, <c>false</c> otherwise.</returns>
	/// <param name="manaCost">Mana cost.</param>
	public bool hasEnoughMana( int manaCost )
	{
		if( manaCost <= manaSlider.value )
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public float getManaAmount()
	{
		return manaSlider.value;
	}

	public void deductMana( int manaToRemove )
	{
		if( manaToRemove > manaSlider.value )
		{
			Debug.LogError("ManaBar-deductMana: the amount of mana you want to deduct, " + manaToRemove + ", is bigger than the mana available.");
		}
		else
		{
			manaSlider.value = manaSlider.value - manaToRemove;
		}
	}
}
