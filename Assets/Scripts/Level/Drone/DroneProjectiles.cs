using UnityEngine;
using System.Collections;

public class DroneProjectiles : MonoBehaviour
{
    [SerializeField] GameObject projectilePrefab;

 	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = gameObject.GetPhotonView ().instantiationData;
		float projectileSpeed = (float) data[0];

		//Left projectile
		GameObject projectileL = Instantiate(projectilePrefab, (Vector3) data[3], Quaternion.identity) as GameObject;
		projectileL.transform.LookAt( (Vector3) data[1] );
		projectileL.GetComponent<Rigidbody>().AddForce(projectileL.transform.forward * projectileSpeed);

		//Right projectile
		GameObject projectileR = Instantiate(projectilePrefab, (Vector3) data[4], Quaternion.identity) as GameObject;
		projectileR.transform.LookAt( (Vector3) data[2] );
		projectileR.GetComponent<Rigidbody>().AddForce(projectileR.transform.forward * projectileSpeed);

		//We don't want the projectiles to collide with each other.
		Physics.IgnoreCollision(projectileL.GetComponent<SphereCollider>(), projectileR.GetComponent<SphereCollider>());
		
		GameObject.Destroy( gameObject );
	}

}

