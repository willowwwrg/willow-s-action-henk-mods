using System;
using System.Security.Cryptography;
using System.Text;

public class PhpMyAdminMan : Singleton<PhpMyAdminMan>
{
	private string secretKey = "ActionHenkHash";

	private string baseURL = "http://www.ragesquid.com/actionhenkdatabase/";

	private string baseURLScripts = "http://www.ragesquid.com/scripts/";

	private string submitScoreURL = "SubmitScore";

	private string submitNameURL = "SubmitName";

	private string fetchLevelScoresURL = "GetScores";

	private string submitNickURL = "updatenickname.php";

	private string fetchNickURL = "getidfromnick.php";

	private string ircCheckURL = "ircmessagecheck.php";

	private string skinURL = "skinunlocks/checkskinunlocks.php";

	private string dailyURL = "dailychallenge/getdailychallenge.php";

	private string streamCheckURL = "checkstream.php";

	public string GetBaseURL()
	{
		return baseURL;
	}

	public string GetSecretKey()
	{
		return secretKey;
	}

	public string GetSubmitScoreURL()
	{
		return baseURL + submitScoreURL + ".php";
	}

	public string GetSubmitNameURL()
	{
		return baseURL + submitNameURL + ".php";
	}

	public string GetFetchLevelScoresURL()
	{
		return baseURL + fetchLevelScoresURL + ".php";
	}

	public string SubmitNickURL()
	{
		return baseURL + submitNickURL;
	}

	public string FetchIDFromNickURL()
	{
		return baseURL + fetchNickURL;
	}

	public string IRCCheckURL()
	{
		return baseURL + ircCheckURL;
	}

	public string SkinUnlocksURL()
	{
		return baseURLScripts + skinURL;
	}

	public string DailyChallengeURL()
	{
		return baseURLScripts + dailyURL;
	}

	public string GetStreamCheckURL()
	{
		return baseURL + streamCheckURL;
	}

	public string Md5Sum(string strToEncrypt)
	{
		byte[] bytes = new UTF8Encoding().GetBytes(strToEncrypt);
		byte[] array = new MD5CryptoServiceProvider().ComputeHash(bytes);
		string text = string.Empty;
		for (int i = 0; i < array.Length; i++)
		{
			text += Convert.ToString(array[i], 16).PadLeft(2, '0');
		}
		return text.PadLeft(32, '0');
	}
}
