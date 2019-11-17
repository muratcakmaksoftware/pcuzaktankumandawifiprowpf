using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace wifiuzaktankontrolpro
{    

    static class ExitWindows
    {

        private struct LUID
        {
            public int LowPart;
            public int HighPart;
        }
        private struct LUID_AND_ATTRIBUTES
        {
            public LUID pLuid;
            public int Attributes;
        }
        private struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }

        //UYKU İÇİN
        [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);


        [DllImport("advapi32.dll")]
        static extern int OpenProcessToken(IntPtr ProcessHandle,
          int DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
          [MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
          ref TOKEN_PRIVILEGES NewState,
          UInt32 BufferLength,
          IntPtr PreviousState,
          IntPtr ReturnLength);

        [DllImport("advapi32.dll")]
        static extern int LookupPrivilegeValue(string lpSystemName,
          string lpName, out LUID lpLuid);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int ExitWindowsEx(uint uFlags, uint dwReason);

        const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        const short SE_PRIVILEGE_ENABLED = 2;
        const short TOKEN_ADJUST_PRIVILEGES = 32;
        const short TOKEN_QUERY = 8;

        const ushort EWX_LOGOFF = 0;
        const ushort EWX_POWEROFF = 0x00000008;
        const ushort EWX_REBOOT = 0x00000002;
        const ushort EWX_RESTARTAPPS = 0x00000040;
        const ushort EWX_SHUTDOWN = 0x00000001;
        const ushort EWX_FORCE = 0x00000004;

        private static void getPrivileges()
        {
            IntPtr hToken;
            TOKEN_PRIVILEGES tkp;

            OpenProcessToken(Process.GetCurrentProcess().Handle,
              TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out hToken);
            tkp.PrivilegeCount = 1;
            tkp.Privileges.Attributes = SE_PRIVILEGE_ENABLED;
            LookupPrivilegeValue("", SE_SHUTDOWN_NAME,
              out tkp.Privileges.pLuid);
            AdjustTokenPrivileges(hToken, false, ref tkp,
              0U, IntPtr.Zero, IntPtr.Zero);
        }

        public static void Shutdown() { Shutdown(false); }
        public static void Shutdown(bool force)
        {
            getPrivileges();
            ExitWindowsEx(EWX_SHUTDOWN |
              (uint)(force ? EWX_FORCE : 0) | EWX_POWEROFF, 0);
        }

        public static void Reboot() { Reboot(false); }
        public static void Reboot(bool force)
        {
            getPrivileges();
            ExitWindowsEx(EWX_REBOOT |
              (uint)(force ? EWX_FORCE : 0), 0);
        }

        public static void LogOff() { LogOff(false); }
        public static void LogOff(bool force)
        {
            getPrivileges();
            ExitWindowsEx(EWX_LOGOFF |
              (uint)(force ? EWX_FORCE : 0), 0);
        }

        public static void Sleep() { Sleep(false); }
        public static void Sleep(bool derinlik)
        {
            if (derinlik)
            {
                // Hibernate
                SetSuspendState(true, true, true);
            }            
            else
            {
                // Standby
                SetSuspendState(false, true, true);
            }
            
        }
    }
}
