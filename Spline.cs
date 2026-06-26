using System.Collections.Generic;
using UnityEngine;

public class Spline : PolyLine
{
	private bool closed;

	public bool catmullRom;

	public int maxParameter => (points.Length - 1) / 3;

	public Spline(Vector2[] sortedVertices)
		: base(null)
	{
		int num = sortedVertices.Length;
		num -= num % 2;
		if (!closed)
		{
			points = new Vector2[num + (num - 4) / 2];
			ref Vector2 reference = ref points[0];
			reference = sortedVertices[0];
		}
		else
		{
			points = new Vector2[num + num / 2 + 1];
			ref Vector2 reference2 = ref points[0];
			reference2 = (sortedVertices[0] + sortedVertices[1]) * 0.5f;
		}
		ref Vector2 reference3 = ref points[1];
		reference3 = sortedVertices[1];
		if (!closed)
		{
			ref Vector2 reference4 = ref points[points.Length - 2];
			reference4 = sortedVertices[num - 2];
			ref Vector2 reference5 = ref points[points.Length - 1];
			reference5 = sortedVertices[num - 1];
		}
		else
		{
			ref Vector2 reference6 = ref points[points.Length - 4];
			reference6 = (sortedVertices[num - 1] + sortedVertices[num - 2]) * 0.5f;
			ref Vector2 reference7 = ref points[points.Length - 3];
			reference7 = sortedVertices[num - 1];
			ref Vector2 reference8 = ref points[points.Length - 2];
			reference8 = sortedVertices[0];
			ref Vector2 reference9 = ref points[points.Length - 1];
			reference9 = points[0];
		}
		int num2 = 2;
		int num3 = 1;
		if (!closed)
		{
			num3 = 3;
		}
		for (int i = num2; i < num - num3; i += 2)
		{
			ref Vector2 reference10 = ref points[num2];
			reference10 = sortedVertices[i];
			num2++;
			ref Vector2 reference11 = ref points[num2];
			reference11 = (sortedVertices[i] + sortedVertices[i + 1]) * 0.5f;
			num2++;
			ref Vector2 reference12 = ref points[num2];
			reference12 = sortedVertices[i + 1];
			num2++;
		}
	}

	public Vector2 PointAtParameter(float inParameter)
	{
		float num = Mathf.Clamp(inParameter, 0f, maxParameter);
		int num2 = (int)num;
		num -= (float)num2;
		num2 *= 3;
		if (num == 0f)
		{
			return points[num2];
		}
		if (num == 1f || num2 == maxParameter * 3)
		{
			return points[num2 + 3];
		}
		Vector2 vector = points[num2];
		Vector2 vector2 = points[num2 + 1];
		Vector2 vector3 = points[num2 + 2];
		Vector2 vector4 = points[num2 + 3];
		if (!catmullRom)
		{
			Vector2 vector5 = (vector2 - vector) * num + vector;
			Vector2 vector6 = (vector3 - vector2) * num + vector2;
			Vector2 vector7 = ((vector4 - vector3) * num + vector3 - vector6) * num + vector6;
			vector6 = (vector6 - vector5) * num + vector5;
			return (vector7 - vector6) * num + vector6;
		}
		float num3 = 0f;
		Vector2 vector8 = 0.5f * (1f - num3) * (vector3 - vector);
		Vector2 vector9 = 0.5f * (1f - num3) * (vector4 - vector2);
		Vector2 vector10 = 2f * vector2 + vector8 - 2f * vector3 + vector9;
		Vector2 vector11 = -3f * vector2 - 2f * vector8 + 3f * vector3 - vector9;
		Vector2 vector12 = vector8;
		Vector2 vector13 = vector2;
		return ((vector10 * num + vector11) * num + vector12) * num + vector13;
	}

	public PolyLine AsPolyLine(int sampleSteps)
	{
		sampleSteps = Mathf.Max(1, sampleSteps);
		Vector2[] array = new Vector2[sampleSteps * maxParameter + 1];
		for (int i = 0; i < maxParameter; i++)
		{
			for (int j = 0; j < sampleSteps; j++)
			{
				float num = (float)j / (float)sampleSteps;
				ref Vector2 reference = ref array[i * sampleSteps + j];
				reference = PointAtParameter((float)i + num);
			}
		}
		ref Vector2 reference2 = ref array[array.Length - 1];
		reference2 = PointAtParameter(maxParameter);
		return new PolyLine(array);
	}

	public PolyLine Resampled(float sampleDistance, int initialSampleSteps)
	{
		sampleDistance = Mathf.Max(0.01f, sampleDistance);
		PolyLine polyLine = AsPolyLine(initialSampleSteps);
		float num = 0f;
		List<Vector2> list = new List<Vector2>();
		list.Add(polyLine.Point(0));
		for (int i = 0; i < polyLine.pointCount - 1; i++)
		{
			Vector2 vector = polyLine.Point(i);
			Vector2 vector2 = polyLine.Point(i + 1);
			float magnitude = (vector2 - vector).magnitude;
			float num2 = (float)list.Count * sampleDistance;
			while (num + magnitude >= num2)
			{
				float t = Mathf.InverseLerp(num, num + magnitude, num2);
				list.Add(Vector2.Lerp(vector, vector2, t));
				num2 = (float)list.Count * sampleDistance;
			}
			num += magnitude;
		}
		return new PolyLine(list.ToArray());
	}

	public override void DrawAsGizmo(Matrix4x4 matrix)
	{
	}
}
