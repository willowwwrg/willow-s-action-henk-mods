// ReplaySkinRemap.cs
// Remaps certain skins to alternatives before they are written into replay strings.
// This affects both local PB saves and Steam leaderboard submissions.
//
// Current remaps:
//   Afronaut skin 8  -> skin 5
//   Afronaut skin 10 -> skin 3

public static class ReplaySkinRemap
{
	public static int RemapSkin(CharacterSelect.Characters character, int skinNum)
	{
		if (character == CharacterSelect.Characters.Afronaut)
		{
			if (skinNum == 8)  return 5;
			if (skinNum == 10) return 3;
		}
		return skinNum;
	}
}
