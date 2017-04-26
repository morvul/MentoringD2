using System;
using System.Runtime.InteropServices;

namespace UnmanagedCode
{
    public static class PowerManagement
    {
        #region Structs

        public enum InfoLevel
        {
            SystemBatteryState = 5,
            SystemHiberFileControl = 10,
            SystemPowerInformation = 12,
            LastWakeTime = 14,
            LastSleepTime = 15
        }

        public enum HiberFileOperation
        {
            RemoveHiberFile = 0,
            ReserveHiberFile = 1,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SystemBatteryState
        {
            [MarshalAs(UnmanagedType.I1)]
            public bool AcOnLine;
            [MarshalAs(UnmanagedType.I1)]
            public bool BatteryPresent;
            [MarshalAs(UnmanagedType.I1)]
            public bool Charging;
            [MarshalAs(UnmanagedType.I1)]
            public bool Discharging;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.I1)]
            public bool[] Spare1;
            public int MaxCapacity;
            public int RemainingCapacity;
            public int Rate;
            public int EstimatedTime;
            public int DefaultAlert1;
            public int DefaultAlert2;
        }

        #endregion


        #region WinApi functions

        [DllImport("kernel32")]
        private static extern ulong GetTickCount64();


        [DllImport("powrprof.dll")]
        public static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

        [DllImport("powrprof.dll")]
        public static extern uint CallNtPowerInformation(
            InfoLevel informationLevel,
            IntPtr inputBuffer,
            int inputBufferSize,
            [MarshalAs(UnmanagedType.U8)]
            out long result,
            int resultSize);

        [DllImport("powrprof.dll")]
        public static extern uint CallNtPowerInformation(
            InfoLevel informationLevel,
            IntPtr inputBuffer,
            int inputBufferSize,
            [MarshalAs(UnmanagedType.Struct)]
            out SystemBatteryState result,
            int resultSize);


        [DllImport("powrprof.dll")]
        public static extern uint CallNtPowerInformation(
            InfoLevel informationLevel,
            IntPtr inputParam,
            int inputParamSize,
            IntPtr result,
            int resultSize);

        #endregion


        #region Power information getting

        public static DateTime GetLastWakeTime()
        {
            return GetTime(InfoLevel.LastWakeTime);
        }

        public static DateTime GetLastSleepTime()
        {
            return GetTime(InfoLevel.LastSleepTime);
        }

        public static SystemBatteryState GetSystemBatteryState()
        {
            var resultSize = Marshal.SizeOf<SystemBatteryState>();
            SystemBatteryState result;
            CallNtPowerInformation(
                InfoLevel.SystemBatteryState,
                IntPtr.Zero,
                0,
                out result,
                resultSize);
            return result;
        }

        private static DateTime GetTime(InfoLevel infoLevel)
        {
            var resultSize = sizeof (ulong);
            long ticksElapsed;
            CallNtPowerInformation(
                infoLevel,
                IntPtr.Zero,
                0,
                out ticksElapsed,
                resultSize);

            var startupTicksElapsed = GetTickCount64()*10000;
            var result = DateTime.Now - TimeSpan.FromTicks((long) startupTicksElapsed) +
                         TimeSpan.FromTicks(ticksElapsed);
            return result;
        }

        #endregion

        #region Hibernation file operations

        public static bool ReserveHiberFile()
        {
            return HiberFileControl(HiberFileOperation.ReserveHiberFile);
        }

        public static bool RemoveHiberFile()
        {
            return HiberFileControl(HiberFileOperation.RemoveHiberFile);
        }

        public static bool HiberFileControl(HiberFileOperation operation)
        {
            var inputBufferSize = sizeof (HiberFileOperation);
            var inputBuffer = Marshal.AllocHGlobal(inputBufferSize);

            Marshal.WriteInt32(inputBuffer, 0, (int)operation);
            var errorCode = CallNtPowerInformation(InfoLevel.SystemHiberFileControl, inputBuffer, inputBufferSize, IntPtr.Zero, 0);

            Marshal.FreeHGlobal(inputBuffer);
            return errorCode == 0;
        }

        #endregion

        public static bool Sleep()
        {
            var result = SetSuspendState(false, false, false);
            return result;
        }

        public static bool Hibernate()
        {
            var result = SetSuspendState(true, false, false);
            return result;
        }
    }
}
