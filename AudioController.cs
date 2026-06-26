using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("ClockStone/Audio/AudioController")]
public class AudioController : SingletonMonoBehaviour<AudioController>
{
	public const string AUDIO_TOOLKIT_VERSION = "6.6";

	public GameObject AudioObjectPrefab;

	public bool Persistent;

	public bool UnloadAudioClipsOnDestroy;

	public bool UsePooledAudioObjects = true;

	public bool PlayWithZeroVolume;

	public bool EqualPowerCrossfade;

	public float musicCrossFadeTime;

	public bool specifyCrossFadeInAndOutSeperately;

	[SerializeField]
	private float _musicCrossFadeTime_In;

	[SerializeField]
	private float _musicCrossFadeTime_Out;

	public AudioCategory[] AudioCategories;

	public string[] musicPlaylist;

	public bool loopPlaylist;

	public bool shufflePlaylist;

	public bool crossfadePlaylist;

	public float delayBetweenPlaylistTracks = 1f;

	protected static PoolableReference<AudioObject> _currentMusicReference = new PoolableReference<AudioObject>();

	protected AudioListener _currentAudioListener;

	private bool _musicEnabled = true;

	private bool _soundMuted;

	private bool _categoriesValidated;

	[SerializeField]
	private bool _isAdditionalAudioController;

	[SerializeField]
	private bool _audioDisabled;

	private Dictionary<string, AudioItem> _audioItems;

	private static List<int> _playlistPlayed;

	private static bool _isPlaylistPlaying = false;

	[SerializeField]
	private float _volume = 1f;

	private static double _systemTime;

	private static double _lastSystemTime = -1.0;

	private static double _systemDeltaTime = -1.0;

	private List<AudioController> _additionalAudioControllers;

	public AudioController_CurrentInspectorSelection _currentInspectorSelection = new AudioController_CurrentInspectorSelection();

	public bool DisableAudio
	{
		get
		{
			return _audioDisabled;
		}
		set
		{
			if (value != _audioDisabled)
			{
				_audioDisabled = value;
			}
		}
	}

	public bool isAdditionalAudioController
	{
		get
		{
			return _isAdditionalAudioController;
		}
		set
		{
			_isAdditionalAudioController = value;
		}
	}

	public float Volume
	{
		get
		{
			return _volume;
		}
		set
		{
			if (value != _volume)
			{
				_volume = value;
				_ApplyVolumeChange();
			}
		}
	}

	public bool musicEnabled
	{
		get
		{
			return _musicEnabled;
		}
		set
		{
			if (_musicEnabled == value)
			{
				return;
			}
			_musicEnabled = value;
			if (!_currentMusic)
			{
				return;
			}
			if (value)
			{
				if (_currentMusic.IsPaused())
				{
					_currentMusic.Play();
				}
			}
			else
			{
				_currentMusic.Pause();
			}
		}
	}

	public bool soundMuted
	{
		get
		{
			return _soundMuted;
		}
		set
		{
			_soundMuted = value;
			_ApplyVolumeChange();
		}
	}

	public float musicCrossFadeTime_In
	{
		get
		{
			if (specifyCrossFadeInAndOutSeperately)
			{
				return _musicCrossFadeTime_In;
			}
			return musicCrossFadeTime;
		}
		set
		{
			_musicCrossFadeTime_In = value;
		}
	}

	public float musicCrossFadeTime_Out
	{
		get
		{
			if (specifyCrossFadeInAndOutSeperately)
			{
				return _musicCrossFadeTime_Out;
			}
			return musicCrossFadeTime;
		}
		set
		{
			_musicCrossFadeTime_Out = value;
		}
	}

	public static double systemTime => _systemTime;

	public static double systemDeltaTime => _systemDeltaTime;

	private static AudioObject _currentMusic
	{
		get
		{
			return _currentMusicReference.Get();
		}
		set
		{
			_currentMusicReference.Set(value, allowNonePoolable: true);
		}
	}

	public override bool isSingletonObject => !_isAdditionalAudioController;

	public AudioController()
	{
		SingletonMonoBehaviour<AudioController>.SetSingletonType(typeof(AudioController));
	}

	public static AudioObject PlayMusic(string audioID, float volume, float delay, float startTime = 0f)
	{
		_isPlaylistPlaying = false;
		return SingletonMonoBehaviour<AudioController>.Instance._PlayMusic(audioID, volume, delay, startTime);
	}

	public static AudioObject PlayMusic(string audioID)
	{
		return PlayMusic(audioID, 1f, 0f);
	}

	public static AudioObject PlayMusic(string audioID, float volume)
	{
		return PlayMusic(audioID, volume, 0f);
	}

	public static AudioObject PlayMusic(string audioID, Vector3 worldPosition, Transform parentObj = null, float volume = 1f, float delay = 0f, float startTime = 0f)
	{
		_isPlaylistPlaying = false;
		return SingletonMonoBehaviour<AudioController>.Instance._PlayMusic(audioID, worldPosition, parentObj, volume, delay, startTime);
	}

	public static AudioObject PlayMusic(string audioID, Transform parentObj, float volume = 1f, float delay = 0f, float startTime = 0f)
	{
		_isPlaylistPlaying = false;
		return SingletonMonoBehaviour<AudioController>.Instance._PlayMusic(audioID, parentObj.position, parentObj, volume, delay, startTime);
	}

	public static bool StopMusic()
	{
		return SingletonMonoBehaviour<AudioController>.Instance._StopMusic(0f);
	}

	public static bool StopMusic(float fadeOut)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._StopMusic(fadeOut);
	}

	public static bool PauseMusic(float fadeOut = 0f)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._PauseMusic(fadeOut);
	}

	public static bool IsMusicPaused()
	{
		if (_currentMusic != null)
		{
			return _currentMusic.IsPaused();
		}
		return false;
	}

	public static bool UnpauseMusic(float fadeIn = 0f)
	{
		if (!SingletonMonoBehaviour<AudioController>.Instance._musicEnabled)
		{
			return false;
		}
		if (_currentMusic != null && _currentMusic.IsPaused())
		{
			_currentMusic.Unpause(fadeIn);
			return true;
		}
		return false;
	}

	public static int EnqueueMusic(string audioID)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._EnqueueMusic(audioID);
	}

	public static string[] GetMusicPlaylist()
	{
		string[] array = new string[(SingletonMonoBehaviour<AudioController>.Instance.musicPlaylist != null) ? SingletonMonoBehaviour<AudioController>.Instance.musicPlaylist.Length : 0];
		if (array.Length != 0)
		{
			Array.Copy(SingletonMonoBehaviour<AudioController>.Instance.musicPlaylist, array, array.Length);
		}
		return array;
	}

	public static void SetMusicPlaylist(string[] playlist)
	{
		string[] array = new string[(playlist != null) ? playlist.Length : 0];
		if (array.Length != 0)
		{
			Array.Copy(playlist, array, array.Length);
		}
		SingletonMonoBehaviour<AudioController>.Instance.musicPlaylist = array;
	}

	public static AudioObject PlayMusicPlaylist()
	{
		return SingletonMonoBehaviour<AudioController>.Instance._PlayMusicPlaylist();
	}

	public static AudioObject PlayNextMusicOnPlaylist()
	{
		if (IsPlaylistPlaying())
		{
			return SingletonMonoBehaviour<AudioController>.Instance._PlayNextMusicOnPlaylist(0f);
		}
		return null;
	}

	public static AudioObject PlayPreviousMusicOnPlaylist()
	{
		if (IsPlaylistPlaying())
		{
			return SingletonMonoBehaviour<AudioController>.Instance._PlayPreviousMusicOnPlaylist(0f);
		}
		return null;
	}

	public static bool IsPlaylistPlaying()
	{
		return _isPlaylistPlaying;
	}

	public static void ClearPlaylist()
	{
		SingletonMonoBehaviour<AudioController>.Instance.musicPlaylist = null;
	}

	public static AudioObject Play(string audioID)
	{
		AudioListener currentAudioListener = GetCurrentAudioListener();
		if (currentAudioListener == null)
		{
			Debug.LogWarning("No AudioListener found in the scene");
			return null;
		}
		return Play(audioID, currentAudioListener.transform.position + currentAudioListener.transform.forward, null, 1f);
	}

	public static AudioObject Play(string audioID, float volume, float delay = 0f, float startTime = 0f)
	{
		AudioListener currentAudioListener = GetCurrentAudioListener();
		if (currentAudioListener == null)
		{
			Debug.LogWarning("No AudioListener found in the scene");
			return null;
		}
		return Play(audioID, currentAudioListener.transform.position + currentAudioListener.transform.forward, null, volume, delay, startTime);
	}

	public static AudioObject Play(string audioID, Transform parentObj)
	{
		return Play(audioID, parentObj.position, parentObj, 1f);
	}

	public static AudioObject Play(string audioID, Transform parentObj, float volume, float delay = 0f, float startTime = 0f)
	{
		return Play(audioID, parentObj.position, parentObj, volume, delay, startTime);
	}

	public static AudioObject Play(string audioID, Vector3 worldPosition, Transform parentObj = null)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._PlayAsSound(audioID, 1f, worldPosition, parentObj, 0f, 0f, playWithoutAudioObject: false);
	}

	public static AudioObject Play(string audioID, Vector3 worldPosition, Transform parentObj, float volume, float delay = 0f, float startTime = 0f)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._PlayAsSound(audioID, volume, worldPosition, parentObj, delay, startTime, playWithoutAudioObject: false);
	}

	public static AudioObject PlayScheduled(string audioID, double dspTime, Vector3 worldPosition, Transform parentObj = null, float volume = 1f, float startTime = 0f)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._PlayAsSound(audioID, volume, worldPosition, parentObj, 0f, startTime, playWithoutAudioObject: false, dspTime);
	}

	public static AudioObject PlayAfter(string audioID, AudioObject playingAudio, double deltaDspTime = 0.0, float volume = 1f, float startTime = 0f)
	{
		double num = AudioSettings.dspTime;
		if (playingAudio.IsPlaying())
		{
			num += (double)playingAudio.timeUntilEnd;
		}
		num += deltaDspTime;
		return PlayScheduled(audioID, num, playingAudio.transform.position, playingAudio.transform.parent, volume, startTime);
	}

	public static bool Stop(string audioID, float fadeOutLength)
	{
		if (SingletonMonoBehaviour<AudioController>.Instance._GetAudioItem(audioID) == null)
		{
			Debug.LogWarning("Audio item with name '" + audioID + "' does not exist");
			return false;
		}
		AudioObject[] playingAudioObjects = GetPlayingAudioObjects(audioID);
		AudioObject[] array = playingAudioObjects;
		foreach (AudioObject audioObject in array)
		{
			if (fadeOutLength < 0f)
			{
				audioObject.Stop();
			}
			else
			{
				audioObject.Stop(fadeOutLength);
			}
		}
		return playingAudioObjects.Length != 0;
	}

	public static bool Stop(string audioID)
	{
		return Stop(audioID, -1f);
	}

	public static void StopAll(float fadeOutLength)
	{
		SingletonMonoBehaviour<AudioController>.Instance._StopMusic(fadeOutLength);
		AudioObject[] playingAudioObjects = GetPlayingAudioObjects();
		for (int i = 0; i < playingAudioObjects.Length; i++)
		{
			playingAudioObjects[i].Stop(fadeOutLength);
		}
	}

	public static void StopAll()
	{
		StopAll(-1f);
	}

	public static void PauseAll(float fadeOutLength = 0f)
	{
		SingletonMonoBehaviour<AudioController>.Instance._PauseMusic(fadeOutLength);
		AudioObject[] playingAudioObjects = GetPlayingAudioObjects();
		for (int i = 0; i < playingAudioObjects.Length; i++)
		{
			playingAudioObjects[i].Pause(fadeOutLength);
		}
	}

	public static void UnpauseAll(float fadeInLength = 0f)
	{
		UnpauseMusic(fadeInLength);
		AudioObject[] playingAudioObjects = GetPlayingAudioObjects(includePausedAudio: true);
		AudioController instance = SingletonMonoBehaviour<AudioController>.Instance;
		AudioObject[] array = playingAudioObjects;
		foreach (AudioObject audioObject in array)
		{
			if (audioObject.IsPaused() && (instance.musicEnabled || !(_currentMusic == audioObject)))
			{
				audioObject.Unpause(fadeInLength);
			}
		}
	}

	public static void PauseCategory(string categoryName, float fadeOutLength = 0f)
	{
		if (_currentMusic != null && _currentMusic.category.Name == categoryName)
		{
			PauseMusic(fadeOutLength);
		}
		AudioObject[] playingAudioObjectsInCategory = GetPlayingAudioObjectsInCategory(categoryName);
		for (int i = 0; i < playingAudioObjectsInCategory.Length; i++)
		{
			playingAudioObjectsInCategory[i].Pause(fadeOutLength);
		}
	}

	public static void UnpauseCategory(string categoryName, float fadeInLength = 0f)
	{
		if (_currentMusic != null && _currentMusic.category.Name == categoryName)
		{
			UnpauseMusic(fadeInLength);
		}
		AudioObject[] playingAudioObjectsInCategory = GetPlayingAudioObjectsInCategory(categoryName, includePausedAudio: true);
		foreach (AudioObject audioObject in playingAudioObjectsInCategory)
		{
			if (audioObject.IsPaused())
			{
				audioObject.Unpause(fadeInLength);
			}
		}
	}

	public static void StopCategory(string categoryName, float fadeOutLength = 0f)
	{
		if (_currentMusic != null && _currentMusic.category.Name == categoryName)
		{
			StopMusic(fadeOutLength);
		}
		AudioObject[] playingAudioObjectsInCategory = GetPlayingAudioObjectsInCategory(categoryName);
		for (int i = 0; i < playingAudioObjectsInCategory.Length; i++)
		{
			playingAudioObjectsInCategory[i].Stop(fadeOutLength);
		}
	}

	public static bool IsPlaying(string audioID)
	{
		return GetPlayingAudioObjects(audioID).Length != 0;
	}

	public static AudioObject[] GetPlayingAudioObjects(string audioID, bool includePausedAudio = false)
	{
		AudioObject[] playingAudioObjects = GetPlayingAudioObjects(includePausedAudio);
		List<AudioObject> list = new List<AudioObject>();
		AudioObject[] array = playingAudioObjects;
		foreach (AudioObject audioObject in array)
		{
			if (audioObject.audioID == audioID)
			{
				list.Add(audioObject);
			}
		}
		return list.ToArray();
	}

	public static AudioObject[] GetPlayingAudioObjectsInCategory(string categoryName, bool includePausedAudio = false)
	{
		AudioObject[] playingAudioObjects = GetPlayingAudioObjects(includePausedAudio);
		List<AudioObject> list = new List<AudioObject>();
		AudioObject[] array = playingAudioObjects;
		foreach (AudioObject audioObject in array)
		{
			if (audioObject.DoesBelongToCategory(categoryName))
			{
				list.Add(audioObject);
			}
		}
		return list.ToArray();
	}

	public static AudioObject[] GetPlayingAudioObjects(bool includePausedAudio = false)
	{
		List<AudioObject> list = new List<AudioObject>();
		object[] allOfType = RegisteredComponentController.GetAllOfType(typeof(AudioObject));
		for (int i = 0; i < allOfType.Length; i++)
		{
			AudioObject audioObject = (AudioObject)allOfType[i];
			if (audioObject.IsPlaying() || (includePausedAudio && audioObject.IsPaused()))
			{
				list.Add(audioObject);
			}
		}
		return list.ToArray();
	}

	public static int GetPlayingAudioObjectsCount(string audioID, bool includePausedAudio = false)
	{
		AudioObject[] playingAudioObjects = GetPlayingAudioObjects(includePausedAudio);
		int num = 0;
		AudioObject[] array = playingAudioObjects;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].audioID == audioID)
			{
				num++;
			}
		}
		return num;
	}

	public static void EnableMusic(bool b)
	{
		SingletonMonoBehaviour<AudioController>.Instance.musicEnabled = b;
	}

	public static void MuteSound(bool b)
	{
		SingletonMonoBehaviour<AudioController>.Instance.soundMuted = b;
	}

	public static bool IsMusicEnabled()
	{
		return SingletonMonoBehaviour<AudioController>.Instance.musicEnabled;
	}

	public static bool IsSoundMuted()
	{
		return SingletonMonoBehaviour<AudioController>.Instance.soundMuted;
	}

	public static AudioListener GetCurrentAudioListener()
	{
		AudioController instance = SingletonMonoBehaviour<AudioController>.Instance;
		if (instance._currentAudioListener != null && instance._currentAudioListener.gameObject == null)
		{
			instance._currentAudioListener = null;
		}
		if (instance._currentAudioListener == null)
		{
			instance._currentAudioListener = (AudioListener)UnityEngine.Object.FindObjectOfType(typeof(AudioListener));
		}
		return instance._currentAudioListener;
	}

	public static AudioObject GetCurrentMusic()
	{
		return _currentMusic;
	}

	public static AudioCategory GetCategory(string name)
	{
		AudioController instance = SingletonMonoBehaviour<AudioController>.Instance;
		AudioCategory audioCategory = instance._GetCategory(name);
		if (audioCategory != null)
		{
			return audioCategory;
		}
		if (instance._additionalAudioControllers != null)
		{
			foreach (AudioController additionalAudioController in instance._additionalAudioControllers)
			{
				audioCategory = additionalAudioController._GetCategory(name);
				if (audioCategory != null)
				{
					return audioCategory;
				}
			}
		}
		return null;
	}

	public static void SetCategoryVolume(string name, float volume)
	{
		bool flag = false;
		AudioController instance = SingletonMonoBehaviour<AudioController>.Instance;
		AudioCategory audioCategory = instance._GetCategory(name);
		if (audioCategory != null)
		{
			audioCategory.Volume = volume;
			flag = true;
		}
		if (instance._additionalAudioControllers != null)
		{
			foreach (AudioController additionalAudioController in instance._additionalAudioControllers)
			{
				audioCategory = additionalAudioController._GetCategory(name);
				if (audioCategory != null)
				{
					audioCategory.Volume = volume;
					flag = true;
				}
			}
		}
		if (!flag)
		{
			Debug.LogWarning("No audio category with name " + name);
		}
	}

	public static float GetCategoryVolume(string name)
	{
		AudioCategory category = GetCategory(name);
		if (category != null)
		{
			return category.Volume;
		}
		Debug.LogWarning("No audio category with name " + name);
		return 0f;
	}

	public static void SetGlobalVolume(float volume)
	{
		AudioController instance = SingletonMonoBehaviour<AudioController>.Instance;
		instance.Volume = volume;
		if (instance._additionalAudioControllers == null)
		{
			return;
		}
		foreach (AudioController additionalAudioController in instance._additionalAudioControllers)
		{
			additionalAudioController.Volume = volume;
		}
	}

	public static float GetGlobalVolume()
	{
		return SingletonMonoBehaviour<AudioController>.Instance.Volume;
	}

	public static AudioCategory NewCategory(string categoryName)
	{
		int num = ((SingletonMonoBehaviour<AudioController>.Instance.AudioCategories != null) ? SingletonMonoBehaviour<AudioController>.Instance.AudioCategories.Length : 0);
		AudioCategory[] audioCategories = SingletonMonoBehaviour<AudioController>.Instance.AudioCategories;
		SingletonMonoBehaviour<AudioController>.Instance.AudioCategories = new AudioCategory[num + 1];
		if (num > 0)
		{
			audioCategories.CopyTo(SingletonMonoBehaviour<AudioController>.Instance.AudioCategories, 0);
		}
		AudioCategory audioCategory = new AudioCategory(SingletonMonoBehaviour<AudioController>.Instance);
		audioCategory.Name = categoryName;
		SingletonMonoBehaviour<AudioController>.Instance.AudioCategories[num] = audioCategory;
		SingletonMonoBehaviour<AudioController>.Instance._InvalidateCategories();
		return audioCategory;
	}

	public static void RemoveCategory(string categoryName)
	{
		int num = -1;
		int num2 = ((SingletonMonoBehaviour<AudioController>.Instance.AudioCategories != null) ? SingletonMonoBehaviour<AudioController>.Instance.AudioCategories.Length : 0);
		for (int i = 0; i < num2; i++)
		{
			if (SingletonMonoBehaviour<AudioController>.Instance.AudioCategories[i].Name == categoryName)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			Debug.LogError("AudioCategory does not exist: " + categoryName);
			return;
		}
		AudioCategory[] array = new AudioCategory[SingletonMonoBehaviour<AudioController>.Instance.AudioCategories.Length - 1];
		for (int j = 0; j < num; j++)
		{
			array[j] = SingletonMonoBehaviour<AudioController>.Instance.AudioCategories[j];
		}
		for (int k = num + 1; k < SingletonMonoBehaviour<AudioController>.Instance.AudioCategories.Length; k++)
		{
			array[k - 1] = SingletonMonoBehaviour<AudioController>.Instance.AudioCategories[k];
		}
		SingletonMonoBehaviour<AudioController>.Instance.AudioCategories = array;
		SingletonMonoBehaviour<AudioController>.Instance._InvalidateCategories();
	}

	public static void AddToCategory(AudioCategory category, AudioItem audioItem)
	{
		int num = ((category.AudioItems != null) ? category.AudioItems.Length : 0);
		AudioItem[] audioItems = category.AudioItems;
		category.AudioItems = new AudioItem[num + 1];
		if (num > 0)
		{
			audioItems.CopyTo(category.AudioItems, 0);
		}
		category.AudioItems[num] = audioItem;
		SingletonMonoBehaviour<AudioController>.Instance._InvalidateCategories();
	}

	public static AudioItem AddToCategory(AudioCategory category, AudioClip audioClip, string audioID)
	{
		AudioItem audioItem = new AudioItem();
		audioItem.Name = audioID;
		audioItem.subItems = new AudioSubItem[1];
		AudioSubItem audioSubItem = new AudioSubItem();
		audioSubItem.Clip = audioClip;
		audioItem.subItems[0] = audioSubItem;
		AddToCategory(category, audioItem);
		return audioItem;
	}

	public static bool RemoveAudioItem(string audioID)
	{
		AudioItem audioItem = SingletonMonoBehaviour<AudioController>.Instance._GetAudioItem(audioID);
		if (audioItem != null)
		{
			int num = audioItem.category._GetIndexOf(audioItem);
			if (num < 0)
			{
				return false;
			}
			AudioItem[] audioItems = audioItem.category.AudioItems;
			AudioItem[] array = new AudioItem[audioItems.Length - 1];
			for (int i = 0; i < num; i++)
			{
				array[i] = audioItems[i];
			}
			for (int j = num + 1; j < audioItems.Length; j++)
			{
				array[j - 1] = audioItems[j];
			}
			audioItem.category.AudioItems = array;
			if (SingletonMonoBehaviour<AudioController>.Instance._categoriesValidated)
			{
				SingletonMonoBehaviour<AudioController>.Instance._audioItems.Remove(audioID);
			}
			return true;
		}
		return false;
	}

	public static bool IsValidAudioID(string audioID)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._GetAudioItem(audioID) != null;
	}

	public static AudioItem GetAudioItem(string audioID)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._GetAudioItem(audioID);
	}

	public static void DetachAllAudios(GameObject gameObjectWithAudios)
	{
		AudioObject[] componentsInChildren = gameObjectWithAudios.GetComponentsInChildren<AudioObject>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].transform.parent = null;
		}
	}

	public static float GetAudioItemMaxDistance(string audioID)
	{
		AudioItem audioItem = GetAudioItem(audioID);
		if (audioItem.overrideAudioSourceSettings)
		{
			return audioItem.audioSource_MaxDistance;
		}
		if (audioItem.category.AudioObjectPrefab != null)
		{
			return audioItem.category.AudioObjectPrefab.audio.maxDistance;
		}
		return audioItem.category.audioController.AudioObjectPrefab.audio.maxDistance;
	}

	public void UnloadAllAudioClips()
	{
		AudioCategory[] audioCategories = AudioCategories;
		for (int i = 0; i < audioCategories.Length; i++)
		{
			audioCategories[i].UnloadAllAudioClips();
		}
	}

	private void _ApplyVolumeChange()
	{
		AudioObject[] playingAudioObjects = GetPlayingAudioObjects(includePausedAudio: true);
		for (int i = 0; i < playingAudioObjects.Length; i++)
		{
			playingAudioObjects[i]._ApplyVolumeBoth();
		}
	}

	internal AudioItem _GetAudioItem(string audioID)
	{
		_ValidateCategories();
		if (_audioItems.TryGetValue(audioID, out var value))
		{
			return value;
		}
		return null;
	}

	protected AudioObject _PlayMusic(string audioID, float volume, float delay, float startTime)
	{
		AudioListener currentAudioListener = GetCurrentAudioListener();
		if (currentAudioListener == null)
		{
			Debug.LogWarning("No AudioListener found in the scene");
			return null;
		}
		return _PlayMusic(audioID, currentAudioListener.transform.position + currentAudioListener.transform.forward, null, volume, delay, startTime);
	}

	protected bool _StopMusic(float fadeOutLength)
	{
		if (_currentMusic != null)
		{
			_currentMusic.Stop(fadeOutLength);
			_currentMusic = null;
			return true;
		}
		return false;
	}

	protected bool _PauseMusic(float fadeOut)
	{
		if (_currentMusic != null)
		{
			_currentMusic.Pause(fadeOut);
			return true;
		}
		return false;
	}

	protected AudioObject _PlayMusic(string audioID, Vector3 position, Transform parentObj, float volume, float delay, float startTime)
	{
		if (!IsMusicEnabled())
		{
			return null;
		}
		bool flag;
		if (_currentMusic != null && _currentMusic.IsPlaying())
		{
			flag = true;
			_currentMusic.Stop(musicCrossFadeTime_Out);
		}
		else
		{
			flag = false;
		}
		_currentMusic = _PlayAsMusic(audioID, volume, position, parentObj, delay, startTime, playWithoutAudioObject: false);
		if ((bool)_currentMusic && flag && musicCrossFadeTime_In > 0f)
		{
			_currentMusic.FadeIn(musicCrossFadeTime_In);
		}
		return _currentMusic;
	}

	protected int _EnqueueMusic(string audioID)
	{
		int num = ((musicPlaylist == null) ? 1 : (musicPlaylist.Length + 1));
		string[] array = new string[num];
		if (musicPlaylist != null)
		{
			musicPlaylist.CopyTo(array, 0);
		}
		array[num - 1] = audioID;
		musicPlaylist = array;
		return num;
	}

	protected AudioObject _PlayMusicPlaylist()
	{
		_ResetLastPlayedList();
		return _PlayNextMusicOnPlaylist(0f);
	}

	private AudioObject _PlayMusicTrackWithID(int nextTrack, float delay, bool addToPlayedList)
	{
		if (nextTrack < 0)
		{
			return null;
		}
		_playlistPlayed.Add(nextTrack);
		_isPlaylistPlaying = true;
		AudioObject audioObject = _PlayMusic(musicPlaylist[nextTrack], 1f, delay, 0f);
		if (audioObject != null)
		{
			audioObject._isCurrentPlaylistTrack = true;
			audioObject.primaryAudioSource.loop = false;
		}
		return audioObject;
	}

	internal AudioObject _PlayNextMusicOnPlaylist(float delay)
	{
		int nextTrack = _GetNextMusicTrack();
		return _PlayMusicTrackWithID(nextTrack, delay, addToPlayedList: true);
	}

	internal AudioObject _PlayPreviousMusicOnPlaylist(float delay)
	{
		int nextTrack = _GetPreviousMusicTrack();
		return _PlayMusicTrackWithID(nextTrack, delay, addToPlayedList: false);
	}

	private void _ResetLastPlayedList()
	{
		_playlistPlayed.Clear();
	}

	protected int _GetNextMusicTrack()
	{
		if (musicPlaylist == null || musicPlaylist.Length == 0)
		{
			return -1;
		}
		if (musicPlaylist.Length == 1)
		{
			return 0;
		}
		if (shufflePlaylist)
		{
			return _GetNextMusicTrackShuffled();
		}
		return _GetNextMusicTrackInOrder();
	}

	protected int _GetPreviousMusicTrack()
	{
		if (musicPlaylist == null || musicPlaylist.Length == 0)
		{
			return -1;
		}
		if (musicPlaylist.Length == 1)
		{
			return 0;
		}
		if (shufflePlaylist)
		{
			return _GetPreviousMusicTrackShuffled();
		}
		return _GetPreviousMusicTrackInOrder();
	}

	private int _GetPreviousMusicTrackShuffled()
	{
		if (_playlistPlayed.Count >= 2)
		{
			int result = _playlistPlayed[_playlistPlayed.Count - 2];
			_RemoveLastPlayedOnList();
			_RemoveLastPlayedOnList();
			return result;
		}
		return -1;
	}

	private void _RemoveLastPlayedOnList()
	{
		_playlistPlayed.RemoveAt(_playlistPlayed.Count - 1);
	}

	private int _GetNextMusicTrackShuffled()
	{
		HashSet_Flash<int> hashSet_Flash = new HashSet_Flash<int>();
		int num = _playlistPlayed.Count;
		if (loopPlaylist)
		{
			int num2 = Mathf.Clamp(musicPlaylist.Length / 4, 2, 10);
			if (num > musicPlaylist.Length - num2)
			{
				num = musicPlaylist.Length - num2;
				if (num < 1)
				{
					num = 1;
				}
			}
		}
		else if (num >= musicPlaylist.Length)
		{
			return -1;
		}
		for (int i = 0; i < num; i++)
		{
			hashSet_Flash.Add(_playlistPlayed[_playlistPlayed.Count - 1 - i]);
		}
		List<int> list = new List<int>();
		for (int j = 0; j < musicPlaylist.Length; j++)
		{
			if (!hashSet_Flash.Contains(j))
			{
				list.Add(j);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	private int _GetNextMusicTrackInOrder()
	{
		if (_playlistPlayed.Count == 0)
		{
			return 0;
		}
		int num = _playlistPlayed[_playlistPlayed.Count - 1] + 1;
		if (num >= musicPlaylist.Length)
		{
			if (!loopPlaylist)
			{
				return -1;
			}
			num = 0;
		}
		return num;
	}

	private int _GetPreviousMusicTrackInOrder()
	{
		if (_playlistPlayed.Count < 2)
		{
			if (loopPlaylist)
			{
				return musicPlaylist.Length - 1;
			}
			return -1;
		}
		int num = _playlistPlayed[_playlistPlayed.Count - 1] - 1;
		_RemoveLastPlayedOnList();
		_RemoveLastPlayedOnList();
		if (num < 0)
		{
			if (!loopPlaylist)
			{
				return -1;
			}
			num = musicPlaylist.Length - 1;
		}
		return num;
	}

	protected AudioObject _PlayAsSound(string audioID, float volume, Vector3 worldPosition, Transform parentObj, float delay, float startTime, bool playWithoutAudioObject, double dspTime = 0.0, AudioObject useExistingAudioObject = null)
	{
		return _PlayEx(audioID, volume, worldPosition, parentObj, delay, startTime, playWithoutAudioObject, dspTime, useExistingAudioObject);
	}

	protected AudioObject _PlayAsMusic(string audioID, float volume, Vector3 worldPosition, Transform parentObj, float delay, float startTime, bool playWithoutAudioObject, double dspTime = 0.0, AudioObject useExistingAudioObject = null)
	{
		return _PlayEx(audioID, volume, worldPosition, parentObj, delay, startTime, playWithoutAudioObject, dspTime, useExistingAudioObject, playAsMusic: true);
	}

	protected AudioObject _PlayEx(string audioID, float volume, Vector3 worldPosition, Transform parentObj, float delay, float startTime, bool playWithoutAudioObject, double dspTime = 0.0, AudioObject useExistingAudioObject = null, bool playAsMusic = false)
	{
		if (_audioDisabled)
		{
			return null;
		}
		AudioItem audioItem = _GetAudioItem(audioID);
		if (audioItem == null)
		{
			Debug.LogWarning("Audio item with name '" + audioID + "' does not exist");
			return null;
		}
		if (audioItem._lastPlayedTime > 0.0 && dspTime == 0.0 && systemTime < audioItem._lastPlayedTime + (double)audioItem.MinTimeBetweenPlayCalls)
		{
			return null;
		}
		if (audioItem.MaxInstanceCount > 0)
		{
			AudioObject[] playingAudioObjects = GetPlayingAudioObjects(audioID);
			if (playingAudioObjects.Length >= audioItem.MaxInstanceCount)
			{
				bool flag = playingAudioObjects.Length > audioItem.MaxInstanceCount;
				AudioObject audioObject = null;
				for (int i = 0; i < playingAudioObjects.Length; i++)
				{
					if ((flag || !playingAudioObjects[i].isFadingOut) && (audioObject == null || playingAudioObjects[i].startedPlayingAtTime < audioObject.startedPlayingAtTime))
					{
						audioObject = playingAudioObjects[i];
					}
				}
				if (audioObject != null)
				{
					audioObject.Stop((!flag) ? 0.2f : 0f);
				}
			}
		}
		return PlayAudioItem(audioItem, volume, worldPosition, parentObj, delay, startTime, playWithoutAudioObject, useExistingAudioObject, dspTime, playAsMusic);
	}

	public AudioObject PlayAudioItem(AudioItem sndItem, float volume, Vector3 worldPosition, Transform parentObj = null, float delay = 0f, float startTime = 0f, bool playWithoutAudioObject = false, AudioObject useExistingAudioObj = null, double dspTime = 0.0, bool playAsMusic = false)
	{
		AudioObject audioObject = null;
		sndItem._lastPlayedTime = systemTime;
		AudioSubItem[] array = _ChooseSubItems(sndItem, useExistingAudioObj);
		if (array == null || array.Length == 0)
		{
			return null;
		}
		AudioSubItem[] array2 = array;
		foreach (AudioSubItem audioSubItem in array2)
		{
			if (audioSubItem == null)
			{
				continue;
			}
			AudioObject audioObject2 = PlayAudioSubItem(audioSubItem, volume, worldPosition, parentObj, delay, startTime, playWithoutAudioObject, useExistingAudioObj, dspTime, playAsMusic);
			if (!audioObject2)
			{
				continue;
			}
			audioObject = audioObject2;
			audioObject.audioID = sndItem.Name;
			if (sndItem.overrideAudioSourceSettings)
			{
				audioObject2._audioSource_MinDistance_Saved = audioObject2.primaryAudioSource.minDistance;
				audioObject2._audioSource_MaxDistance_Saved = audioObject2.primaryAudioSource.maxDistance;
				audioObject2.primaryAudioSource.minDistance = sndItem.audioSource_MinDistance;
				audioObject2.primaryAudioSource.maxDistance = sndItem.audioSource_MaxDistance;
				if (audioObject2.secondaryAudioSource != null)
				{
					audioObject2.secondaryAudioSource.minDistance = sndItem.audioSource_MinDistance;
					audioObject2.secondaryAudioSource.maxDistance = sndItem.audioSource_MaxDistance;
				}
			}
		}
		return audioObject;
	}

	internal AudioCategory _GetCategory(string name)
	{
		AudioCategory[] audioCategories = AudioCategories;
		foreach (AudioCategory audioCategory in audioCategories)
		{
			if (audioCategory.Name == name)
			{
				return audioCategory;
			}
		}
		return null;
	}

	private void Update()
	{
		if (!_isAdditionalAudioController)
		{
			_UpdateSystemTime();
		}
	}

	private static void _UpdateSystemTime()
	{
		double timeSinceLaunch = SystemTime.timeSinceLaunch;
		if (_lastSystemTime >= 0.0)
		{
			_systemDeltaTime = timeSinceLaunch - _lastSystemTime;
			if (_systemDeltaTime <= (double)(Time.maximumDeltaTime + 0.01f))
			{
				_systemTime += _systemDeltaTime;
			}
			else
			{
				_systemDeltaTime = 0.0;
			}
		}
		else
		{
			_systemDeltaTime = 0.0;
			_systemTime = 0.0;
		}
		_lastSystemTime = timeSinceLaunch;
	}

	protected override void Awake()
	{
		base.Awake();
		if (isAdditionalAudioController)
		{
			SingletonMonoBehaviour<AudioController>.Instance._RegisterAdditionalAudioController(this);
		}
		else
		{
			AwakeSingleton();
		}
	}

	protected override void OnDestroy()
	{
		if (UnloadAudioClipsOnDestroy)
		{
			UnloadAllAudioClips();
		}
		base.OnDestroy();
		if (isAdditionalAudioController && (bool)SingletonMonoBehaviour<AudioController>.DoesInstanceExist())
		{
			SingletonMonoBehaviour<AudioController>.Instance._UnregisterAdditionalAudioController(this);
		}
	}

	private void AwakeSingleton()
	{
		_UpdateSystemTime();
		if (Persistent)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		if (AudioObjectPrefab == null)
		{
			Debug.LogError("No AudioObject prefab specified in AudioController.");
		}
		else
		{
			_ValidateAudioObjectPrefab(AudioObjectPrefab);
		}
		_ValidateCategories();
		if (_playlistPlayed == null)
		{
			_playlistPlayed = new List<int>();
			_isPlaylistPlaying = false;
		}
	}

	protected void _ValidateCategories()
	{
		if (!_categoriesValidated)
		{
			InitializeAudioItems();
			_categoriesValidated = true;
		}
	}

	protected void _InvalidateCategories()
	{
		_categoriesValidated = false;
	}

	public void InitializeAudioItems()
	{
		if (isAdditionalAudioController)
		{
			return;
		}
		_audioItems = new Dictionary<string, AudioItem>();
		_InitializeAudioItems(this);
		if (_additionalAudioControllers == null)
		{
			return;
		}
		foreach (AudioController additionalAudioController in _additionalAudioControllers)
		{
			if (additionalAudioController != null)
			{
				_InitializeAudioItems(additionalAudioController);
			}
		}
	}

	private void _InitializeAudioItems(AudioController audioController)
	{
		AudioCategory[] audioCategories = audioController.AudioCategories;
		foreach (AudioCategory audioCategory in audioCategories)
		{
			audioCategory.audioController = audioController;
			audioCategory._AnalyseAudioItems(_audioItems);
			if ((bool)audioCategory.AudioObjectPrefab)
			{
				_ValidateAudioObjectPrefab(audioCategory.AudioObjectPrefab);
			}
		}
	}

	private void _RegisterAdditionalAudioController(AudioController ac)
	{
		if (_additionalAudioControllers == null)
		{
			_additionalAudioControllers = new List<AudioController>();
		}
		_additionalAudioControllers.Add(ac);
		_InvalidateCategories();
	}

	private void _UnregisterAdditionalAudioController(AudioController ac)
	{
		if (_additionalAudioControllers != null)
		{
			for (int i = 0; i < _additionalAudioControllers.Count; i++)
			{
				if (_additionalAudioControllers[i] == ac)
				{
					_additionalAudioControllers.RemoveAt(i);
					_InvalidateCategories();
					break;
				}
			}
		}
		else
		{
			Debug.LogWarning("_UnregisterAdditionalAudioController: AudioController " + ac.name + " not found");
		}
	}

	protected static AudioSubItem[] _ChooseSubItems(AudioItem audioItem, AudioObject useExistingAudioObj)
	{
		return _ChooseSubItems(audioItem, audioItem.SubItemPickMode, useExistingAudioObj);
	}

	internal static AudioSubItem _ChooseSingleSubItem(AudioItem audioItem, AudioPickSubItemMode pickMode, AudioObject useExistingAudioObj)
	{
		return _ChooseSubItems(audioItem, pickMode, useExistingAudioObj)[0];
	}

	protected static AudioSubItem[] _ChooseSubItems(AudioItem audioItem, AudioPickSubItemMode pickMode, AudioObject useExistingAudioObj)
	{
		if (audioItem.subItems == null)
		{
			return null;
		}
		int num = audioItem.subItems.Length;
		if (num == 0)
		{
			return null;
		}
		int num2 = 0;
		bool flag = (object)useExistingAudioObj != null;
		int num3 = ((!flag) ? audioItem._lastChosen : useExistingAudioObj._lastChosenSubItemIndex);
		if (num > 1)
		{
			switch (pickMode)
			{
			case AudioPickSubItemMode.Disabled:
				return null;
			case AudioPickSubItemMode.StartLoopSequenceWithFirst:
				num2 = (flag ? ((num3 + 1) % num) : 0);
				break;
			case AudioPickSubItemMode.Sequence:
				num2 = (num3 + 1) % num;
				break;
			case AudioPickSubItemMode.SequenceWithRandomStart:
				num2 = ((num3 != -1) ? ((num3 + 1) % num) : UnityEngine.Random.Range(0, num));
				break;
			case AudioPickSubItemMode.Random:
				num2 = _ChooseRandomSubitem(audioItem, allowSameElementTwiceInRow: true, num3);
				break;
			case AudioPickSubItemMode.RandomNotSameTwice:
				num2 = _ChooseRandomSubitem(audioItem, allowSameElementTwiceInRow: false, num3);
				break;
			case AudioPickSubItemMode.AllSimultaneously:
			{
				AudioSubItem[] array = new AudioSubItem[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = audioItem.subItems[i];
				}
				return array;
			}
			case AudioPickSubItemMode.TwoSimultaneously:
				return new AudioSubItem[2]
				{
					_ChooseSingleSubItem(audioItem, AudioPickSubItemMode.RandomNotSameTwice, useExistingAudioObj),
					_ChooseSingleSubItem(audioItem, AudioPickSubItemMode.RandomNotSameTwice, useExistingAudioObj)
				};
			}
		}
		if (flag)
		{
			useExistingAudioObj._lastChosenSubItemIndex = num2;
		}
		else
		{
			audioItem._lastChosen = num2;
		}
		return new AudioSubItem[1] { audioItem.subItems[num2] };
	}

	private static int _ChooseRandomSubitem(AudioItem audioItem, bool allowSameElementTwiceInRow, int lastChosen)
	{
		int num = audioItem.subItems.Length;
		int result = 0;
		float num2 = 0f;
		float max;
		if (!allowSameElementTwiceInRow)
		{
			if (lastChosen >= 0)
			{
				num2 = audioItem.subItems[lastChosen]._SummedProbability;
				if (lastChosen >= 1)
				{
					num2 -= audioItem.subItems[lastChosen - 1]._SummedProbability;
				}
			}
			else
			{
				num2 = 0f;
			}
			max = 1f - num2;
		}
		else
		{
			max = 1f;
		}
		float num3 = UnityEngine.Random.Range(0f, max);
		int i;
		for (i = 0; i < num - 1; i++)
		{
			float num4 = audioItem.subItems[i]._SummedProbability;
			if (!allowSameElementTwiceInRow)
			{
				if (i == lastChosen && (num4 != 1f || !audioItem.subItems[i].DisableOtherSubitems))
				{
					continue;
				}
				if (i > lastChosen)
				{
					num4 -= num2;
				}
			}
			if (num4 > num3)
			{
				result = i;
				break;
			}
		}
		if (i == num - 1)
		{
			result = num - 1;
		}
		return result;
	}

	public AudioObject PlayAudioSubItem(AudioSubItem subItem, float volume, Vector3 worldPosition, Transform parentObj, float delay, float startTime, bool playWithoutAudioObject, AudioObject useExistingAudioObj, double dspTime = 0.0, bool playAsMusic = false)
	{
		AudioItem item = subItem.item;
		AudioSubItemType subItemType = subItem.SubItemType;
		if (subItemType != AudioSubItemType.Clip && subItemType == AudioSubItemType.Item)
		{
			if (subItem.ItemModeAudioID.Length == 0)
			{
				Debug.LogWarning("No item specified in audio sub-item with ITEM mode (audio item: '" + item.Name + "')");
				return null;
			}
			return _PlayAsSound(subItem.ItemModeAudioID, volume, worldPosition, parentObj, delay, startTime, playWithoutAudioObject, dspTime, useExistingAudioObj);
		}
		if (subItem.Clip == null)
		{
			return null;
		}
		AudioCategory category = item.category;
		float num = subItem.Volume * item.Volume * volume;
		if (subItem.RandomVolume != 0f)
		{
			num += UnityEngine.Random.Range(0f - subItem.RandomVolume, subItem.RandomVolume);
			num = Mathf.Clamp01(num);
		}
		float num2 = num * category.VolumeTotal;
		AudioController audioController = _GetAudioController(subItem);
		if (!audioController.PlayWithZeroVolume && (num2 <= 0f || Volume <= 0f))
		{
			return null;
		}
		GameObject gameObject = ((category.AudioObjectPrefab != null) ? category.AudioObjectPrefab : ((!(audioController.AudioObjectPrefab != null)) ? AudioObjectPrefab : audioController.AudioObjectPrefab));
		if (playWithoutAudioObject)
		{
			gameObject.audio.PlayOneShot(subItem.Clip, AudioObject.TransformVolume(num2));
			return null;
		}
		GameObject gameObject2;
		AudioObject audioObject;
		if (useExistingAudioObj == null)
		{
			if (item.DestroyOnLoad)
			{
				gameObject2 = ((!audioController.UsePooledAudioObjects) ? ObjectPoolController.InstantiateWithoutPool(gameObject, worldPosition, Quaternion.identity) : ObjectPoolController.Instantiate(gameObject, worldPosition, Quaternion.identity));
			}
			else
			{
				gameObject2 = ObjectPoolController.InstantiateWithoutPool(gameObject, worldPosition, Quaternion.identity);
				UnityEngine.Object.DontDestroyOnLoad(gameObject2);
			}
			if ((bool)parentObj)
			{
				gameObject2.transform.parent = parentObj;
			}
			audioObject = gameObject2.gameObject.GetComponent<AudioObject>();
		}
		else
		{
			gameObject2 = useExistingAudioObj.gameObject;
			audioObject = useExistingAudioObj;
		}
		audioObject.subItem = subItem;
		if ((object)useExistingAudioObj == null)
		{
			audioObject._lastChosenSubItemIndex = item._lastChosen;
		}
		audioObject.primaryAudioSource.clip = subItem.Clip;
		gameObject2.name = "AudioObject:" + audioObject.primaryAudioSource.clip.name;
		if ((bool)parentObj)
		{
			gameObject2.transform.parent = parentObj;
		}
		else
		{
			gameObject2.transform.parent = GameObject.Find("AUDIOCONTAINER").transform;
		}
		audioObject.primaryAudioSource.pitch = AudioObject.TransformPitch(subItem.PitchShift);
		audioObject.primaryAudioSource.pan = subItem.Pan2D;
		if (subItem.RandomStartPosition)
		{
			startTime = UnityEngine.Random.Range(0f, audioObject.clipLength);
		}
		audioObject.primaryAudioSource.time = startTime + subItem.ClipStartTime;
		audioObject.primaryAudioSource.loop = item.Loop == AudioItem.LoopMode.LoopSubitem || item.Loop == (AudioItem.LoopMode)3;
		audioObject._volumeExcludingCategory = num;
		audioObject._volumeFromScriptCall = volume;
		audioObject.category = category;
		audioObject.isPlayedAsMusic = playAsMusic;
		audioObject._ApplyVolumePrimary();
		if (subItem.RandomPitch != 0f)
		{
			audioObject.primaryAudioSource.pitch *= AudioObject.TransformPitch(UnityEngine.Random.Range(0f - subItem.RandomPitch, subItem.RandomPitch));
		}
		if (subItem.RandomDelay > 0f)
		{
			delay += UnityEngine.Random.Range(0f, subItem.RandomDelay);
		}
		if (dspTime > 0.0)
		{
			audioObject.PlayScheduled(dspTime + (double)delay + (double)subItem.Delay + (double)item.Delay);
		}
		else
		{
			audioObject.Play(delay + subItem.Delay + item.Delay);
		}
		if (subItem.FadeIn > 0f)
		{
			audioObject.FadeIn(subItem.FadeIn);
		}
		return audioObject;
	}

	private AudioController _GetAudioController(AudioSubItem subItem)
	{
		if (subItem.item != null && subItem.item.category != null)
		{
			return subItem.item.category.audioController;
		}
		return this;
	}

	internal void _NotifyPlaylistTrackCompleteleyPlayed(AudioObject audioObject)
	{
		audioObject._isCurrentPlaylistTrack = false;
		if (IsPlaylistPlaying() && _currentMusic == audioObject && _PlayNextMusicOnPlaylist(delayBetweenPlaylistTracks) == null)
		{
			_isPlaylistPlaying = false;
		}
	}

	private void _ValidateAudioObjectPrefab(GameObject audioPrefab)
	{
		if (UsePooledAudioObjects)
		{
			if (audioPrefab.GetComponent<PoolableObject>() == null)
			{
				Debug.LogWarning("AudioObject prefab does not have the PoolableObject component. Pooling will not work.");
			}
			else
			{
				ObjectPoolController.Preload(audioPrefab);
			}
		}
		if (audioPrefab.GetComponent<AudioObject>() == null)
		{
			Debug.LogError("AudioObject prefab must have the AudioObject script component!");
		}
	}
}
