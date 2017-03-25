using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour {

	[SerializeField] GameObject Teleport_VFX1;
	[SerializeField] GameObject Teleport_VFX2;
	Color originalColor;

	void Awake()
	{
		originalColor = Teleport_VFX1.GetComponent<MeshRenderer>().material.GetColor("_TintColor");
	}

	void changeColor( Color newColor )
	{
		Teleport_VFX1.GetComponent<MeshRenderer>().material.SetColor("_TintColor", newColor);
		Teleport_VFX2.GetComponent<MeshRenderer>().material.SetColor("_TintColor", newColor);
	}

	void resetColor()
	{
		Teleport_VFX1.GetComponent<MeshRenderer>().material.SetColor("_TintColor", originalColor);
		Teleport_VFX2.GetComponent<MeshRenderer>().material.SetColor("_TintColor", originalColor);
	}

	void OnTriggerEnter(Collider other)
	{
		if( other.gameObject.CompareTag("Player")  )
		{
			GetComponent<AudioSource>().Play();
			other.gameObject.GetComponent<PlayerInput>().doubleJump( 15f );
		}
	}
}
