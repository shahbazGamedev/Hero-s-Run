using UnityEngine;
using System.Collections;

public class CollapsingBridge : MonoBehaviour {

	public GameObject leftFarTile;
	public GameObject middleFarTile;
	public GameObject rightFarTile;

	Transform player;
	bool hasCollapsed = false;

	void Awake ()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;

	}
	
	// Update is called once per frame
	void Update () {
		collapse();
	}

	void collapse()
	{
		if( !hasCollapsed )
		{
			float distance = Vector3.Distance(player.position,middleFarTile.transform.position);
			float collapseDistance;
			collapseDistance = 1f * PlayerController.getPlayerSpeed();
			if( distance < collapseDistance  )
			{
				hasCollapsed = true;
				//leftFarTile.GetComponent<Rigidbody>().isKinematic = false;
				middleFarTile.GetComponent<Rigidbody>().isKinematic = false;
				print("Collapse");
				middleFarTile.GetComponent<Rigidbody>().AddForce( new Vector3( 0, 70f, 0 ) );
				middleFarTile.GetComponent<Rigidbody>().AddTorque( new Vector3( 100f, 20f, -200f ) );
				//The collapsing tag will allow you to jump even if the distance to the ground is bigger than 0.5f
				middleFarTile.tag = "Collapsing";
				Invoke( "nextTime", 0.1f);
			}
		}
	}
	void nextTime()
	{
			rightFarTile.GetComponent<Rigidbody>().isKinematic = false;
			rightFarTile.GetComponent<Rigidbody>().AddForce( new Vector3( 0, 70f, 0 ) );
			rightFarTile.GetComponent<Rigidbody>().AddTorque( new Vector3( -100f, -20f, 200f ) );
			//The collapsing tag will allow you to jump even if the distance to the ground is bigger than 0.5f
			rightFarTile.tag = "Collapsing";
	}
}
