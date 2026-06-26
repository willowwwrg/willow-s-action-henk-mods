using System.Text.RegularExpressions;
using UnityEngine;

public class SiteLock : Singleton<SiteLock>
{
	private string validHost = "ragesquid.com/";

	private string invalidHostRedirect = "http://www.actionhenk.com/";

	public void TestPiracy()
	{
		if (Application.isWebPlayer && Application.absoluteURL.IndexOf("file:///C:/") != 0 && Application.absoluteURL.IndexOf("file:///D:/") != 0)
		{
			if (!ValidURL(Application.absoluteURL, validHost))
			{
				Application.LoadLevel("SitelockScene");
			}
			else if (!ValidURLSRC(Application.srcValue, validHost))
			{
				Application.LoadLevel("SitelockScene");
			}
		}
	}

	private bool ValidURL(string URL, string validH)
	{
		URL = URL.ToLower();
		if (new Regex("http://[a-z]*." + validH).Match(URL).Success)
		{
			return true;
		}
		if (URL.IndexOf("http://" + validH) == 0)
		{
			return true;
		}
		return false;
	}

	private bool ValidURLSRC(string URL, string validH)
	{
		if (ValidURL(URL, validH))
		{
			return true;
		}
		if (URL.Contains("http://"))
		{
			return false;
		}
		if (URL.Contains("https://"))
		{
			return false;
		}
		if (URL.Contains("ftp://"))
		{
			return false;
		}
		if (URL.Contains("www."))
		{
			return false;
		}
		return true;
	}
}
