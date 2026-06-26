using System.Collections.Generic;
using UnityEngine;

public class GUI_LMP_Endgame : GUI_Base
{
	public UILabel nameLabel;

	public UILabel scoreLabel;

	public UISprite backgroundSprite;

	public List<GameObject> scoreBackgroundBars;

	public List<UILabel> rankLabels;

	private float[] backgroundSizesForNumPlayers = new float[3] { 0.65f, 0.8f, 1f };

	private MultiplayerPodium podium;

	private void TransitionCompleted()
	{
		InitializeScreen();
		for (int i = 0; i < scoreBackgroundBars.Count; i++)
		{
			scoreBackgroundBars[i].SetActive(value: false);
		}
		new List<LMPPlayerScoreEntry>();
		nameLabel.text = string.Empty;
		scoreLabel.text = string.Empty;
		int[] playersSortedByScore = Singleton<LocalMultiManager>.SP.GetPlayersSortedByScore();
		if (playersSortedByScore.Length == 2)
		{
			backgroundSprite.transform.localScale = new Vector3(-1f, backgroundSizesForNumPlayers[0], 1f);
		}
		else if (playersSortedByScore.Length == 3)
		{
			backgroundSprite.transform.localScale = new Vector3(-1f, backgroundSizesForNumPlayers[1], 1f);
		}
		else
		{
			backgroundSprite.transform.localScale = new Vector3(-1f, backgroundSizesForNumPlayers[2], 1f);
		}
		for (int j = 0; j < playersSortedByScore.Length; j++)
		{
			string text = Singleton<LocalMultiManager>.SP.playerColorCodes[playersSortedByScore[j]] + Singleton<UnlockManager>.SP.GetCharacterName(Singleton<LocalMultiManager>.SP.playerInfo[playersSortedByScore[j]].selectedCharacter.character, Singleton<LocalMultiManager>.SP.playerInfo[playersSortedByScore[j]].selectedCharacter.skinNum) + "[-]";
			text += "\n";
			int playerScore = Singleton<LocalMultiManager>.SP.playerInfo[playersSortedByScore[j]].playerScore;
			string text2 = playerScore + "\n";
			nameLabel.text += text;
			scoreLabel.text += text2;
			scoreBackgroundBars[j].SetActive(value: true);
			int finishRank = j + 1;
			if (j != 0 && Singleton<LocalMultiManager>.SP.playerInfo[playersSortedByScore[j]].playerScore == Singleton<LocalMultiManager>.SP.playerInfo[playersSortedByScore[j - 1]].playerScore)
			{
				finishRank = Singleton<LocalMultiManager>.SP.playerInfo[playersSortedByScore[j - 1]].finishRank;
			}
			Singleton<LocalMultiManager>.SP.playerInfo[playersSortedByScore[j]].finishRank = finishRank;
			rankLabels[j].text = Singleton<LocalMultiManager>.SP.playerInfo[playersSortedByScore[j]].finishRank.ToString();
		}
		podium = Object.FindObjectOfType<MultiplayerPodium>();
		if (podium == null)
		{
			Debug.LogError("Couldn't find podium in level style " + Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelStyle);
		}
		else
		{
			Camera.main.GetComponent<PlatformerCameraMultiplayer>().enabled = false;
			Camera.main.GetComponent<HandyCam>().SetCameraTarget(podium.camera.transform, snapToTarget: true);
			if ((bool)Camera.main.GetComponent<DepthOfFieldScatter>())
			{
				Camera.main.GetComponent<DepthOfFieldScatter>().enabled = false;
			}
			GameObject[] localPlayers = Singleton<PlayerManager>.SP.GetLocalPlayers();
			foreach (GameObject gameObject in localPlayers)
			{
				Singleton<PlayerManager>.SP.ResetPlayer(gameObject, hard: true);
				gameObject.GetComponent<RaycastCollider>().enabled = false;
				gameObject.GetComponent<PlayerWaypointManager>().enabled = false;
				gameObject.GetComponent<PlatformerPhysics>().enabled = false;
				gameObject.GetComponent<PlayerGraphics>().enabled = false;
				gameObject.GetComponent<PlayerNetworking>().enabled = false;
				int playerRank = Singleton<LocalMultiManager>.SP.GetPlayerRank(gameObject.GetComponent<PlatformerController>().localPlayerNumber);
				if (playerRank == 0)
				{
					gameObject.GetComponent<PlayerGraphics>().GoToPodium(podium.spot1);
				}
				if (playerRank == 1)
				{
					gameObject.GetComponent<PlayerGraphics>().GoToPodium(podium.spot2);
				}
				if (playerRank == 2)
				{
					gameObject.GetComponent<PlayerGraphics>().GoToPodium(podium.spot3);
				}
			}
		}
		AudioController.Play("Finish_improved");
	}

	private void Update()
	{
		if (Singleton<InputManager>.SP.GetJoystickButtonDown(InputButton.Start))
		{
			HenkUtils.BackToMenu();
			Singleton<GamestateManager>.SP.SetState(typeof(State_LMP));
			AudioController.Play("ButtonBackwards");
		}
	}
}
