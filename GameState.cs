using UnityEngine;

public abstract class GameState : MonoBehaviour
{
	public bool MyInputEnabled;

	public bool useCustomHorizontalInput;

	public bool MessageOnSelect;

	public bool firstFrameActive;

	public abstract void OnActivate();

	public abstract void OnDeactivate();

	public abstract void OnUpdate();
}
