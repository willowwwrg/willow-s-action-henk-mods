using System;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes;

public struct ObscuredQuaternion
{
	private static int cryptoKey = 120205;

	private int currentCryptoKey;

	private Quaternion hiddenValue;

	public Quaternion fakeValue;

	private bool inited;

	private ObscuredQuaternion(Quaternion value)
	{
		currentCryptoKey = cryptoKey;
		hiddenValue = value;
		fakeValue = Quaternion.identity;
		inited = true;
	}

	public static void SetNewCryptoKey(int newKey)
	{
		cryptoKey = newKey;
	}

	public Quaternion GetEncrypted()
	{
		if (currentCryptoKey != cryptoKey)
		{
			Quaternion value = InternalDecrypt();
			hiddenValue = Encrypt(value, cryptoKey);
			currentCryptoKey = cryptoKey;
		}
		return hiddenValue;
	}

	public void SetEncrypted(Quaternion encrypted)
	{
		hiddenValue = encrypted;
		if (ObscuredCheatingDetector.isRunning)
		{
			fakeValue = InternalDecrypt();
		}
	}

	public static Quaternion Encrypt(Quaternion value)
	{
		return Encrypt(value, 0);
	}

	public static Quaternion Encrypt(Quaternion value, int key)
	{
		if (key == 0)
		{
			key = cryptoKey;
		}
		value.x = ObscuredDouble.Encrypt(value.x, key);
		value.y = ObscuredDouble.Encrypt(value.y, key);
		value.z = ObscuredDouble.Encrypt(value.z, key);
		value.w = ObscuredDouble.Encrypt(value.w, key);
		return value;
	}

	public static Quaternion Decrypt(Quaternion value)
	{
		return Decrypt(value, 0);
	}

	public static Quaternion Decrypt(Quaternion value, int key)
	{
		if (key == 0)
		{
			key = cryptoKey;
		}
		value.x = (float)ObscuredDouble.Decrypt((long)value.x, key);
		value.y = (float)ObscuredDouble.Decrypt((long)value.y, key);
		value.z = (float)ObscuredDouble.Decrypt((long)value.z, key);
		value.w = (float)ObscuredDouble.Decrypt((long)value.w, key);
		return value;
	}

	private Quaternion InternalDecrypt()
	{
		if (!inited)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = Encrypt(Quaternion.identity);
			fakeValue = Quaternion.identity;
			inited = true;
		}
		int num = cryptoKey;
		if (currentCryptoKey != cryptoKey)
		{
			num = currentCryptoKey;
		}
		Quaternion quaternion = new Quaternion
		{
			x = (float)ObscuredDouble.Decrypt((long)hiddenValue.x, num),
			y = (float)ObscuredDouble.Decrypt((long)hiddenValue.y, num),
			z = (float)ObscuredDouble.Decrypt((long)hiddenValue.z, num),
			w = (float)ObscuredDouble.Decrypt((long)hiddenValue.w, num)
		};
		if (ObscuredCheatingDetector.isRunning && !fakeValue.Equals(Quaternion.identity) && !CompareQuaternionsWithTolerance(quaternion, fakeValue))
		{
			ObscuredCheatingDetector.Instance.OnCheatingDetected();
		}
		return quaternion;
	}

	private bool CompareQuaternionsWithTolerance(Quaternion q1, Quaternion q2)
	{
		if (Math.Abs(q1.x - q2.x) < 0.01f && Math.Abs(q1.y - q2.y) < 0.01f && Math.Abs(q1.z - q2.z) < 0.01f)
		{
			return Math.Abs(q1.w - q2.w) < 0.01f;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return InternalDecrypt().GetHashCode();
	}

	public override string ToString()
	{
		return InternalDecrypt().ToString();
	}

	public string ToString(string format)
	{
		return InternalDecrypt().ToString(format);
	}

	public static implicit operator ObscuredQuaternion(Quaternion value)
	{
		ObscuredQuaternion result = new ObscuredQuaternion(Encrypt(value));
		if (ObscuredCheatingDetector.isRunning)
		{
			result.fakeValue = value;
		}
		return result;
	}

	public static implicit operator Quaternion(ObscuredQuaternion value)
	{
		return value.InternalDecrypt();
	}
}
