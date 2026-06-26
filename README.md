# willow's Action Henk Mods

A collection of mods for Action Henk, applied to the decompiled game source and recompiled into `Assembly-CSharp.dll`.

## What's included

- **Challenge & Bonus Ghost Select** — adds a ghost opponent picker to challenge and bonus levels, matching the options available on normal levels
- **Bonus Level Twitch Ghosts** — allows IRC/Twitch ghosts to spawn on bonus levels
- **Environment Derendering** — extends the existing dev console rendering groups system to Island and City levels, and adds a Snow toggle on Henkmas levels
- **Replay Skin Remap** — remaps certain skins before they are written to replays, affecting both local saves and Steam leaderboard submissions
- **MOTD Credit** — updates the message of the day to credit the mod

---

## Building

### Requirements

- [.NET SDK](https://dotnet.microsoft.com/en-us/download) with .NET 3.5 targeting support
- A copy of **Action Henk** on Steam
- The decompiled game source (this repo) placed somewhere on your machine

### Steps

1. Clone this repo into a folder — this is your working source directory

2. Open the folder and apply the two required compile fixes if they aren't already in the repo:
   - `EventDelegate.cs` — the field named `field` must be renamed to `fieldName`
   - `MegaShapeSXL.cs` — `int num;` must be changed to `int num = 0;`
   
   These are decompiler artifacts in the vanilla source. They are already fixed in this repo.

3. Build the DLL:
   ```
   dotnet build Assembly-CSharp.csproj -c Release
   ```

4. The output will be at:
   ```
   bin\Release\net35\Assembly-CSharp.dll
   ```

5. Copy the DLL into your Action Henk install:
   ```
   <Steam>\steamapps\common\Action Henk\ActionHenk_Data\Managed\Assembly-CSharp.dll
   ```
   
   Back up the original first.

6. Launch the game normally via Steam.

---

## Notes

- The build will produce ~242 warnings — these are all pre-existing decompiler artifacts and can be ignored
- The dev console (rendering groups, FPS counter etc.) is a vanilla feature gated behind a developer Steam ID check — the derender groups added by this mod will only be accessible if you have dev console access
- Tested on Action Henk version available on Steam as of 2025
