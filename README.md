# ModTMNF

Currently this project doesn't have any real functionality but might be a useful resource for creating runtime mods for Trackmania Nations Forever.

See [technical information](/ModTMNF/Analysis/Docs/), [other projects](/ModTMNF/Analysis/Docs/OtherProjects.md), and [official download links](/ModTMNF/Analysis/Docs/DownloadLinks.md).

## Project status

Load .NET into TmForever, hook functions, read/write memory, access the reflection system.

A proper API needs to be devloped, and many things need to be wrapped / exposed.

See [ModTest.cs](/ModTMNF/Mods/ModTest.cs) (2x physics step / reflection example)

## Compiling

- Build `ModTMNF.sln` with Visual Studio or by running `Build.bat`.
- Build `ModTMNF_ManagedLoader.cpp` (see the [readme](/ModTMNF_ManagedLoader/README.md)).
- Copy the `ModTMNF` folder from the `Build` folder into your `TmNationsForever` folder.
- Run `ModTMNF.exe` to launch the game.
