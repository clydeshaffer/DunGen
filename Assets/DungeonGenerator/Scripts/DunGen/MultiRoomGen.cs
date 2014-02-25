using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiRoomGen : MonoBehaviour
{
	LinkedList<RoomNode> allRooms;
	LinkedList<RoomNode> interCorridors;
	public int corridorDepthLimit = 4;
	
	public RoomGenerator[] roomGens;
	
	public RoomStyle interCorridorStyle;
	
	// Use this for initialization
	void Start ()
	{
		allRooms = new LinkedList<RoomNode>();
		interCorridors = new LinkedList<RoomNode>();
		foreach(RoomGenerator rg in roomGens)
		{
			rg.totalBounds.center += rg.transform.position;
			rg.totalBounds.size = Vector3.Scale(rg.totalBounds.size, rg.transform.localScale);
			rg.MakeDungeon();
			Util.LLAppend<RoomNode>(allRooms, rg.roomNodeList);
		}
		ConnectMulti();
		
		foreach(RoomGenerator rg in roomGens)
		{
			RoomGenerator.InstantiateRooms(rg.roomNodeList,rg.roomStyle);
		}
		
		RoomGenerator.InstantiateRooms(interCorridors, interCorridorStyle);
		
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	
	
	void ConnectMulti()
	{
		foreach(RoomGenerator a in roomGens)
		{
			foreach(RoomNode roomA in a.roomNodeList)
			{
				foreach(RoomGenerator b in roomGens)
				{
					if(a != b)
					{
						foreach(RoomNode roomB in b.roomNodeList)
						{
							if(!roomA.isCorridor && !roomB.isCorridor && !roomA.connections.Contains(roomB))
							{
								Vector3 touchDir = RoomNode.CheckRoomTouch(roomA.octTreeNode.boundary,roomB.octTreeNode.boundary);
								if(touchDir != Vector3.zero && ((Random.Range(0,100) < 33) || !roomA.ConnectsTo(roomB)))
								{
									RoomNode.MakeConnections(roomA, roomB);
									foreach(RoomNode c in a.BuildCorridors(roomA, roomB, Vector3.zero, allRooms))
									{
										interCorridors.AddFirst(c);
										allRooms.AddFirst(c);
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
