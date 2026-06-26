using System;
using UnityEngine;

[Serializable]
public class InboxMessage
{
	public string subject = "subject";

	public string from = "from";

	public string body = string.Empty;

	public bool unread;

	public InboxMessageType messageType;

	public long timeSent;

	public HenkSession linkedSession;

	public StoryMessageID storyMessageID;

	public Material storyMessageMaterial;

	public override string ToString()
	{
		return $"[Message] Subject:{subject} -- From:{from}\nUnread:{unread}\nMessageType:{messageType}\nTimeSent:{timeSent}";
	}
}
