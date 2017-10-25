using UnityEngine;
using System.Collections;

public class ClockSetter3D : MonoBehaviour {

	[Header("3D Clock")]
	//This script sets the hour and minute hands of a clock.
	public Transform minuteHand;
	public Transform hourHand;
	public float hours;
	public float minutes;

	// Use this for initialization
	void OnEnable ()
	{
		setTime();
	}

	void setTime()
	{
		hourHand.localRotation = Quaternion.Euler(-(hours + minutes/60f) * 30f,0 ,0 );
		//The -90 in the calculation is simply because the minute hand model pivot has a 90 degree offset
		minuteHand.localRotation = Quaternion.Euler(-minutes*6f-90f,0 ,0 );

	}
}
