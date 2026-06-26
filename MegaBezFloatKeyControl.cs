using System;
using UnityEngine;

[Serializable]
public class MegaBezFloatKeyControl : MegaControl
{
	private const float SCALE = 4800f;

	public MegaBezFloatKey[] Keys;

	public float f;

	public void InitKeys()
	{
		for (int i = 0; i < Keys.Length - 1; i++)
		{
			float num = Times[i + 1] - Times[i];
			float num2 = Keys[i].val + Keys[i].outtan * 4800f * (num / 3f);
			float num3 = Keys[i + 1].val + Keys[i + 1].intan * 4800f * (num / 3f);
			Keys[i].coef1 = Keys[i + 1].val + 3f * (num2 - num3) - Keys[i].val;
			Keys[i].coef2 = 3f * (num3 - 2f * num2 + Keys[i].val);
			Keys[i].coef3 = 3f * (num2 - Keys[i].val);
		}
	}

	public void InitKeys(float scale)
	{
		for (int i = 0; i < Keys.Length - 1; i++)
		{
			float num = Times[i + 1] - Times[i];
			float num2 = Keys[i].val + Keys[i].outtan * scale * (num / 3f);
			float num3 = Keys[i + 1].val + Keys[i + 1].intan * scale * (num / 3f);
			Keys[i].coef1 = Keys[i + 1].val + 3f * (num2 - num3) - Keys[i].val;
			Keys[i].coef2 = 3f * (num3 - 2f * num2 + Keys[i].val);
			Keys[i].coef3 = 3f * (num2 - Keys[i].val);
		}
	}

	public void InitKeysMaya()
	{
		for (int i = 0; i < Keys.Length - 1; i++)
		{
			float num = Times[i];
			float num2 = Times[i] + Keys[i].outtanx;
			float num3 = Times[i + 1] - Keys[i + 1].intanx;
			float num4 = Times[i + 1];
			float val = Keys[i].val;
			float num5 = Keys[i].val + Keys[i].outtan;
			float num6 = Keys[i + 1].val - Keys[i + 1].intan;
			float val2 = Keys[i + 1].val;
			float num7 = num4 - num;
			float num8 = val2 - val;
			float num9 = num2 - num;
			float num10 = 0f;
			float num11 = 0f;
			if (num9 != 0f)
			{
				num10 = (num5 - val) / num9;
			}
			num9 = num4 - num3;
			if (num9 != 0f)
			{
				num11 = (val2 - num6) / num9;
			}
			float num12 = 1f / (num7 * num7);
			float num13 = num7 * num10;
			float num14 = num7 * num11;
			Keys[i].coef0 = (num13 + num14 - num8 - num8) * num12 / num7;
			Keys[i].coef1 = (num8 + num8 + num8 - num13 - num13 - num14) * num12;
			Keys[i].coef2 = num10;
			Keys[i].coef3 = val;
		}
	}

	public float GetHermiteFloat(float tt)
	{
		if (Times.Length == 1)
		{
			return Keys[0].val;
		}
		int key = GetKey(tt);
		float t = Mathf.Clamp01((tt - Times[key]) / (Times[key + 1] - Times[key]));
		t = Mathf.Lerp(Times[key], Times[key + 1], t) - Times[key];
		return t * (t * (t * Keys[key].coef0 + Keys[key].coef1) + Keys[key].coef2) + Keys[key].coef3;
	}

	public void MakeKey(MegaBezFloatKey key, Vector2 pco, Vector2 pleft, Vector2 pright, Vector2 co, Vector2 left, Vector2 right)
	{
		float num = pco.y * 100f;
		float num2 = pright.y * 100f;
		float num3 = left.y * 100f;
		float num4 = co.y * 100f;
		key.val = num;
		key.coef3 = 3f * (num2 - num);
		key.coef2 = 3f * (num - 2f * num2 + num3);
		key.coef1 = num4 - num + 3f * (num2 - num3);
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
		f = Keys[key].coef1 * num2 + Keys[key].coef2 * num + Keys[key].coef3 * alpha + Keys[key].val;
	}

	public override float GetFloat(float t)
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
