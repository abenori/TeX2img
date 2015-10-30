using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TeX2img {
    class TempFilesDeleter : IDisposable{
        public TempFilesDeleter() { }
        public void Dispose() {
            if (Properties.Settings.Default.deleteTmpFileFlag) {
                try {
                    foreach (var f in tmpTeXFiles) {
                        foreach (var ext in new string[] { ".tex", ".dvi", ".pdf", ".log", ".aux", ".tmp", ".out", ".pdf", ".ps" }) {
                            File.Delete(f + ext);
                        }
                    }
                    foreach (var f in tmpFiles) {
                        File.Delete(f);
                    }
                }
                catch (Exception) { }
            }
            tmpTeXFiles.Clear();
            tmpFiles.Clear();
        }
        private List<string> tmpFiles = new List<string>();
        private List<string> tmpTeXFiles = new List<string>();
        public void AddFile(string file) { tmpFiles.Add(file); }
        public void AddTeXFile(string file) { tmpTeXFiles.Add(file); }

        public static string GetTempFileName(string ext = ".tex") {
            return GetTempFileName(ext, Path.GetTempPath());
        }

        public static string GetTempFileName(string ext, string dir) {
            for(int i = 0 ; i < 1000 ; ++i) {
                var random = Path.ChangeExtension(Path.GetRandomFileName(), ext);
                if(!File.Exists(Path.Combine(dir, random))) return random;
            }
            return null;
        }
    }
}
