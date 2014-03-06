using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlatformAssembler : MonoBehaviour
{
		public PlatformingElement[] placeableObjects;

		private LinkedList<PlatformingElement> allPieces;

		
	public void ConnectDoorPieces(PlatformingElement[] initialObjects)
	{
		if (placeableObjects.Length == 0)
						return;

		System.DateTime startTime = System.DateTime.UtcNow;
		allPieces = new LinkedList<PlatformingElement> ();
		LinkedList<PlatformingElement> newPieces;
		BuildPath (initialObjects [0], initialObjects [1], 0, 0, 20, 0, out newPieces);
		foreach (PlatformingElement p in newPieces)
			allPieces.AddFirst (p);
		for (int i = 2; i < initialObjects.Length; i++) {
			PlatformingElement closest = null;
			int connNum = 0;
			float closestDist = Mathf.Infinity;
			Vector3 initialObjCon = initialObjects [i].getConnectorPos (0);
			foreach (PlatformingElement element in newPieces) {
				for (int con = 0; con < element.connectors.Length; con++) {
					Vector3 diff = (element.getConnectorPos (con) - initialObjCon);
					if (element.connectors [con].connectedNode == null){// && (Vector3.Dot (diff, element.getConnectorDir (con)) < 0 || diff.sqrMagnitude < 0.001f)) {
						float dist = diff.sqrMagnitude;
						if (dist < closestDist) {
							closest = element;
							connNum = con;
							closestDist = dist;
						}
					}
				}
			}

			if(closest == null && initialObjects[i-1].connectors[0].connectedNode == null)
			{
				closest = initialObjects[i-1];
				connNum = 0;
			}

			if (closest != null) {
				BuildPath (initialObjects [i], closest, 0, connNum, 20, 0, out newPieces);
				foreach (PlatformingElement p in newPieces)
					allPieces.AddFirst (p);
			}
		}
		System.DateTime endTime = System.DateTime.UtcNow;
		Debug.Log ((endTime - startTime).TotalMilliseconds + " milliseconds --- generated " + newPieces.Count);
	}

		// Use this for initialization
		void Start ()
		{






		}

		// Update is called once per frame
		void Update ()
		{

		}

		bool IsValidPlacement (PlatformingElement existingPiece, int connectorIndex, int newPieceIndex, int newPieceConnectorIndex, int angle)
		{
				if (angle != 0 && !placeableObjects [newPieceIndex].connectors [newPieceConnectorIndex].allowRotationOnConnector)
						return false;
				Vector3 existingNormal = existingPiece.transform.TransformDirection (existingPiece.connectors [connectorIndex].normal);
				Vector3 newNormal = placeableObjects [newPieceIndex].connectors [newPieceConnectorIndex].normal;
				Vector3 eulers = Quaternion.FromToRotation (newNormal, existingNormal * -1).eulerAngles;
				if (eulers.x == 0 && eulers.z == 0)
						return true;
				return false;
		}

		void StretchPiece (PlatformingElement toStretch, Vector3 displacement, int stretchIndex, int connectorToMove, int connectorToNotMove)
		{
				Vector3 fixedConnectorPos = toStretch.getConnectorPos (connectorToNotMove);
				Vector3 movingConnectorPos = toStretch.getConnectorPos (connectorToMove);
				float currDist = Mathf.Abs (movingConnectorPos [stretchIndex] - fixedConnectorPos [stretchIndex]);
				float newDist = currDist + Mathf.Abs (displacement [stretchIndex]);
				float scaleFactor = newDist / currDist;
				Vector3 localScale = toStretch.transform.TransformDirection (toStretch.transform.localScale);
				localScale [stretchIndex] *= scaleFactor;

				toStretch.transform.localScale = toStretch.transform.InverseTransformDirection (localScale);
				toStretch.transform.position = Vector3.zero;
				toStretch.transform.position -= toStretch.getConnectorPos (connectorToNotMove);
				toStretch.transform.position += fixedConnectorPos;
		}

		private int VecMaskAdd (int mask, Vector3 vec)
		{
				return mask | (1 << Util.VecEnum (vec));
		}

		private int VecMaskRemove (int mask, Vector3 vec)
		{
				return  mask & ~(VecMaskAdd (0, vec));
		}

		private bool VecMaskCheck (int mask, Vector3 vec)
		{
				return ((VecMaskAdd (0, vec) & mask) != 0);
		}

		Vector3 BuildPath (PlatformingElement start, PlatformingElement end, int startConn, int endConn, int depth, int directionMask, out LinkedList<PlatformingElement> newPieces)
		{
				foreach (PlatformingElement element in allPieces)
				{
					if(element.getAdjustedExclusionZone().Intersects(start.getAdjustedExclusionZone()))
					{
						newPieces = new LinkedList<PlatformingElement>();
						return Vector3.zero;
					}
				}
				if (depth == 0) {
						newPieces = new LinkedList<PlatformingElement> ();
						return Vector3.one * float.NaN;
				}
				float tol = 0.95f;
				Vector3 diff = end.getConnectorPos (endConn) - start.getConnectorPos (startConn);
				Vector3[] targetDirs = Util.VecBreak (Util.VecSign (diff));

				int neededDirsCount = 0;
				foreach (Vector3 v in targetDirs) {
						if (VecMaskCheck (directionMask, v)) {
								neededDirsCount ++;
						}
				}
				Vector3 startDir = start.getConnectorDir (startConn);
				Vector3 finalDir = end.getConnectorDir (endConn);
				if ((neededDirsCount == targetDirs.Length || (diff.sqrMagnitude < 0.001f)) && Vector3.Dot (startDir, finalDir) < -1 * tol) {
						start.connectors[startConn].connectedNode = end;
						end.connectors[endConn].connectedNode = start;
						newPieces = new LinkedList<PlatformingElement>();
						return end.getConnectorPos (endConn) - start.getConnectorPos (startConn);
				}

				for (int obj = 0; obj < placeableObjects.Length; obj++) {
						for (int con = 0; con < placeableObjects[obj].connectors.Length; con++) {
								for (int rot = 0; rot < 4; rot ++) {
										Quaternion attachRot = Quaternion.FromToRotation (placeableObjects [obj].connectors [con].normal, startDir * -1);
										Quaternion pieceRot = Quaternion.AngleAxis (rot * 90, startDir);
										Quaternion conRot = pieceRot * attachRot;
										if (placeableObjects [obj].allowTilt || ((conRot * Vector3.up).y > tol))
												for (int otherCon = 0; otherCon < placeableObjects[obj].connectors.Length; otherCon++) {
														if (con != otherCon) {
																Vector3 otherConTestDir = placeableObjects [obj].connectors [otherCon].normal;
																Vector3 stretchDir = Vector3.Scale (otherConTestDir, placeableObjects [obj].allowedScaling);
																if (stretchDir.sqrMagnitude > 0.001f) {
																		if (!VecMaskCheck (directionMask, Util.VecSign (conRot * stretchDir))) {
																				PlatformingElement newPiece = PlacePiece (start, startConn, obj, con, rot * 90);
																				Vector3 displace = BuildPath (newPiece, end, otherCon, endConn, depth - 1, VecMaskAdd (directionMask, Util.VecSign (conRot * stretchDir)), out newPieces);

																				if (Vector3.Dot (displace, conRot * stretchDir) > 0) {
																						Vector3 projDir = Vector3.Project (displace, (conRot * stretchDir).normalized);
																						displace = Vector3.Exclude (projDir, displace);
																						newPiece.transform.position += displace;
																						StretchPiece (newPiece, projDir, Util.VecSigIndex (projDir), otherCon, con);
																				} else
																						newPiece.transform.position += displace;
									newPieces.AddFirst(newPiece);
																				return displace;
																		}
																}

																for (int i = 0; i < targetDirs.Length; i ++) {
																		if ((Vector3.Dot (conRot * otherConTestDir, targetDirs [i]) > tol) && !VecMaskCheck (directionMask, targetDirs [i])) {
																				PlatformingElement newPiece = PlacePiece (start, startConn, obj, con, rot * 90);
																				Vector3 displace = BuildPath (newPiece, end, otherCon, endConn, depth - 1, directionMask, out newPieces);
																				newPiece.transform.position += displace;
									newPieces.AddFirst(newPiece);
																				return displace;
																		}
																}
														}
												}
								}
						}
				}

				if (Vector3.Dot (startDir, finalDir) > -1 * tol)
						for (int obj = 0; obj < placeableObjects.Length; obj++) {
								for (int con = 0; con < placeableObjects[obj].connectors.Length; con++) {
										for (int rot = 0; rot < 4; rot ++) {
												Quaternion conRot = Quaternion.AngleAxis (rot * 90, startDir) * Quaternion.FromToRotation (placeableObjects [obj].connectors [con].normal, startDir * -1);
												if (placeableObjects [obj].allowTilt || ((conRot * Vector3.up).y > tol))
														for (int otherCon = 0; otherCon < placeableObjects[obj].connectors.Length; otherCon++) {
																if (con != otherCon) {
																		Vector3 otherConTestDir = placeableObjects [obj].connectors [otherCon].normal;

																		if ((Vector3.Dot (conRot * otherConTestDir, finalDir * -1) > tol)) {
																				PlatformingElement newPiece = PlacePiece (start, startConn, obj, con, rot * 90);
																				Vector3 displace = BuildPath (newPiece, end, otherCon, endConn, depth - 1, directionMask, out newPieces);
																				newPiece.transform.position += displace;
								newPieces.AddFirst(newPiece);
																				return displace;
																		}
																}
														}
										}
								}
						}
				newPieces = new LinkedList<PlatformingElement> ();
				return Vector3.one * float.NaN;
		}

		//Rotate and attach new piece, return PlatformingElement component of new piece.
		public PlatformingElement PlacePiece (PlatformingElement existingPiece, int connectorIndex, int newPieceIndex, int newPieceConnectorIndex, float rotate)
		{
				GameObject newPiece = Instantiate (placeableObjects [newPieceIndex].gameObject) as GameObject;
				PlatformingElement newPlatform = newPiece.GetComponent<PlatformingElement> ();
				newPlatform.typeID = newPieceIndex;
				newPiece.transform.rotation = Quaternion.FromToRotation (newPlatform.connectors [newPieceConnectorIndex].normal,
			existingPiece.transform.TransformDirection (existingPiece.connectors [connectorIndex].normal) * -1);
				newPiece.transform.position = existingPiece.transform.TransformPoint (existingPiece.connectors [connectorIndex].connectionPoint)
						- newPiece.transform.TransformDirection (newPlatform.connectors [newPieceConnectorIndex].connectionPoint);


				newPiece.transform.RotateAround (newPiece.transform.TransformPoint (newPlatform.connectors [newPieceConnectorIndex].connectionPoint),
			existingPiece.transform.TransformDirection (existingPiece.connectors [connectorIndex].normal), rotate);


				existingPiece.connectors [connectorIndex].connectedNode = newPlatform;
				newPlatform.connectors [newPieceConnectorIndex].connectedNode = existingPiece;
				return newPlatform;
		}

		public void ConnectInitials ()
		{

		}

		void OnDrawGizmos ()
		{
		}
}
