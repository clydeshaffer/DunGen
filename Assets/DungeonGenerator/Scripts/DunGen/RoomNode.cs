using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomNode {

	//Essentially a graph node containing a BoundingBox
	
	public OctTree octTreeNode; //Can be null if not created in an octree.
	
	public LinkedList<RoomNode> neighbors; //Immediate neighbors, like corridors
	public LinkedList<RoomNode> connections; //Rooms at the other ends of corridors
	
	public Bounds roomBounds;
	
	public bool isCorridor;
	
	public GameObject roomObject = null;
	
	public int visited = 0;
	
	public bool ConnectsTo(RoomNode dest)
	{
		return ConnectsTo(dest, Random.Range(int.MinValue, int.MaxValue));
	}
	
	public bool ConnectsTo(RoomNode dest, int visitKey)
	{
		if(dest == this) return true;
		visited = visitKey;
		foreach(RoomNode neighbor in neighbors)
		{
			if(neighbor.visited != visitKey)
			{
				if(neighbor.ConnectsTo(dest, visitKey)) return true;
			}
		}
		return false;
	}
	
	public static void MakeNeighbors(RoomNode a, RoomNode b)
	{
		a.neighbors.AddFirst(b);
		b.neighbors.AddFirst(a);
	}
	
	public static void MakeConnections(RoomNode a, RoomNode b)
	{
		a.connections.AddFirst(b);
		b.connections.AddFirst(a);
	}
	
	public RoomNode()
	{
		neighbors = new LinkedList<RoomNode>();
		connections = new LinkedList<RoomNode>();
	}
	
	public RoomNode(Bounds b)
	{
		roomBounds = b;
		neighbors = new LinkedList<RoomNode>();
		connections = new LinkedList<RoomNode>();
	}
	
	public static Vector3 OverlapAxes(Bounds a, Bounds b)
	{
		
		Vector3 result = Vector3.zero;
		
		for(int i = 0; i < 3; i++)
		{
			Interval a_ = new Interval(a.min[i], a.max[i]);
			Interval b_ = new Interval(b.min[i], b.max[i]);
			if(Interval.CheckOverlap(a_, b_)) result[i] = 1;
		}
		
		return result;
	}
	
	public static bool CheckOverlap(Bounds a, Bounds b)
	{
		Interval a_x = new Interval(a.min.x, a.max.x);
		Interval a_y = new Interval(a.min.y, a.max.y);
		Interval a_z = new Interval(a.min.z, a.max.z);
		
		Interval b_x = new Interval(b.min.x, b.max.x);
		Interval b_y = new Interval(b.min.y, b.max.y);
		Interval b_z = new Interval(b.min.z, b.max.z);
		
		if(!Interval.CheckOverlap(a_x, b_x)) return false;
		if(!Interval.CheckOverlap(a_y, b_y)) return false;
		if(!Interval.CheckOverlap(a_z, b_z)) return false;
		
		return true;
	}
	
	
	public static Vector3 CheckRoomTouch(Bounds a, Bounds b)
	{
		return CheckRoomTouch(a, b, 0);
	}
	
	//If touching, returns unit vector pointing from A towards B
	//else returns 0,0,0
	public static Vector3 CheckRoomTouch(Bounds a, Bounds b, float minOverlap)
	{
		Interval a_x = new Interval(a.min.x, a.max.x);
		Interval a_y = new Interval(a.min.y, a.max.y);
		Interval a_z = new Interval(a.min.z, a.max.z);
		
		Interval b_x = new Interval(b.min.x, b.max.x);
		Interval b_y = new Interval(b.min.y, b.max.y);
		Interval b_z = new Interval(b.min.z, b.max.z);
		
		int roomTouchX = Interval.CheckTouching(a_x, b_x);
		int roomTouchY = Interval.CheckTouching(a_y, b_y);
		int roomTouchZ = Interval.CheckTouching(a_z, b_z);
		
		bool roomOverlapX = Interval.CheckOverlap(a_x, b_x, minOverlap);
		bool roomOverlapY = Interval.CheckOverlap(a_y, b_y, minOverlap);
		bool roomOverlapZ = Interval.CheckOverlap(a_z, b_z, minOverlap);
		
		//-.-- -.-- --..
		if(roomOverlapX && roomOverlapY) return Vector3.forward * roomTouchZ;
		if(roomOverlapZ && roomOverlapY) return Vector3.right * roomTouchX;
		if(roomOverlapX && roomOverlapZ) return Vector3.up * roomTouchY;
		
		return Vector3.zero;
	}
	
	public static Bounds OverlapBounds(Bounds a, Bounds b)
	{
		Vector3 min = Vector3.zero;
		Vector3 max = Vector3.zero;
		for(int i = 0; i < 3; i ++)
		{
			Interval a_int = new Interval(a.min[i], a.max[i]);
			Interval b_int = new Interval(b.min[i], b.max[i]);
			Interval overlap = Interval.GetOverlap(a_int, b_int);
			if(overlap.min < overlap.max)
			{
				min[i] = overlap.min;
				max[i] = overlap.max;
			}
			else
			{
				min[i] = a.min[i];
				max[i] = a.max[i];
			}
		}
		Bounds overlapField = new Bounds();
		overlapField.min = min;
		overlapField.max = max;
		return(overlapField);
	}
	
}
