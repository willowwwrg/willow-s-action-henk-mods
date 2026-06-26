using System;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes;

public struct ObscuredVector2
{
	private static int cryptoKey = 120206;

	private int currentCryptoKey;

	private Vector2 hiddenValue;

	private Vector2 fakeValue;

	private bool inited;

	public float x
	{
		get
		{
			float num = InternalDecryptField(hiddenValue.x);
			if (ObscuredCheatingDetector.isRunning && fakeValue != new Vector2(0f, 0f) && Math.Abs(num - fakeValue.x) > 0.0005f)
			{
				ObscuredCheatingDetector.Instance.OnCheatingDetected();
			}
			return num;
		}
		set
		{
			hiddenValue.x = InternalEncryptField(value);
			if (ObscuredCheatingDetector.isRunning)
			{
				fakeValue.x = value;
			}
		}
	}

	public float y
	{
		get
		{
			float num = InternalDecryptField(hiddenValue.y);
			if (ObscuredCheatingDetector.isRunning && fakeValue != new Vector2(0f, 0f) && Math.Abs(num - fakeValue.y) > 0.0005f)
			{
				ObscuredCheatingDetector.Instance.OnCheatingDetected();
			}
			return num;
		}
		set
		{
			hiddenValue.y = InternalEncryptField(value);
			if (ObscuredCheatingDetector.isRunning)
			{
				fakeValue.y = value;
			}
		}
	}

	public float this[int index]
	{
		get
		{
			return index switch
			{
				0 => x, 
				1 => y, 
				_ => throw new IndexOutOfRangeException("Invalid ObscuredVector2 index!"), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				x = value;
				break;
			case 1:
				y = value;
				break;
			default:
				throw new IndexOutOfRangeException("Invalid ObscuredVector2 index!");
			}
		}
	}

	private ObscuredVector2(Vector2 value)
	{
		currentCryptoKey = cryptoKey;
		hiddenValue = value;
		fakeValue = new Vector2(0f, 0f);
		inited = true;
	}

	public static void SetNewCryptoKey(int newKey)
	{
		cryptoKey = newKey;
	}

	public Vector2 GetEncrypted()
	{
		if (currentCryptoKey != cryptoKey)
		{
			Vector2 value = InternalDecrypt();
			hiddenValue = Encrypt(value, cryptoKey);
			currentCryptoKey = cryptoKey;
		}
		return hiddenValue;
	}

	public void SetEncrypted(Vector2 encrypted)
	{
		hiddenValue = encrypted;
		if (ObscuredCheatingDetector.isRunning)
		{
			fakeValue = InternalDecrypt();
		}
	}

	public static Vector2 Encrypt(Vector2 value)
	{
		return Encrypt(value, 0);
	}

	public static Vector2 Encrypt(Vector2 value, int key)
	{
		if (key == 0)
		{
			key = cryptoKey;
		}
		value.x = ObscuredDouble.Encrypt(value.x, key);
		value.y = ObscuredDouble.Encrypt(value.y, key);
		return value;
	}

	public static Vector2 Decrypt(Vector2 value)
	{
		return Decrypt(value, 0);
	}

	public static Vector2 Decrypt(Vector2 value, int key)
	{
		if (key == 0)
		{
			key = cryptoKey;
		}
		value.x = (float)ObscuredDouble.Decrypt((long)value.x, key);
		value.y = (float)ObscuredDouble.Decrypt((long)value.y, key);
		return value;
	}

	private Vector2 InternalDecrypt()
	{
		if (!inited)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = Encrypt(new Vector2(0f, 0f));
			fakeValue = new Vector2(0f, 0f);
			inited = true;
		}
		int num = cryptoKey;
		if (currentCryptoKey != cryptoKey)
		{
			num = currentCryptoKey;
		}
		Vector2 vector = new Vector2
		{
			x = (float)ObscuredDouble.Decrypt((long)hiddenValue.x, num),
			y = (float)ObscuredDouble.Decrypt((long)hiddenValue.y, num)
		};
		if (ObscuredCheatingDetector.isRunning && !fakeValue.Equals(Vector2.zero) && !CompareVectorsWithTolerance(vector, fakeValue))
		{
			ObscuredCheatingDetector.Instance.OnCheatingDetected();
		}
		return vector;
	}

	private bool CompareVectorsWithTolerance(Vector2 vector1, Vector2 vector2)
	{
		if (Math.Abs(vector1.x - vector2.x) < 0.01f)
		{
			return Math.Abs(vector1.y - vector2.y) < 0.01f;
		}
		return false;
	}

	private float InternalDecryptField(float encrypted)
	{
		int num = cryptoKey;
		if (currentCryptoKey != cryptoKey)
		{
			num = currentCryptoKey;
		}
		return (float)ObscuredDouble.Decrypt((long)encrypted, num);
	}

	private float InternalEncryptField(float encrypted)
	{
		return ObscuredDouble.Encrypt(encrypted, cryptoKey);
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

	public static implicit operator ObscuredVector2(Vector2 value)
	{
		ObscuredVector2 result = new ObscuredVector2(Encrypt(value));
		if (ObscuredCheatingDetector.isRunning)
		{
			result.fakeValue = value;
		}
		return result;
	}

	public static implicit operator Vector2(ObscuredVector2 value)
	{
		return value.InternalDecrypt();
	}
}
