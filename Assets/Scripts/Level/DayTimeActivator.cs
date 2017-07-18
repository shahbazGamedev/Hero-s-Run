using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayTimeActivator : MonoBehaviour {

	[SerializeField] SunType activateForThisSunType = SunType.Sky_city_night;

	void Start ()
	{
		SunType sunType = LevelManager.Instance.getSelectedCircuit().sunType;
		if( sunType == activateForThisSunType )
		{
			gameObject.SetActive( true );
			print("DayTimeActivator " + sunType + " " + name );
		}
		else
		{
			gameObject.SetActive( false );
		}
	}
	
}
