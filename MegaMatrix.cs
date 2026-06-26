using UnityEngine;

public class MegaMatrix
{
	public static void Set(ref Matrix4x4 mat, float[] vals)
	{
		if (vals.Length >= 16)
		{
			for (int i = 0; i < 16; i++)
			{
				mat[i] = vals[i];
			}
		}
	}

	public static void Translate(ref Matrix4x4 mat, Vector3 p)
	{
		Translate(ref mat, p.x, p.y, p.z);
	}

	public static void Scale(ref Matrix4x4 mat, Vector3 s, bool trans)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		identity[0, 0] = s.x;
		identity[1, 1] = s.y;
		identity[2, 2] = s.z;
		mat = identity * mat;
		if (trans)
		{
			int row2;
			int row = (row2 = 0);
			int column2;
			int column = (column2 = 3);
			float num = mat[row2, column2];
			mat[row, column] = num * s.x;
			int row3 = (column2 = 1);
			int column3 = (row2 = 3);
			num = mat[column2, row2];
			mat[row3, column3] = num * s.y;
			int row4 = (row2 = 2);
			int column4 = (column2 = 3);
			num = mat[row2, column2];
			mat[row4, column4] = num * s.z;
		}
	}

	public static Vector3 GetTrans(ref Matrix4x4 mat)
	{
		return new Vector3
		{
			x = mat[0, 3],
			y = mat[1, 3],
			z = mat[2, 3]
		};
	}

	public static void SetTrans(ref Matrix4x4 mat, Vector3 p)
	{
		mat[0, 3] = p.x;
		mat[1, 3] = p.y;
		mat[2, 3] = p.z;
	}

	public static void NoTrans(ref Matrix4x4 mat)
	{
		mat[0, 3] = 0f;
		mat[1, 3] = 0f;
		mat[2, 3] = 0f;
	}

	public static void Translate(ref Matrix4x4 mat, float x, float y, float z)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		identity[0, 3] = x;
		identity[1, 3] = y;
		identity[2, 3] = z;
		mat = identity * mat;
	}

	public static void RotateX(ref Matrix4x4 mat, float ang)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		float value = Mathf.Cos(ang);
		float num = Mathf.Sin(ang);
		identity[1, 1] = value;
		identity[1, 2] = num;
		identity[2, 1] = 0f - num;
		identity[2, 2] = value;
		mat = identity * mat;
	}

	public static void RotateY(ref Matrix4x4 mat, float ang)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		float value = Mathf.Cos(ang);
		float num = Mathf.Sin(ang);
		identity[0, 0] = value;
		identity[0, 2] = 0f - num;
		identity[2, 0] = num;
		identity[2, 2] = value;
		mat = identity * mat;
	}

	public static void RotateZ(ref Matrix4x4 mat, float ang)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		float value = Mathf.Cos(ang);
		float num = Mathf.Sin(ang);
		identity[0, 0] = value;
		identity[0, 1] = num;
		identity[1, 0] = 0f - num;
		identity[1, 1] = value;
		mat = identity * mat;
	}

	public static void Rotate(ref Matrix4x4 mat, Vector3 rot)
	{
		RotateX(ref mat, rot.x);
		RotateY(ref mat, rot.y);
		RotateZ(ref mat, rot.z);
	}

	public static void LookAt(ref Matrix4x4 mat, Vector3 source_pos, Vector3 target_pos)
	{
		Vector3 value = target_pos - source_pos;
		Vector3 vector = Vector3.Normalize(target_pos - source_pos);
		Vector3 vector2 = Vector3.Normalize(Vector3.Cross(Vector3.up, vector));
		mat = Matrix4x4.identity;
		mat.SetColumn(1, Vector3.Normalize(Vector3.Cross(vector, vector2)));
		mat.SetColumn(2, Vector3.Normalize(value));
		mat.SetColumn(0, vector2);
	}

	public static void LookAt(ref Matrix4x4 mat, Vector3 source_pos, Vector3 target_pos, Vector3 up)
	{
		Vector3 value = target_pos - source_pos;
		Vector3 vector = Vector3.Normalize(target_pos - source_pos);
		Vector3 vector2 = Vector3.Normalize(Vector3.Cross(up, vector));
		mat = Matrix4x4.identity;
		mat.SetColumn(1, Vector3.Normalize(Vector3.Cross(vector, vector2)));
		mat.SetColumn(2, Vector3.Normalize(value));
		mat.SetColumn(0, vector2);
	}

	public static void Set(ref Matrix4x4 mat, Vector3 right, Vector3 up, Vector3 fwd)
	{
		mat = Matrix4x4.identity;
		mat.SetRow(0, right);
		mat.SetRow(1, up);
		mat.SetRow(2, fwd);
	}

	public static void SetTR(ref Matrix4x4 mat, Vector3 p, Quaternion q)
	{
		float num = q.x * q.x;
		float num2 = q.y * q.y;
		float num3 = q.z * q.z;
		float num4 = q.x * q.y;
		float num5 = q.x * q.z;
		float num6 = q.y * q.z;
		float num7 = q.w * q.x;
		float num8 = q.w * q.y;
		float num9 = q.w * q.z;
		mat.m00 = 1f - 2f * (num2 + num3);
		mat.m01 = 2f * (num4 - num9);
		mat.m02 = 2f * (num5 + num8);
		mat.m10 = 2f * (num4 + num9);
		mat.m11 = 1f - 2f * (num + num3);
		mat.m12 = 2f * (num6 - num7);
		mat.m20 = 2f * (num5 - num8);
		mat.m21 = 2f * (num6 + num7);
		mat.m22 = 1f - 2f * (num + num2);
		mat.m30 = (mat.m31 = (mat.m32 = 0f));
		mat.m33 = 1f;
		mat.m03 = p.x;
		mat.m13 = p.y;
		mat.m23 = p.z;
	}
}
