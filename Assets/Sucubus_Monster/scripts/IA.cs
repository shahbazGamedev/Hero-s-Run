using UnityEngine;

using System.Collections;

 

public class IA: MonoBehaviour

{

    public Transform target;

    public float velocity = 10;

    

    void Update()

    {

        transform.LookAt(target);

        transform.Translate(Vector3.forward * velocity * Time.deltaTime);

    }

}