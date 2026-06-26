using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
	private List<PrefabAudioBackup> prefabAudioBackup = new List<PrefabAudioBackup>();

	public float gameThemeVolume;

	private void Awake()
	{
		prefabAudioBackup.Add(GetAudioBackupForCategory("Music"));
		prefabAudioBackup.Add(GetAudioBackupForCategory("MenuSounds"));
		prefabAudioBackup.Add(GetAudioBackupForCategory("InGameSFX"));
		prefabAudioBackup.Add(GetAudioBackupForCategory("EnvironmentSFX"));
		prefabAudioBackup.Add(GetAudioBackupForCategory("CharacterSFX"));
		prefabAudioBackup.Add(GetAudioBackupForCategory("SlideSounds"));
	}

	private PrefabAudioBackup GetAudioBackupForCategory(string category)
	{
		return new PrefabAudioBackup
		{
			category = category,
			volume = AudioController.GetCategoryVolume(category)
		};
	}

	public float GetPrefabVolumeForCategory(string category)
	{
		float result = 1f;
		foreach (PrefabAudioBackup item in prefabAudioBackup)
		{
			if (item.category == category)
			{
				result = item.volume;
			}
		}
		return result;
	}

	public void Initialize()
	{
		int num = 0;
		int num2 = 0;
		if (PlayerPrefs.HasKey("sfxVolume_i"))
		{
			num = PlayerPrefs.GetInt("sfxVolume_i");
		}
		else
		{
			num = 10;
			PlayerPrefs.SetInt("sfxVolume_i", num);
		}
		if (PlayerPrefs.HasKey("musicVolume_i"))
		{
			num2 = PlayerPrefs.GetInt("musicVolume_i");
		}
		else
		{
			num2 = 10;
			PlayerPrefs.SetInt("musicVolume_i", num2);
		}
		float num3 = (float)num2 / 10f;
		float num4 = (float)num / 10f;
		AudioController.SetCategoryVolume("Music", num3 * GetPrefabVolumeForCategory("Music"));
		AudioController.SetCategoryVolume("MenuSounds", num4 * GetPrefabVolumeForCategory("MenuSounds"));
		AudioController.SetCategoryVolume("InGameSFX", num4 * GetPrefabVolumeForCategory("InGameSFX"));
		AudioController.SetCategoryVolume("EnvironmentSFX", num4 * GetPrefabVolumeForCategory("EnvironmentSFX"));
		AudioController.SetCategoryVolume("CharacterSFX", num4 * GetPrefabVolumeForCategory("CharacterSFX"));
		AudioController.SetCategoryVolume("SlideSounds", num4 * GetPrefabVolumeForCategory("SlideSounds"));
	}

	public string GetCharacterFromGameObject(GameObject go)
	{
		if ((bool)go.GetComponent<PlayerGraphics>())
		{
			return go.GetComponent<PlayerGraphics>().currentCharacter.ToString();
		}
		if (go != null)
		{
			Debug.LogWarning("Character " + go.name + " doesn't have playergraphics");
		}
		else
		{
			Debug.LogWarning("Character is null");
		}
		return string.Empty;
	}

	public AudioObject PlayCharacterDeath(GameObject character)
	{
		float volume = 1f;
		if (Singleton<PlayerManager>.SP.IsGhost(character))
		{
			float value = Vector3.Distance(Singleton<PlayerManager>.SP.GetPlayer().transform.position, Singleton<PlayerManager>.SP.GetGhost().transform.position);
			volume = Mathf.InverseLerp(30f, 10f, value);
		}
		return AudioController.Play(GetCharacterFromGameObject(character) + "_death", volume);
	}

	public void PlayCharacterJump(GameObject character)
	{
		float volume = 1f;
		if (Singleton<PlayerManager>.SP.IsGhost(character))
		{
			float value = Vector3.Distance(Singleton<PlayerManager>.SP.GetPlayer().transform.position, Singleton<PlayerManager>.SP.GetGhost().transform.position);
			volume = Mathf.InverseLerp(30f, 10f, value);
		}
		AudioController.Play(GetCharacterFromGameObject(character) + "_jump", volume);
	}

	public AudioObject PlayCharacterLaugh(GameObject character, float volume = 1f)
	{
		return AudioController.Play(GetCharacterFromGameObject(character) + "_laugh", volume);
	}

	public void PlayCharacterWoo(GameObject character)
	{
		float volume = 1f;
		if (Singleton<PlayerManager>.SP.IsGhost(character))
		{
			float value = Vector3.Distance(Singleton<PlayerManager>.SP.GetPlayer().transform.position, Singleton<PlayerManager>.SP.GetGhost().transform.position);
			volume = Mathf.InverseLerp(30f, 10f, value);
		}
		string audioID = GetCharacterFromGameObject(character) + "_woo";
		if (!AudioController.IsPlaying(audioID))
		{
			AudioController.Play(audioID, volume);
		}
	}

	public void PlayCharacterBoost(GameObject character)
	{
		PlayCharacterWoo(character);
		AdditionalSkinAudio componentInChildren = character.GetComponentInChildren<AdditionalSkinAudio>();
		if (componentInChildren != null)
		{
			componentInChildren.Play(SfxEvents.Speedboost);
		}
	}

	public void PlayEnvironmentAudio(bool pregame)
	{
		LoopingAudio[] array = Object.FindObjectsOfType<LoopingAudio>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play(pregame);
		}
	}

	public void PlayCharacterLavaDeath(GameObject character)
	{
		AudioController.Play("Lava_hit");
		AudioController.Play("Lava_bubbles", 1f, 0.5f);
		Singleton<AudioManager>.SP.PlayCharacterDeath(character);
		AdditionalSkinAudio componentInChildren = character.GetComponentInChildren<AdditionalSkinAudio>();
		if (componentInChildren != null)
		{
			componentInChildren.Play(SfxEvents.LavaDeath);
		}
	}

	private void PlayDelayed(string soundName, float volume, float delay)
	{
		if (delay == 0f)
		{
			AudioController.Play(soundName, volume);
		}
		else
		{
			StartCoroutine(PlayDelayedRoutine(soundName, volume, delay));
		}
	}

	private IEnumerator PlayDelayedRoutine(string soundName, float volume, float delay)
	{
		yield return new WaitForSeconds(delay);
		AudioController.Play(soundName, volume);
	}

	public void PlayCharacterVictory(GameObject character, float delay)
	{
		PlayDelayed(GetCharacterFromGameObject(character) + "_victory", 1f, delay);
	}

	public void PlayCharacterDefeat(GameObject character, float delay = 0f)
	{
		PlayDelayed(GetCharacterFromGameObject(character) + "_defeat", 1f, delay);
	}

	public void PlayCharacterDefeat(CharacterSelect.Characters character, float delay = 0f)
	{
		PlayDelayed(character.ToString() + "_defeat", 1f, delay);
	}

	public void PlayCharacterImpressed(CharacterSelect.Characters character, float delay = 0f)
	{
		PlayDelayed(character.ToString() + "_impressed", 1f, delay);
	}

	public void PlayCharacterTaunt(CharacterSelect.Characters character, float delay)
	{
		PlayDelayed(character.ToString() + "_taunt", 1f, delay);
	}

	public void PlayCharacterTaunt2D(GameObject character)
	{
		AudioController.Play(GetCharacterFromGameObject(character) + "_taunt");
	}

	public AudioObject PlayCharacterTaunt(GameObject character, float volume)
	{
		return AudioController.Play(GetCharacterFromGameObject(character) + "_taunt", volume);
	}

	public void PlayCharacterResetSound(GameObject character)
	{
		if (!Singleton<PlayerManager>.SP.IsGhost(character))
		{
			PlayDelayed(GetCharacterFromGameObject(character) + "_retry", 1f, 1.3f);
		}
	}

	public void PlayCharacterIntro(CharacterSelect.Characters character, int skinNum)
	{
		if (skinNum == 0)
		{
			PlayDelayed(character.ToString() + "_intro", 1f, 0.85f);
		}
		else if (AudioController.GetAudioItem(character.ToString() + skinNum + "_intro") != null)
		{
			PlayDelayed(character.ToString() + skinNum + "_intro", 1f, 0.85f);
		}
		else
		{
			PlayDelayed(character.ToString() + "_intro", 1f, 0.85f);
		}
	}

	public void PlayCharacterJumppad(GameObject character)
	{
		if (Singleton<PlayerManager>.SP.IsGhost(character))
		{
			float value = Vector3.Distance(Singleton<PlayerManager>.SP.GetPlayer().transform.position, Singleton<PlayerManager>.SP.GetGhost().transform.position);
			Mathf.InverseLerp(30f, 10f, value);
		}
		PlayCharacterWoo(character);
		AdditionalSkinAudio componentInChildren = character.GetComponentInChildren<AdditionalSkinAudio>();
		if (componentInChildren != null)
		{
			componentInChildren.Play(SfxEvents.Jumppad);
		}
	}

	public void PlayCharacterSuccess(GameObject character, float delay = 0f)
	{
		string characterFromGameObject = GetCharacterFromGameObject(character);
		if (AudioController.IsPlaying(characterFromGameObject + "_jump"))
		{
			AudioController.Stop(characterFromGameObject + "_jump");
		}
		PlayDelayed(characterFromGameObject + "_success", 1f, delay);
	}

	public void PlayPostgame()
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() != GameMode.LevelEditor && !AudioController.IsPlaying("theme_postgame"))
		{
			AudioController.StopMusic(0.3f);
			AudioController.Play("theme_postgame");
		}
	}

	public void PlayCheckboxToggleSound(bool onOff)
	{
		if (onOff)
		{
			AudioController.Play("CheckboxToggleOff");
		}
		else
		{
			AudioController.Play("CheckboxToggleOn");
		}
	}

	public void PlayLoadingTheme()
	{
		if (!AudioController.IsPlaying("LoadingScreen"))
		{
			StopMusic();
			AudioController.PlayMusic("LoadingScreen");
			AudioController.GetCurrentMusic().audioTime = Random.Range(0f, AudioController.GetCurrentMusic().clipLength * 0.8f);
		}
	}

	public void PlayEditorTheme()
	{
		if (!AudioController.IsPlaying("theme_leveleditor"))
		{
			AudioController.StopMusic(0.3f);
			AudioController.PlayMusic("theme_leveleditor");
		}
	}

	public void StopMusic()
	{
		AudioController.StopCategory("Music");
	}

	public bool IsIngameThemePlaying()
	{
		if (!AudioController.GetCurrentMusic())
		{
			return false;
		}
		if (AudioController.GetCurrentMusic().audioID.StartsWith("theme_"))
		{
			return true;
		}
		return false;
	}

	public void PlayIngameTheme()
	{
		if (IsIngameThemePlaying())
		{
			return;
		}
		int batchNumFromLevel = Singleton<LevelBatchManager>.SP.GetBatchNumFromLevel(Singleton<LevelBatchManager>.SP.GetCurrentLevelObj());
		bool num = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Bonus;
		bool flag = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelStyle == LevelStyle.Credits_Space;
		string text;
		if (num)
		{
			text = "theme_bonus";
		}
		else if (flag)
		{
			text = "theme_credits";
		}
		else if (batchNumFromLevel != -1)
		{
			LevelBatch batchFromNum = Singleton<LevelBatchManager>.SP.GetBatchFromNum(batchNumFromLevel);
			if ((bool)batchFromNum)
			{
				text = batchFromNum.audioTheme.ToString();
				if (text == "theme_henk" && Random.Range(0, 100) < 6)
				{
					text = "theme_henk_8bit";
				}
			}
			else
			{
				text = "theme_henk";
			}
		}
		else
		{
			text = "theme_henk";
		}
		PlayerPrefs.GetInt("playRandomMusic", 0);
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor)
		{
			text = GetAudioStringFromEnvironment(Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelStyle);
		}
		if (!AudioController.IsPlaying(text))
		{
			AudioController.StopCategory("Music", 1f);
			AudioObject audioObject = AudioController.PlayMusic(text);
			gameThemeVolume = audioObject.volume;
		}
	}

	public string GetAudioStringFromEnvironment(LevelStyle style)
	{
		return style switch
		{
			LevelStyle.KidsRoom_Day => "theme_henk", 
			LevelStyle.KidsRoom_Disco => "theme_neil", 
			LevelStyle.KidsRoom_Halloween => "theme_halloween", 
			LevelStyle.City => "theme_bonus", 
			LevelStyle.Island_Water => "theme_henk", 
			LevelStyle.Island_Beach => "theme_henk", 
			LevelStyle.Island_Jungle => "theme_henk", 
			LevelStyle.Island_Night => "theme_henk", 
			_ => "theme_henk", 
		};
	}

	public float GetCurrentMusicTime()
	{
		if (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_InGameLevelEditor)))
		{
			return 0f;
		}
		if (!AudioController.GetCurrentMusic())
		{
			return Time.timeSinceLevelLoad;
		}
		float num = AudioController.GetCurrentMusic().primaryAudioSource.time;
		if (AudioController.GetCurrentMusic().audioItem.subItems.Length > 1 && AudioController.GetCurrentMusic().secondaryAudioSource != null)
		{
			num = ((!AudioController.GetCurrentMusic().secondaryAudioSource.isPlaying) ? (num + AudioController.GetCurrentMusic().secondaryAudioSource.clip.length) : AudioController.GetCurrentMusic().secondaryAudioSource.time);
		}
		return num;
	}
}
