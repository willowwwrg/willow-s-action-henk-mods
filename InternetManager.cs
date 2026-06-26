using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternetManager : Singleton<InternetManager>
{
	private bool internetConnection;

	private List<GameObject> ObjectsToNotify = new List<GameObject>();

	public bool forceOffline;

	public void Awake()
	{
		CheckConnection();
	}

	public void CheckConnection()
	{
		StartCoroutine(TestConnection());
	}

	public bool IsConnected()
	{
		if (forceOffline)
		{
			return false;
		}
		return internetConnection;
	}

	public void ForceOffline(bool forcemode)
	{
		forceOffline = forcemode;
	}

	private IEnumerator TestConnection()
	{
		if (forceOffline)
		{
			internetConnection = false;
		}
		else
		{
			float maxTime = 2f;
			internetConnection = false;
			yield return new WaitForSeconds(0.1f);
			Ping testPing = new Ping("74.125.224.72");
			float timeTaken = 0f;
			while (!testPing.isDone)
			{
				timeTaken += Time.deltaTime;
				if (timeTaken > maxTime)
				{
					internetConnection = false;
					break;
				}
				yield return null;
			}
			if (timeTaken <= maxTime)
			{
				internetConnection = true;
			}
			NotifyObjects(internetConnection);
		}
		yield return null;
	}

	public void RegisterObject(GameObject obj)
	{
		ObjectsToNotify.Add(obj);
	}

	private void NotifyObjects(bool internet)
	{
		foreach (GameObject item in ObjectsToNotify)
		{
			item.SendMessage("OnInternetChecked", internet);
		}
	}
}
