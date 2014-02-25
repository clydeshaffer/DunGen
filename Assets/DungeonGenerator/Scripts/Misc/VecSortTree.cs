using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VecSortTree {
	
	bool isRoot = false;
	
	public VecSortTree(Vector3 v)
	{
		data = v;
	}
	
	//Root is highest thing so it won't get popped
	public VecSortTree()
	{
		isRoot = true;
		data = Vector3.one * Mathf.Infinity;
	}
	
	public VecSortTree left = null;
	public VecSortTree right = null;
	public Vector3 data;
	
	
	
	static int VecCmp(Vector3 a, Vector3 b)
	{
		if(Util.VecSig(a) > Util.VecSig(b)) return 1;
		if(Util.VecSig(a) < Util.VecSig(b)) return -1;
		return 0;
	}
	
	public bool Push(Vector3 v, Interval allowRange)
	{
		float val = Util.VecSum(v);
		val = Mathf.Clamp(val, allowRange.min, allowRange.max);
		return Push(v.normalized * Mathf.Abs(val));
	}
	
	public bool Push(Vector3 v)
	{
		if(v == data) return false;
		if(VecCmp(v, data) <= 0)
		{
			if(left == null)
			{
				left = new VecSortTree(v);
				return true;
			}
			else return left.Push(v);
		}
		else
		{
			if(right == null)
			{
				right = new VecSortTree(v);
				return true;
			}
			else return right.Push(v);
		}
	}
	
	public void PrintAll()
	{
		if(left != null) left.PrintAll();
		Debug.Log(data);
		if(right != null) right.PrintAll();
	}
	
	public int Count()
	{
		int total = 1;
		if(left != null) total += left.Count();
		if(right != null) total += right.Count();
		return total;
	}
	
	//Return lowest and remove
	public Vector3 Pop()
	{
		if(left == null)
		{
			if(isRoot) return Vector3.one * 11030;
			return data;
		}
			
		if(left.left == null)
		{
			Vector3 retVal = left.data;
			left = left.right;
			return retVal;
		}
		else
		{
			return left.Pop();
		}
	}
	
}
