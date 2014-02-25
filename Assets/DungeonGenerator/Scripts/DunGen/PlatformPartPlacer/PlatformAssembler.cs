using UnityEngine;
using System.Collections;

public class PlatformAssembler : MonoBehaviour {
	
	public PlatformingElement[] placeableObjects;
	
	public PlatformingElement[] initialObjects;
	
	public Bounds limitBounds = new Bounds(Vector3.zero, Vector3.one * 10);
	
	private Bounds adjustedBounds
	{
		get {
			Bounds adjBounds = limitBounds;
			adjBounds.center = transform.TransformPoint(adjBounds.center);
			adjBounds.size = transform.TransformDirection(adjBounds.size);
			return adjBounds;
		}
		
		set {
			limitBounds.center = transform.InverseTransformPoint(value.center);
			limitBounds.size = transform.InverseTransformDirection(value.size);
		}
	}
	
	// Use this for initialization
	void Start () {
		Bounds tempBounds = adjustedBounds;
		tempBounds.Encapsulate(initialObjects[0].transform.position);
		tempBounds.Encapsulate(initialObjects[1].transform.position);
		
		
		adjustedBounds = tempBounds;
		
		float firstTimeStamp = Time.realtimeSinceStartup;
		
		int depth = 1;
		float timeStamp = Time.realtimeSinceStartup;
		while((depth < 7) && !BuildPath(initialObjects[0], initialObjects[1], depth))
		{
			print ("Depth " + depth + " search took " + (Time.realtimeSinceStartup - timeStamp));
			depth ++;
			timeStamp = Time.realtimeSinceStartup;
		}
		print("Finished in " + (Time.realtimeSinceStartup - firstTimeStamp) + " at depth " + depth);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
	bool IsValidPlacement(PlatformingElement existingPiece, int connectorIndex, int newPieceIndex, int newPieceConnectorIndex, int angle)
	{
		if(angle != 0 && !placeableObjects[newPieceIndex].connectors[newPieceConnectorIndex].allowRotationOnConnector) return false;
		Vector3 existingNormal = existingPiece.transform.TransformDirection(existingPiece.connectors[connectorIndex].normal);
		Vector3 newNormal = placeableObjects[newPieceIndex].connectors[newPieceConnectorIndex].normal;
		Vector3 eulers = Quaternion.FromToRotation(newNormal, existingNormal * -1).eulerAngles;
		if(eulers.x == 0 && eulers.z == 0) return true;
		return false;
	}
	
	bool BuildPath(PlatformingElement start, PlatformingElement end, int depth)
	{
		Vector3 temp = Vector3.zero;
		return BuildPath(start, end, depth, 0, out temp, 0);
	}
	
	float StretchPiece(PlatformingElement toStretch, Vector3 displacement, int stretchIndex, int connectorToMove, int connectorToNotMove)
	{
		Vector3 fixedConnectorPos = toStretch.transform.TransformPoint(toStretch.connectors[connectorToNotMove].connectionPoint);
		Vector3 movingConnectorPos = toStretch.transform.TransformPoint(toStretch.connectors[connectorToMove].connectionPoint);
		float currDist = Mathf.Abs(movingConnectorPos[stretchIndex] - fixedConnectorPos[stretchIndex]);
		float newDist = currDist + Mathf.Abs(displacement[stretchIndex]);
		float scaleFactor = newDist / currDist;
		Vector3 localScale = toStretch.transform.TransformDirection(toStretch.transform.localScale);
		localScale[stretchIndex] *= scaleFactor;
		
		toStretch.transform.localScale = toStretch.transform.InverseTransformDirection(localScale);
		
		float result = fixedConnectorPos[stretchIndex] - toStretch.transform.TransformPoint(toStretch.connectors[connectorToNotMove].connectionPoint)[stretchIndex] + toStretch.transform.position[stretchIndex];
		return result;
	}
	
	
	
	//Todo: Make this a breadth first search
	bool BuildPath(PlatformingElement start, PlatformingElement end, int depth, int allowedDisplace, out Vector3 displace, int fixedConnector)
	{
		Vector3 startPos = start.transform.position;
		Quaternion startRot = start.transform.rotation;
		Vector3 startScl = start.transform.localScale;
		
		displace = Vector3.zero;
		if(depth == 0) return false;
		if(!adjustedBounds.Contains(start.transform.position))
		{
			return false;
		}
		
		Vector3 transScaling = Util.VecRound(Util.VecAbs(start.transform.TransformDirection(start.allowedScaling)));
		
		for(int myConnector = 0; myConnector < start.connectors.Length; myConnector++)
		{
			Vector3 transNormal = Util.VecRound(start.transform.TransformDirection(start.connectors[myConnector].normal));
			Vector3 startToEnd = end.transform.position - start.transform.position;
			if(start.connectors[myConnector].connectedNode == null && Vector3.Dot(transNormal, startToEnd) >= 0)
			{
				//Set scaling flags, if any
				int c = 0;
				for(int i=0;i<3;i++) 
				{
					if(transNormal[i] * transScaling[i] != 0)
					{
						if(transNormal[i] > 0)
						{
							allowedDisplace |= 1 << i;
							c++;
						}
						else
						{
							allowedDisplace |= 1 << (i + 3);
							c++;
						}
					}
				}
				
				Vector3 displaceOffset = Vector3.zero;
				Vector2 finalConnection = start.CheckForConnection(end, myConnector, allowedDisplace, out displaceOffset);
				displace = displaceOffset;		
				
				if(finalConnection != Vector2.one * -1)
				{
					start.connectors[Mathf.FloorToInt(finalConnection.x)].connectedNode = end;
					end.connectors[Mathf.FloorToInt(finalConnection.y)].connectedNode = start;
					Vector3 startPlatPos = start.transform.position;
					for(int i=0;i<3;i++)
					{
						if((transNormal[i] * transScaling[i] != 0) && (Mathf.Sign(transNormal[i]) == Mathf.Sign(displace[i])))
						{
							//Do some scaling shit
							startPlatPos[i] = StretchPiece(start, displace, i, myConnector, fixedConnector);
							displace[i] = 0;
						}
						else
						{
							startPlatPos[i] += displace[i];
						}
					}
					start.transform.position = startPlatPos;
					print(start.name + " " + System.Convert.ToString(allowedDisplace,2));
					return true;
				}
			
				for(int partType = 0; partType < placeableObjects.Length; partType++)
				{
					if(!start.dontChainToSame || start.typeID != partType)
					for(int partConn = 0; partConn < placeableObjects[partType].connectors.Length; partConn++)
					{	
						for(int angle = 0; angle < 360; angle += 90)
						{
							if(IsValidPlacement(start, myConnector, partType, partConn, angle))
							{
								PlatformingElement newlyPlaced = PlacePiece(start, myConnector, partType, partConn, angle);
								bool shouldPlace = BuildPath(newlyPlaced, end, depth - 1, allowedDisplace, out displace, partConn);
								if(shouldPlace) 
								{
									Vector3 startPlatPos = start.transform.position;
									for(int i=0;i<3;i++)
									{
										if((transNormal[i] * transScaling[i] != 0) && (Mathf.Sign(transNormal[i]) == Mathf.Sign(displace[i])))
										{
											//Do some scaling shit
											startPlatPos[i] = StretchPiece(start, displace, i, myConnector, fixedConnector);
											displace[i] = 0;
										}
										else
										{
											startPlatPos[i] += displace[i];
										}
									}
									start.transform.position = startPlatPos;
									print(start.name + " " + System.Convert.ToString(allowedDisplace,2));
									return true;
								}
								else
								{
									Destroy(newlyPlaced.gameObject);
									start.connectors[myConnector].connectedNode = null;
								}
							}
						}
					}
				}
			}
		}
		return false;
	}
	
	//Rotate and attach new piece, return PlatformingElement component of new piece.
	public PlatformingElement PlacePiece(PlatformingElement existingPiece, int connectorIndex, int newPieceIndex, int newPieceConnectorIndex, float rotate)
	{
		GameObject newPiece = Instantiate(placeableObjects[newPieceIndex].gameObject) as GameObject;
		PlatformingElement newPlatform = newPiece.GetComponent<PlatformingElement>();
		newPlatform.typeID = newPieceIndex;
		newPiece.transform.rotation = Quaternion.FromToRotation(newPlatform.connectors[newPieceConnectorIndex].normal,
			existingPiece.transform.TransformDirection(existingPiece.connectors[connectorIndex].normal) * -1);
		newPiece.transform.position = existingPiece.transform.TransformPoint(existingPiece.connectors[connectorIndex].connectionPoint)
			- newPiece.transform.TransformDirection(newPlatform.connectors[newPieceConnectorIndex].connectionPoint);
		
		
		newPiece.transform.RotateAround(newPiece.transform.TransformPoint(newPlatform.connectors[newPieceConnectorIndex].connectionPoint),
			existingPiece.transform.TransformDirection(existingPiece.connectors[connectorIndex].normal) * -1, rotate);
			
			
		existingPiece.connectors[connectorIndex].connectedNode = newPlatform;
		newPlatform.connectors[newPieceConnectorIndex].connectedNode = existingPiece;
		return newPlatform;
	}
	
	public void ConnectInitials()
	{
		
	}
	
	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Bounds limits = adjustedBounds;
		Gizmos.DrawWireCube(limits.center, limits.size);
	}
}
