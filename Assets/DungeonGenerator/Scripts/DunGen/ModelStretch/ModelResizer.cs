//Selective Model Stretching
//Clyde Shaffer 2013

using UnityEngine;
using System.Collections;

public class ModelResizer : MonoBehaviour {
	
	//Stretched portion relative to model center
	public Bounds stretchZone;
	//Amount by which to scale the "stretch zone" on load
	public Vector3 scale = Vector3.one;
	
	// Use this for initialization
	void Start () {
		StretchMesh(scale);
	}
	
	public void StretchMesh(Vector3 scale)
	{
		Vector3[] meshSet = GetComponent<MeshFilter>().mesh.vertices;
		//This becomes tons easier when you pretend it's a one-dimensional model, and do that three times.
		for(int axis = 0; axis < 3; axis ++)
		{
			//First get the difference between the scaled and unscaled stretch zone edge.
			//This is how much we'll move the non-stretched model portions
			float newMax = stretchZone.extents[axis] * scale[axis] + stretchZone.center[axis];
			float displacement = newMax - stretchZone.max[axis];
			for(int vert = 0; vert < meshSet.Length; vert ++)
			{
				float coordinate = (meshSet[vert])[axis];
				
				//If the vertex is outside the stretch zone, move it.
				if(coordinate > stretchZone.max[axis]) coordinate += displacement;
				else if(coordinate < stretchZone.min[axis]) coordinate -= displacement;
				else
				{
					//If the vertex is inside the stretch zone, scale it.
					coordinate -= stretchZone.center[axis];
					coordinate *= scale[axis];
					coordinate += stretchZone.center[axis];
				}
				
				(meshSet[vert])[axis] = coordinate;
			}
		}
		GetComponent<MeshFilter>().mesh.vertices = meshSet;
		stretchZone.size = Vector3.Scale(stretchZone.size, scale);
	}
	
	void OnDrawGizmosSelected () {
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireCube(transform.TransformPoint(stretchZone.center),transform.TransformDirection(stretchZone.size));
	}
}
