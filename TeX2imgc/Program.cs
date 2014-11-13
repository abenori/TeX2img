using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace TeX2imgc {
    class Program {
        static void Main(string[] args) {
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string tex2img = dir + "\\tex2img.exe";
            if(!File.Exists(tex2img)) {
                Console.WriteLine("TeX2img.exe が見つかりませんでした．");
                Environment.Exit(-1);
            }
            using(Process proc = new Process()) {
                proc.StartInfo.FileName = tex2img;
                proc.StartInfo.Arguments = "/nogui ";
                var reg = new System.Text.RegularExpressions.Regex("^[^ ]*(\".*\")*[ ^]* ");
                var m = reg.Match(Environment.CommandLine);
                if(m.Success) proc.StartInfo.Arguments += Environment.CommandLine.Substring(m.Length);
                else proc.StartInfo.Arguments += Environment.CommandLine;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.OutputDataReceived += ((s, e) => Console.WriteLine(e.Data));
                if(!proc.Start()) { 
                    Console.WriteLine("TeX2img.exe の実行に失敗しました．");
                }
                proc.BeginOutputReadLine();
                proc.WaitForExit();
                proc.CancelOutputRead();
                Environment.ExitCode = proc.ExitCode;
            }
            //Console.ReadKey();
        }
    }
}
