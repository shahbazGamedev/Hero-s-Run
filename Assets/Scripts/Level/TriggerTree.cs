using UnityEngine;
using System.Collections;

public class TriggerTree : MonoBehaviour {

	public GameObject treeToTrigger; //Evil tree to trigger

	//Only trigger if by hero or zombie
	void OnTriggerEnter(Collider other)
	{
		if( other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Zombie") )
		{
			TreeController treeController = treeToTrigger.GetComponent<TreeController>();
			treeController.wakeUp();
		}
	}
}