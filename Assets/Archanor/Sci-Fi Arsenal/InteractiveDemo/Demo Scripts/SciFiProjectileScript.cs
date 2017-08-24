using UnityEngine;
using System.Collections;

namespace SciFiArsenal
{
public class SciFiProjectileScript : MonoBehaviour
{
    public GameObject impactParticle;
    public GameObject projectileParticle;
    public GameObject muzzleParticle;
    public GameObject[] trailParticles;
    [HideInInspector]
    public Vector3 impactNormal; //Used to rotate impact particle.

    private bool hasCollided = false;

    void Start()
    {
        projectileParticle = Instantiate(projectileParticle, transform.position, transform.rotation) as GameObject;
        projectileParticle.transform.parent = transform;
        if (muzzleParticle)
        {
            muzzleParticle = Instantiate(muzzleParticle, transform.position, transform.rotation) as GameObject;
            muzzleParticle.transform.rotation = transform.rotation * Quaternion.Euler(180, 0, 0);
            Destroy(muzzleParticle, 1.5f); // Lifetime of muzzle effect.
        }
    }

    void OnCollisionEnter(Collision hit)
    {
        if (!hasCollided)
        {
			impactNormal = hit.contacts[0].normal; //Used to rotate impactparticle.
            hasCollided = true;
            //transform.DetachChildren();
            impactParticle = Instantiate(impactParticle, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal)) as GameObject;
            //Debug.DrawRay(hit.contacts[0].point, hit.contacts[0].normal * 1, Color.yellow);

            if ( hit.collider.CompareTag("Player") )
            {
                hit.collider.GetComponent<PlayerHealth>().deductHealth( 20 );
            }

             foreach (GameObject trail in trailParticles)
            {
                GameObject curTrail = transform.Find(projectileParticle.name + "/" + trail.name).gameObject;
                curTrail.transform.parent = null;
                Destroy(curTrail, 3f);
            }
            Destroy(projectileParticle, 3f);
            Destroy(impactParticle, 5f);
            Destroy(gameObject);
         }
    }
}
}