using UnityEngine;
using System.Collections;

public class LadderRotater : MonoBehaviour {

	// Use this for initialization
	void Start () {
		PlatformingElement pe = GetComponent<PlatformingElement> ();
		if (pe.connectors [0].connectedNode != null)
		{
			Vector3 rot = transform.localEulerAngles;
			rot.y = pe.connectors[0].connectedNode.transform.localEulerAngles.y;
			transform.localEulerAngles = rot;
			Destroy(this);
		}
	}
	
	// Update is called once per frame
	void Update () {


	}
}
