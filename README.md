# CSharp Alternative Shellcode Callbacks

## Alternative shellcode execution techniques using Windows callback functions

Each CSharp file contains code to execute shellcode using native Windows callbacks. I tried to use much less common callback techniques that weren't typically documented online as far as I could tell. This way they _should_ be more evasive.<br /><br />
_NOTE: The shellcode samples used in every C# file will execute Windows 64-bit Calculator.exe <br />
NOTE 2: I've also included the C/C++ code samples from VX-Underground which some of these were based off of_<br />

**The C# malware samples contain the following execution flows:**
- Base64 decode 64-bit shellcode
- XOR decrypt shellcode
- Allocate memory with VirtualAlloc()
- Copy shellcode to memory with Marshal.Copy()
- Execute shellcode with callback function

For each code sample, any decrypting/decoding routines could be changed to fit your needs. Also, use D/Invoke, obfuscation, junk code, etc!<br />

It's also possible to replace the some functions in the code with alternatives such as:
- Use RtlMoveMemory() instead of Marshal.Copy()
- WriteProcessMemory() instead of Marshal.Copy()
- Use the managed function Marshal.AllocHGlobal() to allocate space instead of VirtualAlloc()

For reducing entropy of payloads, use my [DictionShellcode](https://github.com/wsummerhill/DictionShellcode) tool or a similar technique to obfuscate shellcode without encryption/decryption.

**Compiling intructions with csc.exe**:
```
// Compile EXE
csc.exe /target:exe /out:TestExecutable.exe CSharp-Callback_[FILENAME].cs

// Compile DLL
csc.exe /target:library /out:TestLibrary.dll CSharp-Callback_[FILENAME].cs
```

Enjoy the CSharp samples and good luck! Please povide any feedback if you'd like additional callback functions to be implemented.

Associated blog post: [https://wsummerhill.github.io/malware/2022/12/09/CSharp-Alt-Shellcode-Callbacks.html](https://wsummerhill.github.io/malware/2022/12/09/CSharp-Alt-Shellcode-Callbacks.html)

---------------------------

## Reference

Thanks to [DamonMohammadbagher/NativePayload_CBT](https://github.com/DamonMohammadbagher/NativePayload_CBT) for the inspiration.<br />
Thanks to [VX Underground](https://www.vx-underground.org) for the Windows malware templates in C.
