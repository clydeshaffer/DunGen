using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Util {

	public static float LNot(float n)
	{
		if(n == 0) return 1;
		return 0;
	}
	
	public static float LAnd(float p, float q)
	{
		if(p != 0 && q != 0) return 1;
		else return 0;
	}
	
	public static float LOr(float p, float q)
	{
		if(p != 0 || q != 0) return 1;
		else return 0;
	}
	
	public static Vector3 LNot(Vector3 v)
	{
		return new Vector3(LNot(v.x), LNot(v.y), LNot(v.z));
	}
	
	public static Vector3 LAnd(Vector3 p, Vector3 q)
	{
		Vector3 result = Vector3.zero;
		for(int i = 0; i < 3; i ++) result[i] = LAnd(p[i], q[i]);
		return result;
	}
	
	public static Vector3 LOr(Vector3 p, Vector3 q)
	{
		Vector3 result = Vector3.zero;
		for(int i = 0; i < 3; i ++) result[i] = LOr(p[i], q[i]);
		return result;
	}
	
	public static Vector3 VecAbs(Vector3 v)
	{
		return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
	}
	
	public static float VecSum(Vector3 v)
	{
		return v.x + v.y + v.z;
	}
	
	public static float VecSig(Vector3 v)
	{
		float max = Mathf.Max(Mathf.Abs(v.x),Mathf.Abs(v.y),Mathf.Abs(v.z));
		if(max == Mathf.Abs(v.x)) return v.x;
		if(max == Mathf.Abs(v.y)) return v.y;
		if(max == Mathf.Abs(v.z)) return v.z;
		return 0;
	}
	
	public static Vector3 VecLongestAxis(Vector3 v)
	{
		int greatest = 0;
		if(Mathf.Abs(v[1]) > Mathf.Abs(v[greatest])) greatest = 1;
		if(Mathf.Abs(v[2]) > Mathf.Abs(v[greatest])) greatest = 2;
		Vector3 result = Vector3.zero;
		result[greatest] = v[greatest];
		return result;
	}
	
	public static Vector3 VecRound(Vector3 v)
	{
		v.x = Mathf.Round(v.x);
		v.y = Mathf.Round(v.y);
		v.z = Mathf.Round(v.z);
		return v;
	}
	
	public static Vector3 RandomInWidthTwoCube()
	{
		Vector3 randVec = Vector3.zero;
		for(int i = 0; i < 3; i ++) randVec[i] = Random.Range(-1f, 1f);
		return randVec;
	}
	
	public static Vector3 VecEnum(int i)
	{
		switch(i)
		{
		case 0: return Vector3.forward;
		case 1: return Vector3.right;
		case 2: return Vector3.up;
		case 3: return Vector3.forward * -1;
		case 4: return Vector3.right * -1;
		case 5: return Vector3.up * -1;
		default: return Vector3.zero;
		}
	}
	
	public static int VecNonZeroes(Vector3 v)
	{
		int count = 0;
		for(int i = 0; i < 3; i ++) if(v[i] != 0) count ++;
		return count;
	}
	
	//Returns only nonzero component vectors
	public static Vector3[] VecBreak(Vector3 v)
	{
		int c = 0;
		if(v.x != 0) c ++;
		if(v.y != 0) c ++;
		if(v.z != 0) c ++;
		Vector3[] result = new Vector3[c];
		c = 0;
		
		if(v.x != 0)
		{
			result[c] = v.x * Vector3.right; c ++;
		}
		
		
		if(v.z != 0)
		{
			result[c] = v.z * Vector3.forward; c ++;
		}
		
		if(v.y != 0)
		{
			result[c] = v.y * Vector3.up; c ++;
		}
		
		return result;
	}
	
	public static bool BoundsFit(Bounds inner, Bounds outer)
	{
		if(inner.size.x >= outer.size.x && outer.size.x != 0) return false;
		if(inner.size.y >= outer.size.y && outer.size.x != 0) return false;
		if(inner.size.z >= outer.size.z && outer.size.x != 0) return false;
		return true;
	}
	
	public static void LLAppend<T>(LinkedList<T> dest, LinkedList<T> src)
	{
		foreach(T t in src) dest.AddFirst(t);
	}
	
	
}
