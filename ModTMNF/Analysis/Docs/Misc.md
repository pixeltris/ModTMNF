﻿## Compilation units (.obj)

Each listed `.obj` file name in `TmForever.map` originated from a `.cpp` file (e.g. `GameControlGrid.obj` was `GameControlGrid.cpp`).

Multiple source files will often be compiled into a single obj, but this information is still useful for tracking some types.

You can find the objs listed in a few of the files in the `Generated` folder (represented in different ways).

Also see [SourceFileNames.txt](SourceFileNames.txt).

## Interesting call chains

```
CGbxApp::Init
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
   CGbxApp::StartApp - virtual function (only implemented by CGbxGame). Last call made in "CGbxApp::Start".
    CMwCmdBufferCore::StartSimulation
	CMwCmdBufferCore::Enable
  while(1) - (the game loop)
   CGbxApp::MainLoop - (this is the only function call in the game loop other than Peek/Translate/Dispatch message)
    CMwCmdBufferCore::Run - (invoker for all functions which subscribe to be called by the game loop)
```

```
=======================
Called when after selecting the profile to use
=======================
...
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
This function is one to look at to get a context for rendering
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

```
CTrackMania::UpdateGame
 CTrackMania::ReplayValidatePath
  CTrackMania::ReplayValidate
   CTrackManiaRace::Validate
```

```
CTrackManiaRaceNet::RaceInputsValidateBest
 CTrackManiaRace::Validate_NonBlocking
```

```
=======================
Entering finish (NOTE: This is called many times when entering finish line)
=======================
CGbxApp::MainLoop
 CMwCmdBufferCore::Run
  CMwCmdBuffer::Run
   CHmsZoneDynamic::PhysicsStep2
    CHmsZoneDynamic::ComputeCollisionResponse
     CTrackManiaRaceTriggerAbsorbContact::AbsorbContact - virtual call
      CTrackManiaRace1P::OnFinishLine - virtual call
```

```
=======================
Pressing respawn at cp key
=======================
CGbxApp::MainLoop
 CMwCmdBufferCore::Run
  CMwCmdBuffer::Run
   CTrackManiaRace::InputRace
    CTrackManiaRace::OnInputEvent
	 CTrackManiaPlayerInfo::AddInputEvent
```

```
=======================
Starting a new race
=======================
...
 CTrackMania::ChallengeMainLoop
  CTrackMania::ChallengeCreateSceneGraph
   CTrackManiaSwitcher::Init
    CTrackManiaSwitcher::CreateNewRace
	 CTrackManiaSwitcher::ChangeRace
	  CTrackMania::SetRace
```

## Other info

```
Info for changing the game name...
Nadeo.init - "WindowTitle" will set the window title (fetched in CSystemEngine::InitForGbxGame)
CSystemEngine::s_GameWindowTitle - has a few hard coded locations where it's set to "TmForever"
CGbxGame::CGbxGame
 CSystemEngine::s_GameName = "TmForever" (00D54310)

See CGbxApp::WindowedSetWindowTitle / CGameApp::CallbackSetWindowTitle_Set
```

```
Info on input...
CTrackManiaRace::InputRace - handles input
CTrackManiaRace::OnInputEvent - has no impact on physics (vtable+284)
CTrackManiaControlPlayerInput::UpdateVehicleStateFromInputs - applies forces to the cars physics

------

CInputPort::GetEventsNotTimed / CInputPort::GetEvents actually fetches the input events

------

These are important to register the input events with the player, which are then handled in UpdateVehicleStateFromInputs

CTrackManiaPlayerInfo::AddInputEvent
CTrackManiaPlayerInfo::SetInputState
CTrackManiaPlayerInfo::GetInputState
CTrackManiaPlayerInfo::LockInputs

------

These are actions which are used with the input handler code

00CD3F58 public: static struct SInputActionDesc const * const CTrackManiaRace::ActionSwitchToEditorMode
...
00CD4010 public: static struct SInputActionDesc const * const CTrackManiaRace::ActionFakeIsRaceRunning
```