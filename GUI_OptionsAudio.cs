using UnityEngine;

public class GUI_OptionsAudio : GUI_Base
{
	public InputObject FirstButton;

	public InputObject currentButton;

	public int musicVolume;

	public UISprite musicFull;

	public UISprite musicEmpty;

	public UISprite musicVolumeLevel;

	public int sfxVolume;

	public UISprite sfxFull;

	public UISprite sfxEmpty;

	public UISprite sfxVolumeLevel;

	private void Awake()
	{
		sfxVolume = PlayerPrefs.GetInt("sfxVolume_i", 10);
		SetSliderImages(sfxFull, sfxEmpty, sfxVolume);
		SetMuteImages(sfxVolumeLevel, sfxVolume);
		musicVolume = PlayerPrefs.GetInt("musicVolume_i", 10);
		SetSliderImages(musicFull, musicEmpty, musicVolume);
		SetMuteImages(musicVolumeLevel, musicVolume);
	}

	private void AdjustSFXSlider(bool increment)
	{
		if (increment)
		{
			if (sfxVolume + 1 > 10)
			{
				sfxVolume = 10;
			}
			else
			{
				sfxVolume++;
				AudioController.Play("NextMenuItem");
			}
		}
		else if (sfxVolume - 1 < 0)
		{
			sfxVolume = 0;
		}
		else
		{
			sfxVolume--;
			AudioController.Play("NextMenuItem");
		}
		PlayerPrefs.SetInt("sfxVolume_i", sfxVolume);
		SetSliderImages(sfxFull, sfxEmpty, sfxVolume);
		SetMuteImages(sfxVolumeLevel, sfxVolume);
		float num = (float)sfxVolume / 10f;
		AudioController.SetCategoryVolume("MenuSounds", num * Singleton<AudioManager>.SP.GetPrefabVolumeForCategory("MenuSounds"));
		AudioController.SetCategoryVolume("InGameSFX", num * Singleton<AudioManager>.SP.GetPrefabVolumeForCategory("InGameSFX"));
		AudioController.SetCategoryVolume("EnvironmentSFX", num * Singleton<AudioManager>.SP.GetPrefabVolumeForCategory("EnvironmentSFX"));
		AudioController.SetCategoryVolume("CharacterSFX", num * Singleton<AudioManager>.SP.GetPrefabVolumeForCategory("CharacterSFX"));
		AudioController.SetCategoryVolume("SlideSounds", num * Singleton<AudioManager>.SP.GetPrefabVolumeForCategory("SlideSounds"));
	}

	private void AdjustVolumeSlider(bool increment)
	{
		if (increment)
		{
			if (musicVolume + 1 > 10)
			{
				musicVolume = 10;
			}
			else
			{
				musicVolume++;
				AudioController.Play("NextMenuItem");
			}
		}
		else
		{
			if (musicVolume - 1 < 0)
			{
				musicVolume = 0;
				return;
			}
			musicVolume--;
			AudioController.Play("NextMenuItem");
		}
		PlayerPrefs.SetInt("musicVolume_i", musicVolume);
		SetSliderImages(musicFull, musicEmpty, musicVolume);
		SetMuteImages(musicVolumeLevel, musicVolume);
		AudioController.SetCategoryVolume("Music", (float)musicVolume / 10f * Singleton<AudioManager>.SP.GetPrefabVolumeForCategory("Music"));
	}

	private void SetSliderImages(UISprite spriteFull, UISprite spriteEmpty, int value)
	{
		spriteFull.fillAmount = (float)value / 10f;
		spriteEmpty.fillAmount = 1f - (float)value / 10f;
	}

	private void SetMuteImages(UISprite sprite, int value)
	{
		string text = "volume_icon_";
		text = ((value == 0) ? (text + "0") : ((value < 5) ? (text + "1") : ((value >= 8) ? (text + "3") : (text + "2"))));
		sprite.spriteName = text;
	}

	private void TransitionCompleted()
	{
		InitializeScreen();
		Singleton<InputManager>.SP.Select(FirstButton, delayedTillEndOfFrame: false, playSound: false);
		sfxVolume = PlayerPrefs.GetInt("sfxVolume_i");
		SetSliderImages(sfxFull, sfxEmpty, sfxVolume);
		SetMuteImages(sfxVolumeLevel, sfxVolume);
		musicVolume = PlayerPrefs.GetInt("musicVolume_i");
		SetSliderImages(musicFull, musicEmpty, musicVolume);
		SetMuteImages(musicVolumeLevel, musicVolume);
	}

	private void NextWindow()
	{
	}

	private void PrevWindow()
	{
		FirstButton = currentButton;
		Singleton<GamestateManager>.SP.SetState(typeof(State_Options));
		AudioController.Play("ButtonBackwards");
		Singleton<PermaGUI>.SP.ToggleArrows(state: false);
	}

	private void Update()
	{
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Left))
		{
			if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "slider_Sf")
			{
				AdjustSFXSlider(increment: false);
			}
			else if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "slider_Music")
			{
				AdjustVolumeSlider(increment: false);
			}
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Right))
		{
			if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "slider_Sf")
			{
				AdjustSFXSlider(increment: true);
			}
			else if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "slider_Music")
			{
				AdjustVolumeSlider(increment: true);
			}
		}
		currentButton = Singleton<InputManager>.SP.GetCurrentButton();
	}
}
