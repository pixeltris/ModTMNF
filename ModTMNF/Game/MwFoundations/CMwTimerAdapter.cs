using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Game
{
    // NOTE: The maximum amount of time that can be stepped is 5 seconds. If the game lags or if you
    //       manually wait more than 5 seconds to step the game, then only 5 seconds will pass in game.
    //       (this would be 500 steps of physics? if stepping at a 100/s rate)

    /// <summary>
    /// CMwTimerAdapter keeps time for the race. Where as CMwTimer keeps time of the entire game since launch.
    /// 
    /// NOTE: The "Reference" times are at updated at various locations (race restart, pause menu, misc other conditions during play). 
    ///       But for the most part have a fairly static value. They are used to calculate other time values.
    ///       All synced by CMwTimerAdapter::Resync? (which make for the small time tweaks).
    /// </summary>
    public unsafe struct CMwTimerAdapter
    {
        public IntPtr Address;

        public CMwTimerAdapter(IntPtr address)
        {
            Address = address;
        }

        public static implicit operator CMwTimerAdapter(IntPtr address)
        {
            return new CMwTimerAdapter(address);
        }

        public CMwTimer Timer
        {
            get { return *(IntPtr*)(Address + OT.CMwTimerAdapter.Timer); }
        }

        /// <summary>
        /// Reference system time (timeGetTime)
        /// </summary>
        public uint ReferenceSystemTime
        {
            get { return *(uint*)(Address + OT.CMwTimerAdapter.ReferenceSystemTime); }
            set { *(uint*)(Address + OT.CMwTimerAdapter.ReferenceSystemTime) = value; }
        }

        /// <summary>
        /// Relative speed / time multiplier (during pause screen this is set to 0)
        /// CMwTimerAdapter::SetRelativeSpeed
        /// CMwTimerAdapter::GetRelativeSpeed
        /// </summary>
        public float RelativeSpeed
        {
            get { return *(float*)(Address + OT.CMwTimerAdapter.RelativeSpeed); }
        }

        /// <summary>
        /// Reference game time (game time starts from launch)
        /// </summary>
        public uint ReferenceGameTime
        {
            get { return *(uint*)(Address + OT.CMwTimerAdapter.ReferenceGameTime); }
            set { *(uint*)(Address + OT.CMwTimerAdapter.ReferenceGameTime) = value; }
        }

        /// <summary>
        /// Reference race time (race time starts from the countdown)
        /// NOTE: This value starts with a value of '1' for each race.
        /// </summary>
        public uint ReferenceRaceTime
        {
            get { return *(uint*)(Address + OT.CMwTimerAdapter.ReferenceRaceTime); }
            set { *(uint*)(Address + OT.CMwTimerAdapter.ReferenceRaceTime) = value; }
        }

        /// <summary>
        /// (race time)
        /// CMwTimerAdapter::GetTimeAtHumanTick
        /// </summary>
        public uint TimeAtHumanTick
        {
            get { return *(uint*)(Address + OT.CMwTimerAdapter.TimeAtHumanTick); }
            set { *(uint*)(Address + OT.CMwTimerAdapter.TimeAtHumanTick) = value; }
        }

        /// <summary>
        /// Expected to be 10 in places such as input/physics.
        /// Other locations this can flip between 10/50.
        /// Has a starting value of 100 prior to starting any races.
        /// </summary>
        public uint DeltaTime
        {
            get { return *(uint*)(Address + OT.CMwTimerAdapter.DeltaTime); }
            set { *(uint*)(Address + OT.CMwTimerAdapter.DeltaTime) = value; }
        }

        /// <summary>
        /// (race time)
        /// CMwTimerAdapter::GetTickTime
        /// </summary>
        public uint TickTime
        {
            get { return *(uint*)(Address + OT.CMwTimerAdapter.TickTime); }
            set { *(uint*)(Address + OT.CMwTimerAdapter.TickTime) = value; }
        }

        public void InitTimer(float relativeSpeed)
        {
            FT.CMwTimerAdapter.InitTimer(this, relativeSpeed);
        }

        public void Resync()
        {
            FT.CMwTimerAdapter.Resync(this);
        }

        public float GetRelativeSpeed()
        {
            return FT.CMwTimerAdapter.GetRelativeSpeed(this);
        }

        public void SetRelativeSpeed(float relativeSpeed)
        {
            FT.CMwTimerAdapter.SetRelativeSpeed(this, relativeSpeed);
        }

        public void ComputeTimeAtHumanTick()
        {
            FT.CMwTimerAdapter.ComputeTimeAtHumanTick(this);
        }

        public uint ConvertHumanToGame(uint time)
        {
            return FT.CMwTimerAdapter.ConvertHumanToGame(this, time);
        }

        public uint ConvertSystemToGame(uint time)
        {
            return FT.CMwTimerAdapter.ConvertSystemToGame(this, time);
        }

        public float GetAsyncPeriod()
        {
            return FT.CMwTimerAdapter.GetAsyncPeriod(this);
        }

        public uint GetAsyncPeriodMwTime()
        {
            return FT.CMwTimerAdapter.GetAsyncPeriodMwTime(this);
        }

        public uint GetSchemePeriod()
        {
            return FT.CMwTimerAdapter.GetSchemePeriod(this);
        }

        public uint GetTickTime()
        {
            return *FT.CMwTimerAdapter.GetTickTime(this);
        }

        public uint GetTime()
        {
            return FT.CMwTimerAdapter.GetTime(this);
        }

        public uint GetTimeAtHumanTick()
        {
            return *FT.CMwTimerAdapter.GetTimeAtHumanTick(this);
        }

        public uint GetTimeAtPreviousHumanTick()
        {
            return FT.CMwTimerAdapter.GetTimeAtPreviousHumanTick(this);
        }

        public void SetCurrentTimeAtHumanTick(uint time)
        {
            FT.CMwTimerAdapter.SetCurrentTimeAtHumanTick(this, time);
        }
    }
}
