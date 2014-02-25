using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class OctTree {
	
	//The random seed
	public int seed;
	
	public Bounds boundary;
	public RoomNode roomNode;
	
	public Color gizmoColor = Color.white;
	
	/*
	 pos. z       neg. z
	    y            y
	 1  |  0      5  |  4
   	----+----x   ---------x
	 2  |  3      6  |  7
	 
	*/
	public OctTree[] children;
	private int childCount = 0; //children.size is always 8, so use this instead
	
	public int GetLeafCount()
	{
		if(childCount == 0) return 1;
		int sum = 0;
		foreach(OctTree child in children)
		{
			if(child != null) sum += child.GetLeafCount();
		}
		return sum;
	}
		
	public int GetChildCount()
	{
		return childCount;
	}
	
	public static Vector3 OctantDirection(int octant)
	{
		switch(octant)
		{
		case 0:
			return new Vector3( 1, 1, 1);
		case 1:
			return new Vector3(-1, 1, 1);
		case 2:
			return new Vector3(-1,-1, 1);
		case 3:
			return new Vector3( 1,-1, 1);
		case 4:
			return new Vector3( 1, 1,-1);
		case 5:
			return new Vector3(-1, 1,-1);
		case 6:
			return new Vector3(-1,-1,-1);
		case 7:
			return new Vector3( 1,-1,-1);
		default:
			throw new System.IndexOutOfRangeException("octant must be between 0-7");
		}
	}
	
	public OctTree(Bounds _bounds)
	{
		boundary = _bounds;
		children = new OctTree[8];
		for(int i = 0; i < 8; i ++) children[i] = null;
		
	}
	
	public Vector3 RandomSlice(int minRoomSize)
	{
		
		Vector3 slice;
		
		slice.x = Mathf.Round(Random.Range(boundary.min.x + minRoomSize, boundary.max.x - minRoomSize));
		slice.y = Mathf.Round(Random.Range(boundary.min.y + minRoomSize, boundary.max.y - minRoomSize));
		slice.z = Mathf.Round(Random.Range(boundary.min.z + minRoomSize, boundary.max.z - minRoomSize));
		
		return slice;
		
		
	}
	
	public void GenerateZones(int depth, int minRoomSize)
	{
		if(depth < 1) return;
		
		if(boundary.extents.x < minRoomSize) return;
		if(boundary.extents.y < minRoomSize) return;
		if(boundary.extents.z < minRoomSize) return;
		
		Vector3 slice = RandomSlice(minRoomSize);
		
		
		for(int oct = 0; oct < 8; oct ++)
		{
			Vector3 dir = OctantDirection(oct);
			Vector3 farCorner = boundary.center + Vector3.Scale(boundary.extents,dir);
			Vector3 size = Vector3.Scale(farCorner - slice,dir);
			Vector3 center = slice + Vector3.Scale(size, dir * 0.5f);
			Bounds newBounds = new Bounds(center,size);
			if(newBounds.size.x >= minRoomSize
				&& newBounds.size.y >= minRoomSize
				&& newBounds.size.z >= minRoomSize)
			{
				children[oct] = new OctTree(newBounds);
				children[oct].gizmoColor = Color.Lerp(new Color(Mathf.Max(dir.x, 0),Mathf.Max(dir.y, 0),Mathf.Max(dir.z, 0)),gizmoColor,0.5f);
				children[oct].GenerateZones(depth - 1, minRoomSize);
				childCount ++;
			}
			
		}
		
	}
	
	public void DrawGizmoBoxes()
	{
		int childCount = 0;
		foreach(OctTree child in children)
		{
			if(child != null) 
			{
				child.DrawGizmoBoxes();
			}
		}
		
		if(childCount == 0)
		{
			Gizmos.color = gizmoColor;
			Gizmos.DrawWireCube(boundary.center,boundary.size);
		}
	}
}
