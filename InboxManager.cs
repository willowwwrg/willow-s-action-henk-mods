using System;
using System.Collections.Generic;
using UnityEngine;

public class InboxManager : Singleton<InboxManager>
{
	public List<InboxMessage> storyMessages;

	public int GetNumberOfNotifications()
	{
		int num = 0;
		num += Singleton<HenkSWNotifications>.SP.GetNumberOfNotifications();
		foreach (InboxMessage storyMessage in storyMessages)
		{
			if (storyMessage.unread)
			{
				num++;
			}
		}
		return num;
	}

	public void MarkNotificationsAsRead()
	{
		Singleton<HenkSWNotifications>.SP.MarkNotificationsAsRead();
		foreach (InboxMessage storyMessage in storyMessages)
		{
			storyMessage.unread = false;
		}
	}

	private void Update()
	{
	}

	public void CheckForStorylineMessageUnlocks()
	{
		int num = 0;
		for (int i = 0; i < Singleton<LevelBatchManager>.SP.batches.Count; i++)
		{
			if (Singleton<LevelBatchManager>.SP.batches[i].IsUnlocked())
			{
				num = i;
			}
		}
		for (int j = 0; j <= num; j++)
		{
			if (j != 3 && j != 9 && j == 0)
			{
				UnlockStoryMessage(StoryMessageID.Batch1Start2);
			}
		}
		for (int k = 0; k < Singleton<LevelBatchManager>.SP.batches.Count; k++)
		{
			Singleton<LevelBatchManager>.SP.batches[k].CheckChallengeLevelUnlocked();
			Singleton<PlayerPrefsManager>.SP.GetMedalsEarned(Singleton<LevelBatchManager>.SP.batches[k].GetChallengeLevel());
		}
		for (int l = 0; l < Singleton<LevelBatchManager>.SP.batches.Count; l++)
		{
			Singleton<LevelBatchManager>.SP.batches[l].CheckBonusLevelUnlocked();
		}
	}

	public InboxMessage[] GetAllMessages()
	{
		List<InboxMessage> list = new List<InboxMessage>();
		HenkSession[] allSessions = Singleton<HenkSWNotifications>.SP.GetAllSessions(onlyOpenChallenges: true);
		foreach (HenkSession henkSession in allSessions)
		{
			InboxMessage inboxMessage = new InboxMessage();
			inboxMessage.linkedSession = henkSession;
			inboxMessage.subject = "I've beaten your highscore on " + henkSession.levelName + "!";
			inboxMessage.from = Singleton<HenkSWUserStats>.SP.GetNameBySteamID(henkSession.theirSteamID);
			inboxMessage.unread = henkSession.isUnread;
			inboxMessage.messageType = InboxMessageType.FriendChallenge;
			inboxMessage.timeSent = henkSession.timeUpdated;
			list.Add(inboxMessage);
		}
		list.Sort(new SessionTimeSort());
		return list.ToArray();
	}

	public List<InboxMessage> GetStoryMessagesFromPlayerPrefs()
	{
		List<InboxMessage> list = new List<InboxMessage>();
		for (int i = 0; i < storyMessages.Count; i++)
		{
			string key = "StoryMessage_" + storyMessages[i].storyMessageID;
			string text = Singleton<PlayerPrefsManager>.SP.GetString(key, string.Empty);
			if (text != string.Empty)
			{
				storyMessages[i].timeSent = Convert.ToInt64(text);
				list.Add(storyMessages[i]);
			}
		}
		return list;
	}

	public void UnlockStoryMessage(StoryMessageID id)
	{
		InboxMessage messageFromID = GetMessageFromID(id);
		if (messageFromID != null)
		{
			string key = "StoryMessage_" + id;
			if (!(Singleton<PlayerPrefsManager>.SP.GetString(key, string.Empty) != string.Empty))
			{
				Singleton<PlayerPrefsManager>.SP.SetString(key, string.Empty + HenkUtils.GetUnixTimestamp());
				messageFromID.unread = true;
				AudioController.Play("inbox_newmessage");
			}
		}
		else
		{
			Debug.LogError("StoryMessage with ID: " + id.ToString() + " doesn't exist.");
		}
	}

	private InboxMessage GetMessageFromID(StoryMessageID id)
	{
		for (int i = 0; i < storyMessages.Count; i++)
		{
			if (id == storyMessages[i].storyMessageID)
			{
				return storyMessages[i];
			}
		}
		return null;
	}
}
