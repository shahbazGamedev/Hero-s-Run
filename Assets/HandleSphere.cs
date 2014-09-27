using UnityEngine;
using System.Collections;

public class HandleSphere : MonoBehaviour {

	float rotationIncrement = 1f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		#if UNITY_EDITOR
		handleKeyboard();
		#endif

	}

	private void handleKeyboard()
	{
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.LeftArrow) ) 
		{
			transform.Rotate( Vector3.right, -rotationIncrement );
		}
		else if ( Input.GetKeyDown (KeyCode.RightArrow) ) 
		{
			transform.Rotate( Vector3.right, rotationIncrement );
		}
		else if ( Input.GetKeyDown (KeyCode.DownArrow) ) 
		{
			transform.Rotate( Vector3.up, rotationIncrement );
		}
		else if ( Input.GetKeyDown (KeyCode.UpArrow) ) 
		{
			transform.Rotate( Vector3.up, -rotationIncrement );
		}
	}

	void rotateWorld( Vector3 rotationAxis, float amount )
	{
		transform.Rotate( rotationAxis, amount );
	}

}
