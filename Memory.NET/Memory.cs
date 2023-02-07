using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Diagnostics;

namespace Memory.NET
{
    public class Native
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, uint processId);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out, MarshalAs(UnmanagedType.AsAny)] object lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }
    }

    public class MemoryAccess
    {
        private IntPtr Handle { get; set; }
        public long Base { get; set; }

        public MemoryAccess(IntPtr handle)
        {
            this.Handle = handle;
        }

        public MemoryAccess(int pid) : this(Native.OpenProcess((uint)Native.ProcessAccessFlags.All, false, (uint)pid))
        {
            Base = Process.GetProcessById(pid).MainModule.BaseAddress.ToInt64();
        }

        private T ReadBase<T>(long address) where T : struct
        {
            T[] buffer = new T[Marshal.SizeOf<T>()];
            Native.ReadProcessMemory(Handle, new IntPtr(address), buffer, Marshal.SizeOf<T>(), out var bytesread);
            return buffer.First();
        }

        private long ReadChain(long begin, params long[] offsets)
        {
            var cur = begin;
            for (var i = 0; i < offsets.Length - 1; i++)
            {
                cur = ReadBase<long>(cur + offsets[i]);
            }

            if (offsets.Length > 0)
            {
                cur += offsets[offsets.Length - 1];
            }

            return cur;
        }

        public T Read<T>(long begin, params long[] offsets) where T : struct
        {
            return ReadBase<T>(ReadChain(begin, offsets));
        }

        public byte ReadByte(long begin, params long[] offsets)
        {

            return ReadBase<byte>(ReadChain(begin, offsets));
        }
    }
}
