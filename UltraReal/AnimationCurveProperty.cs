using System;
using UnityEngine;

namespace UltraReal;

[Serializable]
public class AnimationCurveProperty
{
	public AnimationCurve flickerCurve;

	public float timeLength = 1f;

	public bool randomStartTime;

	private float _startTime;

	private float _currentTime;

	public float EvaluateStep(float delta)
	{
		if (_startTime == 0f && randomStartTime)
		{
			_startTime = UnityEngine.Random.Range(0.01f, 1f);
		}
		_currentTime += delta;
		return flickerCurve.Evaluate((_currentTime / timeLength + _startTime) % 1f);
	}
}
