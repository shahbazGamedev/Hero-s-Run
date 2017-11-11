/* 	Breakable Object script version 1.07
	(C) 2013 Unluck Software
	http://www.chemicalbliss.com

Changelog
v.1.07
Removed physicMat variable, PhysicMaterial has to be applied to each fragment manually (performance increase)
Removed loops that add Rigidbody and MeshCollider to fragments, these components has to be added manually (performance increase)
!Changes will now generate null reference errors on fragments without the necessary components
	Fix: select all fragments in fractured prefab then add Rigidbody and MeshCollider with convex

v.1.06
Fixed errors in Android build mode

v.1.05
Added comments
Removed unused Start()

v.1.04
Removed selfCollide option, convex colliders are now always enabled. This is more user friendly.
Fractured object can now contain rigidbodies and mesh colliders before breaking, small optimization and better customization of the fraktured prefab.
Fragments no longer have mass of 0.0001, this is set to default Unity value (default = 1). Set mass manually by adding rigidbodies to fragments if needed.

v1.03
Removed behaviour that replaces material on fractured object, make sure the broken object has the correct material. (improved performance, fixes multiple materials issue)
	- Objects needs to have a unique prefab per material (unfractured and fractured), no longer possible to link directly to model.

v1.02
Removed self destruct function
Added MouseClick to destroy option
Removed delay on selfCollide
Removed unused variable "counter"
Fixed empty clones not being destroyed
Fixed naming swap on functions "removeRigids" and "removeColliders"

v1.01
Added particle system, instantiated on breaking object (does not scale with object)
*/
#pragma strict
#pragma downcast
var fragments: Transform; 				//Place the fractured object
var waitForRemoveCollider: float = 1; 	//Delay before removing collider (negative/zero = never)
var waitForRemoveRigid: float = 10; 	//Delay before removing rigidbody (negative/zero = never)
var waitForDestroy: float = 2; 			//Delay before removing objects (negative/zero = never)
var explosiveForce: float = 350; 		//How much random force applied to each fragment
var durability: float = 5; 				//How much physical force the object can handle before it breaks
var breakParticles:ParticleSystem;		//Assign particle system to apear when object breaks
var mouseClickDestroy:boolean;			//Mouse Click breaks the object
private var fragmentd: Transform;		//Stores the fragmented object after break
private var broken: boolean; 			//Determines if the object has been broken or not 

function OnCollisionEnter(collision: Collision) {
    if (collision.relativeVelocity.magnitude > durability) {
        triggerBreak( null );
    }
}


function Awake() {

	fragmentd = gameObject.Instantiate(fragments, transform.position, transform.rotation); // adds fragments to stage
	fragmentd.localScale = transform.localScale; // set size of fragments
	fragmentd.gameObject.SetActive( false );
	fragmentd.transform.parent = transform;

}
function triggerBreak( playerCollider: Collider ) {
	GetComponent.<AudioSource>().Play();
    transform.Destroy(transform.Find("object").gameObject);
    Destroy(transform.GetComponent.<Collider>());
    Destroy(transform.GetComponent.<Rigidbody>());
    breakObject( playerCollider );
}

function breakObject( playerCollider: Collider ) {// breaks object
    if (!broken) {
    	broken = true;
    	if(breakParticles!=null){
    		var ps:ParticleSystem = gameObject.Instantiate(breakParticles,transform.position, transform.rotation); // adds particle system to stage
    		Destroy(ps.gameObject, ps.duration); // destroys particle system after duration of particle system
    	}
		fragmentd.gameObject.SetActive( true );
        var frags: Transform = fragmentd.Find("fragments");
		if( playerCollider != null )
		{
	        var forward: Vector3 = playerCollider.transform.TransformDirection(Vector3.forward);
	        forward = forward * explosiveForce;
		}
        for (var child: Transform in frags) {
        	if( playerCollider != null )
        	{
	        	Physics.IgnoreCollision(child.GetComponent.<Collider>(), playerCollider);
				child.GetComponent.<Rigidbody>().AddForce(forward.x, Random.Range(-explosiveForce, explosiveForce), forward.z);
				//Debug.Log("breakObject " + forward.x + " " + forward.z );
	            child.GetComponent.<Rigidbody>().AddTorque(Random.Range(-explosiveForce, explosiveForce), Random.Range(-explosiveForce, explosiveForce), Random.Range(-explosiveForce, explosiveForce));
            }
            else
            {
				child.GetComponent.<Rigidbody>().AddForce(Random.Range(-explosiveForce, explosiveForce),  Random.Range(-explosiveForce, explosiveForce), Random.Range(-explosiveForce, explosiveForce));
            	child.GetComponent.<Rigidbody>().AddTorque(Random.Range(-explosiveForce, explosiveForce), Random.Range(-explosiveForce, explosiveForce), Random.Range(-explosiveForce, explosiveForce));
            }
        }
		//transform.position.y -=1000;	// Positions the object out of view to avoid further interaction
        if (transform.Find("particles") != null) transform.Find("particles").GetComponent.<ParticleEmitter>().emit = false;
        removeColliders();
        removeRigids();
        if (waitForDestroy > 0) { // destroys fragments after "waitForDestroy" delay
            yield(WaitForSeconds(waitForDestroy));
            GameObject.Destroy(fragmentd.gameObject);
            GameObject.Destroy(transform.gameObject);
        }else if (waitForDestroy <=0){ // destroys this.gameobject after 1 second if fragments are set to never get destroyed
        	yield(WaitForSeconds(1));
            GameObject.Destroy(transform.gameObject);
        }	
    }
}

function removeRigids() {// removes rigidbodies from fragments after "waitForRemoveRigid" delay
    if (waitForRemoveRigid > 0 && waitForRemoveRigid != waitForDestroy) {
        yield(WaitForSeconds(waitForRemoveRigid));
        for (var child: Transform in fragmentd.Find("fragments")) {
            child.gameObject.Destroy(child.GetComponent(Rigidbody));
        }
    }
}

function removeColliders() {// removes colliders from fragments "waitForRemoveCollider" delay
    if (waitForRemoveCollider > 0){
        yield(WaitForSeconds(waitForRemoveCollider));
        for (var child: Transform in fragmentd.Find("fragments")) {
            child.gameObject.Destroy(child.GetComponent(Collider));
        }
    }
}