using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutatorManager : Singleton<MutatorManager>
{
	private bool fetchingMutator;

	public bool mutatorActive;

	public Mutator selectedMutator;

	public Level selectedLevel;

	public string currentRank;

	public float secondsLeft;

	public int seedOfToday;

	public Mutator dailyChallengeMutator;

	public void Update()
	{
		secondsLeft -= Time.deltaTime;
		if (secondsLeft < 0f && !fetchingMutator)
		{
			FetchMutator();
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.StartDailyChallenge) && Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_MainMenu)) && Singleton<GamestateManager>.SP.CurrentState.GetComponent<State_MainMenu>().initialized && !Singleton<PermaGUI>.SP.inbox.activeInHierarchy)
		{
			selectedMutator = dailyChallengeMutator;
			StartDailyChallenge();
		}
	}

	public void FetchMutator()
	{
		if (!fetchingMutator)
		{
			fetchingMutator = true;
			currentRank = "...";
			selectedMutator = Mutator.None;
			dailyChallengeMutator = Mutator.None;
			selectedLevel = null;
			StartCoroutine(FetchMutatorRoutine());
		}
	}

	private IEnumerator FetchMutatorRoutine()
	{
		WWW webCall = new WWW(Singleton<PhpMyAdminMan>.SP.DailyChallengeURL());
		yield return webCall;
		if (webCall.error != null)
		{
			Debug.LogError("Error retrieving daily challenge: " + webCall.error);
			DateTime value = new DateTime(2015, 11, 7);
			DateTime utcNow = DateTime.UtcNow;
			DateTime dateTime = DateTime.SpecifyKind(utcNow.AddDays(1.0).Date, DateTimeKind.Utc);
			int days = (utcNow - DateTime.SpecifyKind(value, DateTimeKind.Utc)).Days;
			secondsLeft = (float)Math.Floor((dateTime - utcNow).TotalSeconds);
			CalulateFromSeed(days);
			Singleton<HenkSWLeaderboards>.SP.GetDailyChallengeRank();
		}
		else
		{
			string[] array = webCall.text.Split(',');
			int seed = int.Parse(array[0]);
			secondsLeft = int.Parse(array[1]);
			CalulateFromSeed(seed);
			Singleton<HenkSWLeaderboards>.SP.GetDailyChallengeRank();
		}
		fetchingMutator = false;
	}

	private void CalulateFromSeed(int seed)
	{
		seedOfToday = seed;
		UnityEngine.Random.seed = seed;
		selectedMutator = (Mutator)UnityEngine.Random.Range(1, 15);
		dailyChallengeMutator = selectedMutator;
		List<Level> campaignLevels = Singleton<LevelBatchManager>.SP.GetCampaignLevels(excludeChallengeBonusAndExtraLevels: true);
		selectedLevel = campaignLevels[UnityEngine.Random.Range(0, campaignLevels.Count)];
		UnityEngine.Random.seed = UnityEngine.Random.Range(0, int.MaxValue);
	}

	public Mutator GetActiveMutator()
	{
		if (!mutatorActive)
		{
			return Mutator.None;
		}
		return selectedMutator;
	}

	public void StartDailyChallenge()
	{
		if (selectedLevel != null && selectedMutator != Mutator.None)
		{
			Singleton<LevelBatchManager>.SP.lookingAtBatchNum = Singleton<LevelBatchManager>.SP.GetBatchNumFromLevel(selectedLevel);
			Singleton<GamestateManager>.SP.SetCurrentGameMode(GameMode.Singleplayer);
			mutatorActive = true;
			AudioController.Play("LevelStart");
			Singleton<LevelBatchManager>.SP.LoadLevelSceneless(selectedLevel);
		}
	}

	public string GetActiveMutatorString()
	{
		return selectedMutator switch
		{
			Mutator.None => "None", 
			Mutator.LowGravity => "Low Gravity", 
			Mutator.InvertedControls => "Inverted Controls", 
			Mutator.BlindMode => "Blind Mode", 
			Mutator.Trippin => "Trippin'", 
			Mutator.Haste => "Haste", 
			Mutator.DoubleJump => "Double Jump", 
			Mutator.SuperJump => "Super Jump", 
			Mutator.Spiderman => "Sticky Walls", 
			Mutator.TinyWings => "Down Smash", 
			Mutator.FirstPerson => "First Person", 
			Mutator.OppositeCamera => "Opposite Camera", 
			Mutator.Pixelated => "Pixelated", 
			Mutator.RotatingCamera => "Rotating Camera", 
			Mutator.Invisible => "Invisible", 
			Mutator.NUM_MUTATORS => string.Empty, 
			Mutator.SlideOnly => "Slide Only", 
			_ => string.Empty, 
		};
	}

	public Mutator GetMutatorFromString(string mutatorName)
	{
		return mutatorName switch
		{
			"None" => Mutator.None, 
			"Low Gravity" => Mutator.LowGravity, 
			"Slide Only" => Mutator.SlideOnly, 
			"Inverted Controls" => Mutator.InvertedControls, 
			"Blind Mode" => Mutator.BlindMode, 
			"Trippin'" => Mutator.Trippin, 
			"Haste" => Mutator.Haste, 
			"Double Jump" => Mutator.DoubleJump, 
			"Super Jump" => Mutator.SuperJump, 
			"Sticky Walls" => Mutator.Spiderman, 
			"Down Smash" => Mutator.TinyWings, 
			"First Person" => Mutator.FirstPerson, 
			"Opposite Camera" => Mutator.OppositeCamera, 
			"Pixelated" => Mutator.Pixelated, 
			"Rotating Camera" => Mutator.RotatingCamera, 
			"Invisible" => Mutator.Invisible, 
			_ => Mutator.None, 
		};
	}

	public string GetStringFromMutator(Mutator mutator)
	{
		return mutator switch
		{
			Mutator.None => "None", 
			Mutator.LowGravity => "Low Gravity", 
			Mutator.InvertedControls => "Inverted Controls", 
			Mutator.BlindMode => "Blind Mode", 
			Mutator.Trippin => "Trippin'", 
			Mutator.Haste => "Haste", 
			Mutator.DoubleJump => "Double Jump", 
			Mutator.SuperJump => "Super Jump", 
			Mutator.Spiderman => "Sticky Walls", 
			Mutator.TinyWings => "Down Smash", 
			Mutator.FirstPerson => "First Person", 
			Mutator.OppositeCamera => "Opposite Camera", 
			Mutator.Pixelated => "Pixelated", 
			Mutator.RotatingCamera => "Rotating Camera", 
			Mutator.Invisible => "Invisible", 
			Mutator.SlideOnly => "Slide Only", 
			_ => "None", 
		};
	}

	public List<Mutator> GetMultiplayerMutatorsFromPlayerPrefs()
	{
		List<Mutator> list = new List<Mutator>();
		string text = Singleton<PlayerPrefsManager>.SP.GetString("MultiplayerSettings_MUTATORS", string.Empty);
		if (text == string.Empty)
		{
			return list;
		}
		List<string> list2 = new List<string>(text.Split(','));
		for (int i = 0; i < list2.Count; i++)
		{
			Mutator mutatorFromString = GetMutatorFromString(list2[i]);
			if (mutatorFromString != Mutator.None)
			{
				list.Add(mutatorFromString);
			}
		}
		return list;
	}

	public void SetAndEnableMutator(Mutator multiplayerMutator)
	{
		selectedMutator = multiplayerMutator;
		mutatorActive = true;
	}
}
