/****************************************
	BreakableObject Editor v1.01			
	Copyright 2013 Unluck Software	
 	www.chemicalbliss.com																																				
*****************************************/
@CustomEditor (BreakableObject)
@CanEditMultipleObjects

class BreakableObjectEditor extends Editor {
    function OnInspectorGUI () {
    	EditorGUILayout.LabelField("Unluck Software: Breakable Object Editor v1.01");
    	EditorGUILayout.Space();
    	EditorGUILayout.LabelField("Drag & Drop");
    	target.fragments = EditorGUILayout.ObjectField("Fractured Object Prefab", target.fragments, Transform , false);
    	EditorGUILayout.LabelField("Drag & Drop (Optional)");
    	target.breakParticles = EditorGUILayout.ObjectField("Particle System Prefab", target.breakParticles, ParticleSystem ,false);
    	EditorGUILayout.Space();
    	EditorGUILayout.LabelField("Seconds before removing fragment colliders (negative/zero = never)");   	
    	target.waitForRemoveCollider = EditorGUILayout.FloatField("Remove Collider Delay" , target.waitForRemoveCollider);
    	EditorGUILayout.LabelField("Seconds before removing fragment rigidbodies (negative/zero = never)");   	
    	target.waitForRemoveRigid = EditorGUILayout.FloatField("Remove Rigidbody Delay" , target.waitForRemoveRigid);	
  		EditorGUILayout.LabelField("Seconds before removing fragments (negative/zero = never)");   	
    	target.waitForDestroy = EditorGUILayout.FloatField("Destroy Fragments Delay" , target.waitForDestroy);	
    	EditorGUILayout.Space();
    	EditorGUILayout.LabelField("Force applied to fragments after object is broken");   
    	target.explosiveForce = EditorGUILayout.FloatField("Fragment Force" , target.explosiveForce);
    	EditorGUILayout.LabelField("How hard must object be hit before it breaks");   	
    	target.durability = EditorGUILayout.FloatField("Object Durability" , target.durability);	
    	target.mouseClickDestroy = EditorGUILayout.Toggle("Click To Break Object" , target.mouseClickDestroy);
        if (GUI.changed)
            EditorUtility.SetDirty (target);
    }
}