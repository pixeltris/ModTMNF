Information about CMwTimer / CMwTimerAdapter

CMwTimerAdapter keeps time for the race. Where as CMwTimer keeps time of the entire game since launch.

CMwTimerAdapter uses the time values from CMwTimer to increment its time values.

```
CMwTimer::GetElapsedTimeSinceInit - 130 refs
CMwTimer::GetTickTime - 351 refs
```

```
CGbxApp::Init
 CMwCmdBufferCore::CreateCoreCmdBuffer
  CMwCmdBufferCore::CMwCmdBufferCore
   CMwTimer::InitTimer
    CMwTimer_CalibrateStart
   CMwTimerAdapter::InitTimer
```

```
CGbxApp::MainLoop
 CMwCmdBufferCore::Run
  CMwTimer::SimulateDeltaTime
  CMwTimer::Tick
   CMwTimer_CalibrateEnd_ShouldSwitchOff
   CMwTimer_CalibrateStart
  CMwTimer::ChopTime
  CMwTimerAdapter::Resync
  CMwTimerAdapter::ComputeTimeAtHumanTick
  ... (invoke "::Run" functions)
  CMwTimerAdapter::SetCurrentTimeAtHumanTick

NOTE: Either CMwTimer::SimulateDeltaTime or CMwTimer::Tick is called, not both. Tick seems to be used in regular play.
```

```
CGbxGame::StartApp
 CMwCmdBufferCore::StartSimulation
  CMwTimerAdapter::SetCurrentTimeAtHumanTick
   CMwTimerAdapter::Resync

CMwCmdBufferCore::SetSimulationCurrentTime
 CMwTimerAdapter::SetCurrentTimeAtHumanTick
  CMwTimerAdapter::ComputeTimeAtHumanTick
  CMwTimerAdapter::Resync

More calls to CMwCmdBufferCore::StartSimulation:
- CTrackManiaRace::SwitchToRace
- CTrackManiaRace::Validate
- CTrackMania::ResetRaceLocal <---- pressing delete?
- CTrackMania::OnMenuHiden
- CTrackMania::ChallengeMainLoop
- CTrackMania::UpdateGame
- CTrackManiaNetwork::MainLoop_RoundPlay
- CGameApp::ResetGame
```

```
CMwTimerAdapter::ComputeTimeAtHumanTick
 CMwTimerAdapter::ConvertHumanToGame

CMwTimerAdapter::GetTime
 CMwTimerAdapter::ConvertHumanToGame
```