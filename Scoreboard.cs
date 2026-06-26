using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using ExitGames.Client.Photon;
using Steamworks;
using UnityEngine;

public class Scoreboard : Singleton<Scoreboard>
{
	public UILabel rankLabelLeft;

	public UILabel rankLabelRight;

	public GameObject scoreBoard;

	public UILabel scoreBoardNameLabels;

	public UILabel scoreBoardScoreLabels;

	public UILabel pbScoreBoardNameLabel;

	public UILabel pbScoreBoardScoreLabel;

	public UILabel totalTimeLabelTitle;

	public UILabel totalTimeLabel;

	public UILabel levelNameLabel;

	public UILabel timeTitleLabel;

	public UILabel gameOver;

	public GameObject anchorCenter;

	private GameObject anchorBotLeft;

	private Vector3 originalPos;

	public bool forceScoreboardVisible;

	private bool bleeping;

	private PhotonPlayer[] rankedPlayers = new PhotonPlayer[0];

	public ObscuredInt ranking = 0;

	private void Start()
	{
		anchorBotLeft = scoreBoard.transform.parent.gameObject;
		originalPos = scoreBoard.transform.localPosition;
	}

	private void Update()
	{
		if (!IsReadyForUpdates())
		{
			return;
		}
		if ((Singleton<InputManager>.SP.CheckActionContinuous(InputAction.ShowMultiplayerLeaderboard) || forceScoreboardVisible) && !Singleton<InGameMenu>.SP.menuEnabled)
		{
			scoreBoard.SetActive(value: true);
			if (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_EndGameMultiplayer)))
			{
				scoreBoard.transform.parent = anchorBotLeft.transform;
				scoreBoard.transform.localPosition = originalPos;
			}
			else
			{
				scoreBoard.transform.parent = anchorCenter.transform;
				scoreBoard.transform.localPosition = Vector3.zero;
			}
		}
		else
		{
			scoreBoard.SetActive(value: false);
		}
		string text = string.Empty;
		ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.room.customProperties;
		float num = Singleton<MultiManager>.SP.WarmupTimeLeft();
		float num2 = Singleton<MultiManager>.SP.TimeLeftTillSwitch();
		if (num <= 0f)
		{
			float num3 = Singleton<MultiManager>.SP.RoundTimeLeft();
			if (num3 < 0f)
			{
				num3 = 0f;
			}
			string text2 = "[ffffff]";
			if (num3 < 10f)
			{
				text2 = "[ff0000]";
			}
			if (num3 > 12f)
			{
				bleeping = false;
			}
			if (num3 < 10.1f && !bleeping)
			{
				StartCoroutine("CountDownBleeps");
			}
			if (num3 < 3.1f && num3 > 2.9f && !AudioController.IsPlaying("mul_countdown"))
			{
				AudioController.Play("mul_countdown");
			}
			text = text2 + Singleton<HighscoreManager>.SP.ConvertTimeToString(num3, showHundreds: false) + "[-]";
			totalTimeLabelTitle.text = Language.Get("TIMELEFT", "MULTIPLAYER");
		}
		else if (customProperties["levelstarttime"] != null)
		{
			if (num2 < -1f)
			{
				text = "[ffff00]" + Singleton<HighscoreManager>.SP.ConvertTimeToString(num, showHundreds: false) + "[-]";
				totalTimeLabelTitle.text = Language.Get("ROUNDSTART", "MULTIPLAYER");
				if (num < 4.1f && num > 3.9f && !AudioController.IsPlaying("mul_begin"))
				{
					AudioController.Play("mul_begin");
				}
			}
			else if (num2 > 0f)
			{
				text = "[00ff00]" + Singleton<HighscoreManager>.SP.ConvertTimeToString(num2, showHundreds: false) + "[-]";
				totalTimeLabelTitle.text = Language.Get("NEXTLEVEL", "MULTIPLAYER");
			}
		}
		totalTimeLabel.text = text;
		if (!Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_EndGameMultiplayer)))
		{
			UpdateRanking();
			UpdateScoreBoard();
		}
	}

	private IEnumerator CountDownBleeps()
	{
		bleeping = true;
		for (int i = 0; i < 11; i++)
		{
			if (Singleton<MultiManager>.SP.RoundTimeLeft() > 0f)
			{
				AudioController.Play("mul_count_each");
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private bool IsReadyForUpdates()
	{
		if (HenkUtils.IsInALevel() && Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Multiplayer && PhotonNetwork.inRoom)
		{
			return PhotonNetwork.time != 0.0;
		}
		return false;
	}

	public void UpdateRanking()
	{
		ranking = 0;
		List<PhotonPlayer> list = new List<PhotonPlayer>();
		List<PhotonPlayer> list2 = new List<PhotonPlayer>();
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		foreach (PhotonPlayer photonPlayer in playerList)
		{
			if (photonPlayer.customProperties["score"] != null)
			{
				list.Add(photonPlayer);
			}
			else
			{
				list2.Add(photonPlayer);
			}
		}
		list.Sort(new PlayerScoreSort());
		list.AddRange(list2);
		rankedPlayers = list.ToArray();
		for (int j = 0; j < rankedPlayers.Length; j++)
		{
			if (rankedPlayers[j].isLocal)
			{
				ranking = j + 1;
				break;
			}
		}
	}

	public void UpdateLevelDetails()
	{
		levelNameLabel.text = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelName;
	}

	public void ScorePropertyChanged(object[] playerAndScore)
	{
		PhotonPlayer photonPlayer = playerAndScore[0] as PhotonPlayer;
		_ = playerAndScore[1];
		if (rankedPlayers.Length != 0)
		{
			string text = rankedPlayers[0].name;
			int playerRank = GetPlayerRank(photonPlayer);
			UpdateRanking();
			if (text != rankedPlayers[0].name)
			{
				Singleton<PermaGUI>.SP.PopUpNotification(Language.Get("ISTHENEWRANK", "MULTIPLAYER").Replace("{X}", rankedPlayers[0].name).Replace("{Y}", "1!"));
			}
			else if (playerRank != GetPlayerRank(photonPlayer))
			{
				Singleton<PermaGUI>.SP.PopUpNotification(Language.Get("ISTHENEWRANK", "MULTIPLAYER").Replace("{X}", photonPlayer.name).Replace("{Y}", GetPlayerRank(photonPlayer) + "!"));
			}
			else if (photonPlayer.isLocal)
			{
				Singleton<PermaGUI>.SP.PopUpNotification(Language.Get("NEWTOPTIMEYOU", "MULTIPLAYER"));
			}
			else
			{
				Singleton<PermaGUI>.SP.PopUpNotification(Language.Get("NEWTOPTIMEOTHER", "MULTIPLAYER").Replace("{X}", photonPlayer.name));
			}
		}
	}

	public void SetEndGameScores(LeaderboardEntry_t[] entries)
	{
		timeTitleLabel.text = Language.Get("TOTALTIME", "MULTIPLAYER");
		bool flag = false;
		string text = string.Empty;
		string text2 = string.Empty;
		for (int i = 0; i < rankedPlayers.Length; i++)
		{
			if (i >= 13)
			{
				continue;
			}
			PhotonPlayer photonPlayer = rankedPlayers[i];
			if (i != 0)
			{
				text += "\n";
				text2 += "\n";
			}
			if (photonPlayer.isLocal)
			{
				text += "[00FF00]";
				text2 += "[00FF00]";
				flag = true;
			}
			bool flag2 = false;
			for (int j = 0; j < entries.Length; j++)
			{
				if (Singleton<HenkSWUserStats>.SP.GetNameBySteamID(entries[j].m_steamIDUser) == photonPlayer.name)
				{
					text = text + entries[j].m_nGlobalRank + " - " + photonPlayer.name;
					text2 += entries[j].m_nScore;
					if (photonPlayer.customProperties["score"] != null)
					{
						text2 = text2 + " (+" + (rankedPlayers.Length - i) + ")";
					}
					flag2 = true;
				}
			}
			if (!flag2)
			{
				text = text + "- " + photonPlayer.name;
				text2 += "-";
			}
			if (photonPlayer.isLocal)
			{
				text += "[-]";
				text2 += "[-]";
			}
		}
		scoreBoardNameLabels.text = text;
		scoreBoardScoreLabels.text = text2;
		pbScoreBoardNameLabel.text = string.Empty;
		pbScoreBoardScoreLabel.text = string.Empty;
		for (int k = 0; k < PhotonNetwork.playerList.Length; k++)
		{
			PhotonPlayer photonPlayer2 = PhotonNetwork.playerList[k];
			if (photonPlayer2.isLocal && !flag)
			{
				pbScoreBoardNameLabel.text = string.Concat(ranking, " ", photonPlayer2.name);
				pbScoreBoardScoreLabel.text = GetScoreString(photonPlayer2);
			}
		}
	}

	public void UpdateScoreBoard()
	{
		timeTitleLabel.text = Language.Get("TIME", "MULTIPLAYER");
		rankLabelLeft.text = ranking.ToString();
		rankLabelRight.text = PhotonNetwork.playerList.Length.ToString();
		bool flag = false;
		string text = string.Empty;
		string text2 = string.Empty;
		for (int i = 0; i < rankedPlayers.Length; i++)
		{
			if (i < 13)
			{
				PhotonPlayer photonPlayer = rankedPlayers[i];
				if (i != 0)
				{
					text += "\n";
					text2 += "\n";
				}
				if (photonPlayer.isLocal)
				{
					text += "[00FF00]";
					text2 += "[00FF00]";
					flag = true;
				}
				string text3 = string.Empty;
				if (Application.isEditor && photonPlayer.isMasterClient)
				{
					text3 = "(MC)";
				}
				string text4 = text;
				text = text4 + (i + 1) + " " + photonPlayer.name + text3;
				text2 += GetScoreString(photonPlayer);
				if (photonPlayer.isLocal)
				{
					text = text + "[-] (" + PhotonNetwork.GetPing() + ")";
					text2 += "[-]";
				}
			}
		}
		scoreBoardNameLabels.text = text;
		scoreBoardScoreLabels.text = text2;
		pbScoreBoardNameLabel.text = string.Empty;
		pbScoreBoardScoreLabel.text = string.Empty;
		for (int j = 0; j < PhotonNetwork.playerList.Length; j++)
		{
			PhotonPlayer photonPlayer2 = PhotonNetwork.playerList[j];
			if (photonPlayer2.isLocal && !flag)
			{
				pbScoreBoardNameLabel.text = string.Concat(ranking, " ", photonPlayer2.name);
				pbScoreBoardScoreLabel.text = GetScoreString(photonPlayer2);
			}
		}
	}

	public string GetScoreString(PhotonPlayer player)
	{
		string result = "-";
		if (player.customProperties["score"] != null)
		{
			result = Singleton<HighscoreManager>.SP.ConvertTimeToString((float)player.customProperties["score"]);
		}
		return result;
	}

	public PhotonPlayer[] GetRankedPlayers()
	{
		return rankedPlayers;
	}

	public int GetPlayerRank(PhotonPlayer player)
	{
		for (int i = 0; i < rankedPlayers.Length; i++)
		{
			if (player == rankedPlayers[i])
			{
				return i + 1;
			}
		}
		return 0;
	}
}
