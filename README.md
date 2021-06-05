# ModTMNF

This was a hastily put together project exploring modifying `Trackmania Nations Forever` (TMNF) at runtime using C#. TMNF is an enjoyable modding / reverse engineering experience due to the available debug symbols (`TmForever.map`).

There probably wont be any more development on this, but hopefully it will be a useful resource to those getting into modding the game.

See [technical information](/ModTMNF/Analysis/Docs/), [other projects](/ModTMNF/Analysis/Docs/OtherProjects.md), and [official download links](/ModTMNF/Analysis/Docs/DownloadLinks.md).

## Features

Load .NET into TmForever, hook functions, access game state.

[ModTest.cs](/ModTMNF/Mods/ModTest.cs) simulates the input of the active replay file (respawn events aren't handled).

## Compiling

- Build `ModTMNF.sln` with Visual Studio or by running `Build.bat`.
- Build `ModTMNF_ManagedLoader.cpp` (see the [readme](/ModTMNF_ManagedLoader/README.md)).
- Copy the `ModTMNF` folder from the `Build` folder into your `TmNationsForever` folder.
- Run `ModTMNF.exe` to launch the game.