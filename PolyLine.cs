using UnityEngine;

public class PolyLine : PointSet
{
	public int edgeCount => points.Length - 1;

	public int pointCount => points.Length;

	public PolyLine(Vector2[] sortedVertices)
		: base(sortedVertices)
	{
	}

	public float ArcLength()
	{
		float num = 0f;
		for (int i = 0; i < points.Length - 1; i++)
		{
			num += (points[i + 1] - points[i]).magnitude;
		}
		return num;
	}

	public Vector2 Point(int pointId)
	{
		pointId = Mathf.Clamp(pointId, 0, points.Length - 1);
		return points[pointId];
	}

	public Vector2 Edge(int edgeId)
	{
		edgeId = Mathf.Clamp(edgeId, 0, points.Length - 2);
		return points[edgeId + 1] - points[edgeId];
	}

	public Vector2 Normal(int edgeId)
	{
		Vector2 vector = Edge(edgeId);
		return new Vector2(vector.y, 0f - vector.x).normalized;
	}

	public PolyLine Reversed()
	{
		PolyLine polyLine = new PolyLine(points);
		polyLine.Reverse();
		return polyLine;
	}

	public void Reverse()
	{
		int num = points.Length;
		int num2 = num / 2;
		int num3 = num % 2;
		Vector2[] array = new Vector2[num2];
		for (int i = 0; i < num; i++)
		{
			if (i < num2)
			{
				ref Vector2 reference = ref array[i];
				reference = points[i];
				ref Vector2 reference2 = ref points[i];
				reference2 = points[num - 1 - i];
			}
			else if (i >= num2 + num3)
			{
				ref Vector2 reference3 = ref points[i];
				reference3 = array[num2 - 1 - (i - num2 - num3)];
			}
		}
	}

	public PolyLine PeakRandom(float minMagnitude, float maxMagnitude, int seed = 0)
	{
		Random.seed = seed;
		Vector2[] array = new Vector2[points.Length];
		Vector2 vector = Vector2.zero;
		Vector2 lhs = Vector2.zero;
		float[] array2 = new float[points.Length];
		for (int i = 0; i < points.Length; i++)
		{
			float num = Random.Range(minMagnitude, maxMagnitude);
			if (i > 0)
			{
				lhs = vector;
				array[i] += vector;
			}
			vector = Normal(i).normalized;
			array[i] += vector;
			if (i > 0)
			{
				array[i] *= 0.5f;
				array2[i] = num / Vector2.Dot(lhs, array[i]);
			}
			else
			{
				array2[i] = num;
			}
		}
		for (int j = 0; j < points.Length; j++)
		{
			ref Vector2 reference = ref array[j];
			reference = array[j] * array2[j] + points[j];
		}
		return new PolyLine(array);
	}

	public bool Intersects(PolyLine other)
	{
		Vector2 point;
		return Intersects(other, out point);
	}

	public bool Intersects(PolyLine other, out Vector2 point)
	{
		point = Vector2.zero;
		for (int i = 0; i < other.edgeCount; i++)
		{
			Vector2 start = other.Point(i);
			Vector2 end = other.Point(i + 1);
			if (IntersectsLine(start, end, out point))
			{
				return true;
			}
		}
		return false;
	}

	public bool IntersectsLine(Vector2 start, Vector2 end)
	{
		Vector2 point;
		return IntersectsLine(start, end, out point);
	}

	public bool IntersectsLine(Vector2 start, Vector2 end, out Vector2 point)
	{
		point = Vector2.zero;
		Vector2 e1end = end - start;
		for (int i = 0; i < edgeCount; i++)
		{
			if (Collision2D.IntersectLines(points[i] - start, points[i + 1] - start, e1end, out point))
			{
				return true;
			}
		}
		return false;
	}

	public override void DrawAsGizmo(Matrix4x4 matrix)
	{
	}
}
