using System;
using UnityEngine;

[Serializable]
public class MegaEase
{
	public MegaEaseType method = MegaEaseType.InOutQuint;

	public Vector3 Get(Vector3 start, Vector3 end, Vector3 val, float alpha)
	{
		Vector3 zero = Vector3.zero;
		zero.x = Get(start.x, end.x, val.x, alpha);
		zero.y = Get(start.y, end.y, val.y, alpha);
		zero.z = Get(start.z, end.z, val.z, alpha);
		return zero;
	}

	public float Get(float start, float end, float val, float alpha)
	{
		return method switch
		{
			MegaEaseType.bounce => bounce(start, end, alpha), 
			MegaEaseType.InSine => easeInSine(start, end, alpha), 
			MegaEaseType.InOutSine => easeInOutSine(start, end, alpha), 
			MegaEaseType.InOutExpo => easeInOutExpo(start, end, alpha), 
			MegaEaseType.InOutCirc => easeInOutCirc(start, end, alpha), 
			MegaEaseType.InQuad => easeInOutQuad(start, end, alpha), 
			MegaEaseType.OutQuad => easeInQuad(start, end, alpha), 
			MegaEaseType.InOutQuad => easeOutQuad(start, end, alpha), 
			MegaEaseType.InOutCubic => easeInOutCubic(start, end, alpha), 
			MegaEaseType.InOutQuart => easeInOutQuart(start, end, alpha), 
			MegaEaseType.InQuint => easeInQuint(start, end, alpha), 
			MegaEaseType.OutQuint => easeOutQuint(start, end, alpha), 
			MegaEaseType.InOutQuint => easeInOutQuint(start, end, alpha), 
			MegaEaseType.InBack => easeInBack(start, end, alpha), 
			MegaEaseType.OutBack => easeOutBack(start, end, alpha), 
			MegaEaseType.InOutBack => easeInOutBack(start, end, alpha), 
			MegaEaseType.spring => spring(start, end, alpha), 
			MegaEaseType.Clerp => clerp(start, end, alpha), 
			MegaEaseType.Lerp => Lerp(start, end, alpha), 
			_ => nice(start, end, val, alpha), 
		};
	}

	public static Vector3 SpringDamp(Vector3 curr, Vector3 trg, ref Vector3 velocity, float time, float tau, float critical)
	{
		Vector3 vector = -1f / (tau * tau) * (curr - trg) - critical * 2f / tau * velocity;
		Vector3 vector2 = velocity + vector * time;
		return curr += vector2 * time;
	}

	public static float SpringDamp(float curr, float trg, ref float velocity, float time, float tau, float critical)
	{
		float num = -1f / (tau * tau) * (curr - trg) - critical * 2f / tau * velocity;
		velocity += num * time;
		return curr += velocity * time;
	}

	private float clerp(float start, float end, float value)
	{
		float num = 0f;
		float num2 = 360f;
		float num3 = Mathf.Abs((num2 - num) / 2f);
		float num4 = 0f;
		if (end - start < 0f - num3)
		{
			num4 = (num2 - start + end) * value;
			return start + num4;
		}
		if (end - start > num3)
		{
			num4 = (0f - (num2 - end + start)) * value;
			return start + num4;
		}
		return start + (end - start) * value;
	}

	private float nice(float start, float end, float val, float alpha)
	{
		return Mathf.Lerp(start, end, alpha);
	}

	private float bounce(float start, float end, float value)
	{
		value /= 1f;
		end -= start;
		if (value < 0.36363637f)
		{
			return end * (7.5625f * value * value) + start;
		}
		if (value < 0.72727275f)
		{
			value -= 0.54545456f;
			return end * (7.5625f * value * value + 0.75f) + start;
		}
		if (value < 0.90909094f)
		{
			value -= 0.8181818f;
			return end * (7.5625f * value * value + 0.9375f) + start;
		}
		value -= 21f / 22f;
		return end * (7.5625f * value * value + 63f / 64f) + start;
	}

	private float easeInSine(float start, float end, float value)
	{
		end -= start;
		return (0f - end) * Mathf.Cos(value / 1f * ((float)Math.PI / 2f)) + end + start;
	}

	private float easeInQuint(float start, float end, float value)
	{
		value /= 1f;
		end -= start;
		return end * value * value * value * value * value + start;
	}

	private float easeOutQuint(float start, float end, float value)
	{
		value /= 1f;
		value -= 1f;
		end -= start;
		return end * (value * value * value * value * value + 1f) + start;
	}

	private float easeInQuad(float start, float end, float value)
	{
		value /= 1f;
		end -= start;
		return end * value * value + start;
	}

	private float easeOutQuad(float start, float end, float value)
	{
		value /= 1f;
		end -= start;
		return (0f - end) * value * (value - 2f) + start;
	}

	private float easeInOutSine(float start, float end, float value)
	{
		end -= start;
		return (0f - end) / 2f * (Mathf.Cos((float)Math.PI * value / 1f) - 1f) + start;
	}

	private float easeInOutExpo(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end / 2f * Mathf.Pow(2f, 10f * (value - 1f)) + start;
		}
		value -= 1f;
		return end / 2f * (0f - Mathf.Pow(2f, -10f * value) + 2f) + start;
	}

	private float easeInOutCirc(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return (0f - end) / 2f * (Mathf.Sqrt(1f - value * value) - 1f) + start;
		}
		value -= 2f;
		return end / 2f * (Mathf.Sqrt(1f - value * value) + 1f) + start;
	}

	private float easeInOutQuad(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end / 2f * value * value + start;
		}
		value -= 1f;
		return (0f - end) / 2f * (value * (value - 2f) - 1f) + start;
	}

	public float easeInOutCubic(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end / 2f * value * value * value + start;
		}
		value -= 2f;
		return end / 2f * (value * value * value + 2f) + start;
	}

	private float easeInOutQuart(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end / 2f * value * value * value * value + start;
		}
		value -= 2f;
		return (0f - end) / 2f * (value * value * value * value - 2f) + start;
	}

	private float easeInOutQuint(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end / 2f * value * value * value * value * value + start;
		}
		value -= 2f;
		return end / 2f * (value * value * value * value * value + 2f) + start;
	}

	private float easeInBack(float start, float end, float value)
	{
		end -= start;
		value /= 1f;
		float num = 1.70158f;
		return end * value * value * ((num + 1f) * value - num) + start;
	}

	private float easeOutBack(float start, float end, float value)
	{
		float num = 1.70158f;
		end -= start;
		value = value / 1f - 1f;
		return end * (value * value * ((num + 1f) * value + num) + 1f) + start;
	}

	private float easeInOutBack(float start, float end, float value)
	{
		float num = 1.70158f;
		end -= start;
		value /= 0.5f;
		if (value < 1f)
		{
			num *= 1.525f;
			return end / 2f * (value * value * ((num + 1f) * value - num)) + start;
		}
		value -= 2f;
		num *= 1.525f;
		return end / 2f * (value * value * ((num + 1f) * value + num) + 2f) + start;
	}

	private float spring(float start, float end, float value)
	{
		value = Mathf.Clamp01(value);
		value = (Mathf.Sin(value * (float)Math.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + 1.2f * (1f - value));
		return start + (end - start) * value;
	}

	private float Lerp(float start, float end, float value)
	{
		return Mathf.Lerp(start, end, value);
	}

	public float SpringDamp(float curr, float trg, float velocity, float time, float tau, float critical)
	{
		float num = -1f / (tau * tau) * (curr - trg) - critical * 2f / tau * velocity;
		float num2 = velocity + num * time / 1f;
		return curr += num2 * time;
	}
}
