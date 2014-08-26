using UnityEngine;
using System.Collections;

public class Sunrays : MonoBehaviour {
	

	void OnEnable()
	{
		//Since the moon, our source of light is in front (Z+), only display moonrays when we are running to the left or to the right
		if( transform.eulerAngles.y == 0 ) gameObject.SetActive( false );
	}
}
