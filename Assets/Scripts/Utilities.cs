using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Utilities : MonoBehaviour {


	//This method will return the angle clamped between plus or minus delta.
	//So if you pass a delta of 10, the value will be clamped between 0 to 10 and 350 to 360 depending on the angle passed.
	public static float clampRotation( float angle, float delta )
	{
		//Make sure angle is positive and smaller than 360 degrees.
		//Note: transform.eulerAngles always returns a value between 0 and 360 degrees.
		angle += 360;
		angle = angle%360;
		if( angle < delta || angle > (360f - delta) )
		{
			//Valid angle - do nothing
		}
		else if( angle > delta && angle <= 180f)
		{
			angle = delta;
		}
		else
		{
			angle = (360f - delta);
		}
		return angle;
	}
	
	public static Vector2 Bezier2(Vector2 Start, Vector2 Control, Vector2 End, float t )
	{
		return (((1-t)*(1-t)) * Start) + (2 * t * (1 - t) * Control) + ((t * t) * End);
	}
 
	public static Vector3 Bezier2(Vector3 Start, Vector3 Control, Vector3 End, float t )
	{
	    return (((1-t)*(1-t)) * Start) + (2 * t * (1 - t) * Control) + ((t * t) * End);
	}
 
	public static Vector2 Bezier3(Vector2 s, Vector2 st, Vector2 et, Vector2 e, float t )
	{
	    return (((-s + 3*(st-et) + e)* t + (3*(s+et) - 6*st))* t + 3*(st-s))* t + s;
	}
 
	public static Vector3 Bezier3(Vector3 s, Vector3 st, Vector3 et, Vector3 e, float t )
	{
	    return (((-s + 3*(st-et) + e)* t + (3*(s+et) - 6*st))* t + 3*(st-s))* t + s;
	}
	
	//Use the sign of the determinant of vectors (AB,AM), where M(X,Y) is the query point.
	//Return value is true when M is to the right of the AB vector and false if M is to the left.
	public static bool onWhichSide( Vector3 A, Vector3 B, Vector3 M )
	{
		float position = Mathf.Sign( (B.x-A.x)*(M.z-A.z) - (B.z-A.z)*(M.x-A.x) );
		if( position > 0 )
		{
			//On the left of the vector.
			return false;
		}
		else
		{
			//On the right of the vector.
			return true;
			
		}
	}

	public static IEnumerator fadeInCanvasGroup( CanvasGroup canvasGroup, float duration )
	{
		canvasGroup.alpha = 0f;
		float elapsed = 0;	
		do
		{
			elapsed = elapsed + Time.deltaTime;
			canvasGroup.alpha = elapsed/duration;
			yield return null;
		} while ( elapsed < duration );
		canvasGroup.alpha = 1f;
	}

	public static T GetRandomEnum<T>()
	{
		System.Array A = System.Enum.GetValues(typeof(T));
		T V = (T)A.GetValue(UnityEngine.Random.Range(0,A.Length));
		return V;
	}

	public static IEnumerator fadeInLight( Light light, float duration, float endIntensity )
	{
		float elapsedTime = 0;
		
		float startIntensity = 0;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			light.intensity =  Mathf.Lerp( startIntensity, endIntensity, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		light.intensity = endIntensity;
	}

	public static IEnumerator fadeOutLight( Light light, float duration )
	{
		float elapsedTime = 0;
		
		float startIntensity = light.intensity;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			light.intensity =  Mathf.Lerp( startIntensity, 0, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		
		light.intensity = 0;
	}

	// This script finds all the textures in scene:
    public static void printAllTexturesInScene()
    {
 		foreach (Texture go in Resources.FindObjectsOfTypeAll(typeof(Texture)) as Texture[])
        {
			print("Texture: " + go.name  );
        }
    }
}
