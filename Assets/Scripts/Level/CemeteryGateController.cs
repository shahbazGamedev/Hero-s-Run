using UnityEngine;
using System.Collections;

public class CemeteryGateController : MonoBehaviour {

	void Start ()
	{
		Invoke( "openCemeteryGate", 1.5f );
	}
	
	void openCemeteryGate()
	{
		Animation animation = GetComponent<Animation>();
		audio.Play ();
		animation.Play("open slowly");
		animation.PlayQueued("sway in wind");
	}

}
