using UnityEngine;
using System.Collections;

[System.Serializable]
public class Interval {
	
	public Interval(float _min, float _max)
	{
		min = _min;
		max = _max;
	}
	
	public float min;
	public float max;
	
	public bool ContainsInclusive(float n)
	{
		return (n >= min) && (n <= max);
	}

	public Interval randomSub(float minSize, float maxSize)
	{
		float st = Random.Range (min, max - minSize);
		float en = Mathf.Min(Random.Range (minSize, maxSize) + st, max);
		return new Interval (st, en);
	}

	//Check if two intervals share a limit
	//Return 1 if a is less than b, and they touch
	//Return -1 if a is greater than b, and they touch
	//Otherwise return 0
	public static int CheckTouching(Interval a, Interval b)
	{
		if (Mathf.Approximately (a.max, b.min)) return 1;
		if (Mathf.Approximately (b.max, a.min)) return -1;
		return 0;
	}
	
	public static bool CheckOverlap(Interval a, Interval b)
	{
		float min = Mathf.Max(a.min, b.min);
		float max = Mathf.Min(a.max, b.max);
		return (min < max);
	}
	
	//Test for overlap between intervals
	public static bool CheckOverlap(Interval a, Interval b, float minOverlap)
	{
		float min = Mathf.Max(a.min + minOverlap/2, b.min + minOverlap/2);
		float max = Mathf.Min(a.max - minOverlap/2, b.max - minOverlap/2);
		return (min < max);
	}
	
	public static float OverlapSize(Interval a, Interval b)
	{
		float min = Mathf.Max(a.min, b.min);
		float max = Mathf.Min(a.max, b.max);
		return max - min;
	}
	
	//Test for overlap between intervals
	public static bool CheckOverlapInc(Interval a, Interval b)
	{
		float min = Mathf.Max(a.min, b.min);
		float max = Mathf.Min(a.max, b.max);
		return (min <= max);
	}
	
	//Return the overlap interval.
	public static Interval GetOverlap(Interval a, Interval b)
	{
		float min = Mathf.Max(a.min, b.min);
		float max = Mathf.Min(a.max, b.max);
		//if(min > max) return new Interval(0,0);
		return new Interval(min, max);
	}

	public static Interval GetOverlapOrGap(Interval a, Interval b)
	{
		float min = Mathf.Max(a.min, b.min);
		float max = Mathf.Min(a.max, b.max);
		if (min > max) return new Interval(max, min);
		else return new Interval(min, max);
	}
}
