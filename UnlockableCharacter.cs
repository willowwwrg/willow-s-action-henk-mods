using System;

[Serializable]
public class UnlockableCharacter
{
	public string name;

	public CharacterSelect.Characters character;

	public int skinNum;

	public string unlockCriteria;

	public bool hiddenUnlessUnlocked;
}
