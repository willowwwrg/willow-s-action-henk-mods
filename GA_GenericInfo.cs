using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class GA_GenericInfo
{
	private string _userID = string.Empty;

	private string _sessionID;

	private bool _settingUserID;

	public string UserID
	{
		get
		{
			if ((_userID == null || _userID == string.Empty) && !GA.SettingsGA.CustomUserID)
			{
				_userID = GetUserUUID();
			}
			return _userID;
		}
	}

	public string SessionID
	{
		get
		{
			if (_sessionID == null)
			{
				_sessionID = GetSessionUUID();
			}
			return _sessionID;
		}
	}

	public List<Hashtable> GetGenericInfo(string message)
	{
		return new List<Hashtable>
		{
			AddSystemSpecs(GA_Error.SeverityType.info, "unity_sdk " + GA_Settings.VERSION, message),
			AddSystemSpecs(GA_Error.SeverityType.info, "os:" + SystemInfo.operatingSystem, message),
			AddSystemSpecs(GA_Error.SeverityType.info, "processor_type:" + SystemInfo.processorType, message),
			AddSystemSpecs(GA_Error.SeverityType.info, "gfx_name:" + SystemInfo.graphicsDeviceName, message),
			AddSystemSpecs(GA_Error.SeverityType.info, "gfx_version:" + SystemInfo.graphicsDeviceVersion, message)
		};
	}

	public static string GetUserUUID()
	{
		try
		{
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			string text = string.Empty;
			NetworkInterface[] array = allNetworkInterfaces;
			for (int i = 0; i < array.Length; i++)
			{
				PhysicalAddress physicalAddress = array[i].GetPhysicalAddress();
				if (physicalAddress.ToString() != string.Empty && text == string.Empty)
				{
					byte[] bytes = Encoding.UTF8.GetBytes(physicalAddress.ToString());
					text = BitConverter.ToString(new SHA1CryptoServiceProvider().ComputeHash(bytes)).Replace("-", string.Empty);
				}
			}
			return text;
		}
		catch
		{
			return SystemInfo.deviceUniqueIdentifier;
		}
	}

	public static string GetSessionUUID()
	{
		return Guid.NewGuid().ToString();
	}

	public void SetSessionUUID(string newSessionID)
	{
		if (newSessionID == null)
		{
			_sessionID = GetSessionUUID();
		}
		else
		{
			_sessionID = newSessionID;
		}
	}

	public void SetCustomUserID(string customID)
	{
		_userID = customID;
	}

	private Hashtable AddSystemSpecs(GA_Error.SeverityType severity, string type, string message)
	{
		string text = string.Empty;
		if (message != string.Empty)
		{
			text = ": " + message;
		}
		return new Hashtable
		{
			{
				GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Severity],
				severity.ToString()
			},
			{
				GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Message],
				type + text
			},
			{
				GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Level],
				(!GA.SettingsGA.CustomArea.Equals(string.Empty)) ? GA.SettingsGA.CustomArea : Application.loadedLevelName
			}
		};
	}

	public static string GetSystem()
	{
		return "PC";
	}
}
