using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ModTMNF.Game;

namespace ModTMNF.Mods.UI
{
    public partial class StatsForm : Form
    {
        ModTest mod;
        public bool PausePhysics;
        public bool PauseCoreRun;
        public int PhysicsStep;
        public int NumTick;
        public int NumSimulateDeltaTime;
        public string ReplayFilePath;
        public bool DriveReplayFile;
        public bool ShouldStepCore;
        public int StartAtTime;
        public bool StartAtTimeRegularInput;
        StringBuilder sb = new StringBuilder();
        StringBuilder sb2 = new StringBuilder();

        int startSystemTime = Environment.TickCount;

        public StatsForm()
        {
            InitializeComponent();
            mod = ModManager.GetMod<ModTest>();
            PhysicsStep = (int)physicsStepNumericUpDown.Value;
            DriveReplayFile = true;
        }

        private void pausePhysicsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            PausePhysics = pausePhysicsCheckBox.Checked;
        }

        private void pauseCoreRunCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            PauseCoreRun = pauseCoreRunCheckBox.Checked;
        }

        public void UpdateTimerInfo()
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { UpdateTimerInfo(); });
                return;
            }
            CMwCmdBufferCore core = CMwCmdBufferCore.TheCoreCmdBuffer;
            if (core.Address == IntPtr.Zero)
            {
                return;
            }
            CMwTimerAdapter timerAdapter = core.TimerAdapter;
            CMwTimer timer = core.Timer;
            sb.Clear();
            sb.AppendLine("Timer info");
            sb.AppendLine("NumTick:" + NumTick);
            sb.AppendLine("NumSimulateDeltaTime:" + NumSimulateDeltaTime);
            sb.AppendLine();
            try
            {
                sb.AppendLine("TimerAdapter---");
                sb.AppendLine("Timer: " + timerAdapter.Timer);
                sb.AppendLine("SystemTick: " + timerAdapter.ReferenceSystemTime);
                sb.AppendLine("RelativeSpeed: " + timerAdapter.RelativeSpeed);
                sb.AppendLine("StartingTime: " + timerAdapter.ReferenceGameTime);
                sb.AppendLine("ReferenceRaceTime: " + timerAdapter.ReferenceRaceTime);
                sb.AppendLine("TimeAtHumanTick: " + timerAdapter.TimeAtHumanTick);
                sb.AppendLine("DeltaTime: " + timerAdapter.DeltaTime);
                sb.AppendLine("TickTime: " + timerAdapter.TickTime);
                sb.AppendLine();

                sb.AppendLine("Timer---");
                sb.AppendLine("off0: " + timer.Unk_0);
                sb.AppendLine("off4: " + timer.Unk_4);
                sb.AppendLine("TickTime: " + timer.TickTime);
                sb.AppendLine("FrameTime: " + timer.FrameTime);
                sb.AppendLine("FrameTimeInSeconds: " + timer.FrameTimeInSeconds);
                sb.AppendLine("FrameRate: " + timer.FrameRate);
                sb.AppendLine("InitialPerformanceCounter: " + timer.InitialPerformanceCounter);
                sb.AppendLine("InitialSystemTime: " + timer.InitialSystemTime);
                sb.AppendLine("off36: " + timer.Unk_36);
                sb.AppendLine("off40: " + timer.Unk_40);
                sb.AppendLine("off44: " + timer.Unk_44);
                sb.AppendLine();

                sb.AppendLine("Misc---");
                sb.AppendLine("startSystemTime:" + startSystemTime);
                sb.AppendLine("systemTime:" + Environment.TickCount);
                sb.AppendLine();
            }
            catch
            {
                Program.DebugBreak();
            }

            sb2.Clear();
            sb2.AppendLine("Input info");
            CTrackManiaRace race = CTrackMania.TheGame.Race;
            if (race.Address != IntPtr.Zero)
            {
                CTrackManiaPlayerInfo playerInfo = race.GetPlayingPlayerInfo();
                if (playerInfo.Address != IntPtr.Zero)
                {
                    // Potential threading issues?
                    uint time = timerAdapter.TickTime;
                    sb2.AppendLine("Accelerate: " + playerInfo.GetInputState(SInputActionDesc.ActionVehicleAccelerate_1, time));
                    sb2.AppendLine("Brake: " + playerInfo.GetInputState(SInputActionDesc.ActionVehicleBrake_1, time));
                    sb2.AppendLine("Gas: " + playerInfo.GetInputState(SInputActionDesc.ActionVehicleGas_1, time));
                    sb2.AppendLine("SteerLeft: " + playerInfo.GetInputState(SInputActionDesc.ActionVehicleSteerLeft_1, time));
                    sb2.AppendLine("SteerRight: " + playerInfo.GetInputState(SInputActionDesc.ActionVehicleSteerRight_1, time));
                    sb2.AppendLine("Steer: " + playerInfo.GetInputStateF(SInputActionDesc.ActionVehicleSteer_1, time));
                    sb2.AppendLine("Horn: " + playerInfo.GetInputState(SInputActionDesc.ActionVehicleHorn_1, time));
                }
            }

            timerInfoLabel.Text = sb.ToString();
            inputInfoLabl.Text = sb2.ToString();
        }

        private void stepCoreButton_Click(object sender, EventArgs e)
        {
            if (PauseCoreRun)
            {
                ShouldStepCore = true;
            }
        }

        private void applyRelativeSpeedBtn_Click(object sender, EventArgs e)
        {
            CMwCmdBufferCore core = CMwCmdBufferCore.TheCoreCmdBuffer;
            if (core.Address == IntPtr.Zero)
            {
                return;
            }
            // Potential threading issues?
            core.TimerAdapter.SetRelativeSpeed((float)relativeSpeedNumericUpDown.Value);
        }

        private void physicsStepApply_Click(object sender, EventArgs e)
        {
            PhysicsStep = (int)physicsStepNumericUpDown.Value;
        }

        private void fetchReplayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (autoFetchReplayCheckBox.Checked)
            {
                UpdateFetchReplayFile();
            }
        }

        private void driveReplayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            DriveReplayFile = driveReplayCheckBox.Checked;
        }

        public void UpdateFetchReplayFile()
        {
            if (!autoFetchReplayCheckBox.Checked)
            {
                return;
            }
            CTrackMania trackmania = CTrackMania.TheGame;
            if (trackmania.Address == IntPtr.Zero)
            {
                return;
            }
            CGameCtnReplayRecord replayRecord = trackmania.ReplayRecord;
            if (replayRecord.Address != IntPtr.Zero && replayRecord.ReplayFiles.Count > 0)
            {
                string filePath = ((CSystemFidFile)replayRecord.ReplayFiles[0]).GetFullName();
                ReplayFilePath = filePath;
                replayFilePathTextBox.Text = filePath;
            }
        }

        private void replayFilePathTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!autoFetchReplayCheckBox.Checked)
            {
                ReplayFilePath = replayFilePathTextBox.Text;
            }
        }

        private void startAtTimeTextBox_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(startAtTimeTextBox.Text, out StartAtTime);
        }

        private void timeJumpBtn_Click(object sender, EventArgs e)
        {
            CMwCmdBufferCore core = CMwCmdBufferCore.TheCoreCmdBuffer;
            CMwTimer timer = core.Timer;
            
            core.ActiveTimerAdapter.Resync();
        }

        private void useRegularInputCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            StartAtTimeRegularInput = useRegularInputCheckBox.Checked;
        }
    }
}
