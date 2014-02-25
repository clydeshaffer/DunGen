using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour {

	public int minRoomSize;
	
	public int corridorSize;
	
	public Bounds totalBounds;
	
	public int depth;
	
	public int corridorDepthLimit = 4;
	
	public OctTree root;
	
	public bool showOctTree = false;
	public bool showRoomConnections = false;
	
	public bool regen = false;
	
	public LinkedList<RoomNode> roomNodeList = null;
	
	public GameObject roomObj;
	public static GameObject roomObjStatic;
	
	public GameObject indicatorPrefab;
	
	public RoomStyle roomStyle;
	
	public Vector3 TowardsBoxDir(Vector3 start, Bounds target)
	{
		float[] dirs = new float[6];
		
		dirs[0] = Mathf.Abs(start.x - target.min.x);
		dirs[1] = Mathf.Abs(start.y - target.min.y);
		dirs[2] = Mathf.Abs(start.z - target.min.z);
		dirs[3] = Mathf.Abs(start.x - target.max.x);
		dirs[4] = Mathf.Abs(start.y - target.max.y);
		dirs[5] = Mathf.Abs(start.z - target.max.z);
		
		if(start.x > target.min.x) dirs[0] = Mathf.Infinity;
		if(start.y > target.min.y) dirs[1] = Mathf.Infinity;
		if(start.z > target.min.z) dirs[2] = Mathf.Infinity;
		if(start.x < target.max.x) dirs[3] = Mathf.Infinity;
		if(start.y < target.max.y) dirs[4] = Mathf.Infinity;
		if(start.z < target.max.z) dirs[5] = Mathf.Infinity;
		
		int leastIndex = 0;
		for(int i = 0; i < 6; i ++) if(dirs[i] < dirs[leastIndex]) leastIndex = i;
		
		Vector3 result = Vector3.zero;
		result[leastIndex % 3] = 1;
		if(leastIndex > 2) result *= -1;
		return result;
	}
	
	public Vector3 NearestWallDir(Bounds start, Bounds target)
	{
		float[] dirs = new float[6];
		dirs[0] = Mathf.Abs(start.max.x - target.min.x);
		dirs[1] = Mathf.Abs(start.max.y - target.min.y);
		dirs[2] = Mathf.Abs(start.max.z - target.min.z);
		dirs[3] = Mathf.Abs(start.min.x - target.max.x);
		dirs[4] = Mathf.Abs(start.min.y - target.max.y);
		dirs[5] = Mathf.Abs(start.min.z - target.max.z);
		
		int leastIndex = 0;
		for(int i = 0; i < 6; i ++) if(dirs[i] < dirs[leastIndex]) leastIndex = i;
		
		Vector3 result = Vector3.zero;
		result[leastIndex % 3] = -1;
		if(leastIndex > 2) result *= -1;
		return result;
	}
	
	//Gets the axis direction closest to the direction from A to B
	public Vector3 ManhattanDir(Vector3 pointA, Vector3 pointB)
	{
		Vector3 unitDiff = (pointB - pointA).normalized;
		float[] dirs = new float[6];
		dirs[0] = Vector3.Dot(unitDiff, Vector3.right);
		dirs[1] = Vector3.Dot(unitDiff, Vector3.up);
		dirs[2] = Vector3.Dot(unitDiff, Vector3.forward);
		dirs[3] = Vector3.Dot(unitDiff, Vector3.right * -1);
		dirs[4] = Vector3.Dot(unitDiff, Vector3.up * -1);
		dirs[5] = Vector3.Dot(unitDiff, Vector3.forward * -1);
		
		int mostIndex = 0;
		for(int i = 0; i < 6; i ++) if(dirs[i] > dirs[mostIndex]) mostIndex = i;
		
		Vector3 result = Vector3.zero;
		result[mostIndex % 3] = 1;
		if(mostIndex > 2) result *= -1;
		return result;
	}
	
	static RoomNode GetOverlapped(Bounds room, LinkedList<RoomNode> existing, Vector3 corridorDir)
	{
		RoomNode firstHit = null;
		float firstHitDist = Mathf.Infinity;
		foreach(RoomNode e in existing)
		{
			if(RoomNode.CheckOverlap(room, e.roomBounds))
			{
				if(firstHit == null)
				{
					Vector3 hitWall = e.roomBounds.center - Vector3.Scale(e.roomBounds.extents, corridorDir);
					float hitCoord = Util.VecSum(Vector3.Scale(hitWall, Util.VecAbs(corridorDir)));
					firstHit = e;
					firstHitDist = hitCoord;
				}
				else
				{
					Vector3 hitWall = e.roomBounds.center - Vector3.Scale(e.roomBounds.extents, corridorDir);
					float hitCoord = Util.VecSum(Vector3.Scale(hitWall, Util.VecAbs(corridorDir)));
					if(hitCoord < firstHitDist)
					{
						firstHitDist = hitCoord;
						firstHit = e;
					}
				}
			}
		}
		
		return firstHit;
	}
	
	//Connects two rooms, creating corridor rooms if needed
	//Goes recursiveley from A to B
	//Builds the "first" new corridor starting from as far in "direction" as possible
	//Normally just use <0,0,0> to start
	//Updates the "neighbors" 
	//Returns a list of newly created rooms
	public LinkedList<RoomNode> BuildCorridors(RoomNode roomA, RoomNode roomB, Vector3 lastDir, LinkedList<RoomNode> existingRooms)
	{
		if(roomA == roomB) return new LinkedList<RoomNode>();
		Vector3 overlapAxes = RoomNode.OverlapAxes(roomA.roomBounds, roomB.roomBounds);
		int selectedAxis = Random.Range(0, 3);
		if(overlapAxes == Vector3.one) return new LinkedList<RoomNode>();
		while(overlapAxes[selectedAxis] != 0) selectedAxis = Random.Range(0, 3);
		Vector3 corridorDir = Vector3.zero;
		corridorDir[selectedAxis] = Mathf.Sign(roomB.roomBounds.center[selectedAxis] - roomA.roomBounds.center[selectedAxis]);
		Bounds overlapField = RoomNode.OverlapBounds(roomA.roomBounds, roomB.roomBounds);
		Vector3 corridorStart = overlapField.center + Util.VecRound(Vector3.Scale(overlapField.extents - overlapAxes * corridorSize, Util.RandomInWidthTwoCube()));
		corridorStart[selectedAxis] = roomA.roomBounds.center[selectedAxis] + roomA.roomBounds.extents[selectedAxis] * corridorDir[selectedAxis];
		float corridorLen = roomB.roomBounds.center[selectedAxis] - roomB.roomBounds.extents[selectedAxis] * corridorDir[selectedAxis];
		corridorLen += (corridorSize + Mathf.FloorToInt(Random.Range(0, roomB.roomBounds.extents[selectedAxis]))) * corridorDir[selectedAxis];
		Vector3 corridorEnd = corridorStart;
		corridorEnd[selectedAxis] = corridorLen;
		corridorLen = Mathf.Abs(corridorStart[selectedAxis] - corridorEnd[selectedAxis]);
		Vector3 newSizeVec = Vector3.one * corridorSize;
		newSizeVec[selectedAxis] = corridorLen;
		Bounds newRoomBounds = new Bounds((corridorStart + corridorEnd) / 2, newSizeVec);
		RoomNode collision = GetOverlapped(newRoomBounds, existingRooms, corridorDir);
		if(collision != null)
		{
			if(RoomNode.CheckRoomTouch(roomA.roomBounds, collision.roomBounds) != Vector3.zero)
			{
				if(!roomA.neighbors.Contains(collision))
				{
					RoomNode.MakeNeighbors(roomA, collision);
				}
				return BuildCorridors(collision,roomB, corridorDir, existingRooms);
			}
			corridorLen = collision.roomBounds.center[selectedAxis] - collision.roomBounds.extents[selectedAxis] * corridorDir[selectedAxis];
			corridorEnd[selectedAxis] = corridorLen;
			corridorLen = Mathf.Abs(corridorStart[selectedAxis] - corridorEnd[selectedAxis]);
			newSizeVec[selectedAxis] = corridorLen;
			newRoomBounds = new Bounds((corridorStart + corridorEnd) / 2, newSizeVec);		
		}
		RoomNode newRoom = new RoomNode(newRoomBounds);
		RoomNode.MakeNeighbors(roomA, newRoom);
		newRoom.isCorridor = true;
		LinkedList<RoomNode> restOfPath;
		if(collision != null)
		{
			RoomNode.MakeNeighbors(collision, newRoom);
			if(collision == roomB)
			{
				restOfPath = new LinkedList<RoomNode>();
			}
			else
			{
				existingRooms.AddFirst(newRoom);
				restOfPath = BuildCorridors(collision, roomB, corridorDir, existingRooms);
			}
		}
		else
		{
			existingRooms.AddFirst(newRoom);
			restOfPath = BuildCorridors(newRoom, roomB, corridorDir, existingRooms);
		}
		restOfPath.AddFirst(newRoom);
		return restOfPath;
	}
	
	public void GenerateRooms(OctTree tree)
	{
		if(tree.GetChildCount() != 0)
		{
			foreach(OctTree subTree in tree.children)
			{
				if(subTree != null)
				{
					GenerateRooms(subTree);
				}
			}
		}
		else
		{
			RoomNode newRoom = new RoomNode();
			Vector3 center, extents;
			
			extents.x = Mathf.Floor(Random.Range(minRoomSize / 2, tree.boundary.extents.x));
			extents.y = Mathf.Floor(Random.Range(minRoomSize / 2, tree.boundary.extents.y));
			extents.z = Mathf.Floor(Random.Range(minRoomSize / 2, tree.boundary.extents.z));
			
			center = tree.boundary.center;
			center.x = Mathf.Round(center.x + Random.Range(extents.x - tree.boundary.extents.x, tree.boundary.extents.x - extents.x));
			center.y = Mathf.Round(center.y + Random.Range(extents.y - tree.boundary.extents.y, tree.boundary.extents.y - extents.y));
			center.z = Mathf.Round(center.z + Random.Range(extents.z - tree.boundary.extents.z, tree.boundary.extents.z - extents.z));
			
			newRoom.roomBounds.center = center;
			newRoom.roomBounds.extents = extents;
			newRoom.octTreeNode = tree;
			roomNodeList.AddFirst(newRoom);
			tree.roomNode = newRoom;
		}
	}
	
	public LinkedList<RoomNode> ConnectTwoRooms(RoomNode roomA, RoomNode roomB, LinkedList<RoomNode> existing)
	{
		LinkedList<RoomNode> existingCopy = new LinkedList<RoomNode>();
		Util.LLAppend<RoomNode>(existingCopy, existing);
		return BuildCorridors(roomA, roomB, Vector3.zero, existingCopy);
	}
	
	void ConnectRooms()
	{
		LinkedList<RoomNode> newCorridors = new LinkedList<RoomNode>();
		LinkedList<RoomNode> existingRooms = new LinkedList<RoomNode>();
		Util.LLAppend<RoomNode>(existingRooms, roomNodeList);
		foreach(RoomNode roomA in roomNodeList)
		{
			foreach(RoomNode roomB in roomNodeList)
			{
				if(roomA != roomB && !roomA.connections.Contains(roomB))
				{
					Vector3 touchDir = RoomNode.CheckRoomTouch(roomA.octTreeNode.boundary,roomB.octTreeNode.boundary);
					if(touchDir != Vector3.zero && ((Random.Range(0,100) < 50) || !roomA.ConnectsTo(roomB)))
					{
						RoomNode.MakeConnections(roomA, roomB);
						foreach(RoomNode c in ConnectTwoRooms(roomA, roomB, existingRooms))
						{
							newCorridors.AddFirst(c);
							existingRooms.AddFirst(c);
						}
					}
				}
			}
		}
		
		foreach(RoomNode c in newCorridors) roomNodeList.AddFirst(c);
	}
	
	static RoomNode GetNearestInstantiatedRoom(RoomNode start)
	{
		LinkedList<RoomNode> roomsToCheck = new LinkedList<RoomNode>();
		LinkedList<RoomNode> nextRoomsToCheck = new LinkedList<RoomNode>();
		roomsToCheck.AddFirst(start);
		int visitMarker = Random.Range(int.MinValue,int.MaxValue);
		while(roomsToCheck.Count > 0)
		{
			foreach(RoomNode room in roomsToCheck)
			{
				if(room.roomObject != null) return room;
				
				room.visited = visitMarker;
				foreach(RoomNode n in room.neighbors) if(n.visited != visitMarker) nextRoomsToCheck.AddFirst(n);
			}
			roomsToCheck = nextRoomsToCheck;
			nextRoomsToCheck = new LinkedList<RoomNode>();
		}
		return null;
	}
	
	public static void InstantiateRooms(LinkedList<RoomNode> roomList, RoomStyle rms)
	{
		foreach(RoomNode room in roomList)
		{
			
			GameObject newBox = MeshBuilder.BuildRoom(room, rms);
			newBox.transform.position = room.roomBounds.center;
			room.roomObject = newBox;
		}
	}
	
	public void MakeDungeon()
	{
		roomObjStatic = roomObj;
		root = new OctTree(totalBounds);
		root.GenerateZones(depth,minRoomSize);roomNodeList = new LinkedList<RoomNode>();
		GenerateRooms(root);
		ConnectRooms();
	}
	
	
	// Use this for initialization
	void Start ()
	{
		roomObjStatic = roomObj;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(regen)
		{
			regen = false;
			if(roomNodeList != null)
			foreach(RoomNode room in roomNodeList)
			{
				Destroy(room.roomObject);
			}
			MakeDungeon();
			InstantiateRooms(roomNodeList,roomStyle);
		}
	}
	
#if UNITY_EDITOR	
	void OnDrawGizmos ()
	{
		if(roomNodeList != null)
		{
			foreach(RoomNode room in roomNodeList)
			{	
				if(room.octTreeNode != null) Gizmos.color = room.octTreeNode.gizmoColor;
				if(!room.isCorridor)
				Gizmos.DrawWireCube(room.roomBounds.center,room.roomBounds.size);
				if(showRoomConnections)
				{
					foreach(RoomNode neighbor in room.neighbors)
					{
						Gizmos.color = Color.red;
						Gizmos.DrawLine(room.roomBounds.center,neighbor.roomBounds.center);
						Vector3 diff = (neighbor.roomBounds.center - room.roomBounds.center).normalized;
						Vector3 cam = UnityEditor.SceneView.currentDrawingSceneView.camera.transform.forward;
						//Gizmos.DrawLine(room.roomBounds.center + Vector3.Cross(diff, cam),neighbor.roomBounds.center);
					}
				}
 			}
			if(showOctTree) root.DrawGizmoBoxes();
		}
		else
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(totalBounds.center + transform.position, Vector3.Scale(totalBounds.size,transform.localScale));
		}
	}
#endif	
	
	void OnGUI ()
	{
		
	}

	
}
