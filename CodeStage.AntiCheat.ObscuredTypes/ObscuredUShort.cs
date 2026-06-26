using System;
using CodeStage.AntiCheat.Detectors;

namespace CodeStage.AntiCheat.ObscuredTypes;

public struct ObscuredUShort : IEquatable<ObscuredUShort>
{
	private static ushort cryptoKey = 224;

	private ushort currentCryptoKey;

	private ushort hiddenValue;

	private ushort fakeValue;

	private bool inited;

	private ObscuredUShort(ushort value)
	{
		currentCryptoKey = cryptoKey;
		hiddenValue = value;
		fakeValue = 0;
		inited = true;
	}

	public static void SetNewCryptoKey(ushort newKey)
	{
		cryptoKey = newKey;
	}

	public ushort GetEncrypted()
	{
		if (currentCryptoKey != cryptoKey)
		{
			hiddenValue = InternalDecrypt();
			hiddenValue = EncryptDecrypt(hiddenValue, cryptoKey);
			currentCryptoKey = cryptoKey;
		}
		return hiddenValue;
	}

	public void SetEncrypted(ushort encrypted)
	{
		hiddenValue = encrypted;
		if (ObscuredCheatingDetector.isRunning)
		{
			fakeValue = InternalDecrypt();
		}
	}

	public static ushort EncryptDecrypt(ushort value)
	{
		return EncryptDecrypt(value, 0);
	}

	public static ushort EncryptDecrypt(ushort value, ushort key)
	{
		if (key == 0)
		{
			return (ushort)(value ^ cryptoKey);
		}
		return (ushort)(value ^ key);
	}

	private ushort InternalDecrypt()
	{
		if (!inited)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = EncryptDecrypt(0);
			fakeValue = 0;
			inited = true;
		}
		ushort key = cryptoKey;
		if (currentCryptoKey != cryptoKey)
		{
			key = currentCryptoKey;
		}
		ushort num = EncryptDecrypt(hiddenValue, key);
		if (ObscuredCheatingDetector.isRunning && fakeValue != 0 && num != fakeValue)
		{
			ObscuredCheatingDetector.Instance.OnCheatingDetected();
		}
		return num;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ObscuredUShort obscuredUShort))
		{
			return false;
		}
		return hiddenValue == obscuredUShort.hiddenValue;
	}

	public bool Equals(ObscuredUShort obj)
	{
		return hiddenValue == obj.hiddenValue;
	}

	public override string ToString()
	{
		return InternalDecrypt().ToString();
	}

	public string ToString(string format)
	{
		return InternalDecrypt().ToString(format);
	}

	public override int GetHashCode()
	{
		return InternalDecrypt().GetHashCode();
	}

	public string ToString(IFormatProvider provider)
	{
		return InternalDecrypt().ToString(provider);
	}

	public string ToString(string format, IFormatProvider provider)
	{
		return InternalDecrypt().ToString(format, provider);
	}

	public static implicit operator ObscuredUShort(ushort value)
	{
		ObscuredUShort result = new ObscuredUShort(EncryptDecrypt(value));
		if (ObscuredCheatingDetector.isRunning)
		{
			result.fakeValue = value;
		}
		return result;
	}

	public static implicit operator ushort(ObscuredUShort value)
	{
		return value.InternalDecrypt();
	}

	public static ObscuredUShort operator ++(ObscuredUShort input)
	{
		ushort value = (ushort)(input.InternalDecrypt() + 1);
		input.hiddenValue = EncryptDecrypt(value, input.currentCryptoKey);
		if (ObscuredCheatingDetector.isRunning)
		{
			input.fakeValue = value;
		}
		return input;
	}

	public static ObscuredUShort operator --(ObscuredUShort input)
	{
		ushort value = (ushort)(input.InternalDecrypt() - 1);
		input.hiddenValue = EncryptDecrypt(value, input.currentCryptoKey);
		if (ObscuredCheatingDetector.isRunning)
		{
			input.fakeValue = value;
		}
		return input;
	}
}
