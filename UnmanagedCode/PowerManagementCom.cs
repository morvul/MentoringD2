using System;
using System.Runtime.InteropServices;

namespace UnmanagedCode
{
    [ComVisible(true)]
    [Guid("E54F8365-D5EB-4C2B-A3FE-7C2B7EDF902A")]
    [ClassInterface(ClassInterfaceType.None)]
    public class PowerManagementCom : IPowerManagementCom
    {
        public DateTime GetLastWakeTime()
        {
            return PowerManagement.GetLastWakeTime();
        }

        public DateTime GetLastSleepTime()
        {
            return PowerManagement.GetLastSleepTime();
        }

        public PowerManagement.SystemBatteryState GetSystemBatteryState()
        {
            return PowerManagement.GetSystemBatteryState();
        }

        public bool ReserveHiberFile()
        {
            return PowerManagement.ReserveHiberFile();
        }

        public bool RemoveHiberFile()
        {
            return PowerManagement.RemoveHiberFile();
        }

        public bool Sleep()
        {
            return PowerManagement.Sleep();
        }

        public bool Hibernate()
        {
            return PowerManagement.Hibernate();
        }
    }
}
