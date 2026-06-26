using System.Collections.Generic;
using UnityEngine;

public class Polygon : PolyLine
{
	private float normal;

	public Polygon(Vector2[] sortedVertices)
		: base(sortedVertices)
	{
		UpdateNormal();
	}

	private void UpdateNormal()
	{
		normal = 0f;
		for (int i = 0; i < points.Length; i++)
		{
			int num = (i + 2) % points.Length;
			int num2 = (i + 1) % points.Length;
			normal += Mathf.Sign(Vector2Extension.Cross(points[i] - points[num2], points[num] - points[num2]));
		}
	}

	protected int[] Triangulated()
	{
		List<Vector2> list = new List<Vector2>(points);
		List<int> list2 = new List<int>();
		for (int i = 0; i < points.Length; i++)
		{
			list2.Add(i);
		}
		List<int> list3 = new List<int>();
		int num = 0;
		int count = list.Count;
		while (list.Count > 3)
		{
			int count2 = list.Count;
			int index = (num + 1) % count2;
			if (normal != Mathf.Sign(Vector2Extension.Cross(list[num] - list[index], list[(num + 2) % count2] - list[index])))
			{
				list3.Add(list2[num]);
				list3.Add(list2[index]);
				list3.Add(list2[(num + 2) % count2]);
				list.RemoveAt(index);
				list2.RemoveAt(index);
			}
			num++;
			if (num >= list.Count)
			{
				num = 0;
				if (count == list.Count)
				{
					break;
				}
				count = list.Count;
			}
		}
		list3.Add(list2[0]);
		list3.Add(list2[1]);
		list3.Add(list2[2]);
		return list3.ToArray();
	}

	public Polygon[] TriangulatedPolygons()
	{
		int[] array = Triangulated();
		Polygon[] array2 = new Polygon[array.Length / 3];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = new Polygon(new Vector2[3]
			{
				points[array[i * 3]],
				points[array[i * 3 + 1]],
				points[array[i * 3 + 2]]
			});
		}
		return array2;
	}
}
