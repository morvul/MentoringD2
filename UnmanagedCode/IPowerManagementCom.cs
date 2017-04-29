using System;
using System.Runtime.InteropServices;

namespace UnmanagedCode
{
    [ComVisible(true)]
    [Guid("FFC4D428-5773-4AFC-9A55-7AE4B64F17C4")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IPowerManagementCom
    {
        DateTime GetLastWakeTime();

        DateTime GetLastSleepTime();

        PowerManagement.SystemBatteryState GetSystemBatteryState();

        bool ReserveHiberFile();

        bool RemoveHiberFile();

        bool Sleep();

        bool Hibernate();
    }
}
