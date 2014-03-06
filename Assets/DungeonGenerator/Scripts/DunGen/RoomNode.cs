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

	public string name = "Room";
	
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
	
	public static Vector3 OverlapAxes(Bounds a, Bounds b, float minOverlap = 0)
	{
		
		Vector3 result = Vector3.zero;
		
		for(int i = 0; i < 3; i++)
		{
			Interval a_ = new Interval(a.min[i], a.max[i]);
			Interval b_ = new Interval(b.min[i], b.max[i]);
			if(Interval.CheckOverlap(a_, b_, minOverlap)) result[i] = 1;
		}
		
		return result;
	}
	
	public static bool CheckOverlap(Bounds a, Bounds b, float minOverlap = 0)
	{
		Interval a_x = new Interval(a.min.x, a.max.x);
		Interval a_y = new Interval(a.min.y, a.max.y);
		Interval a_z = new Interval(a.min.z, a.max.z);
		
		Interval b_x = new Interval(b.min.x, b.max.x);
		Interval b_y = new Interval(b.min.y, b.max.y);
		Interval b_z = new Interval(b.min.z, b.max.z);
		
		if(!Interval.CheckOverlap(a_x, b_x, minOverlap)) return false;
		if(!Interval.CheckOverlap(a_y, b_y, minOverlap)) return false;
		if(!Interval.CheckOverlap(a_z, b_z, minOverlap)) return false;
		
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

	public void RandomizeBounds(Vector3 mask, Vector3 minSize, Vector3 maxSize)
	{
		for (int i = 0; i < 3; i ++)
		{
			if(mask[i] != 0) RandomizeBoundsOnAxis(i, minSize[i], maxSize[i]);
		}
	}

	public void QuantizeBounds(float increment = 1)
	{
		for (int i = 0; i < 3; i ++)
		{
			roomBounds.min = Util.VecRound(roomBounds.min, increment);
			roomBounds.max = Util.VecRound(roomBounds.max, increment);
		}
	}

	//direction -- a vector where each component is in the set {-1 , 0, 1}
	//newExtents -- extents of resulting bounds
	public void ShoveToEdge(Vector3 direction, Vector3 newExtents)
	{
		roomBounds.center += Vector3.Scale (direction, roomBounds.extents);
		roomBounds.center -= Vector3.Scale(direction, newExtents);
		Vector3 ex = roomBounds.extents;
		for (int i = 0; i < 3; i ++)
		{
			if(direction[i] != 0)
			{
				ex[i] = newExtents[i];
			}
		}
		roomBounds.extents = ex;
	}

	public void RandomizeBoundsOnAxis(int axisIndex, float minSize, float maxSize)
	{
		Interval axisInterval = new Interval (roomBounds.min [axisIndex], roomBounds.max [axisIndex]);
		Interval newInterval = axisInterval.randomSub (minSize, maxSize);
		Vector3 newMin = roomBounds.min;
		Vector3 newMax = roomBounds.max;
		newMin [axisIndex] = newInterval.min;
		newMax [axisIndex] = newInterval.max;
		roomBounds.min = newMin;
		roomBounds.max = newMax;
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
				min[i] = overlap.max;
				max[i] = overlap.min;
			}
		}
		Bounds overlapField = new Bounds();
		overlapField.min = min;
		overlapField.max = max;
		return(overlapField);
	}
	
}
