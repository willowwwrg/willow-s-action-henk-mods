using System;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes;

[Serializable]
public struct ObscuredInt : IEquatable<ObscuredInt>
{
	private static int cryptoKey = 444444;

	[SerializeField]
	private int currentCryptoKey;

	[SerializeField]
	private int hiddenValue;

	private int fakeValue;

	private bool inited;

	private ObscuredInt(int value)
	{
		currentCryptoKey = cryptoKey;
		hiddenValue = value;
		fakeValue = 0;
		inited = true;
	}

	public static void SetNewCryptoKey(int newKey)
	{
		cryptoKey = newKey;
	}

	public int GetEncrypted()
	{
		if (currentCryptoKey != cryptoKey)
		{
			hiddenValue = InternalDecrypt();
			hiddenValue = Encrypt(hiddenValue, cryptoKey);
			currentCryptoKey = cryptoKey;
		}
		return hiddenValue;
	}

	public void SetEncrypted(int encrypted)
	{
		hiddenValue = encrypted;
		if (ObscuredCheatingDetector.isRunning)
		{
			fakeValue = InternalDecrypt();
		}
	}

	public static int Encrypt(int value)
	{
		return Encrypt(value, 0);
	}

	public static int Decrypt(int value)
	{
		return Decrypt(value, 0);
	}

	public static int Encrypt(int value, int key)
	{
		if (value == 0)
		{
			return 0;
		}
		if (key == 0)
		{
			return value ^ cryptoKey;
		}
		return value ^ key;
	}

	public static int Decrypt(int value, int key)
	{
		if (value == 0)
		{
			return 0;
		}
		if (key == 0)
		{
			return value ^ cryptoKey;
		}
		return value ^ key;
	}

	private int InternalDecrypt()
	{
		if (!inited)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = Encrypt(0);
			fakeValue = 0;
			inited = true;
		}
		int key = cryptoKey;
		if (currentCryptoKey != cryptoKey)
		{
			key = currentCryptoKey;
		}
		int num = Decrypt(hiddenValue, key);
		if (ObscuredCheatingDetector.isRunning && fakeValue != 0 && num != fakeValue)
		{
			ObscuredCheatingDetector.Instance.OnCheatingDetected();
		}
		return num;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ObscuredInt obscuredInt))
		{
			return false;
		}
		return hiddenValue == obscuredInt.hiddenValue;
	}

	public bool Equals(ObscuredInt obj)
	{
		return hiddenValue == obj.hiddenValue;
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

	public string ToString(IFormatProvider provider)
	{
		return InternalDecrypt().ToString(provider);
	}

	public string ToString(string format, IFormatProvider provider)
	{
		return InternalDecrypt().ToString(format, provider);
	}

	public static implicit operator ObscuredInt(int value)
	{
		ObscuredInt result = new ObscuredInt(Encrypt(value));
		if (ObscuredCheatingDetector.isRunning)
		{
			result.fakeValue = value;
		}
		return result;
	}

	public static implicit operator int(ObscuredInt value)
	{
		return value.InternalDecrypt();
	}

	public static ObscuredInt operator ++(ObscuredInt input)
	{
		int value = input.InternalDecrypt() + 1;
		input.hiddenValue = Encrypt(value, input.currentCryptoKey);
		if (ObscuredCheatingDetector.isRunning)
		{
			input.fakeValue = value;
		}
		return input;
	}

	public static ObscuredInt operator --(ObscuredInt input)
	{
		int value = input.InternalDecrypt() - 1;
		input.hiddenValue = Encrypt(value, input.currentCryptoKey);
		if (ObscuredCheatingDetector.isRunning)
		{
			input.fakeValue = value;
		}
		return input;
	}
}
