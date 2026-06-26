using System;
using CodeStage.AntiCheat.Detectors;

namespace CodeStage.AntiCheat.ObscuredTypes;

public struct ObscuredChar : IEquatable<ObscuredChar>
{
	private static char cryptoKey = '—';

	private char currentCryptoKey;

	private char hiddenValue;

	private char fakeValue;

	private bool inited;

	private ObscuredChar(char value)
	{
		currentCryptoKey = cryptoKey;
		hiddenValue = value;
		fakeValue = '\0';
		inited = true;
	}

	public static void SetNewCryptoKey(char newKey)
	{
		cryptoKey = newKey;
	}

	public char GetEncrypted()
	{
		if (currentCryptoKey != cryptoKey)
		{
			hiddenValue = InternalDecrypt();
			hiddenValue = EncryptDecrypt(hiddenValue, cryptoKey);
			currentCryptoKey = cryptoKey;
		}
		return hiddenValue;
	}

	public void SetEncrypted(char encrypted)
	{
		hiddenValue = encrypted;
		if (ObscuredCheatingDetector.isRunning)
		{
			fakeValue = InternalDecrypt();
		}
	}

	public static char EncryptDecrypt(char value)
	{
		return EncryptDecrypt(value, '\0');
	}

	public static char EncryptDecrypt(char value, char key)
	{
		if (key == '\0')
		{
			return (char)(value ^ cryptoKey);
		}
		return (char)(value ^ key);
	}

	private char InternalDecrypt()
	{
		if (!inited)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = EncryptDecrypt('\0');
			fakeValue = '\0';
			inited = true;
		}
		char key = cryptoKey;
		if (currentCryptoKey != cryptoKey)
		{
			key = currentCryptoKey;
		}
		char c = EncryptDecrypt(hiddenValue, key);
		if (ObscuredCheatingDetector.isRunning && fakeValue != 0 && c != fakeValue)
		{
			ObscuredCheatingDetector.Instance.OnCheatingDetected();
		}
		return c;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ObscuredChar obscuredChar))
		{
			return false;
		}
		return hiddenValue == obscuredChar.hiddenValue;
	}

	public bool Equals(ObscuredChar obj)
	{
		return hiddenValue == obj.hiddenValue;
	}

	public override string ToString()
	{
		return InternalDecrypt().ToString();
	}

	public string ToString(IFormatProvider provider)
	{
		return InternalDecrypt().ToString(provider);
	}

	public override int GetHashCode()
	{
		return InternalDecrypt().GetHashCode();
	}

	public static implicit operator ObscuredChar(char value)
	{
		ObscuredChar result = new ObscuredChar(EncryptDecrypt(value));
		if (ObscuredCheatingDetector.isRunning)
		{
			result.fakeValue = value;
		}
		return result;
	}

	public static implicit operator char(ObscuredChar value)
	{
		return value.InternalDecrypt();
	}

	public static ObscuredChar operator ++(ObscuredChar input)
	{
		char value = (char)(input.InternalDecrypt() + 1);
		input.hiddenValue = EncryptDecrypt(value, input.currentCryptoKey);
		if (ObscuredCheatingDetector.isRunning)
		{
			input.fakeValue = value;
		}
		return input;
	}

	public static ObscuredChar operator --(ObscuredChar input)
	{
		char value = (char)(input.InternalDecrypt() - 1);
		input.hiddenValue = EncryptDecrypt(value, input.currentCryptoKey);
		if (ObscuredCheatingDetector.isRunning)
		{
			input.fakeValue = value;
		}
		return input;
	}
}
