using UnityEngine;
using System.Collections;

public class PasserbyController : MonoBehaviour {

	UnityEngine.AI.NavMeshAgent nma;
	public GameObject destination;

	// Use this for initialization
	void Start () {

		UnityEngine.AI.NavMeshAgent nma = GetComponent<UnityEngine.AI.NavMeshAgent>();
		nma.SetDestination( destination.transform.position );
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
