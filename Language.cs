using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using UnityEngine;

public static class Language
{
	public static string settingsAssetPath;

	private static LocalizationSettings _settings;

	private static List<string> availableLanguages;

	private static LanguageCode currentLanguage;

	private static Dictionary<string, Dictionary<string, string>> currentEntrySheets;

	private static Dictionary<string, Dictionary<string, string>> englishEntrySheets;

	public static LocalizationSettings settings
	{
		get
		{
			if (_settings == null)
			{
				_settings = (LocalizationSettings)Resources.Load("Languages/" + Path.GetFileNameWithoutExtension(settingsAssetPath), typeof(LocalizationSettings));
			}
			return _settings;
		}
	}

	static Language()
	{
		settingsAssetPath = "Assets/Localization/Resources/Languages/LocalizationSettings.asset";
		LoadAvailableLanguages();
		BackUpEnglish();
		bool useSystemLanguagePerDefault = settings.useSystemLanguagePerDefault;
		LanguageCode code = LocalizationSettings.GetLanguageEnum(settings.defaultLangCode);
		string text = PlayerPrefs.GetString("M2H_lastLanguage", string.Empty);
		if (text != string.Empty && availableLanguages.Contains(text))
		{
			SwitchLanguage(text);
			return;
		}
		if (useSystemLanguagePerDefault)
		{
			LanguageCode languageCode = LanguageNameToCode(Application.systemLanguage);
			if (languageCode == LanguageCode.N)
			{
				string twoLetterISOLanguageName = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
				if (twoLetterISOLanguageName != "iv")
				{
					languageCode = LocalizationSettings.GetLanguageEnum(twoLetterISOLanguageName);
				}
			}
			if (availableLanguages.Contains(string.Concat(languageCode, string.Empty)))
			{
				code = languageCode;
			}
			else
			{
				switch (languageCode)
				{
				case LanguageCode.PT:
					if (availableLanguages.Contains(string.Concat(LanguageCode.PT_BR, string.Empty)))
					{
						code = LanguageCode.PT_BR;
					}
					break;
				case LanguageCode.EN:
					if (availableLanguages.Contains(string.Concat(LanguageCode.EN_GB, string.Empty)))
					{
						code = LanguageCode.EN_GB;
					}
					break;
				default:
					if (languageCode == LanguageCode.EN && availableLanguages.Contains(string.Concat(LanguageCode.EN_US, string.Empty)))
					{
						code = LanguageCode.EN_US;
					}
					break;
				}
			}
		}
		SwitchLanguage(code);
	}

	private static void LoadAvailableLanguages()
	{
		availableLanguages = new List<string>();
		if (settings.sheetTitles == null || settings.sheetTitles.Length == 0)
		{
			Debug.Log("None available");
			return;
		}
		foreach (int value in Enum.GetValues(typeof(LanguageCode)))
		{
			if (HasLanguageFile(string.Concat((LanguageCode)value, string.Empty), settings.sheetTitles[0]))
			{
				availableLanguages.Add(string.Concat((LanguageCode)value, string.Empty));
			}
		}
		Resources.UnloadUnusedAssets();
	}

	public static string[] GetLanguages()
	{
		return availableLanguages.ToArray();
	}

	public static bool SwitchLanguage(string langCode)
	{
		return SwitchLanguage(LocalizationSettings.GetLanguageEnum(langCode));
	}

	public static bool SwitchLanguage(LanguageCode code)
	{
		if (availableLanguages.Contains(string.Concat(code, string.Empty)))
		{
			DoSwitch(code);
			return true;
		}
		Debug.LogError(string.Concat("Could not switch from language ", currentLanguage, " to ", code));
		if (currentLanguage == LanguageCode.N)
		{
			if (availableLanguages.Count > 0)
			{
				DoSwitch(LocalizationSettings.GetLanguageEnum(availableLanguages[0]));
				Debug.LogError(string.Concat("Switched to ", currentLanguage, " instead"));
			}
			else
			{
				Debug.LogError(string.Concat("Please verify that you have the file: Resources/Languages/", code, string.Empty));
				Debug.Break();
			}
		}
		return false;
	}

	private static void BackUpEnglish()
	{
		englishEntrySheets = new Dictionary<string, Dictionary<string, string>>();
		string[] sheetTitles = settings.sheetTitles;
		foreach (string text in sheetTitles)
		{
			englishEntrySheets[text] = new Dictionary<string, string>();
			string englishFileContents = GetEnglishFileContents(text);
			if (!(englishFileContents != string.Empty))
			{
				continue;
			}
			using XmlReader xmlReader = XmlReader.Create(new StringReader(englishFileContents));
			while (xmlReader.ReadToFollowing("entry"))
			{
				xmlReader.MoveToFirstAttribute();
				string value = xmlReader.Value;
				xmlReader.MoveToElement();
				string s = xmlReader.ReadElementContentAsString().Trim();
				s = s.UnescapeXML();
				englishEntrySheets[text][value] = s;
			}
		}
	}

	private static void DoSwitch(LanguageCode newLang)
	{
		PlayerPrefs.SetString("M2H_lastLanguage", string.Concat(newLang, string.Empty));
		currentLanguage = newLang;
		currentEntrySheets = new Dictionary<string, Dictionary<string, string>>();
		string[] sheetTitles = settings.sheetTitles;
		foreach (string text in sheetTitles)
		{
			currentEntrySheets[text] = new Dictionary<string, string>();
			string languageFileContents = GetLanguageFileContents(text);
			if (!(languageFileContents != string.Empty))
			{
				continue;
			}
			using XmlReader xmlReader = XmlReader.Create(new StringReader(languageFileContents));
			while (xmlReader.ReadToFollowing("entry"))
			{
				xmlReader.MoveToFirstAttribute();
				string value = xmlReader.Value;
				xmlReader.MoveToElement();
				string s = xmlReader.ReadElementContentAsString().Trim();
				s = s.UnescapeXML();
				currentEntrySheets[text][value] = s;
			}
		}
		LocalizedAsset[] array = (LocalizedAsset[])UnityEngine.Object.FindObjectsOfType(typeof(LocalizedAsset));
		for (int j = 0; j < array.Length; j++)
		{
			array[j].LocalizeAsset();
		}
		SendMonoMessage("ChangedLanguage", currentLanguage);
	}

	public static UnityEngine.Object GetAsset(string name)
	{
		return Resources.Load(string.Concat("Languages/Assets/", CurrentLanguage(), "/", name));
	}

	private static bool HasLanguageFile(string lang, string sheetTitle)
	{
		return (TextAsset)Resources.Load("Languages/" + lang + "_" + sheetTitle, typeof(TextAsset)) != null;
	}

	private static string GetLanguageFileContents(string sheetTitle)
	{
		TextAsset textAsset = (TextAsset)Resources.Load(string.Concat("Languages/", currentLanguage, "_", sheetTitle), typeof(TextAsset));
		if (textAsset != null)
		{
			return textAsset.text;
		}
		return string.Empty;
	}

	private static string GetEnglishFileContents(string sheetTitle)
	{
		TextAsset textAsset = (TextAsset)Resources.Load("Languages/EN_" + sheetTitle, typeof(TextAsset));
		if (textAsset != null)
		{
			return textAsset.text;
		}
		return string.Empty;
	}

	public static LanguageCode CurrentLanguage()
	{
		return currentLanguage;
	}

	public static string Get(string key)
	{
		return Get(key, settings.sheetTitles[0]);
	}

	public static string Get(string key, string sheetTitle)
	{
		if (currentEntrySheets == null || !currentEntrySheets.ContainsKey(sheetTitle))
		{
			Debug.LogError("The sheet with title \"" + sheetTitle + "\" does not exist!");
			return string.Empty;
		}
		if (currentEntrySheets[sheetTitle].ContainsKey(key))
		{
			if (currentEntrySheets[sheetTitle][key] == string.Empty && englishEntrySheets[sheetTitle].ContainsKey(key))
			{
				return englishEntrySheets[sheetTitle][key];
			}
			return currentEntrySheets[sheetTitle][key];
		}
		if (englishEntrySheets[sheetTitle].ContainsKey(key))
		{
			return englishEntrySheets[sheetTitle][key];
		}
		return "#!#" + key + "#!#";
	}

	public static bool Has(string key)
	{
		return Has(key, settings.sheetTitles[0]);
	}

	public static bool Has(string key, string sheetTitle)
	{
		if (currentEntrySheets == null || !currentEntrySheets.ContainsKey(sheetTitle))
		{
			return false;
		}
		return currentEntrySheets[sheetTitle].ContainsKey(key);
	}

	private static void SendMonoMessage(string methodString, params object[] parameters)
	{
		if (parameters != null && parameters.Length > 1)
		{
			Debug.LogError("We cannot pass more than one argument currently!");
		}
		GameObject[] array = (GameObject[])UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
		foreach (GameObject gameObject in array)
		{
			if ((bool)gameObject && gameObject.transform.parent == null)
			{
				if (parameters != null && parameters.Length == 1)
				{
					gameObject.gameObject.BroadcastMessage(methodString, parameters[0], SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					gameObject.gameObject.BroadcastMessage(methodString, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	public static LanguageCode LanguageNameToCode(SystemLanguage name)
	{
		return name switch
		{
			SystemLanguage.Afrikaans => LanguageCode.AF, 
			SystemLanguage.Arabic => LanguageCode.AR, 
			SystemLanguage.Basque => LanguageCode.BA, 
			SystemLanguage.Belarusian => LanguageCode.BE, 
			SystemLanguage.Bulgarian => LanguageCode.BG, 
			SystemLanguage.Catalan => LanguageCode.CA, 
			SystemLanguage.Chinese => LanguageCode.ZH, 
			SystemLanguage.Czech => LanguageCode.CS, 
			SystemLanguage.Danish => LanguageCode.DA, 
			SystemLanguage.Dutch => LanguageCode.NL, 
			SystemLanguage.English => LanguageCode.EN, 
			SystemLanguage.Estonian => LanguageCode.ET, 
			SystemLanguage.Faroese => LanguageCode.FA, 
			SystemLanguage.Finnish => LanguageCode.FI, 
			SystemLanguage.French => LanguageCode.FR, 
			SystemLanguage.German => LanguageCode.DE, 
			SystemLanguage.Greek => LanguageCode.EL, 
			SystemLanguage.Hebrew => LanguageCode.HE, 
			SystemLanguage.Hugarian => LanguageCode.HU, 
			SystemLanguage.Icelandic => LanguageCode.IS, 
			SystemLanguage.Indonesian => LanguageCode.ID, 
			SystemLanguage.Italian => LanguageCode.IT, 
			SystemLanguage.Japanese => LanguageCode.JA, 
			SystemLanguage.Korean => LanguageCode.KO, 
			SystemLanguage.Latvian => LanguageCode.LA, 
			SystemLanguage.Lithuanian => LanguageCode.LT, 
			SystemLanguage.Norwegian => LanguageCode.NO, 
			SystemLanguage.Polish => LanguageCode.PL, 
			SystemLanguage.Portuguese => LanguageCode.PT, 
			SystemLanguage.Romanian => LanguageCode.RO, 
			SystemLanguage.Russian => LanguageCode.RU, 
			SystemLanguage.SerboCroatian => LanguageCode.SH, 
			SystemLanguage.Slovak => LanguageCode.SK, 
			SystemLanguage.Slovenian => LanguageCode.SL, 
			SystemLanguage.Spanish => LanguageCode.ES, 
			SystemLanguage.Swedish => LanguageCode.SW, 
			SystemLanguage.Thai => LanguageCode.TH, 
			SystemLanguage.Turkish => LanguageCode.TR, 
			SystemLanguage.Ukrainian => LanguageCode.UK, 
			SystemLanguage.Vietnamese => LanguageCode.VI, 
			_ => name switch
			{
				SystemLanguage.Hugarian => LanguageCode.HU, 
				SystemLanguage.Unknown => LanguageCode.N, 
				_ => LanguageCode.N, 
			}, 
		};
	}
}
