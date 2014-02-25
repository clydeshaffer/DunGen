using UnityEngine;
using System.Collections;

public class ItemPlacer : MonoBehaviour {
	
	public GameObject[] mustPlaceOneOfEach;
	private int[] oneEachIndices;
	private int roomIndex = 0;
	
	public GameObject[] prefabs;
	
	public int[] weights;
	
	public int minItems = 1;
	public int maxItems = 6;
	
	private int weightSum = 0;
	
	void SpawnObjects(OctTree node)
	{
		if(node.roomNode == null)
		{
			foreach(OctTree subNode in node.children) SpawnObjects(subNode);
		}
		else
		{
			int uniquesPlaced = 0;
			
			for(int i = 0; i < oneEachIndices.Length; i++)
			{
				if(oneEachIndices[i] == roomIndex)
				{
					Vector3 placement = Util.RandomInWidthTwoCube() * 0.33f;
					placement.y = -1;
					placement = Vector3.Scale(node.roomNode.roomBounds.extents, placement);
					
					Vector3 facing = Vector3.up * Random.Range(0, 360);
					Instantiate(mustPlaceOneOfEach[i], placement + node.roomNode.roomBounds.center, Quaternion.Euler(facing));
					uniquesPlaced ++;
				}
			}
			
			int numObjs = Random.Range(minItems, maxItems + 1);
			numObjs = Mathf.Min(numObjs, Mathf.FloorToInt(node.roomNode.roomBounds.size.x), Mathf.FloorToInt(node.roomNode.roomBounds.size.z));
			numObjs -= uniquesPlaced;
			
			
			if(uniquesPlaced == 0)
			for(int i = 0; i < numObjs; i ++)
			{
				Vector3 placement = Util.RandomInWidthTwoCube();
				placement = Vector3.Scale(node.roomNode.roomBounds.extents, placement);
				Vector3 facing = Vector3.up * Random.Range(0, 360);
				Instantiate(prefabs[weightedSelect()],placement + node.roomNode.roomBounds.center, Quaternion.Euler(facing));
			}
			roomIndex++;
		}
	}
	
	// Use this for initialization
	void Start () {
		for(int i = 0; i < Mathf.Min(prefabs.Length, weights.Length); i ++)
		{
			weightSum += weights[i];
		}
		RoomGenerator rg = GetComponent<RoomGenerator>();
		rg.MakeDungeon();
		RoomGenerator.InstantiateRooms(rg.roomNodeList, rg.roomStyle);
		oneEachIndices = new int[mustPlaceOneOfEach.Length];
		for(int i = 0; i < mustPlaceOneOfEach.Length; i++)
		{
			oneEachIndices[i] = Random.Range(0, Mathf.FloorToInt(rg.root.GetLeafCount()));
		}
		SpawnObjects(rg.root);
	}
	
	int weightedSelect() {
		int rand = Random.Range(0, weightSum);
		int total = weights[0];
		int index = 0;
		while(total < rand)
		{
			index++;
			total += weights[index];
		}
		return index;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
