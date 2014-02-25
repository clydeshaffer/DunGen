using UnityEngine;
using System.Collections;

public class PointIndicatorGizmo : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnDrawGizmos () {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position,(1 - Mathf.Repeat(Time.timeSinceLevelLoad,1.0f)) * 10.0f);
	}
}
