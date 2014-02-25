//Attach this script to the Right Eye of an OVRCamera.  
//Then, add an empty game object as the child of the camera (local pos:0,0,0); and set its rotation rotation to 270,180,0.
//Then add a cube at 0,0,0 to that gameobject (with a rigidbody) with the dimensions .1,.1,1.
//Profit!!
//Leap Motion wrote a bunch of this.
using UnityEngine;
using System.Collections;
using Leap;

public class LeapSword : MonoBehaviour {
    Controller controller;
	public GameObject Sword;
	public static float Scale = .01f;
	private static Vector3 FlippedZ( Vector v ) { return new Vector3( v.x, v.y, -v.z ); }
	private static Vector3 Scaled( Vector3 v ) { return new Vector3( v.x * InputScale.x, v.y * InputScale.y, v.z * InputScale.z ); }
	private static Vector3 Offset( Vector3 v ) { return v + InputOffset; }
	public static Vector3 InputScale = new Vector3(Scale, Scale, Scale);
	public static Vector3 InputOffset = new Vector3(0,0,0);
	public Vector3 LastPos = new Vector3(0,0,0);
    void Start (){
        controller = new Controller();
    }
    void Update (){
        Frame frame = controller.Frame();

	

 		if(!frame.Pointables.IsEmpty){



			Sword.transform.localPosition = Offset(Scaled(FlippedZ(frame.Pointables.Rightmost.TipPosition)));
		    Sword.transform.rigidbody.velocity = (Sword.transform.position-LastPos)*100;
			LastPos = Sword.transform.position;
			Sword.transform.localRotation = Quaternion.FromToRotation(Vector3.forward, Scaled(FlippedZ(frame.Pointables.Rightmost.Direction)));
		}
    }
}