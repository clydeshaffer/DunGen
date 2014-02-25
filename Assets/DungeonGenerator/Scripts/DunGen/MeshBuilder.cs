using UnityEngine;
using System.Collections;

public class MeshBuilder {
	
	public static GameObject BuildRoom(RoomNode room, RoomStyle roomStyle)
	{
		GameObject boxObj = new GameObject("Room");
		for(int i = 0; i < 6; i ++)
		{
			Vector3 dir = Util.VecEnum(i);
			
			
			Bounds wallBounds = new Bounds(room.roomBounds.center + Vector3.Scale(room.roomBounds.extents, dir),
				Vector3.Scale(room.roomBounds.size, Util.LNot(dir)));
			
			int holeCount = 0;
			Bounds[] tempDoorSet = new Bounds[room.neighbors.Count];
			foreach(RoomNode c in room.neighbors)
			{
				if(dir == RoomNode.CheckRoomTouch(room.roomBounds, c.roomBounds))
				{
					Vector3 newCenter = c.roomBounds.center - Vector3.Scale(c.roomBounds.extents, dir);
					Vector3 newSize = Vector3.Scale(c.roomBounds.size, Util.LNot(dir));
					tempDoorSet[holeCount] = new Bounds(newCenter, newSize);
					holeCount ++;
				}
			}
			
			Bounds[] doorSet = new Bounds[holeCount];
			for(int k = 0; k < holeCount; k++)
			{
				doorSet[k] = tempDoorSet[k];
			}
			
			Mesh wallMesh = BuildWall(wallBounds, doorSet, dir * -1);
			if(wallMesh != null)
			{
				TangentSolver.Solve(wallMesh);
				GameObject wallObj = new GameObject("Wall", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
				wallObj.GetComponent<MeshFilter>().mesh = wallMesh;
				wallObj.transform.parent = boxObj.transform;
				wallObj.transform.localPosition = Vector3.Scale(room.roomBounds.extents, dir);
				wallObj.renderer.material = roomStyle.materials[i];
				wallObj.GetComponent<MeshCollider>().sharedMesh = wallMesh;
			}
		}
		
		return boxObj;
	}
	
	static bool CheckDoors(Vector3 v1, Vector3 v2, Bounds[] doors)
	{
		foreach(Bounds d in doors)
		{
			Interval x = new Interval(d.min.x, d.max.x);
			Interval y = new Interval(d.min.y, d.max.y);
			Interval z = new Interval(d.min.z, d.max.z);
			if(x.ContainsInclusive(v1.x)
				&& y.ContainsInclusive(v1.y)
				&& z.ContainsInclusive(v1.z)
				&& x.ContainsInclusive(v2.x)
				&& y.ContainsInclusive(v2.y)
				&& z.ContainsInclusive(v2.z)) return true;
		}
		return false;
	}
	
	static bool TestVecRange(Vector3 v, Interval allowRange)
	{
		float val = Util.VecSig(v);
		return val >= allowRange.min && val <= allowRange.max;
	}
	
	//Bounds given should be zero units wide on exactly dimension
	//They should all share the same zero-dimension
	static Mesh BuildWall(Bounds wall, Bounds[] doorways, Vector3 wallNormal)
	{
		if(doorways == null) doorways = new Bounds[0];
		
		for(int iDoor = 0; iDoor < doorways.Length; iDoor ++)
		{
			Vector3 newMin = Vector3.zero;
			Vector3 newMax = Vector3.zero;
			for(int i = 0; i < 3; i ++)
			{
				newMax[i] = Mathf.Min(doorways[iDoor].max[i], wall.max[i]);
				newMin[i] = Mathf.Max(doorways[iDoor].min[i], wall.min[i]);
			}
			//doorways[iDoor].max = newMax;
			//doorways[iDoor].min = newMin;
			doorways[iDoor].size += Util.LNot(wall.size);
		}
		
		int vcount = 2;//2 + 2 * doorways.Length;
		int hcount = 2;//vcount;
		
		Vector3 offset = wall.center;
		wall.center -= offset;
		
		Vector3[] axes = Util.VecBreak(wall.size);
		if(axes.Length < 2) return null;
		Vector3 horiz = axes[0].normalized;
		Vector3 verti = axes[1].normalized;
		
		VecSortTree hComps = new VecSortTree();
		VecSortTree vComps = new VecSortTree();
		
		Interval hRange = new Interval(Util.VecSum(Vector3.Scale(wall.min, horiz)), Util.VecSum(Vector3.Scale(wall.max, horiz)));
		Interval vRange = new Interval(Util.VecSum(Vector3.Scale(wall.min, verti)), Util.VecSum(Vector3.Scale(wall.max, verti)));
		
		hComps.Push(Vector3.Scale(wall.min, horiz));
		vComps.Push(Vector3.Scale(wall.min, verti));
		hComps.Push(Vector3.Scale(wall.max, horiz));
		vComps.Push(Vector3.Scale(wall.max, verti));
		foreach(Bounds door in doorways)
		{
			if(hComps.Push(Vector3.Scale(door.min - offset, horiz), hRange)) hcount++;
			
			if(hComps.Push(Vector3.Scale(door.max - offset, horiz), hRange)) hcount++;
			
			if(vComps.Push(Vector3.Scale(door.min - offset, verti), vRange)) vcount++;
			
			if(vComps.Push(Vector3.Scale(door.max - offset, verti), vRange)) vcount++;
		}
		
		Vector3[] hArray = new Vector3[hcount];
		Vector3[] vArray = new Vector3[vcount];
		
		for(int i = 0; i < vcount; i ++)
		{
			vArray[i] = vComps.Pop();
			if(vArray[i].x == 11030)
			{
				//throw new Exception();
			}
		}
		
		for(int i = 0; i < hcount; i ++)
		{
			hArray[i] = hComps.Pop();
		}
		
		Vector3[] vertices = new Vector3[vcount * hcount];
		Vector2[] uvs = new Vector2[vcount * hcount];
		int iVert = 0;
		
		for(int i = 0; i < vcount; i ++)
		{
			for(int j = 0; j < hcount; j++)
			{
				vertices[iVert] = hArray[j] + vArray[i];
				uvs[iVert] = new Vector2(Util.VecSum(hArray[j]),
					Util.VecSum(vArray[i]));
				iVert++;
			}
		}
		
		Vector3[] norms = new Vector3[vcount * hcount];
		iVert = 0;
		for(int i = 0; i < vcount; i ++)
		{
			for(int j = 0; j < hcount; j++)
			{
				norms[iVert] = wallNormal;
				iVert++;
			}
		}
		
		Mesh newMesh = new Mesh();
		newMesh.vertices = vertices;
		newMesh.uv = uvs;
		newMesh.normals = norms;
		
		int[] triList = new int[(hcount - 1) * (vcount - 1) * 6];
		
		int iTri = 0;
		
		int p = 0;
		int q = 2;
		
		bool invert = Util.VecSig(wallNormal) > 0;
		if(wallNormal == Vector3.forward || wallNormal == Vector3.forward * -1) invert = !invert;
		if(invert)
		{
			p = 2;
			q = 0;
		}
		
		for(int i = 0; i < vcount - 1; i ++)
		{
			for(int j = 0; j < hcount - 1; j ++)
			{
				int v = i * hcount + j;
				bool makeTriangle = !CheckDoors(vertices[v + hcount] + offset,vertices[v + 1] + offset, doorways);
				if(makeTriangle)
				{
					triList[iTri * 3 + p] = v;
					triList[iTri * 3 + 1] = v + 1;
					triList[iTri * 3 + q] = v + hcount;
					iTri++;
					triList[iTri * 3 + p] = v + 1;
					triList[iTri * 3 + 1] = v + 1 + hcount;
					triList[iTri * 3 + q] = v + hcount;
					iTri++;
				}
			}
		}
		
		newMesh.triangles = triList;
		
		return newMesh;
	}
}
