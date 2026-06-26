using UnityEngine;

[AddComponentMenu("MegaShapes/Rectangle")]
public class MegaShapeRectangle : MegaShape
{
	private const float CIRCLE_VECTOR_LENGTH = 0.5517862f;

	public float length = 1f;

	public float width = 1f;

	public float fillet;

	public override string GetHelpURL()
	{
		return "?page_id=1189";
	}

	public override void MakeShape()
	{
		Matrix4x4 matrix = GetMatrix();
		length = Mathf.Clamp(length, 0f, float.MaxValue);
		width = Mathf.Clamp(width, 0f, float.MaxValue);
		fillet = Mathf.Clamp(fillet, 0f, float.MaxValue);
		MegaSpline megaSpline = NewSpline();
		float num = length / 2f;
		float num2 = width / 2f;
		Vector3 vector = new Vector3(num2, num, 0f);
		if (fillet > 0f)
		{
			float num3 = fillet * 0.5517862f;
			Vector3 vector2 = new Vector3(fillet, 0f, 0f);
			Vector3 vector3 = new Vector3(0f, fillet, 0f);
			Vector3 vector4 = new Vector3(num3, 0f, 0f);
			Vector3 vector5 = new Vector3(0f, num3, 0f);
			Vector3 vector6 = vector - vector3;
			megaSpline.AddKnot(vector6, vector6 - vector5, vector6 + vector5, matrix);
			vector -= vector2;
			megaSpline.AddKnot(vector, vector + vector4, vector - vector4, matrix);
			vector = new Vector3(0f - num2, num, 0f);
			Vector3 vector7 = vector + vector2;
			megaSpline.AddKnot(vector7, vector7 + vector4, vector7 - vector4, matrix);
			vector -= vector3;
			megaSpline.AddKnot(vector, vector + vector5, vector - vector5, matrix);
			vector = new Vector3(0f - num2, 0f - num, 0f);
			vector6 = vector + vector3;
			megaSpline.AddKnot(vector6, vector6 + vector5, vector6 - vector5, matrix);
			vector += vector2;
			megaSpline.AddKnot(vector, vector - vector4, vector + vector4, matrix);
			vector = new Vector3(num2, 0f - num, 0f);
			vector6 = vector - vector2;
			megaSpline.AddKnot(vector6, vector6 - vector4, vector6 + vector4, matrix);
			vector += vector3;
			megaSpline.AddKnot(vector, vector - vector5, vector + vector5, matrix);
		}
		else
		{
			megaSpline.AddKnot(vector, vector, vector, matrix);
			vector = new Vector3(0f - num2, num, 0f);
			megaSpline.AddKnot(vector, vector, vector, matrix);
			vector = new Vector3(0f - num2, 0f - num, 0f);
			megaSpline.AddKnot(vector, vector, vector, matrix);
			vector = new Vector3(num2, 0f - num, 0f);
			megaSpline.AddKnot(vector, vector, vector, matrix);
		}
		megaSpline.closed = true;
		CalcLength();
	}
}
