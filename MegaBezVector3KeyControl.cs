using System;
using UnityEngine;

[Serializable]
public class MegaBezVector3KeyControl : MegaControl
{
	private const float SCALE = 4800f;

	public MegaBezVector3Key[] Keys;

	public Vector3 f;

	public void Scale(float scl)
	{
		for (int i = 0; i < Keys.Length; i++)
		{
			Keys[i].val *= scl;
			Keys[i].intan *= scl;
			Keys[i].outtan *= scl;
		}
		InitKeys();
	}

	public void Scale(Vector3 scl)
	{
		for (int i = 0; i < Keys.Length; i++)
		{
			Keys[i].val.x *= scl.x;
			Keys[i].val.y *= scl.y;
			Keys[i].val.z *= scl.z;
			Keys[i].intan.x *= scl.x;
			Keys[i].intan.y *= scl.y;
			Keys[i].intan.z *= scl.z;
			Keys[i].outtan.x *= scl.x;
			Keys[i].outtan.y *= scl.y;
			Keys[i].outtan.z *= scl.z;
		}
		InitKeys();
	}

	public void Move(Vector3 scl)
	{
		for (int i = 0; i < Keys.Length; i++)
		{
			Keys[i].val += scl;
			Keys[i].intan += scl;
			Keys[i].outtan += scl;
		}
		InitKeys();
	}

	public void Rotate(Matrix4x4 tm)
	{
		for (int i = 0; i < Keys.Length; i++)
		{
			Keys[i].val = tm.MultiplyPoint3x4(Keys[i].val);
			Keys[i].intan = tm.MultiplyPoint3x4(Keys[i].intan);
			Keys[i].outtan = tm.MultiplyPoint3x4(Keys[i].outtan);
		}
		InitKeys();
	}

	public void InitKeys()
	{
		for (int i = 0; i < Keys.Length - 1; i++)
		{
			float num = Times[i + 1] - Times[i];
			Vector3 vector = Keys[i].val + Keys[i].outtan * 4800f * (num / 3f);
			Vector3 vector2 = Keys[i + 1].val + Keys[i + 1].intan * 4800f * (num / 3f);
			Keys[i].coef1 = Keys[i + 1].val + 3f * (vector - vector2) - Keys[i].val;
			Keys[i].coef2 = 3f * (vector2 - 2f * vector + Keys[i].val);
			Keys[i].coef3 = 3f * (vector - Keys[i].val);
		}
	}

	public void Interp(float alpha, int key)
	{
		if (alpha == 0f)
		{
			f = Keys[key].val;
			return;
		}
		if (alpha == 1f)
		{
			f = Keys[key + 1].val;
			return;
		}
		float num = alpha * alpha;
		float num2 = num * alpha;
		f.x = Keys[key].coef1.x * num2 + Keys[key].coef2.x * num + Keys[key].coef3.x * alpha + Keys[key].val.x;
		f.y = Keys[key].coef1.y * num2 + Keys[key].coef2.y * num + Keys[key].coef3.y * alpha + Keys[key].val.y;
		f.z = Keys[key].coef1.z * num2 + Keys[key].coef2.z * num + Keys[key].coef3.z * alpha + Keys[key].val.z;
	}

	public override Vector3 GetVector3(float t)
	{
		if (Times.Length == 1)
		{
			return Keys[0].val;
		}
		int key = GetKey(t);
		float num = (t - Times[key]) / (Times[key + 1] - Times[key]);
		if (num < 0f)
		{
			num = 0f;
		}
		else if (num > 1f)
		{
			num = 1f;
		}
		Interp(num, key);
		lastkey = key;
		lasttime = t;
		return f;
	}
}
