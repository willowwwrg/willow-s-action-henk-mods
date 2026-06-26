using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using UnityEngine;

public class RageSquidServer : Singleton<RageSquidServer>
{
	private XmlSerializer xmlSerializer;

	public void InitDatabase()
	{
		xmlSerializer = new XmlSerializer(typeof(HighscoreEntry));
	}

	public bool SubmitScore()
	{
		StartCoroutine(PostScores());
		return true;
	}

	private IEnumerator PostScores()
	{
		HighscoreEntry highscoreEntry = new HighscoreEntry
		{
			userID = Singleton<ActionHenk>.SP.GetComputerID(),
			score = (int)(Singleton<CheckpointManager>.SP.GetFinishTime() * 1000f),
			level = Singleton<LevelBatchManager>.SP.GetCurrentLevel().ToString()
		};
		string value = Singleton<PhpMyAdminMan>.SP.Md5Sum(highscoreEntry.userID + highscoreEntry.level + highscoreEntry.score + Singleton<PhpMyAdminMan>.SP.GetSecretKey());
		string submitScoreURL = Singleton<PhpMyAdminMan>.SP.GetSubmitScoreURL();
		byte[] array;
		using (Stream stream = new MemoryStream())
		{
			xmlSerializer.Serialize(stream, highscoreEntry);
			stream.Position = 0L;
			array = new byte[(int)stream.Length];
			stream.Read(array, 0, (int)stream.Length);
			stream.Close();
		}
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("Object", Encoding.Default.GetString(array));
		wWWForm.AddField("Hash", value);
		wWWForm.AddField("LevelTries", 0);
		WWW hs_post = new WWW(submitScoreURL, wWWForm);
		yield return hs_post;
		if (hs_post.error != null)
		{
			Debug.LogError("There was an error posting the high score: " + hs_post.error);
			Singleton<HighscoreManager>.SP.FailedToConnect();
		}
		yield return new WaitForEndOfFrame();
		StartCoroutine(GetScoresForCurrentLevel());
	}

	private IEnumerator GetScoresForCurrentLevel()
	{
		List<HighscoreEntry> highscores = new List<HighscoreEntry>();
		string text = Singleton<LevelBatchManager>.SP.GetCurrentLevel().ToString();
		string value = Singleton<PhpMyAdminMan>.SP.Md5Sum(text + Singleton<PhpMyAdminMan>.SP.GetSecretKey());
		string fetchLevelScoresURL = Singleton<PhpMyAdminMan>.SP.GetFetchLevelScoresURL();
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("Level", text);
		wWWForm.AddField("Hash", value);
		WWW hs_get = new WWW(fetchLevelScoresURL, wWWForm);
		yield return hs_get;
		if (hs_get.error != null)
		{
			Debug.LogError("There was an error getting the high score: " + hs_get.error);
		}
		else
		{
			foreach (Match item in Regex.Matches(hs_get.text, "{(.*?)}"))
			{
				highscores.Add(new HighscoreEntry(item.Groups[1].Value));
			}
		}
		Singleton<HighscoreManager>.SP.RageSquidLeaderboard.Clear();
		Singleton<HighscoreManager>.SP.RageSquidLeaderboard.AddRange(highscores);
		Singleton<HighscoreManager>.SP.ScoresUpToDate(toggle: true);
	}
}
