using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;


namespace TeX2img {
    class AlternativeDataStream {
        const uint GENERIC_READ = 0x80000000;
        const uint GENERIC_WRITE = 0x40000000;
        const uint GENERIC_EXECUTE = 0x20000000;

        const uint FILE_SHARE_READ = 0x00000001;
        const uint FILE_SHARE_WRITE = 0x00000002;
        const uint FILE_SHARE_DELETE = 0x00000004;

        const uint CREATE_NEW = 1;
        const uint CREATE_ALWAYS = 2;
        const uint OPEN_EXISTING = 3;
        const uint OPEN_ALWAYS = 4;
        const uint TRUNCATE_EXISTING = 5;

        const uint FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
        const uint FILE_ATTRIBUTE_ENCRYPTED = 0x00004000;
        const uint FILE_ATTRIBUTE_HIDDEN = 0x00000002;
        const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        const uint FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000;
        const uint FILE_ATTRIBUTE_OFFLINE = 0x00001000;
        const uint FILE_ATTRIBUTE_READONLY = 0x00000001;
        const uint FILE_ATTRIBUTE_SYSTEM = 0x00000004;
        const uint FILE_ATTRIBUTE_TEMPORARY = 0x00000100;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        static bool IsDriveNTFS(string path) {
            try {
                string drive;
                if(path[1] == ':') {
                    drive = path.Substring(0, 1);
                } else {
                    drive = System.IO.Directory.GetCurrentDirectory().Substring(0,1);
                }
                var info = new System.IO.DriveInfo(drive);
                return (info.DriveFormat == "NTFS");
            }
            catch {
                return false;
            }
        }

        public static FileStream WriteAlternativeDataStream(string file, string streamname){
            if(!IsDriveNTFS(file)) throw new NotImplementedException();
            var fileHandle = CreateFile(file + ":" + streamname, GENERIC_WRITE, FILE_SHARE_READ, IntPtr.Zero, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
            if(fileHandle.IsInvalid) Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            return new FileStream(fileHandle, FileAccess.Write);
        }

        public static FileStream ReadAlternativeDataStream(string file, string streamname) {
            if(!IsDriveNTFS(file)) throw new NotImplementedException();
            var fileHandle = CreateFile(file + ":" + streamname, GENERIC_READ, FILE_SHARE_READ, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
            if(fileHandle.IsInvalid) Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            return new FileStream(fileHandle, FileAccess.Read);
        }
    }
}
