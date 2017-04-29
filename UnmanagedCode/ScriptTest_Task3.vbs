Dim powerManager
Dim message
Dim info
Set powerManager = CreateObject("UnmanagedCode.PowerManagementCom")
message = "Last Sleep Time: " & powerManager.GetLastSleepTime() & vbCrLf
message = message & "Last Wake Time: " & powerManager.GetLastWakeTime() & vbCrLf
info = powerManager.GetSystemBatteryState()
MsgBox message