using ExitGames.Client.Photon;
using UnityEngine;

public class PhotonStatsGui : MonoBehaviour
{
	public bool statsWindowOn = true;

	public bool statsOn = true;

	public bool healthStatsVisible;

	public bool trafficStatsOn;

	public bool buttonsOn;

	public Rect statsRect = new Rect(0f, 100f, 200f, 50f);

	public int WindowId = 100;

	private bool showLagControl;

	public void Start()
	{
		statsRect.x = (float)Screen.width - statsRect.width;
		base.gameObject.AddComponent<PhotonLagSimulationGui>();
	}

	public void Update()
	{
		if (Input.GetKey(KeyCode.B) && Input.GetKey(KeyCode.N) && Input.GetKeyDown(KeyCode.M))
		{
			statsWindowOn = !statsWindowOn;
			statsOn = true;
		}
		if ((bool)GetComponent<PhotonLagSimulationGui>() && !showLagControl)
		{
			Object.Destroy(GetComponent<PhotonLagSimulationGui>());
		}
		if (!GetComponent<PhotonLagSimulationGui>() && showLagControl)
		{
			base.gameObject.AddComponent<PhotonLagSimulationGui>();
		}
	}

	public void OnGUI()
	{
		if (PhotonNetwork.networkingPeer.TrafficStatsEnabled != statsOn)
		{
			PhotonNetwork.networkingPeer.TrafficStatsEnabled = statsOn;
		}
		if (statsWindowOn)
		{
			statsRect = GUILayout.Window(WindowId, statsRect, TrafficStatsWindow, "Messages (B+N+M)");
		}
	}

	public void TrafficStatsWindow(int windowID)
	{
		bool flag = false;
		TrafficStatsGameLevel trafficStatsGameLevel = PhotonNetwork.networkingPeer.TrafficStatsGameLevel;
		long num = PhotonNetwork.networkingPeer.TrafficStatsElapsedMs / 1000;
		if (num == 0L)
		{
			num = 1L;
		}
		GUILayout.BeginHorizontal();
		GUILayout.Label("Peerstate:\n" + PhotonNetwork.connectionStateDetailed);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Multiplayer status message:\n" + Singleton<MultiManager>.SP.GetMultiplayerStatusMessage());
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		buttonsOn = GUILayout.Toggle(buttonsOn, "buttons");
		healthStatsVisible = GUILayout.Toggle(healthStatsVisible, "health");
		trafficStatsOn = GUILayout.Toggle(trafficStatsOn, "traffic");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		showLagControl = GUILayout.Toggle(showLagControl, "lag tester");
		GUILayout.EndHorizontal();
		string text = $"Out|In|Sum:\t{trafficStatsGameLevel.TotalOutgoingMessageCount,4} | {trafficStatsGameLevel.TotalIncomingMessageCount,4} | {trafficStatsGameLevel.TotalMessageCount,4}";
		string text2 = $"{num}sec average:";
		string text3 = $"Out|In|Sum:\t{trafficStatsGameLevel.TotalOutgoingMessageCount / num,4} | {trafficStatsGameLevel.TotalIncomingMessageCount / num,4} | {trafficStatsGameLevel.TotalMessageCount / num,4}";
		GUILayout.Label(text);
		GUILayout.Label(text2);
		GUILayout.Label(text3);
		if (buttonsOn)
		{
			GUILayout.BeginHorizontal();
			statsOn = GUILayout.Toggle(statsOn, "stats on");
			if (GUILayout.Button("Reset"))
			{
				PhotonNetwork.networkingPeer.TrafficStatsReset();
				PhotonNetwork.networkingPeer.TrafficStatsEnabled = true;
			}
			flag = GUILayout.Button("To Log");
			GUILayout.EndHorizontal();
		}
		string text4 = string.Empty;
		string text5 = string.Empty;
		if (trafficStatsOn)
		{
			text4 = "Incoming: " + PhotonNetwork.networkingPeer.TrafficStatsIncoming.ToString();
			text5 = "Outgoing: " + PhotonNetwork.networkingPeer.TrafficStatsOutgoing.ToString();
			GUILayout.Label(text4);
			GUILayout.Label(text5);
		}
		string text6 = string.Empty;
		if (healthStatsVisible)
		{
			text6 = string.Format("ping: {6}[+/-{7}]ms\nlongest delta between\nsend: {0,4}ms disp: {1,4}ms\nlongest time for:\nev({3}):{2,3}ms op({5}):{4,3}ms", trafficStatsGameLevel.LongestDeltaBetweenSending, trafficStatsGameLevel.LongestDeltaBetweenDispatching, trafficStatsGameLevel.LongestEventCallback, trafficStatsGameLevel.LongestEventCallbackCode, trafficStatsGameLevel.LongestOpResponseCallback, trafficStatsGameLevel.LongestOpResponseCallbackOpCode, PhotonNetwork.networkingPeer.RoundTripTime, PhotonNetwork.networkingPeer.RoundTripTimeVariance);
			GUILayout.Label(text6);
		}
		if (flag)
		{
			Debug.Log(text + "\n" + text2 + "\n" + text3 + "\n" + text4 + "\n" + text5 + "\n" + text6);
		}
		if (GUI.changed)
		{
			statsRect.height = 100f;
		}
		GUI.DragWindow();
	}
}
