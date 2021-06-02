This gives a brief overview of the design patterns used in `Trackmania Nations Forever` / `TmForever.exe`.

## Object system

Most classes in the game belong to the object system. It's possible instantiate objects and modify members at runtime without having to reverse engineer code.

- `CMwNod` is the base object type which most classes in the game inherit from.
- `CMwClassInfo` holds [reflection](https://en.wikipedia.org/wiki/Reflective_programming) info about `CMwNod` classes.
- `CMwEngineManager::First` can be used to traverse the class list (it's global a static `CMwClassInfo*` linked list). Alternatiely itterate `CMwEngineInfo` array in `CMwEngineManager` global instance (see "Engines" below).

As part of the reflection system class members can have a memory offset of -1. These act similar to C# properties where there are C++ implementations to handle getting/setting those members.

## Naming conventions

Prefixes `C` (class), `S` (struct), `E` (enum) are used on most types defined in the code. Namespaces (often shortned) are also prefixed on most types.

`Gm` prefix is used for geometry related types and exist in their own namespace `geom`.

`Gx` prefix is used for graphics related types and exist in their own namespace `graphic`. `Gx` classes often inherit from `CMwNod` so you shouldn't rely on the `C` prefix.

Examples:
- `CFuncVisualShiver` is a class with a shortened namespace of `Func` (full is `function`).
- `CTrackManiaMenus` is a class with a namespace of `trackmania`.
- `CGameCtnCatalog` is a class with a shortned namespace of `Game` (full is `gameengine`).
- `SMwIdInternal` is a struct with a shortnered namespace of `Mw` (full is `mwfoundations`).
- `EMwIconList` is an enum with a shortnered namespace of `Mw` (full is `mwfoundations`).
- `GmVec3`, `GmFrustum` are `geom` types.
- `GxLightAmbient` is a `graphic` type which also belongs to the `CMwNod` object system.
- `EGxLightKind` uses the enum prefix, as well as the `Gx` prefix.

The namespaces should match up to `EMwEngineId`, though these names have the "engine" suffix trimmed. The code in this project uses the `EMwEngineId` names rather than the slightly longer C++ namespace ones.

## Engines

Engines (namespaces) seperate out the logic of different groups of code. `MwFoundations` is the core engine where basic building blocks of the code reside. `Classic` is even simpler being mostly primitive C++ structures / classes such as `CString`.

Here are some file names (taken from ManiaPlanet logs build) which gives an indication of the folder structure used:

- `c:\\codebase\\nadeo\\engines\\classic\\source\\faststring.cpp`
- `c:\\codebase\\nadeo\\engines\\classic\\include\\constarray.h`
- `c:\\codebase\\nadeo\\engines\\mwfoundations\\include\\mwnodref.h`
- `c:\\codebase\\nadeo\\engines\\geom\\source\\gmgrid3.cpp`
- `c:\\codebase\\nadeo\\engines\\netengine\\source\\netipc.cpp`
- `c:\\codebase\\nadeo\\games\\gameengine\\source\\gamemenu.cpp`
- `c:\\codebase\\nadeo\\games\\trackmania\\source\\trackmaniarace.cpp`

Note that `TrackMania` / `Game` are both engines.

Engines are managed by `CMwEngineManager` and each engine is represented by a `CMwEngineInfo` which holds a list of `CMwClassInfo` (classes) which belong to the given engine. `CMwEngineInfo` don't have any functionality other than holding the class list.