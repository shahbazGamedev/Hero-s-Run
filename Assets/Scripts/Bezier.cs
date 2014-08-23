using UnityEngine;
using System.Collections;

public class Bezier {

    public Vector3 P1;
    public Vector3 P2;
    public Vector3 P3;
    public Vector3 P4;
    public Bezier(Vector3 aP1, Vector3 aP2, Vector3 aP3, Vector3 aP4)
    {
        P1 = aP1; 	//start
        P2 = aP2;	//control point 1
        P3 = aP3;	//control point 2
        P4 = aP4;	//end
    }
    public Vector3 Evaluate(float aTime)
    {
        var t = Mathf.Clamp01(aTime);
        return (((-P1 + 3*(P2-P3) + P4)* t + (3*(P1+P3) - 6*P2))* t + 3*(P2-P1))* t + P1;
    }
 
    // Calculates the best fitting time in the given interval
    private float CPOB(Vector3 aP, float aStart, float aEnd, int aSteps)
    {
        aStart = Mathf.Clamp01(aStart);
        aEnd = Mathf.Clamp01(aEnd);
        float step = (aEnd-aStart) / (float)aSteps;
        float Res = 0;
        float Ref = float.MaxValue;
        for (int i = 0; i < aSteps; i++)
        {
            float t = aStart + step*i;
            float L = (Evaluate(t)-aP).sqrMagnitude;
            if (L < Ref)
            {
                Ref = L;
                Res = t;
            }
        }
        return Res;
    }
 
    public float ClosestTimeOnBezier(Vector3 aP)
    {
        float t = CPOB(aP, 0, 1, 10);
        float delta = 1.0f / 10.0f;
        for (int i = 0; i < 4; i++)
        {
            t = CPOB(aP, t - delta, t + delta, 10);
            delta /= 9;//10;
        }
        return t;
    }
 
    public Vector3 ClosestPointOnBezier(Vector3 aP)
    {
        return Evaluate(ClosestTimeOnBezier(aP));
    }
	
	public Vector3 PointOnBezier( float t )
	{
	    return (((-P1 + 3*(P2-P3) + P4)* t + (3*(P1+P3) - 6*P2))* t + 3*(P2-P1))* t + P1;
	}

}
