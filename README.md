# ModTMNF

Currently this project doesn't have any real functionality but might be a useful resource for creating runtime mods for Trackmania Nations Forever.

See [technical information](/Analysis/Docs/README.md), [other projects](/Analysis/Docs/OtherProjects.md), and [official download links](/Analysis/Docs/DownloadLinks.md).

## Functionality (WIP)

- `ModTMNF.exe` launches `TmForever.exe` and injects `ModTMNF_ManagedLoader.dll` which loads .NET and `ModTMNF.exe` into the process.
- Access to the reflection system `CMwClassInfo`.
- Ability to hook functions via `Hook` (see [TestMod.cs](/ModTMNF/Mods/TestMod.cs)).

## Compiling

- Build `ModTMNF.sln` with Visual Studio or by running `Build.bat`.
- Build `ModTMNF_ManagedLoader.cpp` (see the [README](ModTMNF_ManagedLoader/README.md).
- Copy the `ModTMNF` folder from the `Build` folder into your `TmNationsForever` folder.
- Run `ModTMNF.exe` to launch the game.