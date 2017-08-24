using UnityEngine;
using System.Collections;

public class DroneProjectiles : MonoBehaviour
{
    [SerializeField] GameObject projectilePrefab;

 	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		launchProjectiles();
	}

 	void launchProjectiles()
	{
		object[] data = gameObject.GetPhotonView ().instantiationData;
		float projectileSpeed = (float) data[0];
		float aimRange = (float) data[1];

   		RaycastHit hit;
		GameObject projectileL = null;
		GameObject projectileR = null;
		if (Physics.Raycast( (Vector3) data[4], transform.forward, out hit, aimRange))
		{
			projectileL = Instantiate(projectilePrefab, (Vector3) data[4], Quaternion.identity) as GameObject;
			projectileL.transform.LookAt( (Vector3) data[2] );
			projectileL.GetComponent<Rigidbody>().AddForce(projectileL.transform.forward * projectileSpeed);
		}

		if (Physics.Raycast( (Vector3) data[5], transform.forward, out hit, aimRange))
		{
			projectileR = Instantiate(projectilePrefab, (Vector3) data[5], Quaternion.identity) as GameObject;
			projectileR.transform.LookAt( (Vector3) data[3] );
			projectileR.GetComponent<Rigidbody>().AddForce(projectileR.transform.forward * projectileSpeed);
		}
		if( projectileL != null && projectileR != null ) Physics.IgnoreCollision(projectileL.GetComponent<SphereCollider>(), projectileR.GetComponent<SphereCollider>());
		
		GameObject.Destroy( gameObject );
	}

}

