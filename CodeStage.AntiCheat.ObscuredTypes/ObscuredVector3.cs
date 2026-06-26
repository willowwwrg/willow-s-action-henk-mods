using System;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes;

public struct ObscuredVector3
{
	private static int cryptoKey = 120207;

	private int currentCryptoKey;

	private Vector3 hiddenValue;

	private Vector3 fakeValue;

	private bool inited;

	public float x
	{
		get
		{
			float num = InternalDecryptField(hiddenValue.x);
			if (ObscuredCheatingDetector.isRunning && fakeValue != new Vector3(0f, 0f) && Math.Abs(num - fakeValue.x) > 0.0005f)
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
			if (ObscuredCheatingDetector.isRunning && fakeValue != new Vector3(0f, 0f) && Math.Abs(num - fakeValue.y) > 0.0005f)
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

	public float z
	{
		get
		{
			float num = InternalDecryptField(hiddenValue.z);
			if (ObscuredCheatingDetector.isRunning && fakeValue != new Vector3(0f, 0f) && Math.Abs(num - fakeValue.z) > 0.0005f)
			{
				ObscuredCheatingDetector.Instance.OnCheatingDetected();
			}
			return num;
		}
		set
		{
			hiddenValue.z = InternalEncryptField(value);
			if (ObscuredCheatingDetector.isRunning)
			{
				fakeValue.z = value;
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
				2 => z, 
				_ => throw new IndexOutOfRangeException("Invalid ObscuredVector3 index!"), 
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
			case 2:
				z = value;
				break;
			default:
				throw new IndexOutOfRangeException("Invalid ObscuredVector3 index!");
			}
		}
	}

	private ObscuredVector3(Vector3 value)
	{
		currentCryptoKey = cryptoKey;
		hiddenValue = value;
		fakeValue = new Vector3(0f, 0f);
		inited = true;
	}

	public static void SetNewCryptoKey(int newKey)
	{
		cryptoKey = newKey;
	}

	public Vector3 GetEncrypted()
	{
		if (currentCryptoKey != cryptoKey)
		{
			Vector3 value = InternalDecrypt();
			hiddenValue = Encrypt(value, cryptoKey);
			currentCryptoKey = cryptoKey;
		}
		return hiddenValue;
	}

	public void SetEncrypted(Vector3 encrypted)
	{
		hiddenValue = encrypted;
		if (ObscuredCheatingDetector.isRunning)
		{
			fakeValue = InternalDecrypt();
		}
	}

	public static Vector3 Encrypt(Vector3 value)
	{
		return Encrypt(value, 0);
	}

	public static Vector3 Encrypt(Vector3 value, int key)
	{
		if (key == 0)
		{
			key = cryptoKey;
		}
		value.x = ObscuredDouble.Encrypt(value.x, key);
		value.y = ObscuredDouble.Encrypt(value.y, key);
		value.z = ObscuredDouble.Encrypt(value.z, key);
		return value;
	}

	public static Vector3 Decrypt(Vector3 value)
	{
		return Decrypt(value, 0);
	}

	public static Vector3 Decrypt(Vector3 value, int key)
	{
		if (key == 0)
		{
			key = cryptoKey;
		}
		value.x = (float)ObscuredDouble.Decrypt((long)value.x, key);
		value.y = (float)ObscuredDouble.Decrypt((long)value.y, key);
		value.z = (float)ObscuredDouble.Decrypt((long)value.z, key);
		return value;
	}

	private Vector3 InternalDecrypt()
	{
		if (!inited)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = Encrypt(new Vector3(0f, 0f));
			fakeValue = new Vector3(0f, 0f);
			inited = true;
		}
		int num = cryptoKey;
		if (currentCryptoKey != cryptoKey)
		{
			num = currentCryptoKey;
		}
		Vector3 vector = new Vector3
		{
			x = (float)ObscuredDouble.Decrypt((long)hiddenValue.x, num),
			y = (float)ObscuredDouble.Decrypt((long)hiddenValue.y, num),
			z = (float)ObscuredDouble.Decrypt((long)hiddenValue.z, num)
		};
		if (ObscuredCheatingDetector.isRunning && !fakeValue.Equals(Vector3.zero) && !CompareVectorsWithTolerance(vector, fakeValue))
		{
			ObscuredCheatingDetector.Instance.OnCheatingDetected();
		}
		return vector;
	}

	private bool CompareVectorsWithTolerance(Vector3 vector1, Vector3 vector2)
	{
		if (Math.Abs(vector1.x - vector2.x) < 0.01f && Math.Abs(vector1.y - vector2.y) < 0.01f)
		{
			return Math.Abs(vector1.z - vector2.z) < 0.01f;
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

	public override bool Equals(object other)
	{
		return InternalDecrypt().Equals(other);
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

	public static implicit operator ObscuredVector3(Vector3 value)
	{
		ObscuredVector3 result = new ObscuredVector3(Encrypt(value));
		if (ObscuredCheatingDetector.isRunning)
		{
			result.fakeValue = value;
		}
		return result;
	}

	public static implicit operator Vector3(ObscuredVector3 value)
	{
		return value.InternalDecrypt();
	}

	public static ObscuredVector3 operator +(ObscuredVector3 a, ObscuredVector3 b)
	{
		return a.InternalDecrypt() + b.InternalDecrypt();
	}

	public static ObscuredVector3 operator +(Vector3 a, ObscuredVector3 b)
	{
		return a + b.InternalDecrypt();
	}

	public static ObscuredVector3 operator +(ObscuredVector3 a, Vector3 b)
	{
		return a.InternalDecrypt() + b;
	}

	public static ObscuredVector3 operator -(ObscuredVector3 a, ObscuredVector3 b)
	{
		return a.InternalDecrypt() - b.InternalDecrypt();
	}

	public static ObscuredVector3 operator -(Vector3 a, ObscuredVector3 b)
	{
		return a - b.InternalDecrypt();
	}

	public static ObscuredVector3 operator -(ObscuredVector3 a, Vector3 b)
	{
		return a.InternalDecrypt() - b;
	}

	public static ObscuredVector3 operator -(ObscuredVector3 a)
	{
		return -a.InternalDecrypt();
	}

	public static ObscuredVector3 operator *(ObscuredVector3 a, float d)
	{
		return a.InternalDecrypt() * d;
	}

	public static ObscuredVector3 operator *(float d, ObscuredVector3 a)
	{
		return d * a.InternalDecrypt();
	}

	public static ObscuredVector3 operator /(ObscuredVector3 a, float d)
	{
		return a.InternalDecrypt() / d;
	}

	public static bool operator ==(ObscuredVector3 lhs, ObscuredVector3 rhs)
	{
		return lhs.InternalDecrypt() == rhs.InternalDecrypt();
	}

	public static bool operator ==(Vector3 lhs, ObscuredVector3 rhs)
	{
		return lhs == rhs.InternalDecrypt();
	}

	public static bool operator ==(ObscuredVector3 lhs, Vector3 rhs)
	{
		return lhs.InternalDecrypt() == rhs;
	}

	public static bool operator !=(ObscuredVector3 lhs, ObscuredVector3 rhs)
	{
		return lhs.InternalDecrypt() != rhs.InternalDecrypt();
	}

	public static bool operator !=(Vector3 lhs, ObscuredVector3 rhs)
	{
		return lhs != rhs.InternalDecrypt();
	}

	public static bool operator !=(ObscuredVector3 lhs, Vector3 rhs)
	{
		return lhs.InternalDecrypt() != rhs;
	}
}
