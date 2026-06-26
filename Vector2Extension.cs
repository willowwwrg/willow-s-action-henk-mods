using UnityEngine;

public static class Vector2Extension
{
	public static float Cross(Vector2 orig, Vector2 other)
	{
		return orig.x * other.y - other.x * orig.y;
	}
}
