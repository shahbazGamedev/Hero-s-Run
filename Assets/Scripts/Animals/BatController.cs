using UnityEngine;
using System.Collections;

public class BatController : MonoBehaviour {

	Transform player;

	void Awake ()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
	}

	// Update is called once per frame
	void LateUpdate () {
		transform.LookAt(player);

	}
}
