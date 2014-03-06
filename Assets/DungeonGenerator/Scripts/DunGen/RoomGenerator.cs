using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{

		public Vector3 minRoomSize = Vector3.one;
		public Vector3 maxRoomSize = Vector3.one * 2;
		public float minSeparation = 1;
		public int corridorSize;
		public Bounds totalBounds;
		public int depth;
		public int corridorDepthLimit = 4;
		public OctTree root;
		public bool showOctTree = false;
		public bool showRoomConnections = false;
		public bool regen = false;
		public LinkedList<RoomNode> roomNodeList = null;
		public GameObject indicatorPrefab;
		public GameObject doorPiecePrefab;
		public RoomStyle roomStyle;
		public int setSeed = 0;
	
		public Vector3 TowardsBoxDir (Vector3 start, Bounds target)
		{
				float[] dirs = new float[6];
		
				dirs [0] = Mathf.Abs (start.x - target.min.x);
				dirs [1] = Mathf.Abs (start.y - target.min.y);
				dirs [2] = Mathf.Abs (start.z - target.min.z);
				dirs [3] = Mathf.Abs (start.x - target.max.x);
				dirs [4] = Mathf.Abs (start.y - target.max.y);
				dirs [5] = Mathf.Abs (start.z - target.max.z);
		
				if (start.x > target.min.x)
						dirs [0] = Mathf.Infinity;
				if (start.y > target.min.y)
						dirs [1] = Mathf.Infinity;
				if (start.z > target.min.z)
						dirs [2] = Mathf.Infinity;
				if (start.x < target.max.x)
						dirs [3] = Mathf.Infinity;
				if (start.y < target.max.y)
						dirs [4] = Mathf.Infinity;
				if (start.z < target.max.z)
						dirs [5] = Mathf.Infinity;
		
				int leastIndex = 0;
				for (int i = 0; i < 6; i ++)
						if (dirs [i] < dirs [leastIndex])
								leastIndex = i;
		
				Vector3 result = Vector3.zero;
				result [leastIndex % 3] = 1;
				if (leastIndex > 2)
						result *= -1;
				return result;
		}
	
		public Vector3 NearestWallDir (Bounds start, Bounds target)
		{
				float[] dirs = new float[6];
				dirs [0] = Mathf.Abs (start.max.x - target.min.x);
				dirs [1] = Mathf.Abs (start.max.y - target.min.y);
				dirs [2] = Mathf.Abs (start.max.z - target.min.z);
				dirs [3] = Mathf.Abs (start.min.x - target.max.x);
				dirs [4] = Mathf.Abs (start.min.y - target.max.y);
				dirs [5] = Mathf.Abs (start.min.z - target.max.z);
		
				int leastIndex = 0;
				for (int i = 0; i < 6; i ++)
						if (dirs [i] < dirs [leastIndex])
								leastIndex = i;
		
				Vector3 result = Vector3.zero;
				result [leastIndex % 3] = -1;
				if (leastIndex > 2)
						result *= -1;
				return result;
		}
	
		//Gets the axis direction closest to the direction from A to B
		public Vector3 ManhattanDir (Vector3 pointA, Vector3 pointB)
		{
				Vector3 unitDiff = (pointB - pointA).normalized;
				float[] dirs = new float[6];
				dirs [0] = Vector3.Dot (unitDiff, Vector3.right);
				dirs [1] = Vector3.Dot (unitDiff, Vector3.up);
				dirs [2] = Vector3.Dot (unitDiff, Vector3.forward);
				dirs [3] = Vector3.Dot (unitDiff, Vector3.right * -1);
				dirs [4] = Vector3.Dot (unitDiff, Vector3.up * -1);
				dirs [5] = Vector3.Dot (unitDiff, Vector3.forward * -1);
		
				int mostIndex = 0;
				for (int i = 0; i < 6; i ++)
						if (dirs [i] > dirs [mostIndex])
								mostIndex = i;
		
				Vector3 result = Vector3.zero;
				result [mostIndex % 3] = 1;
				if (mostIndex > 2)
						result *= -1;
				return result;
		}
	
		static RoomNode GetOverlapped (Bounds room, LinkedList<RoomNode> existing, Vector3 corridorDir)
		{
				RoomNode firstHit = null;
				float firstHitDist = Mathf.Infinity;
				foreach (RoomNode e in existing) {
						if (RoomNode.CheckOverlap (room, e.roomBounds)) {
								if (firstHit == null) {
										Vector3 hitWall = e.roomBounds.center - Vector3.Scale (e.roomBounds.extents, corridorDir);
										float hitCoord = Util.VecSum (Vector3.Scale (hitWall, Util.VecAbs (corridorDir)));
										firstHit = e;
										firstHitDist = hitCoord;
								} else {
										Vector3 hitWall = e.roomBounds.center - Vector3.Scale (e.roomBounds.extents, corridorDir);
										float hitCoord = Util.VecSum (Vector3.Scale (hitWall, Util.VecAbs (corridorDir)));
										if (hitCoord < firstHitDist) {
												firstHitDist = hitCoord;
												firstHit = e;
										}
								}
						}
				}
		
				return firstHit;
		}

		//Returns a list of newly created rooms
		public LinkedList<RoomNode> BuildCorridors (RoomNode roomA, RoomNode roomB, Vector3 lastDir, LinkedList<RoomNode> existingRooms)
		{
				if (lastDir == Vector3.zero)
						Debug.Log ("Attempting to build new corridor");
				if (roomA == roomB)
						return new LinkedList<RoomNode> ();
				int attempts = 8;
				while (attempts > 0) {
						Vector3 overlapAxes = RoomNode.OverlapAxes (roomA.roomBounds, roomB.roomBounds);
						int numOverlaps = Util.VecNonZeroes (overlapAxes);
						int selectedAxis = Random.Range (0, 3);
						if (overlapAxes == Vector3.one)
								return new LinkedList<RoomNode> ();
						while (overlapAxes[selectedAxis] != 0)
								selectedAxis = Random.Range (0, 3);
						Vector3 corridorDir = Vector3.zero;
						corridorDir [selectedAxis] = Mathf.Sign (roomB.roomBounds.center [selectedAxis] - roomA.roomBounds.center [selectedAxis]);
						//Debug.Log ("Connect " + numOverlaps);
						if (numOverlaps == 2) {
								LinkedList<RoomNode> result = new LinkedList<RoomNode> ();
								Bounds b = RoomNode.OverlapBounds (roomA.roomBounds, roomB.roomBounds);
								RoomNode r = new RoomNode (b);
								Vector3 randomAxes = Vector3.one;
								randomAxes [selectedAxis] = 0;
								if (lastDir != Vector3.zero) {
										r.ShoveToEdge (lastDir, Vector3.one * 0.5f * corridorSize);
								} else
										r.RandomizeBounds (randomAxes, corridorSize * Vector3.one, corridorSize * Vector3.one);
								int limit = 12;
								while (GetOverlapped(r.roomBounds, existingRooms,corridorDir) != null && limit > 0) {
										r.roomBounds = b;
										r.RandomizeBounds (randomAxes, corridorSize * Vector3.one, corridorSize * Vector3.one);
										limit --;
								}
								if (limit == 0) {
										//Debug.Log ("OH GOD DAMMIT " + r.roomBounds.ToString ());
										//Instantiate (indicatorPrefab, r.roomBounds.center, Quaternion.identity);
								} else {
										r.isCorridor = true;
										RoomNode.MakeNeighbors (roomA, r);
										RoomNode.MakeNeighbors (r, roomB);
										r.name = numOverlaps.ToString ();
										result.AddFirst (r);

										return result;
								}
						} else if (numOverlaps < 2) { 
								Bounds b = RoomNode.OverlapBounds (roomA.roomBounds, roomB.roomBounds);
								int selectedOther = Random.Range (0, 3);
								while (selectedOther == selectedAxis || overlapAxes[selectedOther] != 0) {
										selectedOther = Random.Range (0, 3);
								}
								Vector3 newCenter;
								Vector3 newExtents;
								newCenter = b.center;
								newExtents = b.extents;
								if (numOverlaps == 1) {
										newCenter [selectedOther] = roomA.roomBounds.center [selectedOther];
										newExtents [selectedOther] = roomA.roomBounds.extents [selectedOther];
								} else if (numOverlaps == 0) {
										Debug.Log ("Connecting Zero");
										newCenter = roomA.roomBounds.center;
										newExtents = roomA.roomBounds.extents;
										newCenter [selectedAxis] = b.center [selectedAxis];
										newExtents [selectedAxis] = b.extents [selectedAxis];
								}
								float sign = Mathf.Sign (newCenter [selectedAxis] - roomA.roomBounds.center [selectedAxis]);
								float extension = Random.Range (corridorSize, roomB.roomBounds.extents [selectedAxis]);
								newCenter [selectedAxis] += extension * sign;
								newExtents [selectedAxis] += extension;
								b.center = newCenter;
								b.extents = newExtents;
								RoomNode r = new RoomNode (b);
								Vector3 randomAxes = Vector3.one;
								randomAxes [selectedAxis] = 0;
								if (lastDir != Vector3.zero) {
										r.ShoveToEdge (lastDir, Vector3.one * 0.5f * corridorSize);
								} else
										r.RandomizeBounds (randomAxes, corridorSize * Vector3.one, corridorSize * Vector3.one);
								int limit = 12;
								while (GetOverlapped(r.roomBounds, existingRooms,corridorDir) != null && limit > 0) {
										b = RoomNode.OverlapBounds (roomA.roomBounds, roomB.roomBounds);
										newCenter = b.center;
										newExtents = b.extents;
										if (numOverlaps == 1) {
												newCenter [selectedOther] = roomA.roomBounds.center [selectedOther];
												newExtents [selectedOther] = roomA.roomBounds.extents [selectedOther];
										} else if (numOverlaps == 0) {
												newCenter = roomA.roomBounds.center;
												newExtents = roomA.roomBounds.extents;
												newCenter [selectedAxis] = b.center [selectedAxis];
												newExtents [selectedAxis] = b.extents [selectedAxis];
										}
										
										extension = Random.Range (corridorSize, roomB.roomBounds.extents [selectedAxis]);
										newCenter [selectedAxis] += extension * sign;
										newExtents [selectedAxis] += extension;
										b.center = newCenter;
										b.extents = newExtents;
										r.roomBounds = b;
										r.RandomizeBounds (randomAxes, corridorSize * Vector3.one, corridorSize * Vector3.one);
										limit --;
								}
								if (limit == 0) {
										//Debug.Log ("Aw fuck");
				
								} else {
										LinkedList<RoomNode> result = BuildCorridors (r, roomB, corridorDir, existingRooms);
										if (result.Count != 0) {	
												r.isCorridor = true;
												RoomNode.MakeNeighbors (roomA, r);
												r.name = numOverlaps.ToString ();
												result.AddFirst (r);
												return result;
										}
								}
						} 
						attempts--;
				}

				if (lastDir == Vector3.zero)
						Debug.Log ("Couldn't Connect");
				return new LinkedList<RoomNode> ();

		}
	
		public void GenerateRooms (OctTree tree)
		{
				if (tree.GetChildCount () != 0) {
						foreach (OctTree subTree in tree.children) {
								if (subTree != null) {
										GenerateRooms (subTree);
								}
						}
				} else {
						RoomNode newRoom = new RoomNode (tree.boundary);
						newRoom.roomBounds.extents -= Vector3.one * minSeparation;
						newRoom.RandomizeBounds (Vector3.one, minRoomSize, maxRoomSize);
						newRoom.QuantizeBounds (corridorSize);
						/*Vector3 center, extents;
			
						extents.x = Mathf.Floor (Random.Range (minRoomSize / 2, tree.boundary.extents.x));
						extents.y = Mathf.Floor (Random.Range (minRoomSize / 2, tree.boundary.extents.y));
						extents.z = Mathf.Floor (Random.Range (minRoomSize / 2, tree.boundary.extents.z));
			
						center = tree.boundary.center;
						center.x = Mathf.Round (center.x + Random.Range (extents.x - tree.boundary.extents.x, tree.boundary.extents.x - extents.x));
						center.y = Mathf.Round (center.y + Random.Range (extents.y - tree.boundary.extents.y, tree.boundary.extents.y - extents.y));
						center.z = Mathf.Round (center.z + Random.Range (extents.z - tree.boundary.extents.z, tree.boundary.extents.z - extents.z));
			
						newRoom.roomBounds.center = center;
						newRoom.roomBounds.extents = extents;*/

						newRoom.octTreeNode = tree;
						roomNodeList.AddFirst (newRoom);
						tree.roomNode = newRoom;
				}
		}
	
		public LinkedList<RoomNode> ConnectTwoRooms (RoomNode roomA, RoomNode roomB, LinkedList<RoomNode> existing)
		{
				LinkedList<RoomNode> existingCopy = new LinkedList<RoomNode> ();
				Util.LLAppend<RoomNode> (existingCopy, existing);
				return BuildCorridors (roomA, roomB, Vector3.zero, existingCopy);
		}
	
		void ConnectRooms ()
		{
				LinkedList<RoomNode> newCorridors = new LinkedList<RoomNode> ();
				LinkedList<RoomNode> existingRooms = new LinkedList<RoomNode> ();
				Util.LLAppend<RoomNode> (existingRooms, roomNodeList);
				foreach (RoomNode roomA in roomNodeList) {
						foreach (RoomNode roomB in roomNodeList) {
								if (roomA != roomB && !roomA.connections.Contains (roomB)) {
										Vector3 touchDir = RoomNode.CheckRoomTouch (roomA.octTreeNode.boundary, roomB.octTreeNode.boundary);
										if (touchDir != Vector3.zero && ((Random.Range (0, 100) < 50) || !roomA.ConnectsTo (roomB))) {
												RoomNode.MakeConnections (roomA, roomB);
												foreach (RoomNode c in ConnectTwoRooms(roomA, roomB, existingRooms)) {
														newCorridors.AddFirst (c);
														existingRooms.AddFirst (c);
												}
										}
								}
						}
				}
		
				foreach (RoomNode c in newCorridors)
						roomNodeList.AddFirst (c);
		}
	
		static RoomNode GetNearestInstantiatedRoom (RoomNode start)
		{
				LinkedList<RoomNode> roomsToCheck = new LinkedList<RoomNode> ();
				LinkedList<RoomNode> nextRoomsToCheck = new LinkedList<RoomNode> ();
				roomsToCheck.AddFirst (start);
				int visitMarker = Random.Range (int.MinValue, int.MaxValue);
				while (roomsToCheck.Count > 0) {
						foreach (RoomNode room in roomsToCheck) {
								if (room.roomObject != null)
										return room;
				
								room.visited = visitMarker;
								foreach (RoomNode n in room.neighbors)
										if (n.visited != visitMarker)
												nextRoomsToCheck.AddFirst (n);
						}
						roomsToCheck = nextRoomsToCheck;
						nextRoomsToCheck = new LinkedList<RoomNode> ();
				}
				return null;
		}
	
		public static void InstantiateRooms (LinkedList<RoomNode> roomList, RoomStyle rms)
		{
				foreach (RoomNode room in roomList) {
			
						GameObject newBox = MeshBuilder.BuildRoom (room, rms);
						newBox.transform.position = room.roomBounds.center;
						room.roomObject = newBox;
				}
		}

		public static void FurnishRooms (LinkedList<RoomNode> roomList, RoomStyle rmstyle, GameObject doorConnector)
		{
			foreach (RoomNode room in roomList) {
				if(room.isCorridor == false)
				{
					PlatformAssembler asm = room.roomObject.AddComponent<PlatformAssembler>();
					asm.placeableObjects = rmstyle.placeablePlatforms;
					LinkedList<PlatformingElement> doors = new LinkedList<PlatformingElement>();
					foreach(RoomNode neighbor in room.neighbors)
					{
						Bounds overl = RoomNode.OverlapBounds(room.roomBounds, neighbor.roomBounds);
						if(overl.extents.y > 0.01f)
						{
							GameObject newDoorPiece = (GameObject) Instantiate(doorConnector, overl.center - Vector3.Scale(Vector3.up, overl.extents), Quaternion.LookRotation(RoomNode.CheckRoomTouch(neighbor.roomBounds, room.roomBounds)));
							doors.AddFirst(newDoorPiece.GetComponent<PlatformingElement>());
						}
						else
						{
							GameObject newDoorPiece = (GameObject) Instantiate(doorConnector, overl.center, Quaternion.LookRotation(RoomNode.CheckRoomTouch(neighbor.roomBounds, room.roomBounds)));
							doors.AddFirst(newDoorPiece.GetComponent<PlatformingElement>());
						}
					}
					if(doors.Count > 1) {
						PlatformingElement[] starts = new PlatformingElement[doors.Count];
						int doorind = 0;
						foreach(PlatformingElement pe in doors)
						{
							starts[doorind++] = pe;
						}
						asm.ConnectDoorPieces(starts);
					}
				}
			}
		}

		public void MakeDungeon ()
		{
				root = new OctTree (totalBounds);
				root.GenerateZones (depth, minRoomSize);
				roomNodeList = new LinkedList<RoomNode> ();
				GenerateRooms (root);
				ConnectRooms ();
		}
	
	
		// Use this for initialization
		void Start ()
		{
		}
	
		// Update is called once per frame
		void Update ()
		{
				if (regen) {
						System.DateTime timestamp = System.DateTime.Now;
						if (setSeed != 0)
								Random.seed = setSeed;
						regen = false;
						if (roomNodeList != null)
								foreach (RoomNode room in roomNodeList) {
										Destroy (room.roomObject);
								}
						MakeDungeon ();
						InstantiateRooms (roomNodeList, roomStyle);
						FurnishRooms (roomNodeList, roomStyle, doorPiecePrefab);
						Debug.Log ("Finished in " + (System.DateTime.Now - timestamp).TotalMilliseconds + " ms");
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
						Gizmos.DrawLine(room.roomBounds.center + Vector3.Cross(diff, cam),neighbor.roomBounds.center);
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
