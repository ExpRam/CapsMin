using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CapsMin
{
    public class Program
    {
        #region Consts fileds and etc.
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int VK_SHIFT = 0x10;
        
        public  delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        
        public static LowLevelKeyboardProc Proc;
        public static IntPtr HookId = IntPtr.Zero;

        #endregion

        #region Imports
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        static extern short GetKeyState(int nVirtKey);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow(); 
        
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, Int32 wParam, Int32 lParam);
        #endregion

        #region Methods
        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
        
        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr) WM_KEYDOWN || wParam == (IntPtr) WM_SYSKEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                
                if ((Keys) vkCode == Keys.CapsLock && (GetKeyState(VK_SHIFT) >= 0))
                {
                    PostMessage(GetForegroundWindow(), 0x0050, 2, 0);
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(HookId, nCode, wParam, lParam);            
        }

        #endregion
        
        #region MainMethod

        static void Main()
        {
            Proc = HookCallback;
            HookId = SetHook(Proc);
            
            Application.Run();

            UnhookWindowsHookEx(HookId);
        }

        #endregion

    }
}