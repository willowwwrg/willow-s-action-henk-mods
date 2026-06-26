using System;
using CodeStage.AntiCheat.Detectors;

namespace CodeStage.AntiCheat.ObscuredTypes;

public struct ObscuredBool : IEquatable<ObscuredBool>
{
	private static byte cryptoKey = 215;

	private byte currentCryptoKey;

	private int hiddenValue;

	private bool? fakeValue;

	private bool inited;

	private ObscuredBool(int value)
	{
		currentCryptoKey = cryptoKey;
		hiddenValue = value;
		fakeValue = null;
		inited = true;
	}

	public static void SetNewCryptoKey(byte newKey)
	{
		cryptoKey = newKey;
	}

	public int GetEncrypted()
	{
		if (currentCryptoKey != cryptoKey)
		{
			bool value = InternalDecrypt();
			hiddenValue = Encrypt(value, cryptoKey);
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

	public static int Encrypt(bool value)
	{
		return Encrypt(value, 0);
	}

	public static int Encrypt(bool value, byte key)
	{
		if (key == 0)
		{
			key = cryptoKey;
		}
		return ((!value) ? 181 : 213) ^ key;
	}

	public static bool Decrypt(int value)
	{
		return Decrypt(value, 0);
	}

	public static bool Decrypt(int value, byte key)
	{
		if (key == 0)
		{
			key = cryptoKey;
		}
		value ^= key;
		return value != 181;
	}

	private bool InternalDecrypt()
	{
		if (!inited)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = Encrypt(value: false);
			fakeValue = false;
			inited = true;
		}
		byte b = cryptoKey;
		if (currentCryptoKey != cryptoKey)
		{
			b = currentCryptoKey;
		}
		bool flag = (hiddenValue ^ b) != 181;
		if (ObscuredCheatingDetector.isRunning)
		{
			bool? flag2 = fakeValue;
			if (flag2.HasValue && flag != fakeValue)
			{
				ObscuredCheatingDetector.Instance.OnCheatingDetected();
			}
		}
		return flag;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ObscuredBool obscuredBool))
		{
			return false;
		}
		return hiddenValue == obscuredBool.hiddenValue;
	}

	public bool Equals(ObscuredBool obj)
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

	public static implicit operator ObscuredBool(bool value)
	{
		ObscuredBool result = new ObscuredBool(Encrypt(value));
		if (ObscuredCheatingDetector.isRunning)
		{
			result.fakeValue = value;
		}
		return result;
	}

	public static implicit operator bool(ObscuredBool value)
	{
		return value.InternalDecrypt();
	}
}
