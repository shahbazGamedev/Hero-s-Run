using UnityEngine;
using System.Collections;

public class VFX_Rotate : MonoBehaviour {

	public float rotationSpeed;


	void Start () {
	
	}
	

	void Update () {


		transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);


	
	}
}
