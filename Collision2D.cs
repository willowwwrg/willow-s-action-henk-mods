using UnityEngine;

public static class Collision2D
{
	public static bool IntersectLines(Vector2 e0start, Vector2 e0end, Vector2 e1start, Vector2 e1end)
	{
		return IntersectLines(e0end - e0start, e1start - e0start, e1end - e0start);
	}

	public static bool IntersectLines(Vector2 e0start, Vector2 e0end, Vector2 e1start, Vector2 e1end, out Vector2 intersection)
	{
		return IntersectLines(e0end - e0start, e1start - e0start, e1end - e0start, out intersection);
	}

	public static bool IntersectLines(Vector2 e0end, Vector2 e1start, Vector2 e1end)
	{
		Vector2 intersection;
		return IntersectLines(e0end, e1start, e1end, out intersection);
	}

	public static bool IntersectLines(Vector2 e0end, Vector2 e1start, Vector2 e1end, out Vector2 intersection)
	{
		intersection = Vector2.zero;
		float num = Vector2Extension.Cross(e0end, e1end);
		if (num != 0f)
		{
			float num2 = Vector2Extension.Cross(e1start, e1end) / num;
			float num3 = Vector2Extension.Cross(e1start, e0end) / num;
			intersection = num2 * e0end;
			if (0f <= num3 && num3 <= 1f && 0f <= num2 && num2 <= 1f)
			{
				return true;
			}
		}
		return false;
	}
}
