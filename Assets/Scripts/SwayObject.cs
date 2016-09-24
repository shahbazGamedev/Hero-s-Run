using UnityEngine;
using System.Collections;

//Sways an object using a sine curve along Z axis only.
public class SwayObject : MonoBehaviour {

	public float strength = 4f;
	
	// Update is called once per frame
	void Update ()
	{
		transform.rotation = Quaternion.Euler( 0f, 0f, Mathf.Sin( Time.time * strength) );
	}
}
