using UnityEngine;
using System.Collections;

public class BulletScript : MonoBehaviour
{
	public float speed;
	 
	void Update () 
	{
		transform.Translate (0f, 0f, speed * Time.deltaTime);
	}
}
