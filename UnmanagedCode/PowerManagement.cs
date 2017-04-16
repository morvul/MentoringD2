using System;
using System.Runtime.InteropServices;

namespace UnmanagedCode
{
    public static class PowerManagement
    {
        private enum InfoLevel
        {
            SystemPowerInformation = 14
        }


        #region WinApi functions

        [DllImport("kernel32")]
        private static extern ulong GetTickCount64();

        [DllImport("powrprof.dll")]
        public static extern uint CallNtPowerInformation(
            int informationLevel,
            IntPtr inputBuffer,
            uint inputBufferSize,
            out ulong outputBuffer,
            uint outputBufferSize);

        #endregion


        public static DateTime GetLastWakeTime()
        {
            var outputBufferSize = Marshal.SizeOf(typeof(ulong));
            ulong ticksElapsed;
            var result = CallNtPowerInformation(
                (int)InfoLevel.SystemPowerInformation,
                (IntPtr) null,
                0,
                out ticksElapsed,
                (uint)outputBufferSize);

            var lastWakeTime = new DateTime((uint)ticksElapsed);
            return lastWakeTime;
        }
    }
}
