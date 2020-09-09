using System;
using System.Collections.Generic;
using System.Text;

namespace TeX2img {
    class CUIOutput : IOutputController {
        bool quiet = false;
        public CUIOutput(bool q) { quiet = q; }
        public CUIOutput() { }

        public void showPathError(string exeName, string necessary) {
            Console.WriteLine(String.Format(Properties.Resources.PATHERROR, exeName, necessary));
        }
        public void showNoToolError(string item, string tool) {
            Console.WriteLine(string.Format(Properties.Resources.NOTOOLERROR, item, tool));
        }

        public void showExtensionError(string file) {
            Console.WriteLine(String.Format(Properties.Resources.INVALID_EXTENSION, file));
        }

        public void appendOutput(string log) {
            if(!quiet) Console.Write(log);
        }

        public void showGenerateError() {
            Console.WriteLine(Properties.Resources.GENERATEERROR);
        }

        public void scrollOutputTextBoxToEnd() {
        }

        public void showUnauthorizedError(string filePath) {
            Console.WriteLine(String.Format(Properties.Resources.AUTHORIZEDERROR, filePath));
        }

        public void showIOError(string filePath) {
            Console.WriteLine(String.Format(Properties.Resources.IOERROR, filePath));
        }

        public void showError(string msg) {
            Console.WriteLine(msg);
        }

        public bool askYesorNo(string msg) {
            Console.WriteLine(msg + "（y/n）");
            while(true) {
                var s = Console.ReadLine().ToLower();
                switch(s) {
                case "y": return true;
                case "n": return false;
                default:
                    Console.WriteLine(Properties.Resources.PUSHYORN);
                    break;
                }
            }
        }

        public void showToolError(string tool) {
            var path = System.IO.Path.Combine(Converter.ShortToolPath, tool);
            Console.WriteLine(String.Format(Properties.Resources.TOOLERROR, path, Converter.ShortToolPath));
        }

        public void errorIgnoredWarning() {
            Console.WriteLine(Properties.Resources.IGNOREDERRORWARNING);
        }
    }
}
