using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffinController : MonoBehaviour {

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		Invoke("openCoffin", 0.25f );
	}

	void openCoffin()

	{
		GetComponent<Animator>().SetBool("open", true );
	}
}
