using UnityEngine;
using System.Collections;

public class Utilities : MonoBehaviour {

    // Makes the gameobject and all of it's children visible or not. It also handles the Halo if any.
	// Can optionally disable colliders as well.
	public static void makeVisible ( GameObject go, bool enable, bool includeColliders )
	{
   		Renderer[] renderers;
		renderers = go.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in renderers)
		{
            renderer.enabled = enable;
        }
		
		Behaviour halo = (Behaviour)go.GetComponent("Halo");
		if( halo != null ) halo.enabled = enable;
		
		if( includeColliders )
		{
			Collider[] colliders;
			colliders = go.GetComponentsInChildren<Collider>();
			foreach (Collider collider in colliders)
			{
	            collider.enabled = enable;
	        }
		}
	}

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

	public static void drawLabelWithDropShadow( Rect location, GUIContent text, GUIStyle textStyle, Color dropShadowColor )
	{
		Color colorBeforeChange = textStyle.normal.textColor;
		Rect dropShadowRect = new Rect( location.x + 1, location.y + 2, location.width, location.height );
		textStyle.normal.textColor = dropShadowColor;
		GUI.Label ( dropShadowRect, text, textStyle );
		//Reset color
		textStyle.normal.textColor = colorBeforeChange;
		GUI.Label ( location, text, textStyle );
	}	

	public static void drawLabelWithDropShadow( Rect location, GUIContent text, GUIStyle textStyle )
	{
		drawLabelWithDropShadow( location, text, textStyle, Color.black );
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

}
