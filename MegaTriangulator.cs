using System.Collections.Generic;
using UnityEngine;

public class MegaTriangulator
{
	public static List<Vector3> m_points = new List<Vector3>();

	public static List<int> Triangulate(MegaShape shape, MegaSpline spline, float dist, ref List<Vector3> verts, ref List<Vector2> uvs, ref List<int> indices, Vector3 pivot, ref Vector3 size)
	{
		m_points.Clear();
		List<MegaKnot> knots = spline.knots;
		Vector3 p = knots[0].p;
		Vector3 p2 = knots[0].p;
		for (int i = 1; i < knots.Count; i++)
		{
			Vector3 p3 = knots[i].p;
			if (p3.x < p.x)
			{
				p.x = p3.x;
			}
			if (p3.y < p.y)
			{
				p.y = p3.y;
			}
			if (p3.z < p.z)
			{
				p.z = p3.z;
			}
			if (p3.x > p2.x)
			{
				p2.x = p3.x;
			}
			if (p3.y > p2.y)
			{
				p2.y = p3.y;
			}
			if (p3.z > p2.z)
			{
				p2.z = p3.z;
			}
		}
		size = p2 - p;
		int num = 0;
		num = ((!(Mathf.Abs(size.x) < Mathf.Abs(size.y))) ? ((Mathf.Abs(size.y) < Mathf.Abs(size.z)) ? 1 : 2) : ((!(Mathf.Abs(size.x) < Mathf.Abs(size.z))) ? 2 : 0));
		Vector3 zero = Vector3.zero;
		float num2 = spline.length / (spline.length / dist);
		if (num2 > spline.length)
		{
			num2 = spline.length;
		}
		int k = -1;
		Vector3 zero2 = Vector3.zero;
		for (float num3 = 0f; num3 < spline.length; num3 += num2)
		{
			float alpha = num3 / spline.length;
			zero2 = spline.Interpolate(alpha, shape.normalizedInterp, ref k) + pivot;
			switch (num)
			{
			case 0:
				zero.x = zero2.y;
				zero.y = zero2.z;
				break;
			case 1:
				zero.x = zero2.x;
				zero.y = zero2.z;
				break;
			case 2:
				zero.x = zero2.x;
				zero.y = zero2.y;
				break;
			}
			zero.z = num3;
			verts.Add(zero2);
			m_points.Add(zero);
			zero.x -= p.x;
			zero.y -= p.z;
			uvs.Add(zero);
		}
		return Triangulate(indices);
	}

	public static List<int> Triangulate(List<int> indices)
	{
		int count = m_points.Count;
		if (count < 3)
		{
			return indices;
		}
		int[] array = new int[count];
		if (Area() > 0f)
		{
			for (int i = 0; i < count; i++)
			{
				array[i] = i;
			}
		}
		else
		{
			for (int j = 0; j < count; j++)
			{
				array[j] = count - 1 - j;
			}
		}
		int num = count;
		int num2 = 2 * num;
		int num3 = 0;
		int num4 = num - 1;
		while (num > 2)
		{
			if (num2-- <= 0)
			{
				return indices;
			}
			int num5 = num4;
			if (num <= num5)
			{
				num5 = 0;
			}
			num4 = num5 + 1;
			if (num <= num4)
			{
				num4 = 0;
			}
			int num6 = num4 + 1;
			if (num <= num6)
			{
				num6 = 0;
			}
			if (Snip(num5, num4, num6, num, array))
			{
				int item = array[num5];
				int item2 = array[num4];
				int item3 = array[num6];
				indices.Add(item3);
				indices.Add(item2);
				indices.Add(item);
				num3++;
				int num7 = num4;
				for (int k = num4 + 1; k < num; k++)
				{
					array[num7] = array[k];
					num7++;
				}
				num--;
				num2 = 2 * num;
			}
		}
		return indices;
	}

	private static float Area()
	{
		int count = m_points.Count;
		float num = 0f;
		int index = count - 1;
		int num2 = 0;
		while (num2 < count)
		{
			Vector2 vector = m_points[index];
			Vector2 vector2 = m_points[num2];
			num += vector.x * vector2.y - vector2.x * vector.y;
			index = num2++;
		}
		return num * 0.5f;
	}

	private static bool Snip(int u, int v, int w, int n, int[] V)
	{
		Vector2 a = m_points[V[u]];
		Vector2 b = m_points[V[v]];
		Vector2 c = m_points[V[w]];
		if (float.Epsilon > (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x))
		{
			return false;
		}
		for (int i = 0; i < n; i++)
		{
			if (i != u && i != v && i != w)
			{
				Vector2 p = m_points[V[i]];
				if (InsideTriangle(a, b, c, p))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
	{
		float num = C.x - B.x;
		float num2 = C.y - B.y;
		float num3 = A.x - C.x;
		float num4 = A.y - C.y;
		float num5 = B.x - A.x;
		float num6 = B.y - A.y;
		float num7 = P.x - A.x;
		float num8 = P.y - A.y;
		float num9 = P.x - B.x;
		float num10 = P.y - B.y;
		float num11 = P.x - C.x;
		float num12 = P.y - C.y;
		float num13 = num * num10 - num2 * num9;
		float num14 = num5 * num8 - num6 * num7;
		float num15 = num3 * num12 - num4 * num11;
		if (num13 >= 0f && num15 >= 0f)
		{
			return num14 >= 0f;
		}
		return false;
	}
}
