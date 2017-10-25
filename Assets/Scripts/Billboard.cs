using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour {

	[ExecuteInEditMode]
    void Update ()
	{
        transform.LookAt(Camera.main.transform);
    }
}