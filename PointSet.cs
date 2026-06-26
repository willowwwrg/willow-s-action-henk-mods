using System.Collections.Generic;
using UnityEngine;

public class PointSet
{
	protected Vector2[] points;

	public PointSet(Vector2[] sortedVertices)
	{
		points = sortedVertices;
	}

	private int MinX()
	{
		float x = points[0].x;
		int result = 0;
		for (int i = 1; i < points.Length; i++)
		{
			if (points[i].x < x)
			{
				x = points[i].x;
				result = i;
			}
		}
		return result;
	}

	private int MaxX()
	{
		float x = points[0].x;
		int result = 0;
		for (int i = 1; i < points.Length; i++)
		{
			if (points[i].x > x)
			{
				x = points[i].x;
				result = i;
			}
		}
		return result;
	}

	private int MinY()
	{
		float y = points[0].y;
		int result = 0;
		for (int i = 1; i < points.Length; i++)
		{
			if (points[i].y < y)
			{
				y = points[i].y;
				result = i;
			}
		}
		return result;
	}

	private int MaxY()
	{
		float y = points[0].y;
		int result = 0;
		for (int i = 1; i < points.Length; i++)
		{
			if (points[i].y > y)
			{
				y = points[i].y;
				result = i;
			}
		}
		return result;
	}

	public Polygon ConvexHull(out int[] hullIds)
	{
		List<int> list = new List<int>();
		int item = MinX();
		if (!list.Contains(item))
		{
			list.Add(item);
		}
		item = MinY();
		if (!list.Contains(item))
		{
			list.Add(item);
		}
		item = MaxX();
		if (!list.Contains(item))
		{
			list.Add(item);
		}
		item = MaxY();
		if (!list.Contains(item))
		{
			list.Add(item);
		}
		List<Vector2> list2 = new List<Vector2>();
		for (int i = 0; i < list.Count; i++)
		{
			list2.Add((points[list[(i + 1) % list.Count]] - points[list[i]]).normalized);
		}
		int num = 0;
		int count = list.Count;
		while (true)
		{
			int num2 = -1;
			float num3 = 0f;
			for (int j = 0; j < points.Length; j++)
			{
				if (list.Contains(j))
				{
					continue;
				}
				Vector2 other = points[j] - points[list[num]];
				if (!(Vector2Extension.Cross(list2[num], other) >= 0f))
				{
					float num4 = Vector2.Dot(list2[num], other.normalized);
					if (num2 == -1 || num4 < num3)
					{
						num3 = num4;
						num2 = j;
					}
				}
			}
			if (num2 != -1)
			{
				list2[num] = (points[num2] - points[list[num]]).normalized;
				int index = (num + 1) % list.Count;
				list2.Insert(index, (points[list[index]] - points[num2]).normalized);
				list.Insert(index, num2);
			}
			num++;
			if (num >= list.Count)
			{
				num = 0;
				if (list.Count == count)
				{
					break;
				}
				count = list.Count;
			}
		}
		Vector2[] array = new Vector2[list.Count];
		for (int k = 0; k < list.Count; k++)
		{
			ref Vector2 reference = ref array[k];
			reference = points[list[k]];
		}
		hullIds = list.ToArray();
		return new Polygon(array);
	}

	public virtual void DrawAsGizmo(Matrix4x4 matrix)
	{
	}
}
