using UnityEngine;
using System.Collections;

//Container class for platform connectors
[System.Serializable]
public class PlatformConnection {
	public Vector3 connectionPoint;
	public Vector3 normal;
	public PlatformingElement connectedNode; //Adjoining platform element, if any.
	public float slackNormal = 0; //How far along the normal can the connection be offset, and still connect?
	public float slackRadial = 0; //How far along the tangent can the connection be offset, and still connect?
	public Bounds slackBox; //Bounding box of valid connection area. (Optional)
	public float slackAngle = 0.01f; //How far from -1 the dot product can be (use Min of two connectors)
	public bool allowRotationOnConnector = false; //When placing, allow other 90-degree rotations
}
