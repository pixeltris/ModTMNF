using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    public unsafe struct CMwTimer
    {
        public IntPtr Address;

        public CMwTimer(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwTimer(IntPtr address)
        {
            return new CMwTimer(address);
        }

        /// <summary>
        /// Initial value of 1. Never changes?
        /// </summary>
        public uint Unk_0
        {
            get { return *(uint*)(Address + OT.CMwTimer.Unk_0); }
        }

        /// <summary>
        /// Initial value of 100. Never changes? SimplePeriod? See CGameGhostData in gbx readers
        /// </summary>
        public uint Unk_4
        {
            get { return *(uint*)(Address + OT.CMwTimer.Unk_4); }
        }

        /// <summary>
        /// (total game time starting at 0 from launch)
        /// CMwTimer::GetTickTime
        /// </summary>
        public uint TickTime
        {
            get { return *(uint*)(Address + OT.CMwTimer.TickTime); }
            set { *(uint*)(Address + OT.CMwTimer.TickTime) = value; }
        }

        /// <summary>
        /// How long each frame takes milliseconds (i.e. at 60FPS this value will be either 16 or 17 depending on rounding)
        /// (1000 / FPS)
        /// </summary>
        public uint FrameTime
        {
            get { return *(uint*)(Address + OT.CMwTimer.FrameTime); }
        }

        /// <summary>
        /// NOTE: This is WRONG! fix me! Not sure what this is exactly.
        /// How long each frame takes in seconds (i.e. at 60FPS this value will be around 0.0166)
        /// (1 / FPS)
        /// </summary>
        public float FrameTimeInSeconds
        {
            get { return *(uint*)(Address + OT.CMwTimer.FrameTimeInSeconds); }
        }

        /// <summary>
        /// Frame rate / FPS
        /// </summary>
        public float FrameRate
        {
            get { return *(float*)(Address + OT.CMwTimer.FrameRate); }
        }

        /// <summary>
        /// Initial performance counter? Most likely considering it's a int64 type
        /// QueryPerformanceCounter()
        /// </summary>
        public ulong InitialPerformanceCounter
        {
            get { return *(ulong*)(Address + OT.CMwTimer.InitialPerformanceCounter); }
            set { *(ulong*)(Address + OT.CMwTimer.InitialPerformanceCounter) = value; }
        }

        /// <summary>
        /// timeGetTime() fetched when the CMwTimer object was created. Never updated?
        /// </summary>
        public uint InitialSystemTime
        {
            get { return *(uint*)(Address + OT.CMwTimer.InitialSystemTime); }
            set { *(uint*)(Address + OT.CMwTimer.InitialSystemTime) = value; }
        }

        /// <summary>
        /// Always 0?
        /// </summary>
        public uint Unk_36
        {
            get { return *(uint*)(Address + OT.CMwTimer.Unk_36); }
            set { *(uint*)(Address + OT.CMwTimer.Unk_36) = value; }
        }

        /// <summary>
        /// Counts up (several hundred / thousand depending on FPS / lag) then resets to 0 and repeats.
        /// </summary>
        public uint Unk_40
        {
            get { return *(uint*)(Address + OT.CMwTimer.Unk_40); }
            set { *(uint*)(Address + OT.CMwTimer.Unk_40) = value; }
        }

        /// <summary>
        /// Counts up to 19 then resets to 0 and repeats?
        /// </summary>
        public uint Unk_44
        {
            get { return *(uint*)(Address + OT.CMwTimer.Unk_44); }
            set { *(uint*)(Address + OT.CMwTimer.Unk_44) = value; }
        }

        public void InitTimer()
        {
            FT.CMwTimer.InitTimer(this);
        }

        public void ChopTime()
        {
            FT.CMwTimer.ChopTime(this);
        }

        public uint GetElapsedTimeSinceInit()
        {
            return FT.CMwTimer.GetElapsedTimeSinceInit(this);
        }

        public uint GetTickTime()
        {
            return *FT.CMwTimer.GetTickTime(this);
        }

        public void SimulateDeltaTime(uint deltaTime)
        {
            FT.CMwTimer.SimulateDeltaTime(this, deltaTime);
        }

        public void Tick()
        {
            FT.CMwTimer.Tick(this);
        }

        public static uint SecondsToMwTime(float seconds)
        {
            // (uint)(seconds * 1000)
            return FT.CMwTimer.SecondsToMwTime(seconds);
        }

        public static void GetHhMmSsTime24StringFromMwTime(uint time, out string result)
        {
            CFastString str = new CFastString();
            FT.CMwTimer.GetHhMmSsTime24StringFromMwTime(time, ref str);
            result = str;
            str.Delete();
        }

        public static void GetHhMmSsTimeStringFromMwTime(uint time, out string result)
        {
            CFastString str = new CFastString();
            FT.CMwTimer.GetHhMmSsTimeStringFromMwTime(time, ref str);
            result = str;
            str.Delete();
        }

        public static void GetHhMmTimeStringFromMwTime(uint time, out string result)
        {
            CFastString str = new CFastString();
            FT.CMwTimer.GetHhMmTimeStringFromMwTime(time, ref str);
            result = str;
            str.Delete();
        }

        public static void GetMmSsCcTimeStringFromMwTime(uint time, out string result)
        {
            CFastString str = new CFastString();
            FT.CMwTimer.GetMmSsCcTimeStringFromMwTime(time, ref str);
            result = str;
            str.Delete();
        }

        public static void GetMmSsTimeStringFromMwTime(uint time, out string result)
        {
            CFastString str = new CFastString();
            FT.CMwTimer.GetMmSsTimeStringFromMwTime(time, ref str);
            result = str;
            str.Delete();
        }

        public static bool GetMwTimeFromHhMmSsTimeString(string str, out uint result)
        {
            return FT.CMwTimer.GetMwTimeFromHhMmSsTimeString(str, out result);
        }

        public static bool GetMwTimeFromHhMmTimeString(string str, out uint result)
        {
            return FT.CMwTimer.GetMwTimeFromHhMmTimeString(str, out result);
        }

        public static bool GetMwTimeFromMmSsCcTimeString(string str, out uint result)
        {
            return FT.CMwTimer.GetMwTimeFromMmSsCcTimeString(str, out result);
        }

        public static bool GetMwTimeFromMmSsTimeString(string str, out uint result)
        {
            return FT.CMwTimer.GetMwTimeFromMmSsTimeString(str, out result);
        }

        public static bool CMwTimer_CalibrateEnd_ShouldSwitchOff()
        {
            return FT.CMwTimer.CMwTimer_CalibrateEnd_ShouldSwitchOff();
        }

        public static void CMwTimer_CalibrateStart()
        {
            FT.CMwTimer.CMwTimer_CalibrateStart();
        }
    }
}