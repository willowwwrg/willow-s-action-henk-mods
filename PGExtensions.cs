using UnityEngine;

public static class PGExtensions
{
	public static bool Contains(this Transform[] t_arr, Transform t)
	{
		for (int i = 0; i < t_arr.Length; i++)
		{
			if (t_arr[i] == t)
			{
				return true;
			}
		}
		return false;
	}
}
