using System;
using UnityEngine;

public class MegaLoftEase
{
	public delegate float easingFunction(float start, float end, float value);

	public easingFunction easing;

	public MegaLoftEase()
	{
		easing = easeInOutSine;
	}

	public void SetEasing(MegaLoftEaseType ease)
	{
		easing = GetEasing(ease);
	}

	public easingFunction GetEasing(MegaLoftEaseType ease)
	{
		return ease switch
		{
			MegaLoftEaseType.Sine => easeInOutSine, 
			MegaLoftEaseType.Expo => easeInOutExpo, 
			MegaLoftEaseType.Circ => easeInOutCirc, 
			MegaLoftEaseType.Quad => easeInOutQuad, 
			MegaLoftEaseType.Cubic => easeInOutCubic, 
			MegaLoftEaseType.Quart => easeInOutQuart, 
			MegaLoftEaseType.Quint => easeInOutQuint, 
			MegaLoftEaseType.Back => easeInOutBack, 
			MegaLoftEaseType.Square => easeInOutSquare, 
			_ => easeLerp, 
		};
	}

	private float easeInOutSquare(float start, float end, float value)
	{
		if (value < 0.5f)
		{
			return start;
		}
		return end;
	}

	private float easeInOutSine(float start, float end, float value)
	{
		end -= start;
		return (0f - end) / 2f * (Mathf.Cos((float)Math.PI * value / 1f) - 1f) + start;
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

	private float easeLerp(float start, float end, float value)
	{
		return Mathf.Lerp(start, end, value);
	}
}
