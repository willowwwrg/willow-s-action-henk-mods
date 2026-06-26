using UnityEngine;
using XInputDotNetPure;

public class QuoteSpawner : MonoBehaviour
{
	public int controller;

	private GamePadState[] lastState = new GamePadState[4];

	public GameObject[] spawnpoints;

	private CharacterSelect.Characters currentChar = CharacterSelect.Characters.Henk;

	private void Start()
	{
	}

	private void Update()
	{
		if (spawnpoints.Length == 0)
		{
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			GamePadState state = GamePad.GetState((PlayerIndex)i);
			if (state.DPad.Left == ButtonState.Pressed && lastState[i].DPad.Left == ButtonState.Released)
			{
				SpawnCharacter("taunt");
			}
			if (state.DPad.Right == ButtonState.Pressed && lastState[i].DPad.Right == ButtonState.Released)
			{
				SpawnCharacter("woo");
			}
			if (state.DPad.Up == ButtonState.Pressed && lastState[i].DPad.Up == ButtonState.Released)
			{
				SpawnCharacter("defeat");
			}
			if (state.DPad.Down == ButtonState.Pressed && lastState[i].DPad.Down == ButtonState.Released)
			{
				SpawnCharacter("victory");
			}
			if (state.Buttons.LeftStick == ButtonState.Pressed && lastState[i].Buttons.LeftStick == ButtonState.Released)
			{
				SpawnCharacter("retry");
			}
			if (state.Buttons.RightStick == ButtonState.Pressed && lastState[i].Buttons.RightStick == ButtonState.Released)
			{
				SpawnCharacter("intro");
			}
			if (state.Buttons.Back == ButtonState.Pressed && lastState[i].Buttons.Back == ButtonState.Released)
			{
				currentChar = (CharacterSelect.Characters)((int)currentChar % 5 + 1);
			}
			lastState[i] = state;
		}
	}

	private void SpawnCharacter(string voiceType)
	{
		GameObject spawnpoint = spawnpoints[Random.Range(0, spawnpoints.Length)];
		SpawnCharacter(spawnpoint, currentChar, 0, voiceType);
	}

	private void SpawnCharacter(GameObject spawnpoint, CharacterSelect.Characters character, int skinNum, string voiceType)
	{
		GameObject gameObject = Object.Instantiate(Resources.Load(string.Concat("CharacterModels/", character, "_", skinNum)) as GameObject) as GameObject;
		gameObject.transform.parent = spawnpoint.transform;
		gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
		gameObject.transform.localRotation = Quaternion.Euler(0f, gameObject.GetComponent<CharacterModel>().rotationOffset, 0f);
		float num = Random.Range(0.8f, 1f);
		gameObject.transform.localScale = Vector3.one * gameObject.transform.localScale.x * num;
		float z = Random.Range(0, -10);
		gameObject.transform.localPosition = new Vector3(0f, 0f, z);
		AudioObject audioObject = AudioController.Play(character.ToString() + "_" + voiceType, 1f);
		if (!audioObject)
		{
			Object.Destroy(gameObject);
			return;
		}
		float clipLength = audioObject.clipLength;
		gameObject.AddComponent<QuoteCharacter>();
		gameObject.GetComponent<QuoteCharacter>().quoteDuration = clipLength;
	}
}
