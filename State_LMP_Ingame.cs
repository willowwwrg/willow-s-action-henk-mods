using System;
using System.Collections.Generic;
using Henk;
using UnityEngine;

public class State_LMP_Ingame : GameState
{
	public Dictionary<GameObject, float> timeOutsideScreen = new Dictionary<GameObject, float>();

	[NonSerialized]
	public float timeOutsideScreenToDie = 2f;

	[NonSerialized]
	public float timeLeftAfterFirstFinish = 5f;

	public float timeLeftToFinish;

	public GUI_LMP_Ingame guiObject;

	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LMP_Ingame, "none");
		Singleton<PermaGUI>.SP.ToggleGhostLabels(state: true);
		Singleton<Stopwatch>.SP.ResetTimer();
		Singleton<Stopwatch>.SP.StartTimer();
		Time.timeScale = 1f;
		Singleton<PlayerManager>.SP.TogglePlayerControls(PlayerType.All, state: true);
		timeOutsideScreen.Clear();
		timeLeftToFinish = timeLeftAfterFirstFinish;
	}

	public override void OnDeactivate()
	{
		Singleton<Stopwatch>.SP.StopTimer();
		Singleton<PermaGUI>.SP.ToggleGhostLabels(state: false);
		Singleton<PermaGUI>.SP.ToggleIngameTutorial(state: false);
	}

	public override void OnUpdate()
	{
		if (guiObject.menuActive)
		{
			return;
		}
		float num = -1000f;
		Vector2 vector = Vector2.zero;
		Vector3 to = Vector3.zero;
		GameObject[] alivePlayers = Singleton<LocalMultiManager>.SP.GetAlivePlayers();
		foreach (GameObject gameObject in alivePlayers)
		{
			Vector2 offset2D = gameObject.GetComponent<PlayerWaypointManager>().GetOffset2D();
			float positionAlongTrack = gameObject.GetComponent<PlayerWaypointManager>().GetPositionAlongTrack();
			if (positionAlongTrack > num || (positionAlongTrack == num && offset2D.x > vector.x))
			{
				num = positionAlongTrack;
				vector = offset2D;
				to = gameObject.transform.forward;
			}
		}
		bool flag = false;
		alivePlayers = Singleton<PlayerManager>.SP.GetLocalPlayers();
		foreach (GameObject gameObject2 in alivePlayers)
		{
			int localPlayerNumber = gameObject2.GetComponent<PlatformerController>().localPlayerNumber;
			bool flag2 = Singleton<LocalMultiManager>.SP.playerInfo[localPlayerNumber].finishedNumber != 0;
			if (flag2)
			{
				flag = true;
			}
			if (Singleton<LocalMultiManager>.SP.IsPlayerAlive(gameObject2) && !flag2)
			{
				Vector3 viewportPos = Camera.main.WorldToViewportPoint(gameObject2.transform.position);
				float num2 = 0.03f;
				if (viewportPos.z < 0f)
				{
					viewportPos.x = 0f - viewportPos.x;
					viewportPos.y = 0f - viewportPos.y;
				}
				bool num3 = viewportPos.z < 0f || viewportPos.x < 0f - num2 || viewportPos.x > 1f + num2 || viewportPos.y < 0f - num2 || viewportPos.y > 1f + num2 || viewportPos.z < 0f;
				bool flag3 = Vector3.Angle(gameObject2.transform.forward, to) > 90f;
				bool flag4 = timeLeftToFinish < 0f;
				if (num3 || flag3 || flag4)
				{
					if (!timeOutsideScreen.ContainsKey(gameObject2))
					{
						timeOutsideScreen.Add(gameObject2, 0f);
					}
					Dictionary<GameObject, float> dictionary = timeOutsideScreen;
					GameObject key2;
					GameObject key = (key2 = gameObject2);
					float num4 = dictionary[key2];
					dictionary[key] = num4 + Time.deltaTime;
					if (timeOutsideScreen[gameObject2] > timeOutsideScreenToDie)
					{
						Singleton<AudioManager>.SP.PlayCharacterDeath(gameObject2);
						Singleton<LocalMultiManager>.SP.KillPlayer(gameObject2, addParticleEffect: true);
						timeOutsideScreen.Remove(gameObject2);
					}
					else
					{
						float radialFillAmount = timeOutsideScreen[gameObject2] / timeOutsideScreenToDie;
						guiObject.OffscreenIndicatorUpdate(turnIndicatorOn: true, localPlayerNumber, viewportPos, radialFillAmount);
					}
				}
				else
				{
					guiObject.OffscreenIndicatorUpdate(turnIndicatorOn: false, localPlayerNumber, viewportPos);
					if (timeOutsideScreen.ContainsKey(gameObject2))
					{
						timeOutsideScreen.Remove(gameObject2);
					}
				}
				if (Singleton<ControllerInput>.SP.GetKeyDown(localPlayerNumber, "Y") || Singleton<ControllerInput>.SP.GetKeyDown(localPlayerNumber, "B"))
				{
					Singleton<AudioManager>.SP.PlayCharacterDeath(gameObject2);
					Singleton<LocalMultiManager>.SP.KillPlayer(gameObject2, addParticleEffect: true);
					timeOutsideScreen.Remove(gameObject2);
				}
			}
			else
			{
				guiObject.OffscreenIndicatorUpdate(turnIndicatorOn: false, gameObject2.GetComponent<PlatformerController>().localPlayerNumber, Vector3.zero);
				if (timeOutsideScreen.ContainsKey(gameObject2))
				{
					timeOutsideScreen.Remove(gameObject2);
				}
			}
		}
		if (flag)
		{
			timeLeftToFinish -= Time.deltaTime;
		}
	}
}
