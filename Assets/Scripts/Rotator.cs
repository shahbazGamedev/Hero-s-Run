using UnityEngine;
using System.Collections;


public enum Axis{ X,Y,Z };

[AddComponentMenu("Scripts/Obstacles/Rotator")]

//Note: this rotator eats up about 5-10fps
public class Rotator : MonoBehaviour {
	
	public float rotationSpeed = 200f;
	public Axis rotationAxis = Axis.Y;
	Vector3 ratationVector;
	
	// Use this for initialization
	void Awake () {
	
        switch (rotationAxis)
		{
	        case Axis.X:
                ratationVector = Vector3.right;
                break;
	                
	        case Axis.Y:
                ratationVector = Vector3.up;
                break;
	                
	        case Axis.Z:
                ratationVector = Vector3.forward;
                break;
	                
		}
	}
	
	// Update is called once per frame
	void Update () {
	
		transform.Rotate( ratationVector, -rotationSpeed * Time.deltaTime );
	}
}
