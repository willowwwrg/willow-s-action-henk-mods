using System.Collections.Generic;

public class SessionTimeSort : Comparer<InboxMessage>
{
	public override int Compare(InboxMessage x, InboxMessage y)
	{
		return -x.timeSent.CompareTo(y.timeSent);
	}
}
