using UnityEngine;
using System.Collections;

public class OctTreeTest : MonoBehaviour {

	public Vector3 minRoomSize;
	
	public Bounds totalBounds;
	
	public int depth;
	
	public OctTree tree;
	
	// Use this for initialization
	void Start () {
		tree = new OctTree(totalBounds);
		tree.GenerateZones(depth,minRoomSize);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnDrawGizmos () {
		if(tree != null) tree.DrawGizmoBoxes();
	}
}
