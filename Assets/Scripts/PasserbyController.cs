using UnityEngine;
using System.Collections;

public class PasserbyController : MonoBehaviour {

	NavMeshAgent nma;
	public GameObject destination;

	// Use this for initialization
	void Start () {

		NavMeshAgent nma = GetComponent<NavMeshAgent>();
		nma.SetDestination( destination.transform.position );
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
