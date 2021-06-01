﻿## Compilation units (.obj)

Each listed `.obj` file name in `TmForever.map` originated from a `.cpp` file (e.g. `GameControlGrid.obj` was `GameControlGrid.cpp`).

Multiple source files will often be compiled into a single obj, but this information is still useful for tracking some types.

You can find the objs listed in a few of the files in the `Generated` folder (represented in different ways).

Also see [SourceFileNames.txt](SourceFileNames.txt).

## Interesting call chains

```
CGbxApp::Init
 CGbxGame::Init
  CMwNod::StaticInit
   CMwNod::MwBuildClassInfoTree - (traverses all classes and adds fills in the child classes array for each class)
```

```
WinMain
 WinMainInternal
  CGbxApp::CreateStaticInstance - (creates CGbxApp::ThisApp global instance)
  CreateWindowExW
  CGbxApp::InitSystem
  CGbxApp::Start
  while(1) - (the game loop)
   CGbxApp::MainLoop - (this is the only function call in the game loop other than Peek/Translate/Dispatch message)
    CMwCmdBufferCore::Run - (invoker for all functions which subscribe to be called by the game loop)
```

```
======================= 
Called when after selecting the profile to use
=======================
Not quite sure where the invoker of this starts
 CTrackManiaMenus::DoMenus
  CGameCtnMenus::MenuMain
   CGameCtnMenus::MenuMain_Init
```

```
CTrackMania::UpdateGame (00497830)
- 
EGameState CTrackMania::GetCurrentState (00484B30) - TmDbg_GameStateToText
```

```
This function is one to look at for seeing access context for rendering
CVisionViewportDx9::Create (00960D50)
```

```
CTrackManiaRace::SwitchFromRace
 CTrackManiaRace::Ghosts_Stop
 CTrackManiaRace::StopReplayRecordAndKeepCopy
 CGameRace::SwitchFromRace
 CMwCmdBufferCore::StopSimulation
 CMwCmdBufferCore::SetSimulationRelativeSpeed
 CTrackManiaRace::ValidateCleanup
 CTrackManiaRace::AssignCamFreePrimaryActionKeys
```