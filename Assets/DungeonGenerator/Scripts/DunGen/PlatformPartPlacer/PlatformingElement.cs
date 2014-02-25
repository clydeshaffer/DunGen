using UnityEngine;
using System.Collections;

public class PlatformingElement : MonoBehaviour {
	
	public PlatformConnection[] connectors;
	
	public Bounds exclusionZone;
	
	public bool allowPan = true;
	public bool allowTilt = false;
	
	public Vector3 allowedScaling = Vector3.zero;
	
	public bool dontChainToSame = false;
	
	public int typeID; //ID for type list in PlatformAssembler
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public Vector2 CheckForConnection(PlatformingElement other, int myConnector, int allowDisplace, out Vector3 displaceVec)
	{
		for(int yourConnector = 0; yourConnector < other.connectors.Length; yourConnector++)
		{
			Vector3 myConnPoint = connectors[myConnector].connectionPoint;
			Vector3 yourConnPoint = other.connectors[yourConnector].connectionPoint;
			Vector3 myConnNormal = connectors[myConnector].normal;
			Vector3 yourConnNormal = other.connectors[yourConnector].normal;
			myConnPoint = transform.TransformPoint(myConnPoint);
			myConnNormal = transform.TransformDirection(myConnNormal).normalized;
			yourConnPoint = other.transform.TransformPoint(yourConnPoint);
			yourConnNormal = other.transform.TransformDirection(yourConnNormal).normalized;
			
			if(Vector3.Dot(myConnNormal, yourConnNormal) <= (-1 + Mathf.Min(connectors[myConnector].slackAngle, other.connectors[yourConnector].slackAngle)))
			{
				displaceVec = Vector3.zero;
				for(int i = 0; i < 3; i ++)
				{
					float diff = yourConnPoint[i] - myConnPoint[i];
					int mask;
					if(diff > 0) mask = (1 << i);
					else mask = (1 << (i + 3));
					if((allowDisplace & mask) != 0)
					{
						myConnPoint[i] = yourConnPoint[i];
						displaceVec[i] = diff;
					}
				}
				float normDist = Vector3.Dot(yourConnPoint - myConnPoint, myConnNormal);
				if(normDist >= 0 && normDist <= connectors[myConnector].slackNormal + other.connectors[yourConnector].slackNormal)
				{
					if((myConnPoint + myConnNormal * normDist - yourConnPoint).sqrMagnitude <= Mathf.Pow(connectors[myConnector].slackRadial + other.connectors[yourConnector].slackRadial,2))
					{
						return new Vector2(myConnector, yourConnector);
					}
				}
			}
		}
		displaceVec = Vector3.zero;
		return Vector2.one * -1;
	}
	
	void OnDrawGizmos () {
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireCube(transform.TransformPoint(exclusionZone.center), transform.TransformPoint(exclusionZone.size) - transform.position);
		foreach(PlatformConnection connector in connectors)
		{
			if(connector.connectedNode !=  null) Gizmos.color = Color.green;
			else Gizmos.color = Color.red;
			Vector3 start = transform.TransformPoint(connector.connectionPoint);
			Vector3 end = start + transform.TransformDirection(connector.normal);
			Gizmos.DrawLine(start, end);
		}
	}
}
