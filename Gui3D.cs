using UnityEngine;

public class Gui3D : Singleton<Gui3D>
{
	private bool bgPanelVisible;

	private Animation panelBotAnim;

	private Animation panelTopAnim;

	private float animationSpeed = 0.3f;

	public GameObject camera3DParent;

	private void Start()
	{
		panelBotAnim["toggleAnimation"].speed = animationSpeed;
		panelTopAnim["toggleAnimation"].speed = animationSpeed;
		FadeBothPanels(state: false);
	}

	private void Update()
	{
	}

	public void BGPanelToggled(bool state)
	{
	}

	public void FadeBothPanels(bool state)
	{
		FadeBottomPanel(state);
		FadeTopPanel(state);
	}

	public void FadeBottomPanel(bool open)
	{
		PlayAnimation(open, panelBotAnim, "toggleAnimation");
	}

	public void FadeTopPanel(bool open)
	{
		PlayAnimation(open, panelTopAnim, "toggleAnimation");
	}

	private void PlayAnimation(bool forwards, Animation anim, string clip)
	{
		if (forwards)
		{
			anim[clip].speed = Mathf.Abs(anim[clip].speed);
			if (!anim.IsPlaying(clip))
			{
				anim.Play(clip);
			}
			return;
		}
		anim[clip].speed = 0f - Mathf.Abs(anim[clip].speed);
		if (!anim.IsPlaying(clip))
		{
			anim[clip].time = anim[clip].length;
			anim.Play(clip);
		}
	}
}
